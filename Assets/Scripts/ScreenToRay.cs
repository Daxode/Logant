using Systems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public partial class ScreenToRaySystem : SystemBase
{
    static DistortionSetting? m_DistortionSetting;

    protected override void OnCreate()
    {
        var lensDistort = Object.FindObjectOfType<LensDistortion>();
        if (lensDistort != null && lensDistort.IsActive()) 
            m_DistortionSetting = lensDistort;
    }

    public static Ray ScreenToRay(float2 screen)
    {
        float3 viewport = Camera.main.ScreenToViewportPoint(new float3(screen,0));
        var uv = new float3(DistortUV(viewport.xy), 1);
        return Camera.main.ViewportPointToRay(uv);
    }
    
    protected override void OnUpdate() {}

    struct DistortionSetting
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

    static float2 DistortUV(float2 uv)
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
