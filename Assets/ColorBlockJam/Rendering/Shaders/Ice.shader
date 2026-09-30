// The ice around a frozen block: the block's mesh pushed out a little along its normals and drawn see-through over
// it, so the block's color shows through, cooled. The frost texture is projected along the side each face points to,
// in world space, so it tiles evenly on every shape and lines up across blocks. Thick ice looks whiter at grazing
// angles, which a rim gives, and a small glint from the main light makes it read as glossy.
// One pass, one texture sample, the main light only and no keywords: cheap on mobile, and SRP Batcher friendly.
// The push uses normals averaged over vertices that share a position (TEXCOORD3, written by the block builder),
// so the shell stays closed at hard edges; meshes without them use their own normals.
Shader "Color Block Jam/Ice"
{
    Properties
    {
        [MainTexture] _FrostMap ("Frost", 2D) = "white" {}
        _FrostScale ("Frost Tiling (per world unit)", Float) = 0.4
        [MainColor] _IceColor ("Color (A = opacity)", Color) = (0.86, 0.96, 1, 0.62)
        _ShadowBrightness ("Shadow Brightness", Range(0, 1)) = 0.75
        _RimColor ("Rim Color (A = strength)", Color) = (0.94, 0.99, 1, 0.8)
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.5
        _GlintColor ("Glint Color (A = strength)", Color) = (1, 1, 1, 0.6)
        _GlintPower ("Glint Sharpness", Range(4, 256)) = 48
        _Thickness ("Thickness (model units)", Range(0, 0.5)) = 0.16
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Ice"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_FrostMap);
            SAMPLER(sampler_FrostMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _FrostMap_ST;
                float _FrostScale;
                half4 _IceColor;
                half _ShadowBrightness;
                half4 _RimColor;
                half _RimPower;
                half4 _GlintColor;
                half _GlintPower;
                float _Thickness;
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
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
            };

            Varyings Vertex(Attributes input)
            {
                float3 pushOS = dot(input.smoothNormalOS, input.smoothNormalOS) > 0.01 ? input.smoothNormalOS : input.normalOS;

                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz + normalize(pushOS) * _Thickness);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                half3 viewWS = normalize(GetWorldSpaceViewDir(input.positionWS));

                // Projected along the axis the face points to most: the top from above, the sides from the side.
                half3 facing = abs(normalWS);
                float2 uv = facing.y >= max(facing.x, facing.z) ? input.positionWS.xz
                    : facing.x >= facing.z ? input.positionWS.zy : input.positionWS.xy;
                half3 frost = SAMPLE_TEXTURE2D(_FrostMap, sampler_FrostMap, uv * _FrostScale * _FrostMap_ST.xy + _FrostMap_ST.zw).rgb;

                Light light = GetMainLight();
                half halfLambert = dot(normalWS, light.direction) * 0.5h + 0.5h;
                half3 color = frost * _IceColor.rgb * lerp(_ShadowBrightness, 1.0h, halfLambert);

                half rim = pow(1.0h - saturate(dot(normalWS, viewWS)), _RimPower) * _RimColor.a;
                color = lerp(color, _RimColor.rgb, rim);

                half glint = pow(saturate(dot(normalWS, normalize(light.direction + viewWS))), _GlintPower) * _GlintColor.a;
                color += _GlintColor.rgb * glint;

                return half4(color, saturate(_IceColor.a + rim + glint));
            }
            ENDHLSL
        }
    }
}
