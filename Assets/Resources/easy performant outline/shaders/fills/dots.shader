Shader "Hidden/EPO/Fill/Basic/Dots" {
	Properties {
		_PublicColor ("Color", Vector) = (1,0,0,1)
		_PublicHorizontalSize ("Horizontal size", Float) = 1
		_PublicVerticalSize ("Vertical size", Float) = 1
		_PublicHorizontalGapSize ("Horizontal gap size", Range(-1, 1)) = 0.5
		_PublicVerticalGapSize ("Vertical gap size", Range(-1, 1)) = 0.5
		_PublicHorizontalSoftness ("Horizontal softness", Range(0, 1)) = 0.2
		_PublicVerticalSoftness ("Vertical softness", Range(0, 1)) = 0.2
		_PublicHorizontalSpeed ("Horizontal speed", Float) = 1
		_PublicVerticalSpeed ("Vertical speed", Float) = 1
		_PublicAngle ("Angle", Range(0, 360)) = 0
		_PublicSecondaryAlpha ("Secondary alpha", Range(0, 1)) = 0.2
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