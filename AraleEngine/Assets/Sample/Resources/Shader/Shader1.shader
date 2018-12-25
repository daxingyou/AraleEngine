Shader "TestShader/Shader1"
{
	Properties{
		_Color("MainColor", Color) = (1,0.5,0.5,1)
	}

	SubShader{
		Pass{
			Material{
				Diffuse[_Color]
			}
			Lighting On
		}
	}
}
