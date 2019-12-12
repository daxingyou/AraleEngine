//很据环境手动调光，减少光线计算带来的性能损失,绑定在角色或动态物体上
Shader "Arale/Model/ModelLight" {
	Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Color ("Main Color", Color) = (0.6392,0.6039,0.6039,1)
	_LightColor("Light Color" , Color) = (0.1843,0.1725,0.1686,1)
	_LightDir("Light Direction" , Vector) = (1.01,2.33,-0.07,0)
	_SpecularColor("Specular Color" , Color) = (0.302,0.302,0.302,1)
	_Warp("Warp lighting", float) = -0.02
	_Ambient("Ambient Light", float) = 0.82
	_Shininess ("Shininess", float) = 8.21
    _Cutoff("Cutoff", float) = 0.5
	}
	SubShader {	
		Tags { "Queue" = "Geometry-500" "RenderType" = "Opaque"}
		cull off
		CGPROGRAM
		#pragma surface surf SimpleLambert alphatest:_Cutoff 

	fixed4 _SpecularColor;
	fixed4 _LightColor;
	half4 _LightDir;
	half _Warp;
    float _Shininess;
	
    half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
    {
//		half3 h = normalize(lightDir + viewDir);
//		half NdotL = max(0, dot(s.Normal, lightDir));
		half3 h = normalize(_LightDir + viewDir);
		half NdotL = max(0, dot(s.Normal, _LightDir));
		half diff = saturate(NdotL + _Warp) / (1 + _Warp);
		
		half4 c;
		c.rgb = (s.Albedo * _LightColor.rgb * diff) * (atten * 2);
		c.rgb += _SpecularColor.rgb * s.Specular * atten;
	    c.a = s.Alpha;

	    return c;
    }

     struct Input
     {
         half2 uv_MainTex;
         float3 viewDir;
         float3 worldNormal;
     };
          
     sampler2D _MainTex;
     float4 _Color;
     float _Ambient;

     void surf (Input IN, inout SurfaceOutput o) 
     {
     	 fixed4 texColor = tex2D(_MainTex , IN.uv_MainTex) * _Color;
         o.Albedo = texColor.rgb;
         o.Emission = texColor.rgb * _Ambient;
         half3 reflection = normalize(reflect(_LightDir.xyz, IN.worldNormal));
		 o.Specular = saturate(dot(reflection, normalize(-IN.viewDir)));
		 o.Specular = pow(o.Specular, _Shininess);         
         o.Alpha = texColor.a;
     }
      ENDCG
    }
	FallBack "Diffuse"
}

