Shader "Universal Render Pipeline/Custom/HeightMapBlendShader"
{
    Properties
    {
        _HeightMap ("Height Map", 2D) = "white" {}

        _Albedo1 ("Albedo 1", 2D) = "white" {}
        _NormalMap1 ("Normal Map 1", 2D) = "bump" {}
        _Smoothness1 ("Smoothness 1", Range(0,1)) = 0.5
        _NormalInt1 ("Normal Intensity 1", Range(0,1)) = 1.0

        _Albedo2 ("Albedo 2", 2D) = "white" {}
        _NormalMap2 ("Normal Map 2", 2D) = "bump" {}
        _Smoothness2 ("Smoothness 2", Range(0,1)) = 0.5
        _NormalInt2 ("Normal Intensity 2", Range(0,1)) = 1.0

        _Albedo3 ("Albedo 3", 2D) = "white" {}
        _NormalMap3 ("Normal Map 3", 2D) = "bump" {}
        _Smoothness3 ("Smoothness 3", Range(0,1)) = 0.5
        _NormalInt3 ("Normal Intensity 3", Range(0,1)) = 1.0

        _Albedo4 ("Albedo 4", 2D) = "white" {}
        _NormalMap4 ("Normal Map 4", 2D) = "bump" {}
        _Smoothness4 ("Smoothness 4", Range(0,1)) = 0.5
        _NormalInt4 ("Normal Intensity 4", Range(0,1)) = 1.0

        _Layer1Max ("Layer 1 Max Height", Range(0,10)) = 0.25
        _Layer2Max ("Layer 2 Max Height", Range(0,10)) = 0.50
        _Layer3Max ("Layer 3 Max Height", Range(0,10)) = 0.75
        _Layer4Max ("Layer 4 Max Height", Range(0,10)) = 1.0
        _BlendRange("Blend Range", Range(0,10)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float3 positionOS : POSITION;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowVert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 ShadowFrag(Varyings i) : SV_Target
            {
                return half4(0,0,0,1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile _ _NORMALMAP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
            };

            sampler2D _HeightMap;
            float4 _HeightMap_ST;

            sampler2D _Albedo1, _NormalMap1;
            float4 _Albedo1_ST, _NormalMap1_ST;
            float _Smoothness1, _NormalInt1;

            sampler2D _Albedo2, _NormalMap2;
            float4 _Albedo2_ST, _NormalMap2_ST;
            float _Smoothness2, _NormalInt2;

            sampler2D _Albedo3, _NormalMap3;
            float4 _Albedo3_ST, _NormalMap3_ST;
            float _Smoothness3, _NormalInt3;

            sampler2D _Albedo4, _NormalMap4;
            float4 _Albedo4_ST, _NormalMap4_ST;
            float _Smoothness4, _NormalInt4;

            float _Layer1Max, _Layer2Max, _Layer3Max, _Layer4Max, _BlendRange;

            float3 SampleNormalMap(sampler2D normalMap, float2 uv, float3 t, float3 b, float3 n, float intensity)
            {
                float3 normalTS = UnpackNormal(tex2D(normalMap, uv));
                float3 normalWS = normalize(t * normalTS.x + b * normalTS.y + n * normalTS.z);
                return normalize(lerp(n, normalWS, intensity));
            }

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS);
                OUT.positionCS = posInputs.positionCS;
                OUT.shadowCoord = GetShadowCoord(posInputs);

                OUT.uv = IN.uv;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.tangentWS = TransformObjectToWorldDir(IN.tangentOS.xyz);
                OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * IN.tangentOS.w;
                return OUT;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float height = tex2D(_HeightMap, TRANSFORM_TEX(i.uv, _HeightMap)).r;

                float b1 = smoothstep(_Layer1Max - _BlendRange, _Layer1Max + _BlendRange, height);
                float b2 = smoothstep(_Layer2Max - _BlendRange, _Layer2Max + _BlendRange, height);
                float b3 = smoothstep(_Layer3Max - _BlendRange, _Layer3Max + _BlendRange, height);
                float b4 = smoothstep(_Layer4Max - _BlendRange, _Layer4Max + _BlendRange, height);

                float w1 = 1.0 - b1;
                float w2 = b1 * (1.0 - b2);
                float w3 = b2 * (1.0 - b3);
                float w4 = b3 * (1.0 - b4) + b4;

                half4 col = tex2D(_Albedo1, TRANSFORM_TEX(i.uv, _Albedo1)) * w1 +
                            tex2D(_Albedo2, TRANSFORM_TEX(i.uv, _Albedo2)) * w2 +
                            tex2D(_Albedo3, TRANSFORM_TEX(i.uv, _Albedo3)) * w3 +
                            tex2D(_Albedo4, TRANSFORM_TEX(i.uv, _Albedo4)) * w4;

                float3 normalWS = normalize(
                    SampleNormalMap(_NormalMap1, TRANSFORM_TEX(i.uv, _NormalMap1), i.tangentWS, i.bitangentWS, i.normalWS, _NormalInt1) * w1 +
                    SampleNormalMap(_NormalMap2, TRANSFORM_TEX(i.uv, _NormalMap2), i.tangentWS, i.bitangentWS, i.normalWS, _NormalInt2) * w2 +
                    SampleNormalMap(_NormalMap3, TRANSFORM_TEX(i.uv, _NormalMap3), i.tangentWS, i.bitangentWS, i.normalWS, _NormalInt3) * w3 +
                    SampleNormalMap(_NormalMap4, TRANSFORM_TEX(i.uv, _NormalMap4), i.tangentWS, i.bitangentWS, i.normalWS, _NormalInt4) * w4);

                float smoothness = _Smoothness1 * w1 + _Smoothness2 * w2 + _Smoothness3 * w3 + _Smoothness4 * w4;

                Light light = GetMainLight(i.shadowCoord);
                float3 lightDir = normalize(-light.direction);
                float NdotL = saturate(dot(normalWS, lightDir));

                float3 diffuse = col.rgb * light.color * NdotL * light.shadowAttenuation;

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(normalWS, halfDir));
                float specular = pow(NdotH, smoothness * 128.0) * light.shadowAttenuation;

                float3 ambient = 0.3 * col.rgb;
                float3 finalColor = (ambient + diffuse + specular) * 1.5;

                return half4(finalColor, col.a);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/Fallback"
}
