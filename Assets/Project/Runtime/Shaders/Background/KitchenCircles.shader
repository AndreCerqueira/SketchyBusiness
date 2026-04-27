Shader "URP/Custom/AnimatedCircles"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // Cores
        _CircleColor ("Cor dos Círculos", Color) = (1.0, 1.0, 1.0, 1.0)
        _BackgroundColor ("Cor de Fundo", Color) = (0.0, 0.0, 0.0, 1.0)
        
        // Controles do padrão
        _CircleSize ("Tamanho dos Círculos", Range(0.1, 2.0)) = 0.4
        _CircleSpacing ("Espaçamento entre Círculos", Range(0.5, 5.0)) = 2.0
        _MoveSpeed ("Velocidade do Movimento", Range(0.1, 5.0)) = 1.5
        _MoveDirection ("Direção (0=Diagonal, 1=Horizontal, 2=Vertical)", Range(0, 2)) = 0
        
        // Pontos de origem e destino (0 a 1)
        _StartPoint ("Ponto de Início", Vector) = (1, 1, 0, 0)
        _EndPoint ("Ponto Final", Vector) = (0, 0, 0, 0)
        
        // Controles visuais
        _Alpha ("Transparência", Range(0, 1)) = 1.0
        _Contrast ("Contraste", Range(0.1, 3.0)) = 1.0
        _SoftEdges ("Suavidade das Bordas", Range(0.0, 0.5)) = 0.1
        
        // Animação oscilatória
        _OscillateMovement ("Movimento Oscilatório", Range(0, 1)) = 0
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
            Name "AnimatedCircles"
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
            
            float4 _CircleColor;
            float4 _BackgroundColor;
            float _CircleSize;
            float _CircleSpacing;
            float _MoveSpeed;
            float _MoveDirection;
            float4 _StartPoint;
            float4 _EndPoint;
            float _Alpha;
            float _Contrast;
            float _SoftEdges;
            float _OscillateMovement;
            
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
                
                float time = _Time.y * _MoveSpeed;
                
                // Calcular direção do movimento
                float2 dir = _EndPoint.xy - _StartPoint.xy;
                
                if (_MoveDirection > 0.5 && _MoveDirection < 1.5)
                {
                    // Apenas horizontal
                    dir.y = 0;
                }
                else if (_MoveDirection > 1.5)
                {
                    // Apenas vertical
                    dir.x = 0;
                }
                
                float2 animatedUV = uv;
                
                if (_OscillateMovement > 0.5)
                {
                    // Movimento oscilatório — sin é contínuo por natureza, sem reset
                    float oscillation = sin(time) * 0.5 + 0.5;
                    float2 displacement = lerp(_StartPoint.xy, _EndPoint.xy, oscillation);
                    animatedUV = uv + displacement;
                }
                else
                {
                    // Movimento contínuo: offset linear direto no UV antes do frac da grid.
                    // O frac do gridUV já é periódico por célula, por isso o offset pode
                    // crescer infinitamente e os círculos movem-se sem qualquer hard reset.
                    animatedUV = uv + dir * time;
                }
                
                // Grid de círculos
                float2 gridUV = frac(animatedUV * _CircleSpacing);
                float2 gridCenter = float2(0.5, 0.5);
                
                // Distância do pixel ao centro do círculo mais próximo
                float2 toCenter = gridUV - gridCenter;
                float distToCenter = length(toCenter);
                
                // Determinar se estamos dentro ou fora do círculo
                float circle = step(distToCenter, _CircleSize);
                
                // Aplicar suavidade nas bordas
                if (_SoftEdges > 0.001)
                {
                    circle = smoothstep(_CircleSize + _SoftEdges, _CircleSize - _SoftEdges, distToCenter);
                }
                
                // Interpolar entre as cores
                float4 finalColor = lerp(_BackgroundColor, _CircleColor, circle);
                
                // Aplicar contraste
                finalColor.rgb = pow(finalColor.rgb, _Contrast);
                
                // Aplicar transparência
                finalColor.a *= _Alpha;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Forward"
}
