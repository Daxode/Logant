using Data;
using Shapes;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.Default|WorldSystemFilterFlags.Editor)]
    public partial class NodeGraph : SystemBaseDraw
    {

        PolylinePath m_ActivePath;
        bool m_ActivePathValid = false;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_ActivePath = new PolylinePath();
            m_ActivePath.AddPoints(new Vector3(0,2), new Vector3(1,2));
        }

        const float k_NodeRadius = .7f;
        
        float3? m_ActiveNodeLocation;
        public void ActiveNodeSet(float3 nodeLocation) => m_ActiveNodeLocation = nodeLocation;
        public void ActiveNodeDeactivate() => m_ActiveNodeLocation = null;
        
        public void DrawToPoint(float3 pTo)
        {
            if (!m_ActiveNodeLocation.HasValue) return;
            var dir = math.normalizesafe(pTo - m_ActiveNodeLocation.Value);
            var pFrom = m_ActiveNodeLocation.Value + dir * k_NodeRadius;
            pTo = m_ActiveNodeLocation.Value + dir * math.max(k_NodeRadius+0.01f,math.length(pTo - m_ActiveNodeLocation.Value)-.1f);
            m_ActivePathValid = math.any(pFrom != pTo);
            m_ActivePath.SetPoint(0, pFrom);
            m_ActivePath.SetPoint(1, pTo);
        }

        protected override void OnUpdate()
        {
            var ray = ScreenToRaySystem.ScreenToRay(Mouse.current.position.ReadValue());
            float3 point = ray.GetPoint((-ray.origin.y) / ray.direction.y);
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                Debug.Log("Pressed");
                ActiveNodeSet(point);
            } else if (Mouse.current.leftButton.wasReleasedThisFrame) {
                Debug.Log("Released");
                ActiveNodeDeactivate();
            }

            DrawToPoint(point);
        }

        protected override void DrawShapes(Camera cam)
        {
            using (Draw.Command(cam))
            {
                // Setttings
                Draw.BlendMode = ShapesBlendMode.Transparent;
                // Draw.PolylineGeometry = PolylineGeometry.Flat2D;
                Draw.DiscGeometry = DiscGeometry.Flat2D;
                Draw.Color = new Color(.1f, .1f, .15f, 0.5f);
                
                // Default constants
                var rotateUp = Quaternion.Euler(90, 0, 0);
                
                // Debug Bezier
                using (var p = new PolylinePath())
                {
                    p.AddPoint(0,0);
                    p.BezierTo(Vector3.up*0.5f, Vector3.down*0.5f+new Vector3(1,2), new Vector3(1,2),10);
                    Draw.Polyline(p, PolylineJoins.Round);
                }
                
                if (m_ActiveNodeLocation.HasValue&&m_ActivePathValid)
                {   
                    Draw.Push();
                    // Draw.BlendMode = ShapesBlendMode.Opaque;
                    // Draw.Color = new Color(0.2f, 0.2f, 0.3f, 1f);
                    Draw.Polyline(m_ActivePath);
                    
                    // Arrow
                    var dirWithSize = m_ActivePath[1].point - m_ActivePath[0].point;
                    var dir = math.normalizesafe(dirWithSize);
                    var r = Quaternion.LookRotation(dir)*rotateUp;
                    Draw.Pie((float3)m_ActivePath[0].point+dir*(math.length(dirWithSize)+.1f), r, .2f, -math.PI/2f-.5f,-math.PI/2f+.5f);
                    
                    Draw.Pop();
                }

                Draw.Thickness = 0.1f;
                Entities.WithAll<ExecutionLineDataHolder>().ForEach((in Translation t) 
                    => Draw.Ring(t.Value, rotateUp, 0.7f)).WithoutBurst().Run();
            }
        }
    }
}
