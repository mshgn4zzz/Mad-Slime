Shader "MadSlime/TableWood"
{
    Properties
    {
        _MainTex ("Cartoon wood albedo", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _TileMeters ("Tile size in metres", Float) = 18
        _Glossiness ("Smoothness", Range(0,1)) = 0.04
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        fixed4 _Color;
        float _TileMeters, _Glossiness;
        struct Input { float3 worldPos; };
        void surf(Input input, inout SurfaceOutputStandard output)
        {
            float2 uv = input.worldPos.xz / max(_TileMeters,0.01);
            // Mirror each repeat to keep the generated grain continuous at tile edges.
            uv = 1.0 - abs(frac(uv * 0.5) * 2.0 - 1.0);
            output.Albedo = tex2D(_MainTex,uv).rgb * _Color.rgb;
            output.Metallic = 0;
            output.Smoothness = _Glossiness;
            output.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
