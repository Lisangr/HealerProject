Shader "Custom/TerrainPainterV3"
{
    Properties
    {
        [Header(Textures)]
        _SandTex ("Sand Albedo", 2D) = "white" {}
        _SandNormal ("Sand Normal", 2D) = "bump" {}
        _GrassTex ("Grass Albedo", 2D) = "white" {}
        _GrassNormal ("Grass Normal", 2D) = "bump" {}
        _DirtTex ("Dirt Albedo", 2D) = "white" {}
        _DirtNormal ("Dirt Normal", 2D) = "bump" {}
        _StoneTex ("Stone Albedo", 2D) = "white" {}
        _StoneNormal ("Stone Normal", 2D) = "bump" {}
        _SnowTex ("Snow Albedo", 2D) = "white" {}
        _SnowNormal ("Snow Normal", 2D) = "bump" {}
        
        [Header(Height Settings)]
        _SandHeight ("Sand Height", Float) = 2.0
        _SandFade ("Sand Fade Distance", Float) = 1.0
        _GrassHeight ("Grass Height", Float) = 50.0
        _GrassFade ("Grass Fade Distance", Float) = 10.0
        
        [Header(Angle Settings)]
        _AngleThreshold ("Angle Threshold", Range(0,90)) = 30.0
        _AngleFade ("Angle Fade Range", Range(0,30)) = 10.0
        
        [Header(Noise Settings)]
        _NoiseScale ("Noise Scale", Float) = 10.0
        _NoiseBlend ("Noise Blend Factor", Range(0,1)) = 0.3
        
        [Header(Lighting)]
        _ShadowStrength ("Shadow Strength", Range(0,1)) = 0.5
        _Smoothness ("Smoothness", Range(0,1)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

        TEXTURE2D(_SandTex); SAMPLER(sampler_SandTex);
        TEXTURE2D(_GrassTex); SAMPLER(sampler_GrassTex);
        TEXTURE2D(_DirtTex); SAMPLER(sampler_DirtTex);
        TEXTURE2D(_StoneTex); SAMPLER(sampler_StoneTex);
        TEXTURE2D(_SnowTex); SAMPLER(sampler_SnowTex);
        
        TEXTURE2D(_SandNormal); SAMPLER(sampler_SandNormal);
        TEXTURE2D(_GrassNormal); SAMPLER(sampler_GrassNormal);
        TEXTURE2D(_DirtNormal); SAMPLER(sampler_DirtNormal);
        TEXTURE2D(_StoneNormal); SAMPLER(sampler_StoneNormal);
        TEXTURE2D(_SnowNormal); SAMPLER(sampler_SnowNormal);

        CBUFFER_START(UnityPerMaterial)
        float _SandHeight;
        float _SandFade;
        float _GrassHeight;
        float _GrassFade;
        float _AngleThreshold;
        float _AngleFade;
        float _NoiseScale;
        float _NoiseBlend;
        float _ShadowStrength;
        float _Smoothness;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
            };

            float3 mod289(float3 x) { return x - floor(x / 289.0) * 289.0; }
            float4 mod289(float4 x) { return x - floor(x / 289.0) * 289.0; }
            float4 permute(float4 x) { return mod289((x * 34.0 + 1.0) * x); }
            float4 taylorInvSqrt(float4 r) { return 1.79284291400159 - 0.85373472095314 * r; }

            float snoise(float3 v)
            {
                const float2 C = float2(1.0/6.0, 1.0/3.0);
                const float4 D = float4(0.0, 0.5, 1.0, 2.0);

                float3 i = floor(v + dot(v, C.yyy));
                float3 x0 = v - i + dot(i, C.xxx);

                float3 g = step(x0.yzx, x0.xyz);
                float3 l = 1.0 - g;
                float3 i1 = min(g.xyz, l.zxy);
                float3 i2 = max(g.xyz, l.zxy);

                float3 x1 = x0 - i1 + C.x;
                float3 x2 = x0 - i2 + C.y;
                float3 x3 = x0 - D.yyy;

                i = mod289(i);
                float4 p = permute(permute(permute(
                    i.z + float4(0.0, i1.z, i2.z, 1.0))
                    + i.y + float4(0.0, i1.y, i2.y, 1.0))
                    + i.x + float4(0.0, i1.x, i2.x, 1.0));

                float n_ = 0.142857142857;
                float3 ns = n_ * D.wyz - D.xzx;

                float4 j = p - 49.0 * floor(p * ns.z * ns.z);

                float4 x_ = floor(j * ns.z);
                float4 y_ = floor(j - 7.0 * x_);

                float4 x = x_ * ns.x + ns.yyyy;
                float4 y = y_ * ns.x + ns.yyyy;
                float4 h = 1.0 - abs(x) - abs(y);

                float4 b0 = float4(x.xy, y.xy);
                float4 b1 = float4(x.zw, y.zw);

                float4 s0 = floor(b0)*2.0 + 1.0;
                float4 s1 = floor(b1)*2.0 + 1.0;
                float4 sh = -step(h, float4(0.0,0.0,0.0,0.0));

                float4 a0 = b0.xzyw + s0.xzyw*sh.xxyy;
                float4 a1 = b1.xzyw + s1.xzyw*sh.zzww;

                float3 p0 = float3(a0.xy, h.x);
                float3 p1 = float3(a0.zw, h.y);
                float3 p2 = float3(a1.xy, h.z);
                float3 p3 = float3(a1.zw, h.w);

                float4 norm = taylorInvSqrt(float4(dot(p0,p0), dot(p1,p1), dot(p2,p2), dot(p3,p3)));
                p0 *= norm.x;
                p1 *= norm.y;
                p2 *= norm.z;
                p3 *= norm.w;

                float4 m = max(0.6 - float4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
                m = m * m;
                return 42.0 * dot(m*m, float4(dot(p0,x0), dot(p1,x1), dot(p2,x2), dot(p3,x3)));
            }

            float SmoothStep(float edge0, float edge1, float x)
            {
                float t = saturate((x - edge0) / (edge1 - edge0));
                return t * t * (3.0 - 2.0 * t);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                OUT.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float height = IN.positionWS.y;
                float3 normal = normalize(IN.normalWS);
                float angle = acos(dot(normal, float3(0,1,0))) * (180.0 / PI);

                half4 sandColor = SAMPLE_TEXTURE2D(_SandTex, sampler_SandTex, IN.uv * 2.0);
                half4 grassColor = SAMPLE_TEXTURE2D(_GrassTex, sampler_GrassTex, IN.uv * 2.0);
                half4 dirtColor = SAMPLE_TEXTURE2D(_DirtTex, sampler_DirtTex, IN.uv * 2.0);
                half4 stoneColor = SAMPLE_TEXTURE2D(_StoneTex, sampler_StoneTex, IN.uv * 2.0);
                half4 snowColor = SAMPLE_TEXTURE2D(_SnowTex, sampler_SnowTex, IN.uv * 2.0);
                
                half3 sandNormal = UnpackNormal(SAMPLE_TEXTURE2D(_SandNormal, sampler_SandNormal, IN.uv * 2.0));
                half3 grassNormal = UnpackNormal(SAMPLE_TEXTURE2D(_GrassNormal, sampler_GrassNormal, IN.uv * 2.0));
                half3 dirtNormal = UnpackNormal(SAMPLE_TEXTURE2D(_DirtNormal, sampler_DirtNormal, IN.uv * 2.0));
                half3 stoneNormal = UnpackNormal(SAMPLE_TEXTURE2D(_StoneNormal, sampler_StoneNormal, IN.uv * 2.0));
                half3 snowNormal = UnpackNormal(SAMPLE_TEXTURE2D(_SnowNormal, sampler_SnowNormal, IN.uv * 2.0));

                float sandFactor = SmoothStep(_SandHeight - _SandFade, _SandHeight + _SandFade, height);
                float grassFactor = SmoothStep(_GrassHeight - _GrassFade, _GrassHeight + _GrassFade, height);
                float angleFactor = SmoothStep(_AngleThreshold - _AngleFade, _AngleThreshold + _AngleFade, angle);
                
                float noise = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                for (int i = 0; i < 3; i++)
                {
                    noise += amplitude * (snoise(IN.positionWS * _NoiseScale * frequency) + 1) * 0.5;
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                
                half4 baseColor = lerp(sandColor, grassColor, sandFactor);
                baseColor = lerp(baseColor, snowColor, grassFactor);
                half4 steepColor = lerp(dirtColor, stoneColor, noise * _NoiseBlend);
                baseColor = lerp(baseColor, steepColor, angleFactor);
                
                half3 baseNormal = lerp(sandNormal, grassNormal, sandFactor);
                baseNormal = lerp(baseNormal, snowNormal, grassFactor);
                half3 steepNormal = lerp(dirtNormal, stoneNormal, noise * _NoiseBlend);
                baseNormal = lerp(baseNormal, steepNormal, angleFactor);
                
                Light mainLight = GetMainLight(IN.shadowCoord);
                float shadow = mainLight.shadowAttenuation;
                shadow = lerp(1.0, shadow, _ShadowStrength);
                
                float NdotL = saturate(dot(baseNormal, mainLight.direction));
                float3 diffuse = mainLight.color * (NdotL * shadow);
                
                float3 halfDir = normalize(IN.viewDirWS + mainLight.direction);
                float NdotH = saturate(dot(baseNormal, halfDir));
                float specular = pow(NdotH, 64.0) * _Smoothness;
                
                float3 ambient = SampleSH(baseNormal);
                float3 lighting = (diffuse + specular + ambient) * baseColor.rgb;
                
                return half4(lighting, 1.0);
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
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                
                output.positionCS = positionCS;
                output.uv = input.texcoord;
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}