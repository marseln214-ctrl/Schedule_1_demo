Shader "Funly/Sky Studio/Sprite Sheets/Sprite Sheet Instanced" {
	Properties {
		[NoScaleOffset] _SpriteSheetTex ("Sprite Texture", 2D) = "black" {}
		_SpriteColumnCount ("Sprite Columns", Float) = 1
		_SpriteRowCount ("Sprite Rows", Float) = 1
		_SpriteItemCount ("Sprite Total Items", Float) = 1
		_AnimationSpeed ("Animation Speed", Float) = 25
		_Intensity ("Intensity", Range(0, 1)) = 0.7
		_TintColor ("Tint Color", Vector) = (1,1,1,1)
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
	Fallback "Unlit/Color"
}