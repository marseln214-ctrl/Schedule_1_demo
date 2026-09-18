Shader "Hidden/EPO/Fill/Basic/Fresnel" {
	Properties {
		_PublicOuterColor ("Outer color", Vector) = (1,0,0,1)
		_PublicInnerColor ("Inner color", Vector) = (0,1,0,1)
		_PublicFresnelPower ("Fresnel power", Float) = 2
		_PublicFresnelMultiplier ("Fresnel multipler", Float) = 1
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