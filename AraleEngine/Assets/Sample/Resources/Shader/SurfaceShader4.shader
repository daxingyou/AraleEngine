Shader "TestShader/细节贴图" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Detail ("Detail", 2D) = "gray" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Detail;

		struct Input {
			float2 uv_MainTex;
			float2 uv_Detail;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb*tex2D (_Detail, IN.uv_Detail).rgb*2;
		}
		ENDCG
	}
	FallBack "Diffuse"
}