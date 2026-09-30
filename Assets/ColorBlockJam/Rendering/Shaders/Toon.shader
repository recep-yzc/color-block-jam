// The toon look of the whole gameplay scene: blocks, doors, walls, ground and bursts.
// Light falls in two soft bands from a half-Lambert term, so shadow sides take a tinted color instead of going dark;
// a small glint and a soft rim make surfaces read like candy. Only the main light is used, with its shadows.
// Everything is per pixel but cheap: no textures, one light, and the only keywords are the ones URP needs for
// main light shadows, which it strips when shadows are off, and GPU instancing.
// Blocks and doors share one material and set their color through a property block (_Tint); with instancing the
// tint is read per instance, so each renderer keeps its color. All passes share one material buffer.
Shader "Color Block Jam/Toon"
{
    Properties
    {
        [MainColor] _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Tint", Color) = (0.62, 0.55, 0.9, 1)
        _LightThreshold ("Light Threshold", Range(0, 1)) = 0.5
        _LightSoftness ("Light Softness", Range(0.001, 0.5)) = 0.06
        _HighlightColor ("Highlight Color (A = strength)", Color) = (1, 0.95, 0.84, 0.55)
        _HighlightSize ("Highlight Size", Range(0, 0.5)) = 0.06
        _RimColor ("Rim Color (A = strength)", Color) = (1, 0.88, 0.97, 0.3)
        _RimWidth ("Rim Width", Range(0, 1)) = 0.3
        _VertexColorWeight ("Use Vertex Color", Range(0, 1)) = 0
        [HideInInspector] _Tint ("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _ShadowColor;
            half _LightThreshold;
            half _LightSoftness;
            half4 _HighlightColor;
            half _HighlightSize;
            half4 _RimColor;
            half _RimWidth;
            half _VertexColorWeight;
            #ifndef UNITY_INSTANCING_ENABLED
                half4 _Tint;
            #endif
        CBUFFER_END

        // Set per renderer through a property block; per instance when drawn instanced.
        #ifdef UNITY_INSTANCING_ENABLED
            UNITY_INSTANCING_BUFFER_START(ToonPerInstance)
                UNITY_DEFINE_INSTANCED_PROP(half4, _Tint)
            UNITY_INSTANCING_BUFFER_END(ToonPerInstance)
            #define TOON_TINT UNITY_ACCESS_INSTANCED_PROP(ToonPerInstance, _Tint)
        #else
            #define TOON_TINT _Tint
        #endif
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half3 tint : TEXCOORD2;
            };

            Varyings Vertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                // Particles bring their color in the vertices; everything else ignores it.
                output.tint = _BaseColor.rgb * TOON_TINT.rgb * lerp(half3(1, 1, 1), input.color.rgb, _VertexColorWeight);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                half3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));

                // Two soft bands of light, darkened where a shadow falls.
                half halfLambert = dot(normalWS, light.direction) * 0.5 + 0.5;
                half lit = smoothstep(_LightThreshold - _LightSoftness, _LightThreshold + _LightSoftness, halfLambert);
                lit *= light.shadowAttenuation;
                half3 color = input.tint * lerp(_ShadowColor.rgb, light.color, lit);

                // A small glint where the light catches the surface.
                half3 halfDirection = normalize(light.direction + viewWS);
                half glint = smoothstep(1.0 - _HighlightSize - 0.01, 1.0 - _HighlightSize + 0.01, dot(normalWS, halfDirection));
                color += _HighlightColor.rgb * (_HighlightColor.a * glint * lit);

                // A soft rim along the edges turned away from the camera.
                half facing = 1.0 - saturate(dot(normalWS, viewWS));
                half rim = smoothstep(1.0 - _RimWidth - 0.05, 1.0 - _RimWidth + 0.05, facing);
                color += _RimColor.rgb * (_RimColor.a * rim);

                return half4(color, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 ShadowVertex(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                // Keep casters behind the near plane of the shadow camera from being clipped.
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return positionCS;
            }

            half4 ShadowFragment() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVertex
            #pragma fragment DepthFragment
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 DepthVertex(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return TransformObjectToHClip(input.positionOS.xyz);
            }

            half4 DepthFragment() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
