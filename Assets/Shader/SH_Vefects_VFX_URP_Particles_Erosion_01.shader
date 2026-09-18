Shader "SH_Vefects_VFX_URP_Particles_Erosion_01" {
	Properties {
		[HideInInspector] _EmissionColor ("Emission Color", Vector) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff ("Alpha Cutoff ", Range(0, 1)) = 0.5
		_Noise_01_Texture ("Noise_01_Texture", 2D) = "white" {}
		_Noise_02_Texture ("Noise_02_Texture", 2D) = "white" {}
		_MaskTexture ("Mask Texture", 2D) = "white" {}
		_MaskMoveTexture ("Mask Move Texture", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,0)
		_NoiseDistortionTexture ("Noise Distortion Texture", 2D) = "white" {}
		_Noise01Scale ("Noise 01 Scale", Vector) = (0.8,0.8,0,0)
		_Noise02Scale ("Noise 02 Scale", Vector) = (1,1,0,0)
		_NoiseDistortionScale ("Noise Distortion Scale", Vector) = (1,1,0,0)
		_Noise01Speed ("Noise 01 Speed", Vector) = (0.5,0.5,0,0)
		_Noise02Speed ("Noise 02 Speed", Vector) = (-0.2,0.4,0,0)
		_MaskMoveScale ("Mask Move Scale", Vector) = (1,1,0,0)
		_MaskScale ("Mask Scale", Vector) = (1,1,0,0)
		_MaskOffset ("Mask Offset", Vector) = (0,0,0,0)
		_NoiseDistortionSpeed ("Noise Distortion Speed", Vector) = (0.2,0.25,0,0)
		_MaskMultiply ("Mask Multiply", Float) = 1
		_MaskMoveMultiply ("Mask Move Multiply", Float) = 1
		_NoisesMultiply ("Noises Multiply", Float) = 1
		_MaskPower ("Mask Power", Float) = 1
		_NoisesPower ("Noises Power", Float) = 1
		_MaskMovePower ("Mask Move Power", Float) = 1
		_Distortion ("Distortion", Float) = 1
		_DistortionIntensity ("Distortion Intensity", Float) = 0
		_OpacityBoost ("Opacity Boost", Float) = 5
		_DepthFade ("Depth Fade", Float) = 1
		_EmissionIntensity ("Emission Intensity", Float) = 1
		_WindSpeed ("Wind Speed", Float) = 1
		_MaskSpeed ("Mask Speed", Float) = 0
		_Dissolve ("Dissolve", Float) = 0
		[Space(13)] [Header(AR)] [Space(13)] _Cull1 ("Cull", Float) = 2
		_Src1 ("Src", Float) = 5
		_Dst1 ("Dst", Float) = 10
		_ZWrite1 ("ZWrite", Float) = 0
		_ZTest1 ("ZTest", Float) = 2
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
		[ToggleOff] [HideInInspector] _ReceiveShadows ("Receive Shadows", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
}