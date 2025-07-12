Shader "URP/Custom/RadialRaysCustomOrigin"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _RotationSpeed("Rotation Speed", Float) = 1.0
        _RayColor("Ray Color", Color) = (0.2, 0.8, 0.8, 0.8)
        _BackgroundColor("Background Color", Color) = (0.1, 0.6, 0.6, 1.0)
        _OriginPoint("Origin Point", Vector) = (0.5, 0.5, 0, 0)
        _CircleRadius("Circle Radius", Range(0.01, 0.5)) = 0.15
        _RayCount("Ray Count", Range(4, 32)) = 16
        _RayWidth("Ray Width", Range(0.1, 0.8)) = 0.4
        _FadeDistance("Fade Distance", Range(0.1, 2.0)) = 1.0
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
            Name "RadialRays"
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

            float _RotationSpeed;
            float4 _RayColor;
            float4 _BackgroundColor;
            float4 _OriginPoint;
            float _CircleRadius;
            float _RayCount;
            float _RayWidth;
            float _FadeDistance;

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
                
                // Normalizar coordenadas para [-1, 1] baseado no menor lado da tela
                float2 uv = (fragCoord - resolution * 0.5) / min(resolution.x, resolution.y);
                
                // Converter o ponto de origem de [0,1] para coordenadas normalizadas [-1, 1]
                float2 originNormalized = (_OriginPoint.xy - 0.5) * 2.0;
                
                // Calcular a distância e ângulo relativos ao ponto de origem
                float2 fromOrigin = uv - originNormalized;
                float dist = length(fromOrigin);
                float angle = atan2(fromOrigin.y, fromOrigin.x);
                
                // Adicionar rotação
                angle += _Time.y * _RotationSpeed;
                
                // Calcular os raios
                float normalizedAngle = (angle + PI) / TWO_PI;
                float raySegment = normalizedAngle * _RayCount;
                float rayMask = step(frac(raySegment), _RayWidth);
                
                // Máscara do círculo central
                float centerMask = step(dist, _CircleRadius);
                
                // Fator de fade baseado na distância
                float fadeFactor = 1.0 - smoothstep(_CircleRadius + 0.05, _FadeDistance, dist);
                
                // Cor final
                float4 centerColor = _RayColor;
                float4 finalColor;
                
                if (centerMask > 0.5)
                {
                    // Dentro do círculo central
                    finalColor = centerColor;
                }
                else
                {
                    // Fora do círculo - aplicar raios com fade
                    finalColor = lerp(_BackgroundColor, _RayColor, rayMask * fadeFactor);
                }
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Forward"
}