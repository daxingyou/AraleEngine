Shader "TestShader/滤色变色" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ColorTint ("Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert finalcolor:setcolor
		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _ColorTint;

		struct Input {
			float2 uv_MainTex;
		};

		void setcolor(Input IN, SurfaceOutput o, inout fixed4 color)
		{
			color *= _ColorTint;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
