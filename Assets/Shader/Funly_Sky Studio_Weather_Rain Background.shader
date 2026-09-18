Shader "Funly/Sky Studio/Weather/Rain Background" {
	Properties {
		_NearTex ("Near Rain Texture", 2D) = "black" {}
		_NearRainSpeed ("Near Rain Speed", Range(0, 3)) = 0.5
		_NearDensity ("Near Rain Density", Range(0, 1)) = 0.7
		_FarTex ("Far Rain Texture", 2D) = "black" {}
		_FarRainSpeed ("Far Rain Speed", Range(0, 3)) = 0.75
		_FarDensity ("Far Rain Density", Range(0, 1)) = 0.5
		_TintColor ("Tint Color", Vector) = (1,1,1,1)
		_Turbulence ("Turbulence", Range(0, 1)) = 0
		_TurbulenceSpeed ("Turbulence Speed", Range(0, 5)) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
}