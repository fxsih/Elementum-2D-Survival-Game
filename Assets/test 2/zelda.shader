Shader "Custom/ProceduralWater_Flowing"
{
    Properties
    {
        [Header(Colors)]
        _WaterCol ("Deep Water", Color) = (0.0, 0.4453, 0.7305, 1.0)
        _Water2Col ("Shallow Water", Color) = (0.0, 0.4180, 0.6758, 1.0)
        _FoamCol ("Foam Tint", Color) = (0.8125, 0.9609, 0.9648, 1.0)

        [Header(Animation and Scale)]
        _Scale ("Pattern Scale", Float) = 32.0
        _Speed ("Wave Wobble Speed", Range(0, 5)) = 1.0
        _Distortion ("Distortion Strength", Range(0, 2)) = 1.0
        
        [Header(Flow Movement)]
        _FlowX ("Flow Speed X", Range(-2, 2)) = 0.5
        _FlowY ("Flow Speed Y", Range(-2, 2)) = 0.2

        [Header(Random Particles)]
        _ParticleCol ("Particle Color", Color) = (1, 1, 1, 1)
        _ParticleDensity ("Particle Density", Range(0, 100)) = 40.0
        _ParticleSize ("Particle Size", Range(0.001, 1)) = 0.02
        _ParticleFlickerSpeed ("Flicker Speed", Range(0, 20)) = 8.0
        _ParticleIntensity ("Particle Brightness", Range(0, 50)) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
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

            float4 _WaterCol, _Water2Col, _FoamCol, _ParticleCol;
            float _Scale, _Speed, _Distortion, _FlowX, _FlowY;
            float _ParticleDensity, _ParticleSize, _ParticleFlickerSpeed, _ParticleIntensity;

            #define M_2PI 6.283185307
            #define M_6PI 18.84955592

            // Helper for randomizing particle positions
            float hash21(float2 p) {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float circ(float2 pos, float2 c, float s)
            {
                c = abs(pos - c);
                c = min(c, 1.0 - c);
                return smoothstep(0.0, 0.002, sqrt(s) - sqrt(dot(c, c))) * -1.0;
            }

            float waterlayer(float2 uv)
            {
                uv = frac(uv);
                float ret = 1.0;
                
                // Original foam pattern circles
                ret += circ(uv, float2(0.37378, 0.277169), 0.0268181);
                ret += circ(uv, float2(0.0317477, 0.540372), 0.0193742);
                ret += circ(uv, float2(0.430044, 0.882218), 0.0232337);
                ret += circ(uv, float2(0.641033, 0.695106), 0.0117864);
                ret += circ(uv, float2(0.0146398, 0.0791346), 0.0299458);
                ret += circ(uv, float2(0.43871, 0.394445), 0.0289087);
                ret += circ(uv, float2(0.909446, 0.878141), 0.028466);
                ret += circ(uv, float2(0.310149, 0.686637), 0.0128496);
                ret += circ(uv, float2(0.928617, 0.195986), 0.0152041);
                ret += circ(uv, float2(0.0438506, 0.868153), 0.0268601);
                ret += circ(uv, float2(0.308619, 0.194937), 0.00806102);
                ret += circ(uv, float2(0.349922, 0.449714), 0.00928667);
                ret += circ(uv, float2(0.0449556, 0.953415), 0.023126);
                ret += circ(uv, float2(0.117761, 0.503309), 0.0151272);
                ret += circ(uv, float2(0.563517, 0.244991), 0.0292322);
                ret += circ(uv, float2(0.566936, 0.954457), 0.00981141);
                ret += circ(uv, float2(0.0489944, 0.200931), 0.0178746);
                ret += circ(uv, float2(0.569297, 0.624893), 0.0132408);
                ret += circ(uv, float2(0.298347, 0.710972), 0.0114426);
                ret += circ(uv, float2(0.878141, 0.771279), 0.00322719);
                ret += circ(uv, float2(0.150995, 0.376221), 0.00216157);
                ret += circ(uv, float2(0.119673, 0.541984), 0.0124621);
                ret += circ(uv, float2(0.629598, 0.295629), 0.0198736);
                ret += circ(uv, float2(0.334357, 0.266278), 0.0187145);
                ret += circ(uv, float2(0.918044, 0.968163), 0.0182928);
                ret += circ(uv, float2(0.965445, 0.505026), 0.006348);
                ret += circ(uv, float2(0.514847, 0.865444), 0.00623523);
                ret += circ(uv, float2(0.710575, 0.0415131), 0.00322689);
                ret += circ(uv, float2(0.71403, 0.576945), 0.0215641);
                ret += circ(uv, float2(0.748873, 0.413325), 0.0110795);
                ret += circ(uv, float2(0.0623365, 0.896713), 0.0236203);
                ret += circ(uv, float2(0.980482, 0.473849), 0.00573439);
                ret += circ(uv, float2(0.647463, 0.654349), 0.0188713);
                ret += circ(uv, float2(0.651406, 0.981297), 0.00710875);
                ret += circ(uv, float2(0.428928, 0.382426), 0.0298806);
                ret += circ(uv, float2(0.811545, 0.62568), 0.00265539);
                ret += circ(uv, float2(0.400787, 0.74162), 0.00486609);
                ret += circ(uv, float2(0.331283, 0.418536), 0.00598028);
                ret += circ(uv, float2(0.894762, 0.0657997), 0.00760375);
                ret += circ(uv, float2(0.525104, 0.572233), 0.0141796);
                ret += circ(uv, float2(0.431526, 0.911372), 0.0213234);
                ret += circ(uv, float2(0.658212, 0.910553), 0.000741023);
                ret += circ(uv, float2(0.514523, 0.243263), 0.0270685);
                ret += circ(uv, float2(0.0249494, 0.252872), 0.00876653);
                ret += circ(uv, float2(0.502214, 0.47269), 0.0234534);
                ret += circ(uv, float2(0.693271, 0.431469), 0.0246533);
                ret += circ(uv, float2(0.415, 0.884418), 0.0271696);
                ret += circ(uv, float2(0.149073, 0.41204), 0.00497198);
                ret += circ(uv, float2(0.533816, 0.897634), 0.00650833);
                ret += circ(uv, float2(0.0409132, 0.83406), 0.0191398);
                ret += circ(uv, float2(0.638585, 0.646019), 0.0206129);
                ret += circ(uv, float2(0.660342, 0.966541), 0.0053511);
                ret += circ(uv, float2(0.513783, 0.142233), 0.00471653);
                ret += circ(uv, float2(0.124305, 0.644263), 0.00116724);
                ret += circ(uv, float2(0.99871, 0.583864), 0.0107329);
                ret += circ(uv, float2(0.894879, 0.233289), 0.00667092);
                ret += circ(uv, float2(0.246286, 0.682766), 0.00411623);
                ret += circ(uv, float2(0.0761895, 0.16327), 0.0145935);
                ret += circ(uv, float2(0.949386, 0.802936), 0.0100873);
                ret += circ(uv, float2(0.480122, 0.196554), 0.0110185);
                ret += circ(uv, float2(0.896854, 0.803707), 0.013969);
                ret += circ(uv, float2(0.292865, 0.762973), 0.00566413);
                ret += circ(uv, float2(0.0995585, 0.117457), 0.00869407);
                ret += circ(uv, float2(0.377713, 0.00335442), 0.0063147);
                ret += circ(uv, float2(0.506365, 0.531118), 0.0144016);
                ret += circ(uv, float2(0.408806, 0.894771), 0.0243923);
                ret += circ(uv, float2(0.143579, 0.85138), 0.00418529);
                ret += circ(uv, float2(0.0902811, 0.181775), 0.0108896);
                ret += circ(uv, float2(0.780695, 0.394644), 0.00475475);
                ret += circ(uv, float2(0.298036, 0.625531), 0.00325285);
                ret += circ(uv, float2(0.218423, 0.714537), 0.00157212);
                ret += circ(uv, float2(0.658836, 0.159556), 0.00225897);
                ret += circ(uv, float2(0.987324, 0.146545), 0.0288391);
                ret += circ(uv, float2(0.222646, 0.251694), 0.00092276);
                ret += circ(uv, float2(0.159826, 0.528063), 0.00605293);

                return max(ret, 0.0);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                // Core Movement calculation
                float2 flowOffset = float2(_FlowX, _FlowY) * _Time.y;
                float2 uv = (i.uv * _Scale) + flowOffset;
                float3 cdir = float3(0, 1, 0); 
                float time = _Time.y * _Speed;

                uv *= 0.25;

                // Core Distortion logic (Unchanged)
                float2 a = (0.025 * _Distortion) * cdir.xz / cdir.y;
                float h = sin(uv.x + time);
                uv += a * h;
                h = sin(0.841471 * uv.x - 0.540302 * uv.y + time);
                uv += a * h;

                float d1 = (uv.x + uv.y) % M_2PI;
                float d2 = ((uv.x + uv.y + 0.25) * 1.3) % M_6PI;
                d1 = time * 0.07 + d1;
                d2 = time * 0.5 + d2;
                
                float2 dist = float2(
                    sin(d1) * 0.15 + sin(d2) * 0.05,
                    cos(d1) * 0.15 + cos(d2) * 0.05
                ) * _Distortion;

                // Base Water Layers (Unchanged)
                float layer1 = waterlayer(uv + dist.xy);
                float layer2 = waterlayer(1.0 - uv - dist.yx);

                float3 color = lerp(_WaterCol.rgb, _Water2Col.rgb, layer1);
                color = lerp(color, _FoamCol.rgb, layer2);

                // --- NEW PARTICLE EFFECT ---
                // We use the same UV as the water so they flow together
                float2 pUV = uv * _ParticleDensity;
                float2 id = floor(pUV);
                float2 gv = frac(pUV) - 0.5;

                // Random position for particle within its grid cell
                float n = hash21(id);
                float2 pPos = float2(frac(n*15.3), frac(n*94.2)) - 0.5;

                // Create the shape and flicker
                float distToCenter = length(gv - pPos);
                float flicker = sin(_Time.y * _ParticleFlickerSpeed + n * M_2PI) * 0.5 + 0.5;
                flicker = pow(flicker, 4.0); // Sharpen the "blink"

                float particleMask = smoothstep(_ParticleSize, _ParticleSize * 0.5, distToCenter);
                float3 particles = _ParticleCol.rgb * particleMask * flicker * _ParticleIntensity;

                // Add particles to the final color
                color += particles;

                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }
}