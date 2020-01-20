//后期处理:彩色渐变灰度图，模拟角色死亡，画面灰化
Shader "Arale/Image/ImageGray"
{
	Properties
	{
		_MainTex ("Base", 2D) = "white" {}
		_Progress ("Animat Progress", Float) = 0
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
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _Progress;
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float3 grey = Luminance(col.rgb);//和下面灰度算法一致
				//float grey = dot(col.rgb, float3(0.299,0.587,0.114));
				col.rgb = col.rgb+_Progress*(grey-col.rgb);
				return col;
			}
			ENDCG
		}
	}
}
