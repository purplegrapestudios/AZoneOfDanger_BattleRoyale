Shader "Custom/WaterWithFoam" 
{
    Properties{
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _WaveSpeed("Wave Speed", Vector) = (1, 1, 1)
        _WaveHeight("Wave Height", Range(0, 30)) = 5
        _WaterLevel("Water Level", Range(0, 2000)) = 5
        _ShallowColor("Shallow Color", Color) = (0, 0.5, 1, 1)
        _DeepColor("Banding Color", Color) = (0, 0, 1, 1)
    }
        SubShader{
            Cull off
            Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 100

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                };

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float3 worldPos : TEXCOORD1;
                    float3 worldNormal : TEXCOORD2;
                    float4 clipPos : SV_POSITION;
                };

                float4 _FoamColor;
                float3 _WaveSpeed;
                float _WaveHeight;
                float _WaterLevel;
                float4 _ShallowColor;
                float4 _DeepColor;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.clipPos = UnityObjectToClipPos(v.vertex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.worldNormal = mul(unity_ObjectToWorld, v.normal).xyz;
                    o.uv = v.uv;

                    float wavePattern = sin(o.worldPos.x * _WaveSpeed.x + o.worldPos.z * _WaveSpeed.y + _Time.x * _WaveSpeed.z) * _WaveHeight;
                    o.clipPos.y += wavePattern;

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    // Calculate the foam effect
                    float foam = pow(dot(normalize(i.worldNormal), normalize(UnityWorldSpaceViewDir(i.clipPos))), 3.0);
                    foam = smoothstep(0.5, 1.0, foam);
                    
                    // Calculate the color of the water surface
                    float3 waterColor = lerp(_ShallowColor, _DeepColor, i.clipPos.y / _WaterLevel);
                    waterColor += foam * _FoamColor;

                    // Animate the water surface using time
                    //float wavePattern = sin(i.worldPos.x * _WaveSpeed.x + i.worldPos.z * _WaveSpeed.y + _Time.x * _WaveSpeed.z) * _WaveHeight;
                    //i.worldPos.y += wavePattern + 2000;

                    // Output the final color of the fragment
                    return fixed4(waterColor, 0.5);
                }
                ENDCG
            }
        }
            FallBack "Diffuse"
}