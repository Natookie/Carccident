Shader "Unlit/CameraOverlay"
{
    Properties
    {
        _Tint ("Tint", Color) = (0.23,0.53,0.23,1)

        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.015
        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.025
        _WaveStrength ("Wave Strength", Range(0,0.01)) = 0.0015

        _OverlayAlpha ("Overlay Alpha", Range(0,1)) = 0.08
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Tint;

            float _NoiseStrength;
            float _ScanlineStrength;
            float _WaveStrength;
            float _OverlayAlpha;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                uv.x += sin(uv.y * 6 + _Time.y * 1.2) * _WaveStrength;

                float scan =
                    sin(uv.y * _ScreenParams.y * 0.35) * 0.5 + 0.5;

                scan = lerp(1.0, scan, _ScanlineStrength);

                float noise =
                    random(floor(uv * 512) + floor(_Time.y * 5));

                noise *= _NoiseStrength;

                float3 col = _Tint.rgb;

                col *= scan;
                col += noise;

                return fixed4(col, _OverlayAlpha);
            }

            ENDCG
        }
    }
}