Shader "Flamingo/Diffuse Selective UV"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        [NoScaleOffset] _MainTex("Base (RGB)", 2D) = "white" {}
        [KeywordEnum(Zero, One, Two, Three, Four, Five)] _UV("UV Channel", Float) = 0
    }

    SubShader{
        Tags { "RenderType" = "Opaque" }
        LOD 150

        CGPROGRAM
        #pragma surface surf Lambert noforwardadd vertex:vert
        #pragma multi_compile _UV_ZERO _UV_ONE _UV_TWO _UV_THREE _UV_FOUR _UV_FIVE

        fixed4 _Color;
        sampler2D _MainTex;

        struct appdata
        {
            float4 vertex : POSITION;
            half3 normal : NORMAL;
#if _UV_ZERO
            half2 uv : TEXCOORD0;
#elif _UV_ONE
            half2 uv : TEXCOORD1;
#elif _UV_TWO
            half2 uv : TEXCOORD2;
#elif _UV_THREE
            half2 uv : TEXCOORD3;
#elif _UV_FOUR
            half2 uv : TEXCOORD4;
#else
            half2 uv : TEXCOORD5;
#endif
        };

        struct Input {
            float2 texCoords;
        };

        void vert(inout appdata v, out Input o) {
            o.texCoords = v.uv;
        }

        void surf(Input IN, inout SurfaceOutput o) {
            fixed4 c = tex2D(_MainTex, IN.texCoords) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }

    Fallback "Mobile/VertexLit"
}