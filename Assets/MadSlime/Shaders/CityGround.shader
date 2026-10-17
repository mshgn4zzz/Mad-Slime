Shader "MadSlime/CityGround"
{
    Properties
    {
        _MainTex ("Paths and grass", 2D) = "white" {}
        _TileMeters ("Town block repeat in metres", Float) = 48
        _Saturation ("Ground saturation", Range(0,1)) = 0.6
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        float _TileMeters, _Saturation;
        struct Input { float3 worldPos; };
        void surf(Input input, inout SurfaceOutputStandard output)
        {
            float2 uv = input.worldPos.xz / max(_TileMeters,0.01);
            uv = 1.0 - abs(frac(uv * 0.5) * 2.0 - 1.0);
            fixed3 color = tex2D(_MainTex,uv).rgb;
            float luminance = dot(color,float3(0.299,0.587,0.114));
            output.Albedo = lerp(luminance.xxx,color,_Saturation);
            output.Metallic = 0;
            output.Smoothness = 0.025;
            output.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
