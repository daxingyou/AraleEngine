Shader "Shader/RimHighLight" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_OtherColor ("Other Color", Color) = (1,1,1,1)
	_RimColor ("Rim Color", Color) = (1,1,1,1)
     _RimPower ("Power", Range(0.1,5)) = 3.0
     _OuterThick ("_OuterThick", Range(0,1)) = 0
     _MaxPower("_MaxPower", Range(1,5)) = 1
     _InnerThick ("_InnerThick", Range(-5,5)) = 0
	_MainTex ("Base (RGB) ", 2D) = "white" {}
	_Illum("SpecularIllumOther(RGB)", 2D) = "white" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }
	CGPROGRAM
	#pragma surface surf Lambert noforwardadd nolightmap exclude_path:prepass 
	#pragma only_renderers gles d3d9 opengl Metal
	fixed4 _Color;
	fixed4 _OtherColor;
	fixed4 _RimColor;
	fixed _RimPower;
	fixed _OuterThick;
	fixed _MaxPower;
	fixed _InnerThick;
	sampler2D _MainTex;
	sampler2D _Illum;


	struct Input {
		half2 uv_MainTex;
		float3 viewDir;
	};

	void surf (Input IN, inout SurfaceOutput o) {
		fixed3 tex = tex2D(_MainTex, IN.uv_MainTex);
		fixed3 si= tex2D (_Illum, IN.uv_MainTex);
		fixed3 c = tex * si.r * _Color + tex * (1-si.r) * _OtherColor;
		
		o.Albedo = c.rgb;
	 	half rim = 1.0 - dot (normalize(IN.viewDir), o.Normal);

		half di=rim+_OuterThick;
		if(di>_MaxPower)
			di=_MaxPower;
		o.Emission = _RimColor.rgb *( pow (di,_RimPower)+_InnerThick)+ c.rgb * si.r;

	}
	ENDCG
	}
FallBack "Legacy Shaders/Self-Illumin/VertexLit"
CustomEditor "LegacyIlluminShaderGUI"
}
