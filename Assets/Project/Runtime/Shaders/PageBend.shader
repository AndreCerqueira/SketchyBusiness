Shader "Custom/URP/PageBend"
{
    Properties
    {
        _BaseMap       ("Texture", 2D)                   = "white" {}
        _BaseColor     ("Base Color", Color)             = (1, 1, 1, 1)
        _PeakHeight    ("Peak Height", Range(-1, 1))     = 0.3
        _BendAxis      ("Bend Axis (0=U | 1=V)", Range(0, 1)) = 0
        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Transparent" // Alterado de Geometry para Transparent
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            // Estas duas linhas ativam a transparência real
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float  _PeakHeight;
                float  _BendAxis;
                float  _AmbientStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
            };

            void BendPage(float uvCoord, out float disp, out float grad)
            {
                float angle = uvCoord * PI;
                disp = sin(angle) * _PeakHeight;
                grad = cos(angle) * _PeakHeight * PI;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 pos = IN.positionOS.xyz;

                float useV    = step(0.5, _BendAxis);
                float uvCoord = lerp(IN.uv.x, IN.uv.y, useV);

                float disp, grad;
                BendPage(uvCoord, disp, grad);
                pos.y += disp;

                float3 newNormal;
                if (useV < 0.5)
                {
                    float3 tang = normalize(float3(1.0, grad, 0.0));
                    float3 bita = float3(0.0, 0.0, 1.0);
                    newNormal   = normalize(cross(bita, tang));
                }
                else
                {
                    float3 tang = normalize(float3(0.0, grad, 1.0));
                    float3 bita = float3(1.0, 0.0, 0.0);
                    newNormal   = normalize(cross(tang, bita));
                }

                VertexPositionInputs posInputs = GetVertexPositionInputs(pos);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(newNormal, IN.tangentOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = nrmInputs.normalWS;
                OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);

                return OUT;
            }

            half4 frag(Varyings IN, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);
                if (!isFrontFace) normalWS = -normalWS;

                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                Light  mainLight = GetMainLight();
                float  NdotL     = saturate(dot(normalWS, mainLight.direction));

                float3 ambient  = SampleSH(normalWS) * _AmbientStrength;
                float3 lighting = mainLight.color * NdotL + ambient;

                return half4(texColor.rgb * lighting, texColor.a);
            }

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            // Se o objeto for totalmente transparente, pode não fazer sentido projetar sombras opacas.
            // Podes deixar isto como está para ter sombras, mas o ZWrite tem de estar ligado apenas no ShadowCaster.
            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float  _PeakHeight;
                float  _BendAxis;
                float  _AmbientStrength;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings ShadowVert(Attributes IN)
            {
                Varyings OUT;
                float3 pos     = IN.positionOS.xyz;
                float  useV    = step(0.5, _BendAxis);
                float  uvCoord = lerp(IN.uv.x, IN.uv.y, useV);
                pos.y += sin(uvCoord * PI) * _PeakHeight;
                OUT.positionCS = TransformObjectToHClip(pos);
                return OUT;
            }

            half4 ShadowFrag(Varyings IN) : SV_Target { return 0; }

            ENDHLSL
        }
    }
}