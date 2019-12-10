﻿//场景技能指示器
Shader "Custom/Indicator"
{
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_HalfWidth("HalfWidth", Float) = 1
		_HalfHieght("HalfHeight", Float) = 1
		_Ang("Fan Ang", Float) = 60
	}
	 
	Category {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }//"IgnoreProjector"="True"总显示在最上面
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off 
		Lighting Off 
		ZWrite On 
		
		BindChannels {
			Bind "Color", color
			Bind "Vertex", vertex
		}
		
		SubShader {
			Pass {
			//ZTest Always//显示在上面
				CGPROGRAM   
					#pragma vertex vert  
					#pragma fragment frag
					
					#include "UnityCG.cginc"
					#define PI 3.1415926
					float4 _Color;
					float _HalfWidth;
					float _HalfHieght;
					float _Ang;

					struct v2f {   
						float4 pos : SV_POSITION; 
						float2 uv1 : TEXCOORD0;
					}; 
					
					v2f vert(appdata_tan v)   
					{   
						v2f o;   
						o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
						o.uv1.x =  v.vertex.x;
						o.uv1.y =  v.vertex.z;
						return o;    
					}   


					float smoothAlpha(float k)
					{
						return 1-abs(k-0.5f)/0.5; 
					}

					float getFanAlpha(float2 uv)
					{
						float d = sqrt(uv.x*uv.x + uv.y*uv.y);
						float w = 0.2f;
						float r = _HalfWidth;
						float begin = 0.5*PI-radians(_Ang/2);
						float end = 0.5*PI+radians(_Ang/2);
						if(d>r)return 0;
						d=abs(d-r);
						float rad = 0;
						if(uv.x==0)
						{
							rad =  uv.y>0?0.5*PI:1.5*PI;
						}
						else
						{
							rad = atan(uv.y/uv.x);
							if(uv.x<0)rad+=PI;
						}
						if(rad<begin||rad>end)return 0;//角度内显示
						float x0y = abs(sin(-begin)*uv.x+cos(-begin)*uv.y);
						float x1y = abs(sin(-end)*uv.x+cos(-end)*uv.y);
						if(x0y<2*w || x1y<2*w)
						{
							float ad = x0y<x1y?x0y:x1y;
							if(d>ad)d=ad;
						}
						if(d>w)return 0.2;
						return smoothAlpha(d/w)+0.2f;
					}

					float getCircleAlpha(float2 uv)
					{
						float d = sqrt(uv.x*uv.x + uv.y*uv.y);
						float w = 0.2f;
						float r = _HalfWidth;
						if(d>r)return 0;
						d=abs(d-r);
						if(d>w)return 0.2;
						return smoothAlpha(d/w)+0.2f;
					}

					float getRectAlpha(float2 uv)
					{
						float w = 0.2f;
						float dw = _HalfWidth - abs(uv.x);
						if(dw<0)return 0;
						float dy = _HalfHieght- abs(uv.y);
						if(dy<0)return 0;
						float d = dw<dy?dw:dy;
						if(d>w)return 0.2;
						return smoothAlpha(d/w)+0.2f;
					}

					half4 frag (v2f i) : COLOR   
					{
						half4 result = _Color;
						result.a = _Ang>0?(_Ang>=360?getCircleAlpha(i.uv1):getFanAlpha(i.uv1)):getRectAlpha(i.uv1);
						if(result.a<=0)discard;
						return result; 
					} 
				ENDCG 
			}
			//Pass {
			//ZTest Greater//当被遮挡时该pass起效
			//	CGPROGRAM   
			//		#pragma vertex vert  
			//		#pragma fragment frag
					
			//		#include "UnityCG.cginc"   
					
			//		struct v2f {   
			//			float4 pos : SV_POSITION;   
			//			float2 uv : TEXCOORD0;   
			//		};   
			//		v2f vert(appdata_tan v)   
			//		{   
			//			v2f o;   
			//			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);  
			//			o.uv = v.texcoord; 
			//			return o;    
			//		}
			//		
			//		sampler2D _MainTex;  
			//		float4 _Color; 
	    	//
			//		half4 frag (v2f i) : SV_Target   
			//		{   
			//			half4 result = tex2D (_MainTex, i.uv); 
			//			result.rgb=1;
			//			result*=_Color;
			//			result.a /= 3;
			//			return result; 
			//		}    
			//	ENDCG 
			//}
		}
	}
}