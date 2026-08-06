Shader "Custom/LiquidGlobe_Standalone"
{
    Properties
    {
        _LightColor ("Light Color", Color) = (1, 0, 0, 1)
        _DarkColor ("Dark Color", Color) = (0.2, 0, 0, 1)
        _Fill ("Fill Percentage", Range(0, 1)) = 1
        _CurrentTilt ("Current Tilt", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry-100" 
            "RenderPipeline"="UniversalPipeline" 
        }

        // --- PASS 1: STANDARD RENDERING ---
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionOS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float3 viewDirWS    : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _LightColor;
                float4 _DarkColor;
                float4 _CurrentTilt;
                float _Fill;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionOS = input.positionOS.xyz;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);

                return output;
            }

            float4 frag(Varyings input, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                // 2D Wave Motion across X and Z
                float waveX = sin(input.positionOS.x * 7.0 + _Time.y * 2.5) * 0.012;
                float waveZ = cos(input.positionOS.z * 7.0 + _Time.y * 2.0) * 0.012;
                float wave = waveX + waveZ;

                float height = (input.positionOS.y - (-0.5)) / (0.5 - (-0.5));
                float tiltedHeight = height + dot(input.positionOS.xyz, _CurrentTilt.xyz) + wave;

                float fillLine = tiltedHeight - _Fill;
                if (fillLine > 0.0)
                {
                    discard;
                }

                // Dynamic cap normal based on tilt and wave slope
                float3 tiltNormal = normalize(float3(-_CurrentTilt.x - waveX * 2.0, 1.0, -_CurrentTilt.z - waveZ * 2.0));
                float3 normalWS = isFrontFace ? normalize(input.normalWS) : tiltNormal;
                float3 viewDir = normalize(input.viewDirWS);

                if (isFrontFace)
                {
                    float edge = saturate(1.0 - abs(fillLine * 10.0));
                    float surfaceGlow = pow(edge, 8.0) * 5.0;
                    float fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), 3.0);

                    float depth = saturate(height);
                    float3 liquidColor = lerp(_DarkColor.rgb, _LightColor.rgb, depth);
                    float3 baseColor = lerp(liquidColor, _LightColor.rgb, fresnel);

                    float pulse = 0.8 + sin(_Time.y * 3.0) * 0.2;
                    float3 emissiveColor = _LightColor.rgb * surfaceGlow * pulse;

                    return float4(baseColor + emissiveColor, 1.0);
                }
                else
                {
                    // Radial distance for meniscus contact rim
                    float radialDist = saturate(length(input.positionOS.xz) * 2.0);
                    float meniscus = smoothstep(0.55, 1.0, radialDist);

                    // Fixed HUD Light Specular Glint
                    float3 fakeLightDir = normalize(float3(-0.4, 1.0, -0.4));
                    float3 halfDir = normalize(fakeLightDir + viewDir);
                    float spec = pow(saturate(dot(normalWS, halfDir)), 24.0);

                    // Radial center depth gradient
                    float centerDepth = 1.0 - saturate(length(input.positionOS.xz) * 1.5);
                    float3 capColor = lerp(_DarkColor.rgb, _LightColor.rgb, centerDepth * 0.8 + 0.2);

                    // Apply Meniscus Rim Darkening and Specular Highlight
                    capColor *= lerp(1.0, 0.3, meniscus);
                    capColor += spec * _LightColor.rgb * 0.9;

                    return float4(capColor, 1.0);
                }
            }
            ENDHLSL
        }

        // --- PASS 2: DEPTH PASS FOR CAMERA OPAQUE TEXTURE ---
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            Cull Off
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionOS   : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _LightColor;
                float4 _DarkColor;
                float4 _CurrentTilt;
                float _Fill;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float waveX = sin(input.positionOS.x * 7.0 + _Time.y * 2.5) * 0.012;
                float waveZ = cos(input.positionOS.z * 7.0 + _Time.y * 2.0) * 0.012;
                float wave = waveX + waveZ;

                float height = (input.positionOS.y - (-0.5)) / (0.5 - (-0.5));
                float tiltedHeight = height + dot(input.positionOS.xyz, _CurrentTilt.xyz) + wave;

                if (tiltedHeight - _Fill > 0.0)
                {
                    discard;
                }

                return 0;
            }
            ENDHLSL
        }
    }
}