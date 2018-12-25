Shader "TestShader/自定义光照" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_SpecColor ("Color", Color) = (0.3,0.3,0.3,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf MyLightingName
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		fixed4 LightingMyLightingName(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{
			float3 N = normalize(s.Normal);
			float3 L = lightDir;
			float3 V = viewDir;
			float3 R = normalize(reflect(-L,N));
			float3 H = normalize(L+V);
			float NL = max(0, dot(N, L));
			float NH = max(0, dot(N, H));
			float VR = max(0, dot(viewDir,  R));
			float spec = pow(NH, 256*(_Glossiness+0.1));
			float diff = NL; 

			half4 c;
			c.rgb = (s.Albedo*_LightColor0.rgb*diff + _SpecColor.rgb*_LightColor0.rgb*spec)*(atten*2);
			c.a = s.Alpha+_LightColor0.a*_SpecColor.a*spec*atten;
			return c;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
