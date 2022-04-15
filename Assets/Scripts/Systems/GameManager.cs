using System;
using System.Linq;
using Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Systems
{
    public partial class GameManager : SystemBase
    {
        bool m_JumpPressed;
        Label m_Label;

        NativeArray<FixedString64Bytes> m_ResourceTypeToString;
        FixedString64Bytes m_AntString;
        FixedString64Bytes m_ColorTagStart;
        EntityToColorSystem m_EntityToColorSystem;
        protected override void OnCreate()
        {
            m_ResourceTypeToString = new NativeArray<FixedString64Bytes>(Enum.GetNames(typeof(ResourceType)).Select(n=>new FixedString64Bytes(n)).ToArray(), Allocator.Persistent);
            m_AntString = new FixedString64Bytes("Ants");
            m_ColorTagStart = new FixedString64Bytes("<color=#");
            m_EntityToColorSystem = World.GetExistingSystem<EntityToColorSystem>();
        }

        protected override void OnDestroy() => m_ResourceTypeToString.Dispose();
        
        protected override void OnStartRunning()
        {
            var docEntity = GetSingletonEntity<UIDocument>();
            var doc = EntityManager.GetComponentObject<UIDocument>(docEntity);

            // Setup UI
            var jumpBtn = doc.rootVisualElement.Q<Button>("Jump");
            if (jumpBtn!=null)
            {
                jumpBtn.text = "<i>Jump</i> with <b>this</b>";
                jumpBtn.clicked += () => m_JumpPressed = true;
                jumpBtn.AddToClassList("jump");
            }
            
            var startBtn = doc.rootVisualElement.Q<Button>("Start");
            if (startBtn!=null)
            {
                startBtn.clicked += () =>
                {
                    if (TryGetSingleton<GlobalData>(out var gd))
                    {
                        gd.HasStarted = true;
                        SetSingleton(gd);
                    }
                };
                startBtn.AddToClassList("Start");
            }
            
            m_Label = doc.rootVisualElement.Q<Label>();
        }
        
        protected override void OnUpdate()
        { 
            if (m_JumpPressed)
            {
                Entities.ForEach((ref PhysicsVelocity vel, in PhysicsMass mass) 
                    => vel.ApplyLinearImpulse(in mass, math.up() * 0.5f)).Run();
                m_JumpPressed = false;
            }

            if (m_Label != null)
            {
                var str = new FixedString512Bytes();
                var antString = m_AntString;
                var colorTagStart = m_ColorTagStart;
                var colors = m_EntityToColorSystem.EntityToColor;
                Entities.ForEach((Entity e, in AntHillData hill) =>
                {
                    str.Append(colorTagStart);
                    str.Append(colors[e]);
                    str.Append('>');
                    str.Append(antString);
                    str.Append(' ');
                    str.Append('[');
                    str.Append(hill.Current);
                    str.Append('/');
                    str.Append(hill.Total);
                    str.Append(']');
                    str.Append('\n');
                }).Run();

                var resourceTypeToString = m_ResourceTypeToString;
                Entities.ForEach((Entity e, in ResourceStore resourceStore) =>
                {
                    str.Append(colorTagStart);
                    str.Append(colors[e]);
                    str.Append('>');
                    str.Append(resourceTypeToString[(int)resourceStore.Type]);
                    str.Append(' ');
                    str.Append('[');
                    str.Append(resourceStore.Current);
                    str.Append('/');
                    str.Append(resourceStore.Total);
                    str.Append(']');
                    str.Append('\n');
                }).Run();
                m_Label.text = str.ToString();
            }
#if !UNITY_EDITOR
        if (Keyboard.current.escapeKey.isPressed) 
            Application.Quit();
#endif
        }
    }
}