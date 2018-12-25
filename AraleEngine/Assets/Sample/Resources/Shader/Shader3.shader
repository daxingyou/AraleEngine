Shader "TestShader/Shader3"
{
	Properties{
		_Color("MainColor",Color)=(1,1,1,0.5)
		_MainTex("Texture",2D) = "white"{}
	}
	SubShader{
		Pass{
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

			//
			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct v2f {
				float4 pos : SV_POSITION;
				fixed3 color : COLOR0;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos=mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.normal * 0.5 + 0.5;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 clr = tex2D(_MainTex, i.uv);
				return clr * fixed4(i.color,1);
			}
			//

			ENDCG
		}
	}
	FallBack "VertexLit"
}
