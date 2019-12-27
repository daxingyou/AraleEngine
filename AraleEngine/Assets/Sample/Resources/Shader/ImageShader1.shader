// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TestShader/Image-水幕特效"
{
	Properties
	{
		_MainTex ("Base", 2D) = "white" {}
		_ScreenWaterTex("ScreenWaterTex", 2D)="white"{}
		_CurTime("Time", Range(0.0, 1.0)) = 1.0
		_SizeX("SizeX", Range(0.0, 1.0)) = 1.0
		_SizeY("SizeY", Range(0.0, 1.0)) = 1.0
		_DropSpeed("Speed", Range(0.0, 1.0)) = 1.0
		_Distortion("Distortion", Range(0.0, 1.0)) = 0.87
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

			sampler2D _MainTex;
			sampler2D _ScreenWaterTex;
			float _CurTime;
			float _DropSpeed;
			float _SizeX;
			float _SizeY;
			float _Distortion;
			float2 _MainTex_TexelSize;

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

			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv.xy;
				#if UNITY_UV_STARTS_AT_TOP
				if(_MainTex_TexelSize.y < 0)
					_DropSpeed = -_DropSpeed;
				#endif
				float3 rainTex1 = tex2D(_ScreenWaterTex, float2(uv.x*_SizeX*1.15, uv.y*_SizeY*1.1+_CurTime*_DropSpeed*0.15)).rgb / _Distortion;
				float3 rainTex2 = tex2D(_ScreenWaterTex, float2(uv.x*_SizeX*1.25, uv.y*_SizeY*1.2+_CurTime*_DropSpeed*0.2)).rgb / _Distortion;
				float3 rainTex3 = tex2D(_ScreenWaterTex, float2(uv.x*_SizeX*0.9, uv.y*_SizeY*1.25+_CurTime*_DropSpeed*0.032)).rgb / _Distortion;
				float2 finalRainTex = uv.xy - (rainTex1.xy - rainTex2.xy - rainTex3.xy) / 24;
				float3  finalColor = tex2D(_MainTex, float2(finalRainTex.x, finalRainTex.y)).rgb;
				return fixed4(finalColor, 1.0);
			}
			ENDCG
		}
	}
}
