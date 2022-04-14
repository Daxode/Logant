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
            var lensDistort = Object.FindObjectOfType<LensDistortion>();
            if (lensDistort != null && lensDistort.IsActive()) 
                m_DistortionSetting = lensDistort;
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
            pTo = m_ActiveNodeLocation.Value + dir * math.max(k_NodeRadius+0.01f,math.length(pTo - m_ActiveNodeLocation.Value));
            m_ActivePathValid = !math.all(pFrom == pTo);
            m_ActivePath.SetPoint(0, pFrom);
            m_ActivePath.SetPoint(1, pTo);
        }

        protected override void OnUpdate()
        {
            float3 viewport = Camera.main.ScreenToViewportPoint(Mouse.current.position.ReadValue());
            var uv = new float3(DistortUV(viewport.xy), 1);
            var ray = Camera.main.ViewportPointToRay(uv);
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
                    Draw.Color = new Color(0.93f, 1f, 0.96f, 0.69f);
                    Draw.Polyline(m_ActivePath);
                    for (int i = 0; i < m_ActivePath.Count; i++)
                        Draw.Disc(m_ActivePath[i].point,rotateUp,.2f);
                    Draw.Pop();
                }

                Draw.Thickness = 0.1f;
                Entities.WithAll<ExecutionLineDataHolder>().ForEach((in Translation t) 
                    => Draw.Ring(t.Value, rotateUp, 0.7f)).WithoutBurst().Run();
            }
        }

        DistortionSetting? m_DistortionSetting;

        public struct DistortionSetting
        {
            public float4 centerScale;
            public float4 amount;
            public DistortionSetting(float4 centerScale, float4 amount)
            {
                this.centerScale = centerScale;
                this.amount = amount;
            }

            public static implicit operator DistortionSetting(LensDistortion d)
            {
                var amount = 1.6f * math.max(math.abs(d.intensity.value * 100f), 1f);
                var theta = math.radians(math.min(160f, amount));
                var sigma = 2f * math.tan(theta * 0.5f);
                var center = d.center.value * 2f - Vector2.one;
                return new DistortionSetting(
                    new float4(
                        center.x, center.y,
                        math.max(d.xMultiplier.value, 1e-4f), 
                        math.max(d.yMultiplier.value, 1e-4f)
                        ), 
                    new float4(
                        d.intensity.value >= 0f ? theta : 1f / theta, 
                        sigma, 1f / d.scale.value, 
                        d.intensity.value * 100f
                        )
                    );
            }
        }

        float2 DistortUV(float2 uv)
        {
            if (!m_DistortionSetting.HasValue) return uv;
            var distortionSetting = m_DistortionSetting.Value;
            
            // Actual distortion code
            uv = (uv - 0.5f) * distortionSetting.amount.z + 0.5f;
            var ruv = distortionSetting.centerScale.zw * (uv - 0.5f - distortionSetting.centerScale.xy);
            var ru = math.length(ruv);

            if (distortionSetting.amount.w > 0.0f)
            {
                float wu = ru * distortionSetting.amount.x;
                ru = math.tan(wu) * (1.0f / (ru * distortionSetting.amount.y));
                uv += ruv * (ru - 1.0f);
            } else {
                ru = (1.0f / ru) * distortionSetting.amount.x * math.atan(ru * distortionSetting.amount.y);
                uv += ruv * (ru - 1.0f);
            }
            
            return uv;
        }
    }
}
