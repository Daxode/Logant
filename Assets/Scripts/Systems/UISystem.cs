using Data;
using Systems.GameObjects;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(EntityToColorSystem))]
    public partial class UISystem : SystemBase
    {
        RenderTexture m_Texture;
        Material m_DrawNodesMaterial;
        Camera m_Cam;
        Mesh m_NodeData;
        NativeList<float3> m_NodesVerts;
        NativeList<int> m_NodesIndices;
        protected override void OnCreate()
        {
            m_DrawNodesMaterial = new Material(Shader.Find("Unlit/DrawNodes"));
            m_Cam = Camera.main;
            m_Texture = new RenderTexture(m_Cam.pixelWidth, m_Cam.pixelHeight, 0);
            m_NodeData = new Mesh();
            m_NodesVerts = new NativeList<float3>(1024, Allocator.Persistent);
            m_NodesIndices = new NativeList<int>(1024, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            m_NodesVerts.Dispose();
            m_NodesIndices.Dispose();
        }

        static readonly int k_ColorR = Shader.PropertyToID("_ColorR");
        static readonly int k_ColorG = Shader.PropertyToID("_ColorG");
        static readonly int k_ColorB = Shader.PropertyToID("_ColorB");
        static readonly int k_MainTex = Shader.PropertyToID("_MainTex");
        protected override void OnStartRunning()
        {
            var uiData = this.GetSingleton<UIData>();
            m_DrawNodesMaterial.SetColor(k_ColorR, uiData.ColorR);
            m_DrawNodesMaterial.SetColor(k_ColorG, uiData.ColorG);
            m_DrawNodesMaterial.SetColor(k_ColorB, uiData.ColorB);
            m_DrawNodesMaterial.SetTexture(k_MainTex, uiData.texture);
            
            var doc = EntityManager.GetComponentObject<UIDocument>(GetSingletonEntity<UIDocument>());
            var overlayStyle = doc.rootVisualElement.Q<VisualElement>("VectorOverlay").style;
            overlayStyle.backgroundImage = new StyleBackground {value = new Background {renderTexture = m_Texture}};
            
            Entities.WithAll<ExecutionLineDataHolder>().ForEach((Translation t) =>
            {
                var screenPoint = m_Cam.WorldToScreenPoint(t.Value);
                AddNode(new float2(screenPoint.x,screenPoint.y),100);
            }).WithoutBurst().Run();
            
            UpdateNodeData();
        }

        public void UpdateNodeData()
        {
            m_NodeData.SetVertices(m_NodesVerts.AsArray());
            m_NodeData.SetIndices(m_NodesIndices.AsArray(), MeshTopology.Triangles,0);
        }

        public void AddNode(float2 center, float radius)
        {
            var index = m_NodesVerts.Length;
            m_NodesIndices.Add(0+index);
            m_NodesIndices.Add(1+index);
            m_NodesIndices.Add(2+index);
            m_NodesIndices.Add(2+index);
            m_NodesIndices.Add(1+index);
            m_NodesIndices.Add(3+index);

            var data = new float3(center, radius);
            for (var i = 0; i<4; i++)
                m_NodesVerts.Add(data);
        }
        
        protected override void OnUpdate()
        {
            var oldRT = RenderTexture.active;
            RenderTexture.active = m_Texture;
            m_DrawNodesMaterial.SetPass(0);
            //GL.Clear(false, true, Color.yellow);
            Graphics.DrawMeshNow(m_NodeData, Matrix4x4.zero);
            RenderTexture.active = oldRT;
            
            
            var uiData = this.GetSingleton<UIData>();
            m_DrawNodesMaterial.SetColor(k_ColorR, uiData.ColorR);
            m_DrawNodesMaterial.SetColor(k_ColorG, uiData.ColorG);
            m_DrawNodesMaterial.SetColor(k_ColorB, uiData.ColorB);
            m_DrawNodesMaterial.SetTexture(k_MainTex, uiData.texture);
        }
    }
}
