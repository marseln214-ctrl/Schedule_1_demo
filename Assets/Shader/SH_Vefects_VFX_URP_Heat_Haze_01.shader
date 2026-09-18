Shader "SH_Vefects_VFX_URP_Heat_Haze_01" {
	Properties {
		[HideInInspector] _AlphaCutoff ("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor ("Emission Color", Vector) = (1,1,1,1)
		_Texture ("Texture", 2D) = "white" {}
		_TextureChannel ("Texture Channel", Vector) = (0,1,0,0)
		_DistortionStrength ("Distortion Strength", Float) = 7
		_DissolveMask ("Dissolve Mask", 2D) = "white" {}
		_DissolveUVS ("Dissolve UV S", Vector) = (0,0,0,0)
		_DissolveUVP ("Dissolve UV P", Vector) = (0,0,0,0)
		[Space(13)] [Header(AR)] [Space(13)] _Cull1 ("Cull", Float) = 2
		_Src1 ("Src", Float) = 5
		_Dst1 ("Dst", Float) = 10
		_ZWrite1 ("ZWrite", Float) = 0
		_ZTest1 ("ZTest", Float) = 2
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
		[ToggleOff] [HideInInspector] _ReceiveShadows ("Receive Shadows", Float) = 1
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
	//CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
}