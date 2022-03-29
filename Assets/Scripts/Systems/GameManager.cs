using System;
using Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace Systems
{
    public partial class GameManager : SystemBase
    {
        bool m_ButtonClicked;
        Label m_Label;

        protected override void OnStartRunning()
        {
            var doc = this.GetSingleton<UIStore>().Doc;
            if (doc.rootVisualElement.Q(className: "Switch") is Button button)
            {
                button.text = "Spam to Jump!";
                button.RegisterCallback<ClickEvent>(ev => m_ButtonClicked = true);
            }

            if (doc.rootVisualElement.Q(className: "Label") is Label label)
                m_Label = label;

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