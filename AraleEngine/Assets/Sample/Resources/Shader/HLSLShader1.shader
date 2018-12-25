Shader "TestShader/多通道"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Outline("Outline width", float)=0.007
		_Factor("Factor", float)=1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			ZWrite Off
			ZTEST Less
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float _Factor;
			float _Outline;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD1;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = mul(UNITY_MATRIX_MVP, v.normal);
				float3 dir = normalize(v.vertex);
				float3 dir2= v.normal;
				float D = dot(dir, dir2);
				D=(D/_Factor+1)/(1+1/_Factor);
				dir = lerp(dir2,dir,D);
				dir = mul((float3x3)UNITY_MATRIX_IT_MV, dir);
				float2 offset=TransformViewToProjection(dir.xy);
				o.vertex.xy += offset*_Outline;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(0,0,0,1);
			}
			ENDCG
		}

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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = fixed4(0,1,0,1);//tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}

	Fallback "Diffuse"
}
