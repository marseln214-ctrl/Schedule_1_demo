Shader "Shader Graphs/6D Lighting Shader URP" {
	Properties {
		[KeywordEnum(Diffuse, SixPoint, DiffuseSixPoint)] _RENDERTYPE ("Render Type", Float) = 2
		_Flipbook_Columns ("Flipbook Columns", Float) = 8
		_Flipbook_Rows ("Flipbook Rows", Float) = 8
		_Cutoff ("Alpha Clipping", Range(0, 1)) = 0.01
		_Lightness ("Lightness", Range(0, 3)) = 1
		_ShadowMultiplier ("6-Way Shadow Multiplier", Range(0, 3)) = 0.3
		_BaseColor ("Base Color Tint", Vector) = (1,1,1,1)
		_BaseMap ("Base Color Texture", 2D) = "white" {}
		[NoScaleOffset] _SixPointPOS ("Six Point TLR", 2D) = "white" {}
		[NoScaleOffset] _SixPointNEG ("Six Point BBF", 2D) = "white" {}
		_EmissionColor ("Emissive Color Tint", Vector) = (0,0,0,0)
		[NoScaleOffset] _EmissionMap ("Emission Map", 2D) = "white" {}
		_EmissionMultiplier ("Emissive Multiplier", Range(1, 10)) = 1
		[NoScaleOffset] _Motion_Vector_Texture ("Motion Vector Texture", 2D) = "white" {}
		_Motion_Vector_Influence ("Motion Vector Influence", Float) = 0.0008
		_DepthLenght ("Depth Fade Lenght", Range(0, 1)) = 0.01
		_Depth_Map_Multiplier ("Depth Map Multiplier", Float) = 1
		[ToggleUI] _Emissive_Highlights ("Emissive Highlights", Float) = 0
		[HDR] _Emissive_Highlights_Color ("Emissive Highlights Color", Vector) = (1,0.4303665,0.15,0)
		_Emissive_Highlights_Multiplier ("Emissive Highlights Multiplier", Range(0, 20)) = 1
		_Emissive_Highlights_Amount ("Emissive Highlights Amount", Range(0, 1)) = 0.2
		_Emissive_Highlights_Contrast ("Emissive Highlights Contrast", Range(1, 3)) = 1
		_Emissive_Highlights_Softness ("Emissive Highlights Softness", Range(0, 3)) = 1
		[ToggleUI] _Customize_Color ("Customize Color", Float) = 0
		[ToggleUI] _Has_Emission_Map ("Has Emission Map", Float) = 1
		_Hue ("Hue", Range(0, 360)) = 0
		[HDR] _Fire_Color ("Fire Color", Vector) = (4.237095,0.5399282,0,0)
		[HDR] _Highlights_Color ("Highlights Color", Vector) = (16,3.419466,0.3350782,0)
		_Highlights_Contrast ("Highlights Contrast", Range(0, 10)) = 2
		_Smoke_Color ("Smoke Color", Vector) = (0.3735848,0.3735848,0.3735848,0)
		_Smoke_Color_2 ("Smoke Color 2", Vector) = (0.8113208,0.8113208,0.8113208,0)
		_Smoke_Contrast ("Smoke Contrast", Range(0, 5)) = 1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
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
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}