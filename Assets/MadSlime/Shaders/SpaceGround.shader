Shader "MadSlime/SpaceGround"
{
    Properties
    {
        _MainTex ("Quiet stars and nebula", 2D) = "black" {}
        _TileMeters ("Background repeat in metres", Float) = 64
        _Saturation ("Background saturation", Range(0,1)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _TileMeters, _Saturation;
            struct v2f { float4 position : SV_POSITION; float2 worldXZ : TEXCOORD0; };
            v2f vert(appdata_base input)
            {
                v2f output;
                output.position=UnityObjectToClipPos(input.vertex);
                output.worldXZ=mul(unity_ObjectToWorld,input.vertex).xz;
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                float2 uv=input.worldXZ/max(_TileMeters,0.01);
                uv=1.0-abs(frac(uv*0.5)*2.0-1.0);
                fixed3 color=tex2D(_MainTex,uv).rgb;
                float luminance=dot(color,float3(0.299,0.587,0.114));
                return fixed4(lerp(luminance.xxx,color,_Saturation),1);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
}
