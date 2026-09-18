Shader "Shader Graphs/GlassShader" {
	Properties {
		_TintTexture ("TintTexture", 2D) = "white" {}
		_DistortionOnTexture ("DistortionOnTexture", Range(0, 1)) = 0
		_TintColor ("TintColor", Vector) = (0,1,0.8042793,0)
		_Metallic ("Metallic", Range(0, 1)) = 0.1
		_Smoothness ("Smoothness", Range(0, 1)) = 1
		_NormalStrength ("NormalStrength", Range(0.01, 10)) = 0.1
		_ReflectionStrength ("ReflectionStrength", Range(0, 5)) = 0.1
		_DisortStrength ("DisortStrength", Range(0.01, 10)) = 1
		_Tiling ("Tiling", Range(0.01, 1000)) = 400
		_Offset ("Offset", Vector) = (0,0,0,0)
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
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}