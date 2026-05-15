Shader "Custom/DarkBarrier"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (0.05,0.06,0.1,1)
        _EdgeColor ("Edge Color", Color) = (0.2,0.4,1,1)

        _Opacity ("Opacity", Range(0,1)) = 0.85

        _FresnelPower ("Fresnel Power", Range(0.1,10)) = 3
        _FresnelStrength ("Fresnel Strength", Range(0,5)) = 1.5

        _NoiseScale ("Noise Scale", Float) = 2
        _NoiseSpeed ("Noise Speed", Float) = 0.1

        _TopFade ("Top Fade", Float) = 15
        _BottomFade ("Bottom Fade", Float) = -5

        _DepthFade ("Depth Fade", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "ForwardPass"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };

            float4 _MainColor;
            float4 _EdgeColor;

            float _Opacity;

            float _FresnelPower;
            float _FresnelStrength;

            float _NoiseScale;
            float _NoiseSpeed;

            float _TopFade;
            float _BottomFade;

            float _DepthFade;

            // --------------------------
            // Simple noise
            // --------------------------

            float hash(float2 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * (p.x + p.y));
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1,0));
                float c = hash(i + float2(0,1));
                float d = hash(i + float2(1,1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) +
                       (c - a) * u.y * (1.0 - u.x) +
                       (d - b) * u.x * u.y;
            }

            // --------------------------
            // Vertex
            // --------------------------

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs =
                    GetVertexPositionInputs(IN.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.worldPos = posInputs.positionWS;
                OUT.worldNormal = normalInputs.normalWS;
                OUT.screenPos = ComputeScreenPos(posInputs.positionCS);
                OUT.uv = IN.uv;

                return OUT;
            }

            // --------------------------
            // Fragment
            // --------------------------

            half4 frag(Varyings IN) : SV_Target
            {
                // View direction
                float3 viewDir =
                    normalize(_WorldSpaceCameraPos - IN.worldPos);

                // Fresnel
                float fresnel =
                    pow(
                        1.0 - saturate(dot(viewDir, IN.worldNormal)),
                        _FresnelPower
                    );

                fresnel *= _FresnelStrength;

                // Animated noise
                float2 noiseUV =
                    IN.worldPos.xz * _NoiseScale;

                noiseUV += _Time.y * _NoiseSpeed;

                float n = noise(noiseUV);

                // Height fade
                float heightFade =
                    saturate(
                        (IN.worldPos.y - _BottomFade) /
                        (_TopFade - _BottomFade)
                    );

                heightFade = 1.0 - heightFade;

                // Depth fade
                float2 screenUV =
                    IN.screenPos.xy / IN.screenPos.w;

                float sceneDepth =
                    SampleSceneDepth(screenUV.xy);

                float sceneLinearDepth =
                    LinearEyeDepth(
                        sceneDepth,
                        _ZBufferParams
                    );

                float objectDepth =
                    LinearEyeDepth(
                        IN.screenPos.z / IN.screenPos.w,
                        _ZBufferParams
                    );

                float depthDifference =
                    abs(sceneLinearDepth - objectDepth);

                float depthFade =
                    saturate(depthDifference / _DepthFade);

                // Final alpha
                float alpha =
                    (_Opacity *
                    heightFade *
                    depthFade);

                alpha += fresnel * 0.5;
                alpha += n * 0.08;

                alpha = saturate(alpha);

                // Final color
                float3 color =
                    lerp(
                        _MainColor.rgb,
                        _EdgeColor.rgb,
                        fresnel
                    );

                return half4(color, alpha);
            }

            ENDHLSL
        }
    }
}