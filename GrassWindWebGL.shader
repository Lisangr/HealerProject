/*
Shader "Custom/URPGrassWindSimpleLit" {
    Properties {
        _BaseMap("Base Map", 2D) = "white" {}
        _Color("Color Tint", Color) = (1,1,1,1)
        _AlphaCutoff("Alpha Cutoff", Range(0,1)) = 0.5
        
        [Header(Wind Settings)]
        _WindStrength("Wind Strength", Range(0,1)) = 0.1
        _WindSpeed("Wind Speed", float) = 1.0
        _WindDirection("Wind Direction", Vector) = (1,0,0.5,0)
        _WindNoiseScale("Wind Noise Scale", Range(0,0.5)) = 0.1
        _WindTurbulence("Wind Turbulence", Range(0,2)) = 0.5
        _WindFrequency("Wind Frequency", Range(0.1, 10)) = 2.0
        _WindNoiseStrength("Wind Noise Strength", Range(0, 2)) = 0.5
        _MaxDisplacement("Max Displacement", Range(0, 1)) = 0.3

        [Header(Normal Settings)]
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Scale", Range(0,2)) = 1.0
        
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2
    }

    SubShader {
        Tags { 
            "RenderPipeline"="UniversalPipeline" 
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }
        LOD 200

        Pass {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull [_Cull]
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            float4 _Color;
            float _AlphaCutoff;

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            float _NormalScale;

            // Wind properties
            float _WindStrength;
            float _WindSpeed;
            float4 _WindDirection;
            float _WindNoiseScale;
            float _WindTurbulence;
            float _MaxDisplacement; // Объявление добавлено здесь

            struct Attributes {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float3 posWS : TEXCOORD4;
                float faceSign : TEXCOORD5;
            };

            Varyings Vert(Attributes IN) {
                Varyings OUT;
                float3 posOS = IN.vertex.xyz;
                
                // Wind calculations
                float3 windDir = normalize(_WindDirection.xyz);
                float timeVal = _Time.y * _WindSpeed;
                float3 noisePos = (posOS + _WorldSpaceCameraPos) * _WindNoiseScale;

                // Improved noise calculation
                float noise = sin(noisePos.x * 5.0 + timeVal * 2.0) * 0.5 +
                            cos(noisePos.z * 7.0 + timeVal * 1.5) * 0.5;
                noise *= _WindTurbulence;

                float displacement = clamp(noise * _WindStrength, -_MaxDisplacement, _MaxDisplacement);
                float heightFactor = pow(saturate(posOS.y), 2.0);
                float3 offset = windDir * displacement * heightFactor;
                
                posOS.xyz += offset;

                OUT.vertex = TransformObjectToHClip(posOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.posWS = TransformObjectToWorld(posOS);

                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normal, IN.tangent);
                OUT.normalWS = normalInput.normalWS;
                OUT.tangentWS = normalInput.tangentWS;
                OUT.bitangentWS = normalInput.bitangentWS;

                float3 viewDir = GetWorldSpaceViewDir(OUT.posWS);
                OUT.faceSign = dot(normalInput.normalWS, viewDir) > 0 ? 1 : -1;

                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target {
                half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _Color;
                #ifdef _ALPHATEST_ON
                    clip(baseCol.a - _AlphaCutoff);
                #endif

                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv), _NormalScale);
                float3x3 TBN = float3x3(IN.tangentWS, IN.bitangentWS, IN.normalWS);
                float3 normalWS = IN.faceSign * normalize(TransformTangentToWorld(normalTS, TBN));

                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                NdotL = abs(NdotL) * 0.7 + 0.3;
                
                half3 diffuse = baseCol.rgb * mainLight.color * NdotL;
                half3 ambient = SampleSH(normalWS) * baseCol.rgb;

                return half4(diffuse + ambient, baseCol.a);
            }
            ENDHLSL
        }

        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma shader_feature _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float _AlphaCutoff;
            float4 _BaseMap_ST;

            // Wind properties
            float _WindStrength;
            float _WindSpeed;
            float4 _WindDirection;
            float _WindNoiseScale;
            float _WindTurbulence;
            float _MaxDisplacement; // Объявление добавлено и здесь

            struct VertexInput {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            VertexOutput ShadowVert(VertexInput IN) {
                VertexOutput OUT;
                float3 posOS = IN.vertex.xyz;

                float3 windDir = normalize(_WindDirection.xyz);
                float timeVal = _Time.y * _WindSpeed;
                float3 noisePos = (posOS + _WorldSpaceCameraPos) * _WindNoiseScale;

                float noise = sin(noisePos.x * 5.0 + timeVal * 2.0) * 0.5 +
                            cos(noisePos.z * 7.0 + timeVal * 1.5) * 0.5;
                noise *= _WindTurbulence;

                float displacement = clamp(noise * _WindStrength, -_MaxDisplacement, _MaxDisplacement);
                float heightFactor = pow(saturate(posOS.y), 2.0);
                float3 offset = windDir * displacement * heightFactor;
                
                posOS.xyz += offset;

                OUT.vertex = TransformObjectToHClip(posOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 ShadowFrag(VertexOutput IN) : SV_TARGET {
                #ifdef _ALPHATEST_ON
                    half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                    clip(baseCol.a - _AlphaCutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}*/
                Shader "Custom/URPGrassWindSimpleLit" {
    Properties {
        _BaseMap("Base Map", 2D) = "white" {}
        _Color("Color Tint", Color) = (1,1,1,1)
        _AlphaCutoff("Alpha Cutoff", Range(0,1)) = 0.5
        
        [Header(Wind Settings)]
        _WindStrength("Wind Strength", Range(0,2)) = 0.5
        _WindSpeed("Wind Speed", Range(0,5)) = 1.0
        _WindDirection("Wind Direction", Vector) = (1,0,0.5,0)
        _WindNoiseScale("Noise Scale", Range(0,1)) = 0.2
        _WindTurbulence("Turbulence", Range(0,3)) = 1.0
        _MaxDisplacement("Max Displacement", Range(0,0.5)) = 0.2
        _WindFrequency("Frequency", Range(0.1,5)) = 2.0

        [Header(Normal Settings)]
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Scale", Range(0,2)) = 1.0
        
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2
    }

    SubShader {
        Tags { 
            "RenderPipeline"="UniversalPipeline" 
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }
        LOD 200

        Pass {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull [_Cull]
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _Color;
                half _AlphaCutoff;
                half _NormalScale;
                
                // Wind parameters
                half _WindStrength;
                half _WindSpeed;
                half4 _WindDirection;
                half _WindNoiseScale;
                half _WindTurbulence;
                half _MaxDisplacement;
                half _WindFrequency;
            CBUFFER_END

            struct Attributes {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 posWS : TEXCOORD2;
            };

            // Функция шума с частотной модуляцией
            float WindNoise(float3 position, float time) {
                float3 samplePos = position * _WindNoiseScale;
                float noise = sin(samplePos.x * _WindFrequency + time);
                noise += cos(samplePos.z * _WindFrequency * 0.8 + time * 0.7);
                noise += sin(samplePos.y * _WindFrequency * 0.5 + time * 1.2);
                return noise * _WindTurbulence * 0.3;
            }

            Varyings Vert(Attributes IN) {
                Varyings OUT;
                
                // Исходная позиция вершины
                float3 posOS = IN.vertex.xyz;
                
                // Расчет параметров ветра
                float3 windDir = normalize(_WindDirection.xyz);
                float time = _Time.y * _WindSpeed;
                
                // Генерация шума
                float noise = WindNoise(posOS, time);
                
                // Смещение вершин
                float displacement = noise * _WindStrength;
                displacement = clamp(displacement, -_MaxDisplacement, _MaxDisplacement);
                
                // Зависимость от высоты
                float heightFactor = saturate(posOS.y * 2.0); // Усиливаем влияние высоты
                displacement *= heightFactor;
                
                // Применение смещения
                posOS += windDir * displacement;

                // Трансформации
                OUT.vertex = TransformObjectToHClip(posOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.posWS = TransformObjectToWorld(posOS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normal);

                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target {
                half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _Color;
                #ifdef _ALPHATEST_ON
                    clip(baseCol.a - _AlphaCutoff);
                #endif

                // Нормаль из карты нормалей
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv), _NormalScale);
                half3x3 TBN = half3x3(normalize(cross(IN.normalWS, float3(0,1,0))), IN.normalWS, float3(0,1,0));
                half3 normalWS = normalize(mul(normalTS, TBN));

                // Освещение
                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(normalWS, mainLight.direction)) * 0.7 + 0.3;
                half3 lighting = mainLight.color * NdotL + SampleSH(normalWS);
                
                return half4(lighting * baseCol.rgb, baseCol.a);
            }
            ENDHLSL
        }

        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma shader_feature _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;

            CBUFFER_START(UnityPerMaterial)
                // Те же параметры что и в основном пассе
                half _WindStrength;
                half _WindSpeed;
                half4 _WindDirection;
                half _WindNoiseScale;
                half _WindTurbulence;
                half _MaxDisplacement;
                half _WindFrequency;
                half _AlphaCutoff;
            CBUFFER_END

            struct VertexInput {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float WindNoise(float3 position, float time) {
                // Та же реализация что и в основном пассе
                float3 samplePos = position * _WindNoiseScale;
                float noise = sin(samplePos.x * _WindFrequency + time);
                noise += cos(samplePos.z * _WindFrequency * 0.8 + time * 0.7);
                noise += sin(samplePos.y * _WindFrequency * 0.5 + time * 1.2);
                return noise * _WindTurbulence * 0.3;
            }

            VertexOutput ShadowVert(VertexInput IN) {
                VertexOutput OUT;
                
                float3 posOS = IN.vertex.xyz;
                float3 windDir = normalize(_WindDirection.xyz);
                float time = _Time.y * _WindSpeed;
                
                float noise = WindNoise(posOS, time);
                float displacement = clamp(noise * _WindStrength, -_MaxDisplacement, _MaxDisplacement);
                displacement *= saturate(posOS.y * 2.0);
                
                posOS += windDir * displacement;
                
                OUT.vertex = TransformObjectToHClip(posOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 ShadowFrag(VertexOutput IN) : SV_TARGET {
                #ifdef _ALPHATEST_ON
                    half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).a;
                    clip(alpha - _AlphaCutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}