Shader "Custom/MetricsShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Tiling("Tiling Factor", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard
        #pragma multi_compile_instancing

        sampler2D _MainTex;
        float _Tiling;

        struct Input
        {
            float3 worldPos;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Utilisation des coordonnées XY pour mapper la texture sur les murs
            float2 UV = IN.worldPos.xy * _Tiling;
            UV = frac(UV); // Assure un tiling correct

            fixed4 texColor = tex2D(_MainTex, UV);
            o.Albedo = texColor.rgb;
        }
        ENDCG
    }

    Fallback "Standard"
}