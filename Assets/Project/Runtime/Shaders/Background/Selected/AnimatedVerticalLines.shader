Shader "URP/Custom/AnimatedVerticalLines"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // Cores
        _Color1 ("Cor das Linhas", Color) = (0.2, 0.5, 0.4, 1.0)
        _Color2 ("Cor de Fundo", Color) = (0.1, 0.3, 0.25, 1.0)
        
        // Controles do padrão
        _LineWidth ("Largura das Linhas", Range(0.1, 6.0)) = 0.5
        _LineSpacing ("Espaçamento entre Linhas", Range(0.5, 15.0)) = 2.0
        _MoveSpeed ("Velocidade do Movimento", Range(0, 5.0)) = 1.5
        _MoveDirection ("Direção (0=Cima, 1=Baixo)", Range(0, 1)) = 0
        
        // Controles visuais
        _Alpha ("Transparência", Range(0, 1)) = 1.0
        _Contrast ("Contraste", Range(0.1, 3.0)) = 1.0
        _SoftEdges ("Suavidade das Bordas", Range(0.0, 0.5)) = 0.1
        
        // Animação oscilatória
        _OscillateMovement ("Movimento Oscilatório", Range(0, 1)) = 0
        _OscillationAmplitude ("Amplitude da Oscilação", Range(0.1, 2.0)) = 1.0
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
            Name "AnimatedVerticalLines"
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
            float _LineWidth;
            float _LineSpacing;
            float _MoveSpeed;
            float _MoveDirection;
            float _Alpha;
            float _Contrast;
            float _SoftEdges;
            float _OscillateMovement;
            float _OscillationAmplitude;
            
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
                
                float time = _Time.y * _MoveSpeed;
                
                // Calcular deslocamento vertical
                float displacement = 0.0;
                
                if (_OscillateMovement > 0.5)
                {
                    // Movimento oscilatório
                    displacement = sin(time) * _OscillationAmplitude;
                }
                else
                {
                    // Movimento contínuo
                    float direction = _MoveDirection > 0.5 ? -1.0 : 1.0;
                    displacement = frac(time) * direction;
                }
                
                // Aplicar deslocamento na coordenada Y
                float animatedY = uv.y + displacement;
                
                // Criar linhas horizontais (que se movem verticalmente)
                float linePattern = frac(animatedY * _LineSpacing);
                
                // Determinar se estamos dentro ou fora da linha
                float lineValue = step(linePattern, _LineWidth / _LineSpacing);
                
                // Aplicar suavidade nas bordas
                if (_SoftEdges > 0.001)
                {
                    lineValue = smoothstep(_LineWidth / _LineSpacing + _SoftEdges, 
                                     _LineWidth / _LineSpacing - _SoftEdges, 
                                     linePattern);
                }
                
                // Interpolar entre as cores
                float4 finalColor = lerp(_Color2, _Color1, lineValue);
                
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