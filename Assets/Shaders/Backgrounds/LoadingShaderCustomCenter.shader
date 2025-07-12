Shader "URP/Custom/LoadingShaderCustomCenter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // Center point control
        _CenterPoint ("Center Point", Vector) = (0.5, 0.5, 0, 0)
        
        // Scale controls
        _Scale1 ("Scale 1 (X, Y, Z)", Vector) = (15, 0.4, 0.975, 0)
        _Scale2 ("Scale 2 (X, Y, Z)", Vector) = (25, 0.8, 0.5, 0)
        _Scale3 ("Scale 3 (X, Y, Z)", Vector) = (75, 3.2, 0.8, 0)
        
        // Color controls
        _Color1 ("Color 1", Color) = (1, 0.67256093, 0, 1)
        _Color2 ("Color 2", Color) = (1, 0.7411765, 0, 1)
        _Color3 ("Color 3", Color) = (1, 0.7411765, 0, 1)
        _BaseColor ("Base Color", Color) = (1, 0.8078432, 0, 1)
        
        // Time multiplier
        _TimeMultiplier ("Time Multiplier", Float) = 1.0
        
        // Alpha control
        _Alpha ("Alpha", Range(0, 1)) = 1.0
        
        // Additional controls
        _WaveIntensity ("Wave Intensity", Range(0.1, 5.0)) = 1.0
        _RadialScale ("Radial Scale", Range(0.1, 2.0)) = 1.0
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
            Name "LoadingEffect"
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
            
            float4 _CenterPoint;
            float3 _Scale1;
            float3 _Scale2;
            float3 _Scale3;
            
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float4 _BaseColor;
            
            float _TimeMultiplier;
            float _Alpha;
            float _WaveIntensity;
            float _RadialScale;
            
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
                
                float2 iResolution = _ScreenParams.xy;
                float iTime = _Time.y * _TimeMultiplier;
                
                // Convert center point from [0,1] to screen coordinates
                float2 centerScreen = _CenterPoint.xy * iResolution.xy;
                
                // Calculate distance from custom center point
                float2 fromCenter = fragCoord - centerScreen;
                float pos = length(fromCenter / iResolution.yy) * _RadialScale;
                
                // Wave calculations with intensity control
                float f1 = sin(pos * _Scale1.x - iTime * _Scale1.y) * _WaveIntensity;
                float f2 = sin(pos * _Scale2.x - iTime * _Scale2.y) * _WaveIntensity;
                float f3 = sin(pos * _Scale3.x - iTime * _Scale3.y) * _WaveIntensity;
                
                // Color selection based on wave values
                float3 col = _BaseColor.rgb;
                
                if (f1 > _Scale1.z) {
                    col = _Color1.rgb;
                }              
                else if (f2 > _Scale2.z) {
                    col = _Color2.rgb;
                }
                else if (f3 > _Scale3.z) {
                    col = _Color3.rgb;
                }
                
                return float4(col, _Alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Forward"
}