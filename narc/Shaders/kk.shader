Shader "pink"
{
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Pass{ Color(1,0,1,1) }
	}
	FallBack "Specular"
}