Shader "URP/Custom/AnimatedDiagonalStripes"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // Cores
        _Color1 ("Cor dos Traços", Color) = (0.2, 0.5, 0.4, 1.0)
        _Color2 ("Cor de Fundo", Color) = (0.1, 0.3, 0.25, 1.0)
        
        // Controles do padrão
        _StripeWidth ("Largura dos Traços", Range(0.1, 2.0)) = 0.5
        _StripeSpacing ("Espaçamento entre Traços", Range(0.5, 5.0)) = 2.0
        _MoveSpeed ("Velocidade do Movimento", Range(0, 5.0)) = 1.5
        _MoveDirection ("Direção (0=Diagonal, 1=Horizontal, 2=Vertical)", Range(0, 2)) = 0
        
        // Pontos de origem e destino
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
            Name "AnimatedDiagonalStripes"
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
            
            float4 _Color1;
            float4 _Color2;
            float _StripeWidth;
            float _StripeSpacing;
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
                // Sample da textura base para obter alpha
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                // Se o pixel é transparente na textura, descarta
                if (texColor.a < 0.01)
                    discard;
                
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float2 fragCoord = screenUV * _ScreenParams.xy;
                float2 resolution = _ScreenParams.xy;
                
                // Normalizar coordenadas para [0, 1]
                float2 uv = fragCoord / resolution;
                
                // Corrigir aspect ratio
                float aspectRatio = resolution.x / resolution.y;
                uv.x *= aspectRatio;
                
                float time = _Time.y * _MoveSpeed;
                
                // Calcular direção do movimento com base nos parâmetros
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
                float continuousOffset = 0;
                
                if (_OscillateMovement > 0.5)
                {
                    // Movimento oscilatório: interpola entre StartPoint e EndPoint com sin
                    float oscillation = sin(time) * 0.5 + 0.5;
                    float2 displacement = lerp(_StartPoint.xy, _EndPoint.xy, oscillation);
                    animatedUV = uv + displacement;
                }
                else
                {
                    // Movimento contínuo: offset entra diretamente no frac do padrão,
                    // evitando o hard reset que acontecia com frac(time) no displacement.
                    // O frac do stripePattern já é periódico, por isso um offset linear
                    // infinito move os traços suavemente sem qualquer salto visível.
                    continuousOffset = time * (dir.x + dir.y);
                }
                
                // Criar traços diagonais com offset contínuo embutido
                float diagonal = animatedUV.x + animatedUV.y + continuousOffset;
                float stripePattern = frac(diagonal * _StripeSpacing);
                
                // Determinar se estamos dentro ou fora do traço
                float stripe = step(stripePattern, _StripeWidth / _StripeSpacing);
                
                // Aplicar suavidade nas bordas
                if (_SoftEdges > 0.001)
                {
                    stripe = smoothstep(_StripeWidth / _StripeSpacing + _SoftEdges, 
                                       _StripeWidth / _StripeSpacing - _SoftEdges, 
                                       stripePattern);
                }
                
                // Interpolar entre as cores
                float4 finalColor = lerp(_Color2, _Color1, stripe);
                
                // Aplicar contraste
                finalColor.rgb = pow(finalColor.rgb, _Contrast);
                
                // Aplicar transparência combinada (shader + textura)
                finalColor.a *= _Alpha * texColor.a;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Forward"
}
