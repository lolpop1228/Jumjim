Shader "Custom/RetroDuskQuantized"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _LightSteps ("Light Steps", Range(2,8)) = 4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert

        sampler2D _MainTex;
        fixed4 _Color;
        float _LightSteps;

        struct Input
        {
            float2 uv_MainTex;
        };

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.uv_MainTex = v.texcoord.xy;
        }

        half QuantizeLight(half lightIntensity, half steps)
        {
            return floor(lightIntensity * steps) / steps;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Get Lambert diffuse lighting (dot product of normal and light dir)
            half NdotL = max(0, dot(o.Normal, _WorldSpaceLightPos0.xyz));

            // Quantize lighting into steps for retro look
            NdotL = QuantizeLight(NdotL, _LightSteps);

            o.Albedo = c.rgb * NdotL;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
