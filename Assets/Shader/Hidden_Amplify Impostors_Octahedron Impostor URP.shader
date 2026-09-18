Shader "Hidden/Amplify Impostors/Octahedron Impostor URP" {
	Properties {
		[NoScaleOffset] _Albedo ("Albedo & Alpha", 2D) = "white" {}
		[NoScaleOffset] _Normals ("Normals & Depth", 2D) = "white" {}
		[NoScaleOffset] _Specular ("Specular & Smoothness", 2D) = "black" {}
		[NoScaleOffset] _Emission ("Emission & Occlusion", 2D) = "black" {}
		[HideInInspector] _Frames ("Frames", Float) = 16
		[HideInInspector] _ImpostorSize ("Impostor Size", Float) = 1
		[HideInInspector] _Offset ("Offset", Vector) = (0,0,0,0)
		[HideInInspector] _AI_SizeOffset ("Size & Offset", Vector) = (0,0,0,0)
		_TextureBias ("Texture Bias", Float) = -1
		_Parallax ("Parallax", Range(-1, 1)) = 1
		[HideInInspector] _DepthSize ("DepthSize", Float) = 1
		_ClipMask ("Clip", Range(0, 1)) = 0.5
		_AI_ShadowBias ("Shadow Bias", Range(0, 2)) = 0.25
		_AI_ShadowView ("Shadow View", Range(0, 1)) = 1
		[Toggle(_HEMI_ON)] _Hemi ("Hemi", Float) = 0
		[Toggle(EFFECT_HUE_VARIATION)] _Hue ("Use SpeedTree Hue", Float) = 0
		_HueVariation ("Hue Variation", Vector) = (0,0,0,0)
		[Toggle] _AI_AlphaToCoverage ("Alpha To Coverage", Float) = 0
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
	Fallback "Hidden/InternalErrorShader"
}