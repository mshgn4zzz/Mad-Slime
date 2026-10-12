Shader "MadSlime/PlayerSquash"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _GlossMapScale ("Smoothness Scale", Range(0,1)) = 1
        _MetallicGlossMap ("Metallic", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1
        _OcclusionMap ("Occlusion", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Strength", Range(0,1)) = 1
        [HDR] _EmissionColor ("Emission", Color) = (0,0,0,0)
        _EmissionMap ("Emission", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _SquashCompression ("Compression", Range(0,1)) = 0
        _SquashBottom ("Bottom", Float) = 0
        _SquashHeight ("Height", Float) = 1
        _SquashPinnedFraction ("Pinned Bottom", Range(0,0.5)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "DisableBatching"="True" }
        LOD 200
        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf Standard fullforwardshadows vertex:DeformVertex addshadow
        #pragma shader_feature_local _NORMALMAP
        #pragma shader_feature_local _METALLICGLOSSMAP
        #pragma shader_feature_local _EMISSION
        #pragma shader_feature_local _ALPHATEST_ON
        #include "UnityCG.cginc"
        #include "UnityStandardUtils.cginc"
        sampler2D _MainTex;
        sampler2D _MetallicGlossMap;
        sampler2D _BumpMap;
        sampler2D _OcclusionMap;
        sampler2D _EmissionMap;
        fixed4 _Color;
        half _Metallic;
        half _Glossiness;
        half _GlossMapScale;
        half _BumpScale;
        half _OcclusionStrength;
        fixed4 _EmissionColor;
        half _Cutoff;
        float _SquashCompression;
        float _SquashBottom;
        float _SquashHeight;
        float _SquashPinnedFraction;
        float4x4 _SquashToModel;
        float4x4 _SquashToObject;
        float4x4 _SquashNormalToModel;
        float4x4 _SquashNormalToObject;
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        };
        void DeformVertex(inout appdata_full vertex)
        {
            const float MinimumSize = 0.0001;
            const float MinimumSlope = 0.05;
            float4 position = mul(_SquashToModel, vertex.vertex);
            float height = max(_SquashHeight, MinimumSize);
            float normalizedHeight = (position.y - _SquashBottom) / height;
            float movableRange = max(1.0 - _SquashPinnedFraction, MinimumSize);
            float progress = saturate((normalizedHeight - _SquashPinnedFraction) / movableRange);
            float influence = progress * progress * (3.0 - 2.0 * progress);
            float derivative = 6.0 * progress * (1.0 - progress) / movableRange;
            float slope = max(MinimumSlope, 1.0 - _SquashCompression * derivative);
            position.y -= _SquashCompression * height * influence;
            vertex.vertex = mul(_SquashToObject, position);
            float3 normal = mul((float3x3)_SquashNormalToModel, vertex.normal);
            normal.y /= slope;
            vertex.normal = normalize(mul((float3x3)_SquashNormalToObject, normal));
            float3 tangent = mul((float3x3)_SquashToModel, vertex.tangent.xyz);
            tangent.y *= slope;
            vertex.tangent.xyz = normalize(mul((float3x3)_SquashToObject, tangent));
        }
        void surf(Input input, inout SurfaceOutputStandard output)
        {
            fixed4 color = tex2D(_MainTex, input.uv_MainTex) * _Color;
            output.Albedo = color.rgb;
            output.Alpha = color.a;
            output.Metallic = _Metallic;
            output.Smoothness = _Glossiness;
            #ifdef _METALLICGLOSSMAP
            fixed4 metallic = tex2D(_MetallicGlossMap, input.uv_MainTex);
            output.Metallic = metallic.r;
            output.Smoothness = metallic.a * _GlossMapScale;
            #endif
            #ifdef _NORMALMAP
            output.Normal = UnpackScaleNormal(tex2D(_BumpMap, input.uv_BumpMap), _BumpScale);
            #endif
            output.Occlusion = lerp(1.0, tex2D(_OcclusionMap, input.uv_MainTex).g, _OcclusionStrength);
            #ifdef _EMISSION
            output.Emission = tex2D(_EmissionMap, input.uv_MainTex).rgb * _EmissionColor.rgb;
            #endif
            #ifdef _ALPHATEST_ON
            clip(color.a - _Cutoff);
            #endif
        }
        ENDCG
    }
    Fallback "Diffuse"
}
