Shader "Custom/URPGlassRefraction"
{
    Properties
    {
        [Header(Surface Detail)]
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0, 1)) = 0.5

        [Header(Glass Tint and Textures)]
        _BaseColor("Glass Tint Color", Color) = (1, 1, 1, 1)
        _MainTex("Tint / Surface Texture", 2D) = "white" {}
        _AlbedoOpacity("Tint Blend Strength", Range(0, 0.5)) = 0.05

        [Header(Refraction)]
        _DistortionTex("Noise / Distortion Mask", 2D) = "white" {}
        _Distortion("Refraction Distortion", Range(0.0, 0.05)) = 0.01

        [Header(Fresnel Edge Highlight)]
        _FresnelColor("Fresnel Rim Color", Color) = (1, 1, 1, 1)
        _FresnelPower("Fresnel Tightness", Range(1.0, 8.0)) = 5.0
        _MinAlpha("Center Alpha Floor", Range(0, 0.3)) = 0.05
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent" 
            "Queue" = "Transparent+500"
            "IgnoreProjector" = "True"
        }

        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 screenPos    : TEXCOORD1;
                float3 positionWS   : TEXCOORD2;
                float3 normalWS     : TEXCOORD3;
                float3 tangentWS    : TEXCOORD4;
                float3 bitangentWS  : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_DistortionTex);
            SAMPLER(sampler_DistortionTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _FresnelColor;
                float _AlbedoOpacity;
                float _Distortion;
                float _BumpScale;
                float _FresnelPower;
                float _MinAlpha;
                float4 _BumpMap_ST;
                float4 _MainTex_ST;
                float4 _DistortionTex_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv         = TRANSFORM_TEX(input.uv, _BumpMap);

                output.normalWS   = normalInput.normalWS;
                output.tangentWS  = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;

                output.screenPos  = ComputeScreenPos(vertexInput.positionCS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // 1. Surface Normals
                float4 bumpSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv);
                float3 normalTS = UnpackNormalScale(bumpSample, _BumpScale);
                float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                float3 N = normalize(mul(normalTS, tangentToWorld));
                float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS);

                // 2. Texture & Noise Sampling
                float distortionMask = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, input.uv).r;
                half4 tintTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;

                // 3. Screen Refraction with Aspect Ratio Correction
                float2 screenUV = input.screenPos.xy / max(input.screenPos.w, 0.0001);
                float2 distortionOffset = normalTS.xy * _Distortion * distortionMask;
                float2 refractedUV = screenUV + distortionOffset;

                // 4. Sample Opaque/Liquid Background
                half3 sceneColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, refractedUV).rgb;

                // 5. Fresnel Edge & Specular Shading
                float NdotV = saturate(dot(N, V));
                float fresnel = pow(1.0 - NdotV, _FresnelPower);

                // Hardcoded fixed HUD specular glint for glass highlights
                float3 fakeLightDir = normalize(float3(-0.4, 0.8, -0.4));
                float3 halfDir = normalize(fakeLightDir + V);
                float spec = pow(saturate(dot(N, halfDir)), 64.0);

                // 6. Compositing Glass Layers
                half3 finalColor = sceneColor;
                finalColor = lerp(finalColor, tintTex.rgb, _AlbedoOpacity * fresnel); // Subtle edge tint
                finalColor += _FresnelColor.rgb * fresnel * 0.35;                      // Fresnel rim reflection
                finalColor += spec * _FresnelColor.rgb * 1.5;                         // Sharp specular glint

                // 7. Dynamic Transparency
                float alpha = saturate(fresnel * 0.85 + _MinAlpha);

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}