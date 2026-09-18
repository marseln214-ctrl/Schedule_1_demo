Shader "Hidden/BOXOPHOBIC/Atmospherics/Height Fog Global" {
	Properties {
		[HideInInspector] _AlphaCutoff ("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor ("Emission Color", Vector) = (1,1,1,1)
		[StyledCategory(Fog Settings, false, _HeightFogStandalone, 10, 10)] _FogCat ("[ Fog Cat]", Float) = 1
		[Enum(Perspective,0,Orthographic,1,Both,2)] _FogCameraMode ("Fog Camera Mode", Float) = 0
		[StyledCategory(Skybox Settings, false, _HeightFogStandalone, 10, 10)] _SkyboxCat ("[ Skybox Cat ]", Float) = 1
		[StyledCategory(Directional Settings, false, _HeightFogStandalone, 10, 10)] _DirectionalCat ("[ Directional Cat ]", Float) = 1
		[StyledCategory(Noise Settings, false, _HeightFogStandalone, 10, 10)] _NoiseCat ("[ Noise Cat ]", Float) = 1
		[StyledCategory(Advanced Settings, false, _HeightFogStandalone, 10, 10)] _AdvancedCat ("[ Advanced Cat ]", Float) = 1
		[HideInInspector] _HeightFogGlobal ("_HeightFogGlobal", Float) = 1
		[HideInInspector] _IsHeightFogShader ("_IsHeightFogShader", Float) = 1
		[ASEEnd] [StyledBanner(Height Fog Global)] _Banner ("[ Banner ]", Float) = 1
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
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
	Fallback "Hidden/InternalErrorShader"
	//CustomEditor "HeightFogShaderGUI"
}