Shader "Custom/URPHeightAngleBlend"
{
    Properties
    {
        _SandTex ("Sand Texture", 2D) = "white" {}
        _SandNormal ("Sand Normal", 2D) = "bump" {}
        _GrassTex ("Grass Texture", 2D) = "white" {}
        _GrassNormal ("Grass Normal", 2D) = "bump" {}
        _DirtTex ("Dirt Texture", 2D) = "white" {}
        _DirtNormal ("Dirt Normal", 2D) = "bump" {}
        _StoneTex ("Stone Texture", 2D) = "white" {}
        _StoneNormal ("Stone Normal", 2D) = "bump" {}
        _ExtraTex ("Extra Texture", 2D) = "white" {}
        _ExtraNormal ("Extra Normal", 2D) = "bump" {}

        _SandHeight ("Sand Height", Float) = 2.0
        _GrassHeight ("Grass Height", Float) = 50.0
        _SlopeMin ("Slope Min", Float) = 10.0
        _SlopeMax ("Slope Max", Float) = 50.0
        _NoiseScale ("Noise Scale", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Определяем SamplerState с линейной фильтрацией и режимом Clamp.
            SamplerState sampler_LinearClamp
            {
                Filter = MIN_MAG_LINEAR_MIP_LINEAR;
                AddressU = Clamp;
                AddressV = Clamp;
            };

            struct Attributes
            {
                float4 position : POSITION;
                float3 normal   : NORMAL;
                float2 uv       : TEXCOORD0;
            };

            struct Varyings
            {
                float4 position    : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            // Объявляем текстуры как Texture2D.
            Texture2D _SandTex;
            Texture2D _GrassTex;
            Texture2D _DirtTex;
            Texture2D _StoneTex;
            Texture2D _ExtraTex;

            // Если нормальные карты будут использоваться – их тоже можно объявить:
            Texture2D _SandNormal;
            Texture2D _GrassNormal;
            Texture2D _DirtNormal;
            Texture2D _StoneNormal;
            Texture2D _ExtraNormal;

            float _SandHeight;
            float _GrassHeight;
            float _SlopeMin;
            float _SlopeMax;
            float _NoiseScale;

            // Функция для получения псевдослучайного значения.
            float rand(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
            }

            // Простейшая функция шума, похожая на Perlin noise.
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = rand(i);
                float b = rand(i + float2(1.0, 0.0));
                float c = rand(i + float2(0.0, 1.0));
                float d = rand(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.position = TransformObjectToHClip(IN.position);
                OUT.uv = IN.uv;
                OUT.worldPos = TransformObjectToWorld(IN.position).xyz;
                OUT.worldNormal = normalize(TransformObjectToWorldNormal(IN.normal));
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Определяем высоту (по оси Y) и угол наклона.
                float height = IN.worldPos.y;
                float slopeAngle = degrees(acos(saturate(dot(IN.worldNormal, float3(0,1,0)))));

                // Плавное смешение песка и травы в зависимости от высоты.
                float sandFactor = saturate(1.0 - height / _SandHeight);
                float grassFactor = saturate((height - _SandHeight) / (_GrassHeight - _SandHeight));

                float4 sandCol = _SandTex.Sample(sampler_LinearClamp, IN.uv);
                float4 grassCol = _GrassTex.Sample(sampler_LinearClamp, IN.uv);
                float4 baseCol = lerp(sandCol, grassCol, grassFactor);

                // Маска наклона для смешения грязи и камня.
                float slopeMask = saturate((slopeAngle - _SlopeMin) / (_SlopeMax - _SlopeMin));

                float n = noise(IN.worldPos.xz * _NoiseScale);

                float4 dirtCol = _DirtTex.Sample(sampler_LinearClamp, IN.uv);
                float4 stoneCol = _StoneTex.Sample(sampler_LinearClamp, IN.uv);
                float4 dirtStoneBlend = lerp(dirtCol, stoneCol, n);

                float4 finalColor = lerp(baseCol, dirtStoneBlend, slopeMask);

                // Дополнительное наложение для очень крутых участков.
                float extraMask = smoothstep(_SlopeMax, _SlopeMax + 10.0, slopeAngle);
                float4 extraCol = _ExtraTex.Sample(sampler_LinearClamp, IN.uv);
                finalColor = lerp(finalColor, extraCol, extraMask);

                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}
