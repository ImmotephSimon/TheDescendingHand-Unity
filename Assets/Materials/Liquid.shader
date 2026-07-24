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
            "Queue"="Geometry" 
            "RenderPipeline"="UniversalPipeline" 
        }

        Pass
        {
            Cull Off // Render backfaces to form the top cap surface

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
                // 1. Map local Y position (-0.5 to 0.5) to 0.0 -> 1.0 height range
                float height = (input.positionOS.y - (-0.5)) / (0.5 - (-0.5));

                // 2. Add tilt offset from C# script
                float tiltedHeight = height + dot(input.positionOS.xyz, _CurrentTilt.xyz);

                // 3. Clip everything above fill level
                float fillLine = tiltedHeight - _Fill;
                if (fillLine > 0.0)
                {
                    discard;
                }

                // 4. Handle Normal Vector
                // Front faces use the mesh normal; backfaces force an upward normal (0,1,0)
                float3 normalWS = isFrontFace ? normalize(input.normalWS) : float3(0.0, 1.0, 0.0);
                float3 viewDir = normalize(input.viewDirWS);

                // 5. Separate Front Glass / Liquid shading vs Backface Liquid Cap Surface
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
                    // Render backface as top surface liquid cap color
                    float capLighting = saturate(dot(normalWS, viewDir)) * 0.5 + 0.5;
                    float3 capColor = lerp(_DarkColor.rgb, _LightColor.rgb, capLighting);

                    return float4(capColor, 1.0);
                }
            }
            ENDHLSL
        }
    }
}