Shader "Custom/UI/DeathFog"
{
    Properties
    {
        // Obrigatório para UI shaders — a Unity mete aqui a textura do sprite (pode ser null/white)
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Intensity          ("Intensity",           Range(0, 1))    = 0.0
        _FogColor           ("Fog Color",            Color)          = (0.18, 0.0, 0.32, 1.0)
        _VignetteStrength   ("Vignette Strength",    Range(1, 10))   = 5.0
        _VignetteSoftness   ("Vignette Softness",    Range(0.1, 1))  = 0.45
        _PulseSpeed         ("Pulse Speed",          Range(0, 5))    = 1.2
        _PulseAmount        ("Pulse Amount",         Range(0, 0.15)) = 0.06
        _FogDensity         ("Fog Density",          Range(0, 1))    = 0.75
        _NoiseScale         ("Noise Scale",          Range(1, 20))   = 5.0
        _NoiseSpeed         ("Noise Speed",          Range(0, 2))    = 0.25
        _Desaturation       ("Desaturation",         Range(0, 1))    = 0.0
    }

    SubShader
    {
        // Tags standard de UI
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        // Blend normal de UI (respeita alpha)
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex       : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float2 screenUV     : TEXCOORD1;  // UV normalizado 0-1 baseado no rect da Image
                float4 color        : COLOR;
                float4 worldPos     : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float  _Intensity;
            float4 _FogColor;
            float  _VignetteStrength;
            float  _VignetteSoftness;
            float  _PulseSpeed;
            float  _PulseAmount;
            float  _FogDensity;
            float  _NoiseScale;
            float  _NoiseSpeed;
            float  _Desaturation;

            // Cliprect para masking de UI (ScrollView, etc.)
            float4 _ClipRect;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex    = UnityObjectToClipPos(v.vertex);
                o.uv        = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenUV  = v.uv;   // 0-1 dentro do rect da Image
                o.color     = v.color;
                o.worldPos  = v.vertex;
                return o;
            }

            // --- Noise ---
            float2 hash2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            float gnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(dot(hash2(i + float2(0,0)), f - float2(0,0)),
                         dot(hash2(i + float2(1,0)), f - float2(1,0)), u.x),
                    lerp(dot(hash2(i + float2(0,1)), f - float2(0,1)),
                         dot(hash2(i + float2(1,1)), f - float2(1,1)), u.x),
                    u.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                float f = 1.0;
                for (int i = 0; i < 4; i++)
                {
                    v += a * gnoise(p * f);
                    f *= 2.1;
                    a *= 0.5;
                }
                return v;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv       = i.screenUV;   // 0-1 dentro da Image
                float  time     = _Time.y;
                float  intensity = _Intensity;

                // --- Vignette ---
                float2 vigUV    = uv - 0.5;
                vigUV.x        *= 0.85;         // oval horizontal
                float  dist     = length(vigUV);
                float  pulse    = 1.0 + sin(time * _PulseSpeed) * _PulseAmount * intensity;
                float  radius   = lerp(0.65, 0.1, intensity);   // fecha ao centro com intensity
                float  vignette = smoothstep(radius * pulse, radius * pulse - _VignetteSoftness, dist);
                // vignette = 1 no centro, 0 nas bordas
                float  vigDark  = 1.0 - vignette;               // escuridão nas bordas

                // --- Fog Noise ---
                float2 noiseUV  = uv * _NoiseScale;
                float  noiseA   = fbm(noiseUV + float2(time * _NoiseSpeed, time * _NoiseSpeed * 0.7));
                float  noiseB   = fbm(noiseUV * 1.4 - float2(time * _NoiseSpeed * 0.6, 0));
                float  fog      = noiseA * 0.6 + noiseB * 0.4;
                fog             = fog * 0.5 + 0.5;              // remap [-1,1] → [0,1]

                // Fog mais denso nas bordas
                float fogMask   = lerp(fog * 0.3, fog, vigDark);
                fogMask         = saturate(fogMask * _FogDensity * intensity);

                // --- Alpha final ---
                // Bordas: alpha vem da escuridão da vignette + fog
                // Centro: transparente (vê-se o que está por baixo)
                float alpha = saturate(vigDark * intensity * 1.2 + fogMask);
                alpha       = pow(alpha, 1.0 / _VignetteStrength);  // controla o falloff

                // Cor do fog com variação de noise (bordas mais escuras)
                float3 fogCol   = _FogColor.rgb * lerp(0.3, 1.0, fog);
                // Pulse subtil na luminosidade
                fogCol         *= 1.0 + sin(time * _PulseSpeed * 0.7) * 0.05 * intensity;

                // Respeitar cliprect (ScrollView masking, etc.)
                alpha *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);

                // Multiplicar pelo alpha do vertex color (controlo via CanvasGroup, etc.)
                alpha *= i.color.a;

                return fixed4(fogCol, alpha * intensity);
            }
            ENDCG
        }
    }
}
