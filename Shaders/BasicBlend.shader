Shader "Unlit/BasicBlend"
{
	Properties {
		_ColorBase ("ColorBase", Color) = (1, 1, 1, 1)
		_ColorTip ("ColorTip", Color) = (1, 1, 1, 1)
	}

  SubShader {
    Tags {"Queue"="Transparent" "RenderType"="Transparent" }
    Blend SrcAlpha OneMinusSrcAlpha


		Pass {
			CGPROGRAM

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "UnityCG.cginc"

			float4 _ColorBase;
			float4 _ColorTip;

			struct Interpolators {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			struct VertexData {
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			Interpolators MyVertexProgram (VertexData v) {
				Interpolators i;
				i.position = UnityObjectToClipPos(v.position);
				i.uv = v.uv;
				return i;
			}

			float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
				return _ColorBase * (1 - i.uv.y) + _ColorTip * i.uv.y;
			}

			ENDCG

		}
	}
}
