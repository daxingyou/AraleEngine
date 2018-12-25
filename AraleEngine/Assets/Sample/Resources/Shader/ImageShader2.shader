Shader "TestShader/Image-径向模糊"
{
	Properties
	{
		_MainTex ("Base", 2D) = "white" {}
		_IterationNumber("迭代次数",Int)=16
		_CenterX("CenterX", Range(0, 1))=0.5
		_CenterY("CenterY", Range(0, 1))=0.5
		_Value("Value", Float)=1.0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			int _IterationNumber;
			float _Value;
			float _CenterX;
			float _CenterY;
			float4 c = float4(0,0,0,0);
			fixed4 frag (v2f i) : SV_Target
			{
				float2 center = float2(_CenterX, _CenterY);
				float2 diff = i.uv - center;
				float scale = 1;
				_Value*=0.085;
				for(int j=1;j<_IterationNumber;++j)
				{
					c += tex2D(_MainTex, diff*scale + center);
					scale = 1 + (float(j*_Value));
				}
				c /= _IterationNumber;
				return c;
			}
			ENDCG
		}
	}
}
