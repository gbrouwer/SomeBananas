Shader "URP/Debug/TriangleIndexColor"
{
    Properties
    {
        _ColorIntensity ("Color Intensity", Range(0.1, 10)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float _ColorIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            // 👇 Primitive ID is read here directly!
            half4 frag(Varyings input, uint primID : SV_PrimitiveID) : SV_Target
            {
                float t = frac(primID * 0.61803398875); // golden ratio
                float3 rgb = float3(t, 1.0 - t, sin(t * 6.2831));
                return half4(rgb * _ColorIntensity, 1.0);
            }

            ENDHLSL
        }
    }
}
