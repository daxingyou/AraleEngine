Shader "Shader/Self-Illumin/Diffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Illum ("Illumin (A)", 2D) = "white" {}
	_Emission ("Emission (Lightmapper)", Float) = 1.0
	_Factor ("offset factor", Range(-1,1)) = 1
	_Units ("offset units", Range(-1,1)) = 1
}
SubShader {
	Tags { "RenderType"="Opaque" }
	offset 0,-20	
CGPROGRAM
#pragma surface surf Lambert nolightmap exclude_path:forward //noforwardadd nolightmap exclude_path:prepass 



sampler2D _MainTex;
sampler2D _Illum;
fixed4 _Color;
fixed _Emission;

struct Input {
	float2 uv_MainTex;
	float2 uv_Illum;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 c = tex * _Color;
	o.Albedo = c.rgb;
	o.Emission = c.rgb * tex2D(_Illum, IN.uv_Illum).a;
#if defined (UNITY_PASS_META)
	o.Emission *= _Emission.rrr;
#endif
	o.Alpha = c.a;
}
ENDCG
} 
FallBack "Legacy Shaders/Self-Illumin/VertexLit"
CustomEditor "LegacyIlluminShaderGUI"
}
