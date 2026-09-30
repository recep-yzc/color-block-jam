// Second half of the held block's outline: the block's mesh pushed out along its normals, drawn in one flat color
// only where the outline mask did not mark the silhouette, so just the outer rim shows. It ignores depth and draws
// after everything else, so the rim stays on top of walls and other blocks.
// One pass, no lighting, no textures and no keywords: cheap on mobile, and it keeps the SRP Batcher working.
// The push uses normals averaged over vertices that share a position (TEXCOORD3, written by the block builder),
// so hard edges do not split the rim; meshes without them use their own normals.
Shader "Color Block Jam/Block Outline"
{
    Properties
    {
        _OutlineColor ("Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Width (world units)", Range(0, 0.3)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent+500"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Back
            ZWrite Off
            ZTest Always

            // Outside the silhouette the outline mask marked with 1.
            Stencil
            {
                Ref 1
                Comp NotEqual
            }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float3 smoothNormalOS : TEXCOORD3;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vertex(Attributes input)
            {
                float3 normalOS = dot(input.smoothNormalOS, input.smoothNormalOS) > 0.01 ? input.smoothNormalOS : input.normalOS;

                // Pushed in world space, so the rim keeps its width while the block is scaled.
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                positionWS += TransformObjectToWorldNormal(normalOS) * _OutlineWidth;

                Varyings output;
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
