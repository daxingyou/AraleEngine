Shader "TestShader/Shader5"
{
	Properties{
		_Color("MainColor", Color) = (1,0.5,0.5,0.5)
	}

	SubShader{
		Tags { "Queue" = "Transparent"}
		Lighting On
		BlendOp Min
		Pass{
			Material{
				Diffuse[_Color]
			}
		}
	}
}
