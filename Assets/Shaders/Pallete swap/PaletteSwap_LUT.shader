Shader "Custom/PaletteSwap_LUT"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _PaletteTex ("Palette", 2D) = "white" {}

        _EmissionStrength ("Emission Strength", Float) = 2.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_PaletteTex);
            SAMPLER(sampler_PaletteTex);

            float _EmissionStrength;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Varyings i) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // palette lookup
                float index = color.r;

                float4 paletteColor =
                    SAMPLE_TEXTURE2D(_PaletteTex, sampler_PaletteTex, float2(index, 0.5));

                paletteColor.a = color.a;

                // 🔥 ADD EMISSION (this is the key)
                float3 emission = paletteColor.rgb * _EmissionStrength;

                // Add emission to base color (for bloom)
                float3 finalColor = paletteColor.rgb + emission;

                return float4(finalColor, paletteColor.a);
            }

            ENDHLSL
        }
    }
}