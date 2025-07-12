Shader "Custom/MapTileUnlit_URP"
{
    Properties
    {
        // --- Main Texture ---
        [Header(Main Texture)] [Space]
        _MainTex ("Tile Set (RGB)", 2D) = "white" {}
        _TileIndex ("Tile Index", Float) = 0
        _TileQuantity ("Tile Quantity", Float) = 15
        _Color ("Color", Color) = (1,1,1,1)
        
        // --- Overlay Effect ---
        [Header(Overlay Effect)] [Space]
        _OverlayColor ("Overlay Color", Color) = (0,0,0,0)
        _OverlayStrength ("Overlay Strength", Range(0, 1)) = 0

        [Header(Pulsing Circle Effect)] [Space]
        
        [Toggle]
        _EnablePulsingCircleEffect ("Enable Pulsing Circle Effect", Float) = 0
        
        // --- Circle Effect Settings ---
        _CircleTex ("Circle Texture", 2D) = "white" {}
        _IntensityMin ("Min Intensity", Range(0, 1)) = 0.3
        _IntensityMax ("Max Intensity", Range(0, 1)) = 1
    
        // --- Zoom and Speed Controls ---
        _ZoomScaleMax ("Max Zoom Scale", Range(1, 5)) = 1.5
        _ZoomSpeed ("Zoom Speed", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags 
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        LOD 100

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv         : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_CircleTex);
            SAMPLER(sampler_CircleTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _CircleTex_ST;
            float4 _Color;
            float4 _OverlayColor;
            float _OverlayStrength;
            float _TileIndex;
            float _TileQuantity;
            float _EnablePulsingCircleEffect;
            float _IntensityMin;
            float _IntensityMax;
            float _ZoomScaleMax;
            float _ZoomSpeed;
            CBUFFER_END

            float2 GetTileUV(float2 uv, float tileIndex, float tileQuantity)
            {
                float finalUVX = uv.x / tileQuantity;
                finalUVX += (1.0 / tileQuantity) * tileIndex;
                return float2(finalUVX, uv.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // --- Tile Texture ---
                float2 tileUV = GetTileUV(input.uv, _TileIndex, _TileQuantity);
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, tileUV);
                c *= _Color;

                // --- Circle Effect Calculation ---
                if (_EnablePulsingCircleEffect > 0.5)
                {
                    // --- Oscillation Calculation ---
                    float oscillation = (sin(_TimeParameters.y * _ZoomSpeed) + 1.0) * 0.5;

                    // --- Zoom and Intensity ---
                    float zoom = lerp(1.0, _ZoomScaleMax, oscillation);
                    float intensity = lerp(_IntensityMin, _IntensityMax, oscillation);

                    float2 center = float2(0.5, 0.5);
                    float2 circleUV = (input.uv - center) * zoom + center;

                    half4 circleColor = SAMPLE_TEXTURE2D(_CircleTex, sampler_CircleTex, circleUV);
                    circleColor.rgb *= _Color.rgb;

                    c.rgb = lerp(c.rgb, circleColor.rgb, circleColor.a * intensity);
                }

                // --- Overlay Effect ---
                // Usa additive blending com clamp para manter cores vibrantes
                c.rgb = saturate(c.rgb + (_OverlayColor.rgb * _OverlayStrength));
                
                return c;
            }

            ENDHLSL
        }
    }
}