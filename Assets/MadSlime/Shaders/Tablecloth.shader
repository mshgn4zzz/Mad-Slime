Shader "MadSlime/Tablecloth"
{
    Properties
    {
        _Color ("Linen", Color) = (0.78,0.89,0.94,1)
        _GridColor ("Woven grid", Color) = (0.64,0.78,0.86,1)
        _CellMeters ("Square size in metres", Float) = 2.8
        _LineWidth ("Line width / square", Range(0.01,0.15)) = 0.0125
        _Glossiness ("Smoothness", Range(0,1)) = 0.08
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Color, _GridColor;
        float _CellMeters, _LineWidth, _Glossiness;
        struct Input { float3 worldPos; };
        void surf(Input input, inout SurfaceOutputStandard output)
        {
            float2 uv = input.worldPos.xz / max(_CellMeters,0.01);
            float2 footprint = max(fwidth(uv),0.0001);
            float2 distance = abs(frac(uv + 0.5) - 0.5);
            float2 lines = 1.0 - smoothstep(_LineWidth, _LineWidth + footprint, distance);
            // Fade to the average coverage when a square becomes smaller than one pixel.
            lines = lerp(lines, saturate(2.0 * _LineWidth), saturate(footprint));
            float grid = max(lines.x,lines.y);
            output.Albedo = lerp(_Color.rgb,_GridColor.rgb,grid);
            output.Metallic = 0;
            output.Smoothness = _Glossiness;
            output.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
