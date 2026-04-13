Shader "Custom/UI/DitherFilter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Dither Settings)]
        _DitherScale ("Dither Pixel Scale", Range(1, 8)) = 1.0
        _ColorSteps ("Color Steps (Lower = crunchier)", Range(2, 16)) = 4.0
        _DitherSpread ("Dither Intensity", Range(0, 1)) = 0.5

        // Required properties for Unity UI Masking to work
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        // Standard UI Blending and Stencil setup
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _DitherScale;
            float _ColorSteps;
            float _DitherSpread;

            // 4x4 Bayer Matrix calculation
            float GetBayer4x4(uint2 screenPixelPos)
            {
                // Mathematical representation of a 4x4 Bayer matrix
                const float4x4 bayer = float4x4(
                    0.0, 8.0, 2.0, 10.0,
                    12.0, 4.0, 14.0, 6.0,
                    3.0, 11.0, 1.0, 9.0,
                    15.0, 7.0, 13.0, 5.0
                );

                int x = screenPixelPos.x % 4;
                int y = screenPixelPos.y % 4;
                
                // Return normalized value between -0.5 and 0.5
                return (bayer[y][x] / 16.0) - 0.5; 
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                
                // Calculate screen position for pixel-perfect dithering
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 1. Sample the base UI Texture
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;

                // 2. Get exact screen pixel coordinates
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                uint2 pixelPos = uint2(screenUV.x * _ScreenParams.x, screenUV.y * _ScreenParams.y);
                
                // Scale the pixels if you want a chunkier retro look
                pixelPos = pixelPos / max(1.0, _DitherScale);

                // 3. Get the dither value for this specific pixel
                float ditherValue = GetBayer4x4(pixelPos) * _DitherSpread;

                // 4. Apply dither and posterize (crunch the colors)
                col.rgb = col.rgb + ditherValue;
                col.rgb = floor(col.rgb * _ColorSteps + 0.5) / _ColorSteps;

                return col;
            }
            ENDCG
        }
    }
}