Shader "Custom/SkyboxShader"
{
    Properties
    {
        _ColorA ("ColorA", Color) = (1,1,1,0.5)
        _ColorB ("ColorB", Color) = (1,1,1,0.5)
        _ColorC ("ColorC", Color) = (1,1,1,0.5)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ColorA;
            float4 _ColorB;
            float4 _ColorC;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 hsv2rgb(float4 col)
            {
                float3 rgb = clamp(
                    abs(frac(col.x + float4(0, 2.0/3.0, 1.0/3.0, 1.0)) * 6.0 - 3.0) - 1.0, 
                    0.0, 
                    1.0
                );
                
                float3 finalRGB = col.z * lerp(float3(1.0, 1.0, 1.0), rgb, col.y);

                return float4(finalRGB, col.w);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // uv.y += frac(uv.y + _Time.y);
                
                // sample the texture
                // fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                float t = frac(_Time.y * 0.1 + uv.y);
                return hsv2rgb(float4(t, 1.0, 1.0, 0.25));
            }
            ENDCG
        }
    }
}
