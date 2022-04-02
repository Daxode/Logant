using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Systems.GameObjects;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Rendering;
using Unity.Transforms;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Material = UnityEngine.Material;

namespace Systems
{
    public partial class GameManager : SystemBase
    {
        bool m_ButtonClicked;
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
        
        Material m_Material;
        RenderTexture m_Texture;
        float2 m_CameraResolution;
        protected override void OnStartRunning()
        {
            var docEntity = GetSingletonEntity<UIDocument>();
            var doc = EntityManager.GetComponentObject<UIDocument>(docEntity);

            // Setup UI
            var button = doc.rootVisualElement.Q<Button>("Jump");
            if (button!=null)
            {
                button.text = "<i>Jump</i> with <b>this</b>";
                button.clicked += () => m_ButtonClicked = true;
                button.AddToClassList("jump");
            }
            m_Label = doc.rootVisualElement.Q<Label>();

            // Setup Drawing
            var cam = Camera.main;
            m_CameraResolution = new int2(cam.pixelWidth, cam.pixelHeight);
            m_Texture = new RenderTexture((int) m_CameraResolution.x, (int) m_CameraResolution.y,0);
            m_Material = new Material(Shader.Find("Unlit/Vector"));
            var overlayStyle = doc.rootVisualElement.Q<VisualElement>("VectorOverlay").style;
            overlayStyle.backgroundImage = new StyleBackground {value = new Background {renderTexture = m_Texture}};
            
            // Create Scene
            m_PathProp = new PathProperties {Stroke = new Stroke {Color = Color.red, HalfThickness = 5f}};
        }
        

        PathProperties m_PathProp;
        protected override void OnUpdate()
        {
            // Create sprite and draw it.
            var circle = new Shape
            {
                PathProps = m_PathProp
            };
            VectorUtils.MakeCircleShape(circle, m_CameraResolution*0.5f, (float)(((math.sin(Time.ElapsedTime)*0.5+0.5)*0.9+0.1)*500));
            var scene = new Scene{Root = new SceneNode{Shapes = new List<Shape> {circle}}};
            var geo = VectorUtils.TessellateScene(scene, new VectorUtils.TessellationOptions
            {
                SamplingStepSize = 50f,
                StepDistance = 10f,
            });
            var sprite = VectorUtils.BuildSprite(geo, new Rect(0,0,m_CameraResolution.x,m_CameraResolution.y),1f, VectorUtils.Alignment.Center, Vector2.zero, 64);
            var oldRT = RenderTexture.active;
            RenderTexture.active = m_Texture;
            VectorUtils.RenderSprite(sprite, m_Material);
            RenderTexture.active = oldRT;
            
            if (m_ButtonClicked)
            {
                Entities.ForEach((ref PhysicsVelocity vel, in PhysicsMass mass) 
                    => vel.ApplyLinearImpulse(in mass, math.up() * 0.5f)).Run();
                m_ButtonClicked = false;
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