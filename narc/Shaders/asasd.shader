Shader "Transparent/Cutout/Transparent" { 
	Properties { 
		_Color ("Main Color", Color) = (1,1,1,1) 
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {} 
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5 
		_Transpa ("Transparency", Range(0,1)) = 0.75
	}

	SubShader 
	{ 
		Tags 
		{
			"Queue"="OverLay" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent"
		} 
	
		LOD 200


		CGPROGRAM 
		#pragma surface surf Lambert alpha

		sampler2D _MainTex; 
		fixed4 _Color; 
		float _Cutoff;
		float _Transpa;

		struct Input { float2 uv_MainTex; };

		void surf (Input IN, inout SurfaceOutput o) 
		{ 
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color; 
			float ca = c.a; 
			o.Albedo = c.rgb;
			if (ca > _Cutoff)
				o.Alpha = c.a*_Transpa;
			else
				o.Alpha = 0;
		} 
		ENDCG 
	}

	Fallback Off
}