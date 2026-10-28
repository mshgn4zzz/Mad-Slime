Shader "MadSlime/FillCubeUnlit"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Glossiness ("Highlight Smoothness", Range(0, 1)) = 0.86
        _HighlightStrength ("Soft Highlight", Range(0, 0.5)) = 0.18
        _ShadeStrength ("Shape Shading", Range(0, 0.5)) = 0.18
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Tags { "LightMode" = "Always" }
            ZWrite On
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            fixed4 _Color;
            half _Glossiness;
            half _HighlightStrength;
            half _ShadeStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                half3 viewNormal : TEXCOORD0;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                float3 worldNormal = UnityObjectToWorldNormal(input.normal);
                output.viewNormal = mul((float3x3)UNITY_MATRIX_V, worldNormal);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                half3 normal = normalize(input.viewNormal);

                // A camera-relative studio finish, independent of scene lights,
                // ambient lighting, shadows and reflection probes.
                half3 studioDirection = normalize(half3(-0.35h, 0.55h, 0.76h));
                half facing = saturate(dot(normal, studioDirection));
                half shade = 1.0h - _ShadeStrength * (1.0h - facing);
                half exponent = lerp(8.0h, 48.0h, _Glossiness);
                half highlight = _HighlightStrength * pow(facing, exponent);

                // Blend towards white instead of adding HDR energy, so even a
                // bright scene cannot blow out the cube's picture colors.
                half3 color = lerp(_Color.rgb * shade, half3(1.0h, 1.0h, 1.0h), highlight);
                return fixed4(color, 1.0h);
            }
            ENDCG
        }
    }

    Fallback Off
}
