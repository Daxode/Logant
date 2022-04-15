using Data;
using Shapes;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.Build.Pipeline.WriteTypes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Systems
{
    public partial class UISystem : SystemBase
    {
        NodeGraph m_Graph;
        protected override void OnCreate() => m_Graph = World.GetExistingSystem<NodeGraph>();

        protected override void OnUpdate()
        {
            var ray = ScreenToRaySystem.ScreenToRay(Mouse.current.position.ReadValue());
            float3 point = ray.GetPoint((-ray.origin.y) / ray.direction.y);
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                Debug.Log("Pressed");
                m_Graph.ActiveNodeSet(point);
            } else if (Mouse.current.leftButton.wasReleasedThisFrame) {
                Debug.Log("Released");
                m_Graph.ActiveNodeDeactivate();
            }

            m_Graph.DrawToPoint(point);
        }
    }
    
    [WorldSystemFilter(WorldSystemFilterFlags.Default|WorldSystemFilterFlags.Editor)]
    public partial class NodeGraph : SystemBaseDraw
    {
        PolylinePath m_ActivePath;
        bool m_ActivePathValid;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_ActivePath = new PolylinePath();
        }

        const float k_NodeRadius = .7f;
        const float arrowRadius = .25f;
        
        float3? m_ActiveNodeLocation;
        public void ActiveNodeSet(float3 nodeLocation) => m_ActiveNodeLocation = nodeLocation;
        public void ActiveNodeDeactivate()
        {
            m_ActivePathValid = false;
            m_ActiveNodeLocation = null;
        }

        public void DrawToPoint(float3 destination)
        {
            // Create Path
            if (!m_ActiveNodeLocation.HasValue) return;
            var activeNodeToDestination = destination - m_ActiveNodeLocation.Value;
            var dirNodeToDest = math.normalizesafe(activeNodeToDestination);
            var endLengthMinusArrow = math.max(k_NodeRadius+0.01f,math.length(activeNodeToDestination) - arrowRadius);
            var pTo = m_ActiveNodeLocation.Value + dirNodeToDest * endLengthMinusArrow;
            var pFrom = m_ActiveNodeLocation.Value + dirNodeToDest * k_NodeRadius;

            // If valid path
            m_ActivePathValid = math.any(pFrom != pTo);
            if (!m_ActivePathValid) return;
            
            // Construct path
            m_ActivePath.ClearAllPoints();
            m_ActivePath.AddPoint(pFrom);
            m_ActivePath.AddPoint(pTo);
        }

        protected override void DrawShapes(Camera cam)
        {
            using (Draw.Command(cam))
            {
                // Setttings
                Draw.ZTest = CompareFunction.Always;
                Draw.BlendMode = ShapesBlendMode.Transparent;
                Draw.Thickness = 0.1f;

                // Default constants
                var rotateUp = Quaternion.Euler(90, 0, 0);
                var blueHSV = RGBToHSV(0f, 1f, 0.8f);
                var purpleHSV = RGBToHSV(0f, 0.55f, 0.85f);

                if (m_ActivePathValid)
                {
                    SetLinearGradient(m_ActivePath, blueHSV, purpleHSV);
                    Draw.Polyline(m_ActivePath);

                    // Arrow
                    DrawArrowHead(
                        m_ActivePath[m_ActivePath.Count-2].point,
                        m_ActivePath[m_ActivePath.Count-1].point,
                        arrowRadius, rotateUp, .6f, arrowRadius,m_ActivePath[m_ActivePath.Count-1].color);
                }

                using (Draw.ColorScope)
                {
                    Draw.Color = new Color(.1f, .1f, .15f, 0.5f);
                    Entities.WithAll<ExecutionLineDataHolder>().ForEach((in Translation t) 
                        => Draw.Ring(t.Value, rotateUp, 0.7f)).WithoutBurst().Run();
                }
            }
        }

        static void DrawArrowHead(float3 startP, float3 endP, float overshoot, Quaternion rotation, float width, float radius, DiscColors col)
        {
            var dirWithSize = endP - startP;
            var dir = math.normalizesafe(dirWithSize);
            var rotationWithDir = Quaternion.LookRotation(dir)*rotation;
            Draw.Pie(startP + dir * (math.length(dirWithSize) + overshoot), rotationWithDir, radius, -math.PI / 2f - width, -math.PI / 2f + width, col);
        }
        
        static float3 RGBToHSV(float r, float g, float b) => RGBToHSV(new Color(r, g, b));
        static float3 RGBToHSV(Color color)
        {
            var blueHSV = float3.zero;
            Color.RGBToHSV(color, out blueHSV.x, out blueHSV.y, out blueHSV.z);
            return blueHSV;
        }

        static void SetLinearGradient(PolylinePath p, float3 startColorHSV, float3 endColorHSV)
        {
            for (int i = 0; i < p.Count; i++)
            {
                var color = math.lerp(startColorHSV, endColorHSV, i / (float) (p.Count - 1));
                p.SetColor(i, Color.HSVToRGB(color.x, color.y, color.z));
                // Draw.Sphere(p[i].point, 0.05f, p[i].color);
            }
        }
    }
}
