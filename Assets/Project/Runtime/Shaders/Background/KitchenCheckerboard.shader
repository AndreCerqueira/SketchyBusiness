Shader "URP/Custom/AnimatedCheckerboard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // Cores do axadrezado
        _Color1 ("Cor Clara", Color) = (1.0, 1.0, 1.0, 1.0)
        _Color2 ("Cor Escura", Color) = (0.0, 0.0, 0.0, 1.0)
        
        // Controles do padrão
        _CheckerSize ("Tamanho dos Quadrados", Range(2, 50)) = 16
        _MoveSpeed ("Velocidade do Movimento", Range(0.1, 5.0)) = 1.5
        _MoveDirection ("Direção (0=Diagonal, 1=Horizontal, 2=Vertical)", Range(0, 2)) = 0
        
        // Pontos de origem e destino (0 a 1)
        _StartPoint ("Ponto de Início", Vector) = (1, 1, 0, 0)
        _EndPoint ("Ponto Final", Vector) = (0, 0, 0, 0)
        
        // Controles visuais
        _Alpha ("Transparência", Range(0, 1)) = 1.0
        _Contrast ("Contraste", Range(0.1, 3.0)) = 1.0
        _Smoothness ("Suavidade das Bordas", Range(0.0, 0.5)) = 0.1
        
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
            Name "AnimatedCheckerboard"
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
            float _CheckerSize;
            float _MoveSpeed;
            float _MoveDirection;
            float4 _StartPoint;
            float4 _EndPoint;
            float _Alpha;
            float _Contrast;
            float _Smoothness;
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
                
                // Aplicar aspect ratio para manter quadrados perfeitos
                float aspectRatio = resolution.x / resolution.y;
                uv.x *= aspectRatio;
                
                // Rodar UV 45 graus para quadrados inclinados
                float2 center = float2(0.5, 0.5);
                float2 uvCentered = uv - center;
                float angle = radians(45.0);
                float cos45 = cos(angle);
                float sin45 = sin(angle);
                float2 uvRotated = float2(
                    uvCentered.x * cos45 - uvCentered.y * sin45,
                    uvCentered.x * sin45 + uvCentered.y * cos45
                );
                uv = uvRotated + center;
                
                float time = _Time.y * _MoveSpeed;
                
                // Calcular direção do movimento
                float2 dir = _EndPoint.xy - _StartPoint.xy;
                
                if (_MoveDirection > 0.5 && _MoveDirection < 1.5)
                {
                    // Movimento apenas horizontal
                    dir.y = 0;
                }
                else if (_MoveDirection > 1.5)
                {
                    // Movimento apenas vertical
                    dir.x = 0;
                }
                
                float2 animatedUV = uv;
                
                if (_OscillateMovement > 0.5)
                {
                    // Movimento oscilatório (vai e volta) — sem reset, sin é contínuo por natureza
                    float oscillation = sin(time) * 0.5 + 0.5;
                    float2 displacement = lerp(_StartPoint.xy, _EndPoint.xy, oscillation);
                    animatedUV = uv + displacement;
                }
                else
                {
                    // Movimento contínuo: o offset linear entra diretamente no UV antes do floor.
                    // O padrão checker é periódico em cada quadrado, por isso crescer o offset
                    // infinitamente move os quadrados suavemente sem qualquer hard reset.
                    animatedUV = uv + dir * time;
                }
                
                // Criar padrão axadrezado
                float2 checkerUV = animatedUV * _CheckerSize;
                float2 checkerID = floor(checkerUV);
                
                // Determinar se estamos numa casa clara ou escura
                float checker = frac((checkerID.x + checkerID.y) * 0.5) * 2.0;
                
                // Aplicar suavidade nas bordas se desejado
                if (_Smoothness > 0.001)
                {
                    float2 checkerFrac = frac(checkerUV);
                    float2 edgeDist = min(checkerFrac, 1.0 - checkerFrac);
                    float edgeSmooth = smoothstep(0.0, _Smoothness, min(edgeDist.x, edgeDist.y));
                    checker = lerp(0.5, checker, edgeSmooth);
                }
                
                // Interpolar entre as cores
                float4 finalColor = lerp(_Color2, _Color1, step(0.5, checker));
                
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
