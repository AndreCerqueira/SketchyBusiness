Shader "URP/Custom/AnimatedTextureScroll"
{
    Properties
    {
        _MainTex ("Textura", 2D) = "white" {}
        
        // Controles do movimento
        _ScrollSpeedX ("Velocidade X", Range(-5.0, 5.0)) = 1.0
        _ScrollSpeedY ("Velocidade Y", Range(-5.0, 5.0)) = 0.0
        
        // Escala da textura
        _Scale ("Escala da Textura", Range(0.1, 10.0)) = 1.0
        
        // Controles visuais
        _Alpha ("Transparência", Range(0, 1)) = 1.0
        _Tint ("Cor de Matiz", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            Name "AnimatedTextureScroll"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            
            float _ScrollSpeedX;
            float _ScrollSpeedY;
            float _Scale;
            float _Alpha;
            float4 _Tint;
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float4 positionWS = TransformObjectToHClip(IN.positionOS);
                OUT.positionHCS = positionWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.screenPos = ComputeScreenPos(positionWS);
                return OUT;
            }
            
            float4 frag(Varyings IN) : SV_Target
            {
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float2 fragCoord = screenUV * _ScreenParams.xy;
                float2 resolution = _ScreenParams.xy;
                
                // Normalizar coordenadas para [0, 1]
                float2 uv = fragCoord / resolution;
                
                // Corrigir aspect ratio
                float aspectRatio = resolution.x / resolution.y;
                uv.x *= aspectRatio;
                
                // Aplicar escala
                uv *= _Scale;
                
                // Calcular tempo
                float time = _Time.y;
                
                // Aplicar scroll contínuo
                float2 scrollOffset = float2(_ScrollSpeedX * time, _ScrollSpeedY * time);
                
                // Aplicar o offset usando frac para fazer loop contínuo
                float2 scrolledUV = frac(uv + scrollOffset);
                
                // Amostrar a textura
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, scrolledUV);
                
                // Aplicar tint
                texColor.rgb *= _Tint.rgb;
                
                // Aplicar transparência
                texColor.a *= _Alpha;
                
                return texColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Forward"
}