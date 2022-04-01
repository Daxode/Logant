using System;
using Data;
using Systems.GameObjects;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;

namespace Systems
{
    public partial class GameManager : SystemBase
    {
        bool m_ButtonClicked;
        Label m_Label;

        protected override void OnStartRunning()
        {
            var hybridEntity = GetEntityQuery(typeof(UIDocument)).GetSingletonEntity();
            var doc = EntityManager.GetComponentObject<UIDocument>(hybridEntity);

            var button = doc.rootVisualElement.Q<Button>("Jump");
            if (button!=null)
            {
                button.text = "<i>Jump</i> with this";
                button.clicked += () => m_ButtonClicked = true;
                // var idk1 = new StyleValues {backgroundColor = Color.black};
                // var idk2 = new StyleValues {backgroundColor = new Color(0.2f, 0.67f, 0.65f)};
                // button.experimental.animation.Start( idk1, idk2, 5000);
                button.AddToClassList("jump");
            }

            m_Label = doc.rootVisualElement.Q<Label>();

        }

        protected override void OnUpdate()
        {
            if (m_ButtonClicked)
            {
                Entities.ForEach((ref PhysicsVelocity vel, in PhysicsMass mass) 
                    => vel.ApplyLinearImpulse(in mass, math.up() * 0.5f)).Run();
                m_ButtonClicked = false;
            }

            var label = m_Label;
            if (label != null)
            {
                label.text = "";
                Entities.ForEach((in AntHillData hill) => label.text += $"Ants spawned: {hill.numberOfAntsSpawned}/{hill.numberOfAnts}\n").WithoutBurst().Run();
                Entities.ForEach((in ResourceStore resourceStore) => label.text += $"{resourceStore.Type}[{resourceStore.Current}/{resourceStore.Total}]\n").WithoutBurst().Run();
            }


#if !UNITY_EDITOR
        if (Keyboard.current.escapeKey.isPressed) 
            Application.Quit();
#endif
        }
    }
}