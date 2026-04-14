Shader "Hidden/FullScreen/ComicBookColor"
{
    Properties
    {
        _DitherScale ("Dither Scale (Pixel Size)", Range(1, 10)) = 2
        _DitherOpacity ("Dither Opacity", Range(0, 1)) = 0.7
        _EdgeThickness ("Ink Line Thickness", Range(0, 5)) = 1.0
        _EdgeThreshold ("Ink Line Threshold", Range(0, 1)) = 0.2
        _InkColor ("Ink Color", Color) = (0.05, 0.05, 0.05, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off ZTest Always

        Pass
        {
            Name "ComicBookColorPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _DitherScale;
            float _DitherOpacity;
            float _EdgeThickness;
            float _EdgeThreshold;
            float4 _InkColor;

            static const float ditherMatrix[16] = {
                0.0f,    0.5f,    0.125f,  0.625f,
                0.75f,   0.25f,   0.875f,  0.375f,
                0.1875f, 0.6875f, 0.0625f, 0.5625f,
                0.9375f, 0.4375f, 0.8125f, 0.3125f
            };

            float GetDitherValue(float2 screenPos)
            {
                int x = int(fmod(screenPos.x / _DitherScale, 4.0));
                int y = int(fmod(screenPos.y / _DitherScale, 4.0));
                return ditherMatrix[x + y * 4];
            }

            float GetLuminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float2 uv = input.texcoord;
                float3 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
                float luminance = GetLuminance(sceneColor);

                float2 screenPos = uv * _ScreenParams.xy;
                float dither = GetDitherValue(screenPos);
                
                float isDot = step(luminance, dither);
                
                float3 finalColor = lerp(sceneColor, _InkColor.rgb, isDot * _DitherOpacity);

                float2 texelSize = 1.0 / _ScreenParams.xy;
                float offset = _EdgeThickness;
                
                float lumaTop = GetLuminance(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, texelSize.y * offset)).rgb);
                float lumaBottom = GetLuminance(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, -texelSize.y * offset)).rgb);
                float lumaLeft = GetLuminance(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-texelSize.x * offset, 0)).rgb);
                float lumaRight = GetLuminance(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize.x * offset, 0)).rgb);

                float edgeX = lumaRight - lumaLeft;
                float edgeY = lumaTop - lumaBottom;
                float edge = sqrt(edgeX * edgeX + edgeY * edgeY);

                if (edge > _EdgeThreshold)
                {
                    finalColor = _InkColor.rgb; 
                }

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}