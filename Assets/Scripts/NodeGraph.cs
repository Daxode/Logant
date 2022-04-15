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
        const float k_ArrowRadius = .3f;
        
        float3? m_ActiveNodeLocation;
        // float? m_ActiveNodeRotation;
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
            var toDest = destination - m_ActiveNodeLocation.Value;
            var dirToDest = math.normalizesafe(toDest);
            var destLengthMinusArrow = math.max(k_NodeRadius+0.01f,math.length(toDest));
            var pTo = m_ActiveNodeLocation.Value + dirToDest * destLengthMinusArrow;
            var pFrom = m_ActiveNodeLocation.Value + dirToDest * k_NodeRadius;

            // If valid path
            m_ActivePathValid = math.any(pFrom != pTo);
            if (!m_ActivePathValid) return;
            
            // Construct path
            m_ActivePath.ClearAllPoints();
            m_ActivePath.AddPoint(pFrom, 1);
            m_ActivePath.LineTo(pTo,100);
            ThinAtArrowEndPoint(pTo);
        }

        void ThinAtArrowEndPoint(float3 ArrowEndPoint)
        {
            for (var i = m_ActivePath.Count - 1; i >= 0; i--)
            {
                var dist = math.distance(m_ActivePath[i].point, ArrowEndPoint);
                m_ActivePath.SetThickness(i, math.clamp(dist * 10, .05f, 1));
                if (dist > k_ArrowRadius)
                    break;
            }
        }

        protected override void DrawShapes(Camera cam)
        {
            using (Draw.Command(cam))
            {
                // Setttings
                Draw.ZTest = CompareFunction.Always;
                Draw.BlendMode = ShapesBlendMode.Transparent;
                Draw.DiscGeometry = DiscGeometry.Flat2D;
                Draw.Thickness = 0.1f;

                // Default constants
                var rotateUp = Quaternion.Euler(90, 0, 0);
                
                // Draw Active Arrow Path
                if (m_ActivePathValid)
                {
                    SetLinearGradientHSV(m_ActivePath, new Color(0f, 1f, 0.8f), new Color(0f, 0.5f, 1f));

                    Draw.Polyline(m_ActivePath, PolylineJoins.Round);
                    DrawArrowHead(
                        m_ActivePath[m_ActivePath.Count-2].point,
                        m_ActivePath[m_ActivePath.Count-1].point,
                        .15f, k_ArrowRadius, m_ActivePath[m_ActivePath.Count-1].color);
                }

                using (Draw.ColorScope)
                {
                    Draw.Color = new Color(.1f, .1f, .15f, 0.5f);
                    Entities.WithAll<ExecutionLineDataHolder>().ForEach((in Translation t) 
                        => Draw.Ring(t.Value, rotateUp, 0.7f)).WithoutBurst().Run();
                }
            }
        }

        static void DrawArrowHead(float3 startP, float3 endP, float radius, float length, Color col)
        {
            var dirWithSize = endP - startP;
            var dir = math.normalizesafe(dirWithSize);
            var rotationWithDir = Quaternion.LookRotation(dir);
            var arrowLocation = startP + dir * (math.length(dirWithSize)-length);
            Draw.Cone(arrowLocation, rotationWithDir, radius, length, col);
        }
        
        static float3 RGBToHSV(Color color)
        {
            var blueHSV = float3.zero;
            Color.RGBToHSV(color, out blueHSV.x, out blueHSV.y, out blueHSV.z);
            return blueHSV;
        }

        static void SetLinearGradientHSV(PolylinePath p, Color startColor, Color endColor) => SetLinearGradientHSV(p, RGBToHSV(startColor), RGBToHSV(endColor));
        static void SetLinearGradientHSV(PolylinePath p, float3 startColorHSV, float3 endColorHSV)
        {
            for (var i = 0; i < p.Count; i++)
            {
                var color = math.lerp(startColorHSV, endColorHSV, i / (float) (p.Count - 1));
                p.SetColor(i, Color.HSVToRGB(color.x, color.y, color.z));
            }
        }
    }
}
