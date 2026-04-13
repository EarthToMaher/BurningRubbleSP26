Shader "Custom/PostProcess/TextureDitherFilter"
{
    Properties
    {
        [MainTexture] _BlitTexture("Source", 2D) = "white" {}
        _DitherTex ("Bayer Matrix Texture", 2D) = "white" {}
        
        [Header(Resolution)]
        _DitherScale ("Dither Pixel Scale", Range(1, 8)) = 1.0
        
        [Header(Color Depth)]
        _ColorSteps ("Color Steps", Range(2, 64)) = 8.0
        _DitherStrength ("Dither Strength", Range(0, 2)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Textures
            TEXTURE2D(_DitherTex);
            SAMPLER(sampler_DitherTex);
            float4 _DitherTex_TexelSize; // Auto-filled by Unity (x=1/width, y=1/height, z=width, w=height)

            float _DitherScale;
            float _ColorSteps;
            float _DitherStrength;

            half4 frag (Varyings input) : SV_Target
            {
                // 1. Sample the original screen color
                half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);

                // 2. Calculate Dither Coordinates
                // We use screen space pixels divided by the scale to keep the pattern "chunky"
                float2 screenPos = input.texcoord * _ScreenParams.xy;
                float2 ditherUV = screenPos / (_DitherTex_TexelSize.zw * _DitherScale);

                // 3. Sample the Bayer Texture
                // We use 'sampler_DitherTex' which should be set to 'Repeat' and 'Point' in the inspector
                float ditherValue = SAMPLE_TEXTURE2D(_DitherTex, sampler_DitherTex, ditherUV).r;
                
                // Shift dither from [0,1] to centered [-0.5, 0.5]
                ditherValue -= 0.5;

                // 4. Apply Dither to Color
                // The strength is scaled by the number of color steps to prevent over-brightening
                col.rgb += ditherValue * (_DitherStrength / _ColorSteps);

                // 5. Posterize
                // Quantize the color into discrete steps
                col.rgb = floor(col.rgb * _ColorSteps) / max(1.0, _ColorSteps - 1.0);

                return half4(col.rgb, 1.0);
            }
            ENDHLSL
        }
    }
}