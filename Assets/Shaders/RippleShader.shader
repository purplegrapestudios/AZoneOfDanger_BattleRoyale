Shader "Custom/RippleEffect" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
        _RipplePos("Ripple Position", Vector) = (0,0,0,0)
        _RippleStrength("Ripple Strength", Range(0.0, 1.0)) = 1.0
        _RippleSpeed("Ripple Speed", Range(0.0, 10.0)) = 1.0
    }

        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                float4 _RipplePos;
                float _RippleStrength;
                float _RippleSpeed;

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                float4 frag(v2f i) : SV_Target {
                    float2 uv = i.uv;

                    float2 delta = uv - _RipplePos.xy;
                    float dist = length(delta);

                    float wave = _RippleStrength * sin((dist * 50.0 - _Time.y * _RippleSpeed) * 6.28);

                    uv += delta * wave;

                    return tex2D(_MainTex, uv);
                }
                ENDCG
            }
        }
}