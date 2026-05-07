Shader "Custom/Cubemap"
{
    Properties
    {
        _MainTex ("Cubemap", Cube) = "white" {}
        _RotationSpeed ("Rotation Speed", Float) = 0.1
        _Tint ("Tint", Color) = (1,1,1,1)
        _Exposure ("Exposure", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            samplerCUBE _MainTex;
            float _RotationSpeed;
            float4 _Tint;
            float _Exposure;

            struct appdata
            {
                float4 vertex : POSITION;
                // float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                // float2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.vertex.xyz;
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float angle = _Time.y * _RotationSpeed;

                float s = sin(angle);
                float c = cos(angle);

                float3 dir = normalize(i.texcoord);

                float3 rotatedDir;
                rotatedDir.x = dir.x * c - dir.z * s;
                rotatedDir.y = dir.y;
                rotatedDir.z = dir.x * s + dir.z * c;
                
                // sample the texture
                fixed4 col = texCUBE(_MainTex, rotatedDir);
                col.rgb *= _Tint.rgb * _Exposure;

                return col;
            }
            ENDCG
        }
    }
}
