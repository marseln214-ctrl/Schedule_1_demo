Shader "Hidden/EPO/Fill/Basic/Interlaced" {
	Properties {
		_PublicColor ("Color", Vector) = (1,0,0,1)
		_PublicGapColor ("Gap color", Vector) = (1,0,0,0.2)
		_PublicSize ("Size", Float) = 1
		_PublicGapSize ("Gap size", Range(-1, 1)) = 0.2
		_PublicSoftness ("Softness", Range(0, 3)) = 0.75
		_PublicSpeed ("Speed", Float) = 1
		_PublicAngle ("Angle", Range(0, 360)) = 0
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