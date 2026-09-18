Shader "Universal Render Pipeline/Nature/Stylized Grass" {
	Properties {
		_BaseMap ("Albedo", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
		[MaterialEnum(Both,0,Front,1,Back,2)] _Cull ("Render faces", Float) = 0
		[Toggle] _AlphaToCoverage ("Alpha to coverage", Float) = 0
		_BaseColor ("Color", Vector) = (0.49,0.89,0.12,1)
		_HueVariation ("Hue Variation (Alpha = Intensity)", Vector) = (1,0.63,0,0.15)
		_ColorMapStrength ("Colormap Strength", Range(0, 1)) = 0
		_ColorMapHeight ("Colormap Height", Range(0, 1)) = 1
		_ScalemapInfluence ("Scale influence", Vector) = (0,1,0,0)
		_OcclusionStrength ("Ambient Occlusion", Range(0, 1)) = 0.25
		_VertexDarkening ("Random Darkening", Range(0, 1)) = 0.1
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		_TranslucencyDirect ("Translucency (Direct)", Range(0, 1)) = 1
		_TranslucencyIndirect ("Translucency (Indirect)", Range(0, 1)) = 0
		_TranslucencyFalloff ("Translucency Falloff", Range(1, 8)) = 4
		_TranslucencyOffset ("Translucency Offset", Range(0, 1)) = 0
		[HDR] _EmissionColor ("Emission Color", Vector) = (0,0,0,1)
		_NormalFlattening ("Normal Flattening", Range(0, 1)) = 1
		_NormalSpherify ("Normal Spherifying", Range(0, 1)) = 0
		_NormalSpherifyMask ("Normal Spherifying (tip mask)", Range(0, 1)) = 0
		_NormalFlattenDepthNormals ("Normal Spherifying (DepthNormals pass)", Range(0, 1)) = 0
		_BumpScale ("Normal Map Strength", Range(0, 1)) = 1
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_BendPushStrength ("Push Strength (XZ)", Range(0, 1)) = 1
		[MaterialEnum(Per Vertex,0,Uniform,1)] _BendMode ("Bend Mode", Float) = 0
		_BendFlattenStrength ("Flatten Strength (Y)", Range(0, 1)) = 1
		_BendTint ("Bending tint", Vector) = (0.8,0.8,0.8,1)
		_PerspectiveCorrection ("Perspective Correction", Range(0, 1)) = 1
		_WindAmbientStrength ("Ambient Strength", Range(0, 1)) = 0.2
		_WindSpeed ("Speed", Range(0, 10)) = 3
		_WindDirection ("Direction", Vector) = (1,0,0,0)
		_WindVertexRand ("Vertex randomization", Range(0, 1)) = 0.6
		_WindObjectRand ("Object randomization", Range(0, 1)) = 0.5
		_WindRandStrength ("Random per-object strength", Range(0, 1)) = 0.5
		_WindSwinging ("Swinging", Range(0, 1)) = 0.15
		_WindGustStrength ("Gusting strength", Range(0, 1)) = 0.2
		_WindGustFreq ("Gusting frequency", Range(0, 10)) = 4
		[NoScaleOffset] _WindMap ("Wind map", 2D) = "black" {}
		_WindGustTint ("Max Gusting tint", Range(0, 3)) = 1
		[MinMaxSlider(0, 25)] _FadeNear ("Near", Vector) = (0.25,0.5,0,0)
		[MinMaxSlider(0, 500)] _FadeFar ("Far", Vector) = (50,100,0,0)
		_FadeAngleThreshold ("Angle fading threshold", Range(0, 90)) = 15
		[MaterialEnum(Unlit,0,Simple,1,Advanced,2)] _LightingMode ("Lighting Mode", Float) = 2
		[Toggle] _Scalemap ("Scale grass by scalemap", Float) = 0
		[Toggle] _Billboard ("Billboard", Float) = 0
		[ToggleOff] _ReceiveShadows ("Receive Shadows", Float) = 1
		[ToggleOff] _SpecularHighlights ("Specular Highlights", Float) = 1
		[Toggle] _EnvironmentReflections ("Environment Reflections", Float) = 1
		[Toggle] _FadingOn ("Distance/Angle Fading", Float) = 0
		[HideInInspector] _QueueOffset ("Queue offset", Float) = 0
		_LODDebugColor ("LOD Debug color", Vector) = (1,1,1,1)
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
	Fallback "Hidden/Universal Render Pipeline/FallbackError"
	//CustomEditor "StylizedGrass.MaterialUI"
}