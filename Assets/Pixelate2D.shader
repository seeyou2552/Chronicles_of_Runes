Shader "Custom/Pixelate2D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size (Pixels)", Float) = 500
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PixelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = o.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 스크린 공간에서 픽셀 단위로 UV 계산
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                // _ScreenParams: (width, height, 1 + 1/width, 1 + 1/height)
                float2 resolution = _ScreenParams.xy;

                // 픽셀 크기에 맞춰 자르기
                float2 pixelSize = _PixelSize / resolution;
                float2 snappedUV = floor(i.uv / pixelSize) * pixelSize;

                fixed4 col = tex2D(_MainTex, snappedUV);
                return col;
            }
            ENDCG
        }
    }
}
