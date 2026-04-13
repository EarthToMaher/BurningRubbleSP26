Shader "Hidden/ComicPostProcess"
{
    Properties
    {
        _DitherScale ("Dither Scale", Float) = 4.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "ComicPass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // This library allows us to use SampleSceneColor
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _DitherScale;

            Varyings vert (Attributes input) {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag (Varyings input) : SV_Target {
                // 1. Grab the scene color using URP's built-in function
                float3 rawColor = SampleSceneColor(input.uv).rgb;
                
                // 2. Calculate brightness
                float brightness = dot(rawColor, float3(0.2126, 0.7152, 0.0722));

                // 3. Bayer Matrix Dithering
                // We use screen coordinates to keep the dots fixed to the glass
                float2 screenPos = input.uv * _ScreenParams.xy / _DitherScale;
                int x = (int)fmod(screenPos.x, 4);
                int y = (int)fmod(screenPos.y, 4);
                
                float bayer[4][4] = {
                    {0.0,    0.5,    0.125,  0.625},
                    {0.75,   0.25,   0.875,  0.375},
                    {0.1875, 0.6875, 0.0625, 0.5625},
                    {0.9375, 0.4375, 0.8125, 0.3125}
                };

                // 4. Final Color Logic
                float threshold = bayer[x][y];
                
                // If the pixel is dark, give it a "Comic Ink" tint (Dark Navy)
                float3 inkColor = float3(0.02, 0.02, 0.08); 
                
                // If brightness is higher than dither threshold, show original color
                float3 finalColor = (brightness > threshold) ? rawColor : inkColor;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}