Shader "TestShader/自定义光照"
{
	Properties
	{
		_Color("Diffuse",Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Specular("Specular",Color) = (1,1,1,1)
		_Shininess("Shininess",Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"}
		LOD 100

		Pass
		{
			Tags {"LightMode"="Vertex"}
			CGPROGRAM
			//===============
			#define SD_PIXEL_LIGHT 1
			//===============
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

#ifdef SD_VERTEX_LIGHT
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};
#elif SD_PIXEL_LIGHT
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 color : COLOR;
				float3 normal : TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				float3 lightDir : TEXCOORD3;
				float3 viewDir : TEXCOORD4;
			};
#else
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
#endif

			//======================
			float4 _Color;
			sampler2D _MainTex;
			float4 _Specular;
			float _Shininess;
			//======================
			float4 _MainTex_ST;
			//======================
			fixed3 VertexLights(float4 v, float3 n)
			{
				fixed3 c = float3(0,0,0);
				for(int i=0;i<8;++i)
				{
					float4 lightPos  = unity_LightPosition[i];
					half4  lightColor= unity_LightColor[i];
					if(lightColor.a<=0.1f)break;
					//float4 lightPos  = _WorldSpaceLightPos0;
					//half4 lightColor = _LightColor0;
					float3 ambientLighting = float3(UNITY_LIGHTMODEL_AMBIENT.rgb) * float3(_Color.rgb);
					fixed3 worldNormal = normalize(mul(n, (float3x3)_World2Object));
					fixed3 worldLightDir = normalize(lightPos.xyz);
					float3 diffuseLighting = lightColor.rgb*_Color.rgb*saturate(dot(worldNormal,worldLightDir));
					fixed3 reflectDir = normalize(reflect(-worldLightDir,worldNormal));
					fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(_Object2World,v).xyz);
					float k = _Shininess;
					fixed3 specularLight = lightColor.rgb*_Specular.rgb*pow(saturate(dot(reflectDir,viewDir)),k);
					fixed3 PhongLight = ambientLighting + diffuseLighting + specularLight;
					c += PhongLight;
				}
				return c;
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
#ifdef SD_VERTEX_LIGHT
				o.color = float4(VertexLights(v.vertex, v.normal), _Color.a);
				//o.color = float4(ShadeVertexLights(v.vertex, v.normal), _Color.a);
#elif SD_PIXEL_LIGHT
				o.color = float3(UNITY_LIGHTMODEL_AMBIENT.rgb) * float3(_Color.rgb);
				o.normal = normalize(mul(v.normal, (float3x3)_World2Object));
				o.worldPos = mul(v.vertex, _Object2World);
				o.lightDir = WorldSpaceLightDir(v.vertex);
				o.viewDir  = WorldSpaceViewDir(v.vertex);
#endif
				return o;
			}


#ifdef SD_VERTEX_LIGHT
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color+_Color*tex2D(_MainTex, i.uv);
			}
#elif SD_PIXEL_LIGHT
			fixed4 frag(v2f i) : SV_Target
			{
				float3 normalDir = normalize(i.normal);
				fixed3 viewDir = normalize(i.viewDir);
				float3 lightDir;
				float  attenuation;
				if(0.0 == _WorldSpaceLightPos0.w)//directional light
				{
					attenuation = 1.0;
					lightDir = normalize(_WorldSpaceLightPos0.xyz);
					//return fixed4(1,0,0,1);
				}
				else//point or spot light
				{
					lightDir = normalize(i.lightDir);
					float3 distance = length(_WorldSpaceLightPos0.xyz - i.worldPos);
					attenuation = 1.0 / distance;
					lightDir = normalize(i.lightDir);
					//return fixed4(0,1,0,1);
				}

				float3 diffuseLight = _LightColor0.rgb*_Color.rgb* saturate(dot(normalDir,lightDir));
				fixed3 reflectDir = normalize(reflect(-lightDir,normalDir));
				fixed3 specularLight = _LightColor0.rgb*_Specular.rgb*pow(1.01*saturate(dot(reflectDir,viewDir)),_Shininess);
				return fixed4(diffuseLight+specularLight,1);
			}
#else
			fixed4 frag (v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv);
			}
#endif
			ENDCG
		}
	}
	Fallback "Specular"
}
