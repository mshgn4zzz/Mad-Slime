//
//  OutlineMask.shader
//  QuickOutline
//
//  Created by Chris Nolet on 2/21/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//

Shader "Custom/Outline Mask" {
  Properties {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
  }

  SubShader {
    Tags {
      "Queue" = "Transparent+100"
      "RenderType" = "Transparent"
    }

    Pass {
      Name "Mask"
      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      ColorMask 0

      Stencil {
        Ref 1
        Pass Replace
      }

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_instancing
      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f
      {
        float4 position : SV_POSITION;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      v2f vert(appdata input)
      {
        v2f output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.position = UnityObjectToClipPos(input.vertex);
        return output;
      }

      fixed4 frag(v2f input) : SV_Target
      {
        return 0;
      }
      ENDCG
    }
  }
}
