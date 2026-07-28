Shader "Custom/LiquidGlobe_Standalone"
{
    Properties
    {
        _LightColor ("Light Color", Color) = (1, 0, 0, 1)
        _DarkColor ("Dark Color", Color) = (0.2, 0, 0, 1)
        _Fill ("Fill Percentage", Range(0, 1)) = 0.5
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
                float height = (input.positionOS.y - (-0.5)) / (0.5 - (-0.5));
                float tiltedHeight = height + dot(input.positionOS.xyz, _CurrentTilt.xyz);

                float fillLine = tiltedHeight - _Fill;
                if (fillLine > 0.0)
                {
                    discard;
                }

                float3 normalWS = isFrontFace ? normalize(input.normalWS) : float3(0.0, 1.0, 0.0);
                float3 viewDir = normalize(input.viewDirWS);

                if (isFrontFace)
                {
                    float edge = saturate(1.0 - abs(fillLine * 10.0));
                    float surfaceGlow = pow(edge, 8.0) * 5.0;
                    float fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), 3.0);

                    float3 baseColor = lerp(_LightColor.rgb, _DarkColor.rgb, fresnel);
                    float3 emissiveColor = _LightColor.rgb * surfaceGlow;

                    return float4(baseColor + emissiveColor, 1.0);
                }
                else
                {
                    float capLighting = saturate(dot(normalWS, viewDir)) * 0.5 + 0.5;
                    float3 capColor = lerp(_DarkColor.rgb, _LightColor.rgb, capLighting);

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
                float height = (input.positionOS.y - (-0.5)) / (0.5 - (-0.5));
                float tiltedHeight = height + dot(input.positionOS.xyz, _CurrentTilt.xyz);

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