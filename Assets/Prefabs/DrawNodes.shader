Shader "Unlit/DrawNodes"
{
    Properties
    {
        _MainTex ("Atlas", 2D) = "white" {}
        _ColorR ("Color R", color) = (1,0,0)
        _ColorG ("Color G", color) = (0,1,0)
        _ColorB ("Color B", color) = (0,0,1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Overlay" "Queue"="Overlay"}
        Pass
        {
            Blend SrcAlpha Zero
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct v2_f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // static const float2 v[6] =
            // {
            //     float2(-1, 1), float2( 1, 1), float2( 1,-1), // 0,1,2
            //     float2( 1,-1), float2(-1,-1), float2(-1, 1),  // 2,3,0
            // };
            static const float2 v[4] =
            {
                float2(-1, 1), float2( 1, 1),
                float2(-1,-1), float2( 1,-1),
            };
            
            // XY: pos, Z: Radius, W: Id
            v2_f vert (float4 vertex : POSITION, uint index: SV_VertexID)
            {
                v2_f o;
                o.uv = v[index % 4];
                const fixed2 screen_pos = vertex.xy + o.uv * vertex.z;
                o.vertex = float4((screen_pos/_ScreenParams.xy)*2-1,0,1);
                o.vertex.y *= -1;
                
                o.uv = o.uv*0.5f+0.5f;
                const uint id = vertex.w;
                o.uv += uint2(id%3u,id/3u);
                o.uv *= 0.33f;
                
                return o;
            }

            sampler2D _MainTex;
            float4 _ColorR;
            float4 _ColorG; 
            float4 _ColorB;

            fixed3 hue(const in float hue)
            {
                const fixed3 rgb = abs(hue * 6. - fixed3(3, 2, 4)) * fixed3(1, -1, -1) + fixed3(-1, 2, 2);
                return clamp(rgb, 0., 1.);
            }
            
            fixed4 frag (const v2_f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 r = col.r*_ColorR + col.g*_ColorG + col.b*_ColorB;
                return fixed4(r.rgb, col.a);
            }
            ENDCG
        }
    }
}
