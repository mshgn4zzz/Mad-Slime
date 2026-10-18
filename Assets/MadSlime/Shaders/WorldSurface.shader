Shader "MadSlime/WorldSurface"
{
    Properties
    {
        _BaseColor ("Base color", Color) = (0.3,0.3,0.3,1)
        _DetailColor ("Detail color", Color) = (0.4,0.4,0.4,1)
        _LineColor ("Panel seams", Color) = (0.08,0.12,0.18,1)
        _TileMeters ("Detail size", Float) = 12
        _PanelStrength ("Panel strength", Range(0,1)) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _BaseColor, _DetailColor, _LineColor;
        float _TileMeters, _PanelStrength, _Glossiness;
        struct Input
        {
            float3 worldPos;
        };

        float Hash(float3 position)
        {
            position = frac(position * 0.1031);
            position += dot(position, position.yzx + 33.33);
            return frac((position.x + position.y) * position.z);
        }

        void surf(Input input, inout SurfaceOutputStandard output)
        {
            float3 coordinates = input.worldPos / max(_TileMeters, 0.01);
            float broadNoise = Hash(floor(coordinates * 4));
            float fineNoise = Hash(floor(coordinates * 120));
            float detail = saturate(broadNoise * 0.45 + fineNoise * 0.55);
            fixed3 color = lerp(_BaseColor.rgb, _DetailColor.rgb, detail);
            float2 panelPosition = frac(coordinates.xz);
            float seamDistance = min(min(panelPosition.x, 1 - panelPosition.x), min(panelPosition.y, 1 - panelPosition.y));
            float seam = 1 - smoothstep(0.008, 0.025, seamDistance);
            color = lerp(color, _LineColor.rgb, seam * _PanelStrength);
            output.Albedo = color;
            output.Smoothness = _Glossiness;
            output.Metallic = _PanelStrength * 0.45;
            output.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
