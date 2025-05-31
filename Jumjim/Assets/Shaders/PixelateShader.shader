Shader "Custom/Pixelate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Vector) = (320, 180, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _PixelSize;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                uv = floor(uv * _PixelSize.xy) / _PixelSize.xy;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
