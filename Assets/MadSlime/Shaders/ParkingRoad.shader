Shader "MadSlime/ParkingRoad"
{
    Properties
    {
        _MainTex ("Cartoon asphalt", 2D) = "white" {}
        _TileMeters ("Asphalt repeat in metres", Float) = 24
        _MarkingMeters ("Parking rows repeat in metres", Float) = 40
        _PaintColor ("Road paint", Color) = (0.91,0.91,0.84,1)
        _PaintStrength ("Paint contrast", Range(0,1)) = 0.48
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        float _TileMeters, _MarkingMeters, _PaintStrength;
        fixed4 _PaintColor;
        struct Input { float3 worldPos; };
        float band(float distance, float halfWidth)
        {
            return 1.0 - smoothstep(halfWidth,halfWidth + max(fwidth(distance),0.015),abs(distance));
        }
        void surf(Input input, inout SurfaceOutputStandard output)
        {
            float2 uv = input.worldPos.xz / max(_TileMeters,0.01);
            uv = 1.0 - abs(frac(uv * 0.5) * 2.0 - 1.0);
            fixed3 asphalt = tex2D(_MainTex,uv).rgb;
            float2 p = (frac(input.worldPos.xz / max(_MarkingMeters,1) + 0.5) - 0.5) * _MarkingMeters;
            // Sparse rows of bays leave a broad open driving lane between them.
            float row = band(abs(p.y)-12,0.07) * (1.0-smoothstep(17.4,17.6,abs(p.x)));
            float bayDistance = (frac(p.x/7.0+0.5)-0.5)*7.0;
            float dividers = band(bayDistance,0.07) * smoothstep(11.9,12.1,abs(p.y))
                * (1.0-smoothstep(18.9,19.1,abs(p.y))) * (1.0-smoothstep(17.4,17.6,abs(p.x)));
            float2 a = p-float2(8,0);
            float shaft = (1.0-smoothstep(0.14,0.19,abs(a.x))) * (1.0-smoothstep(0.85,0.92,abs(a.y)));
            float head = (1.0-smoothstep((2.0-a.y)*0.65,(2.0-a.y)*0.65+0.06,abs(a.x)))
                * smoothstep(0.65,0.72,a.y) * (1.0-smoothstep(1.96,2.02,a.y));
            float paint = saturate(max(max(row,dividers),max(shaft,head)));
            output.Albedo = lerp(asphalt,_PaintColor.rgb,paint*_PaintStrength);
            output.Metallic = 0;
            output.Smoothness = 0.03;
            output.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
