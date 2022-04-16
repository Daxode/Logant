using System;
using System.Linq;
using Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Systems
{
    public partial class UIDisplaySystem : SystemBase
    {
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
        
        bool m_JumpPressed;
        Label m_Label;
        protected override void OnStartRunning()
        {
            var doc = EntityManager.GetComponentObject<UIDocument>(GetSingletonEntity<UIDocument>());

            // Setup UI
            var jumpBtn = doc.rootVisualElement.Q<Button>("Jump");
            if (jumpBtn!=null)
            {
                jumpBtn.text = "<i>Jump</i> with <b>this</b>";
                jumpBtn.clicked += OnJumpBtnOnClicked;
                jumpBtn.AddToClassList("jump");
            }
            
            var startBtn = doc.rootVisualElement.Q<Button>("Start");
            if (startBtn!=null)
            {
                startBtn.clicked += OnStartBtnOnClicked;
                startBtn.AddToClassList("Start");
            }
            
            m_Label = doc.rootVisualElement.Q<Label>();
        }

        protected override void OnStopRunning()
        {
            var doc = EntityManager.GetComponentObject<UIDocument>(GetSingletonEntity<UIDocument>());
            
            var jumpBtn = doc.rootVisualElement.Q<Button>("Jump");
            if (jumpBtn!=null) 
                jumpBtn.clicked -= OnJumpBtnOnClicked;

            var startBtn = doc.rootVisualElement.Q<Button>("Start");
            if (startBtn!=null) 
                startBtn.clicked -= OnStartBtnOnClicked;
        }

        void OnJumpBtnOnClicked()
        {
            Entities.ForEach((ref PhysicsVelocity vel, in PhysicsMass mass) 
                => vel.ApplyLinearImpulse(in mass, math.up() * 0.5f)).Run();
        }

        void OnStartBtnOnClicked()
        {
            if (TryGetSingleton<GlobalData>(out var gd))
            {
                gd.HasStarted = true;
                SetSingleton(gd);
            }
        }

        protected override void OnUpdate()
        {
            if (m_EntityToColorSystem.EntityToColor.IsCreated)
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