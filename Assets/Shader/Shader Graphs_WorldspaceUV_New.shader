Shader "Shader Graphs/WorldspaceUV_New" {
	Properties {
		[NoScaleOffset] _DiffuseTexture ("DiffuseTexture", 2D) = "white" {}
		_Color ("Color", Vector) = (0,0,0,0)
		[NoScaleOffset] [Normal] _NormalTexture ("NormalTexture", 2D) = "bump" {}
		_NormalStrength ("NormalStrength", Float) = 0
		_Smoothness ("Smoothness", Float) = 0
		_Metallic ("Metallic", Float) = 0
		_BlendingEdges ("BlendingEdges", Float) = 1
		_Tiling ("Tiling", Float) = 1
		_Rotation ("Rotation", Float) = 0
		[NoScaleOffset] _Metallic_Map ("Metallic Map", 2D) = "white" {}
		[NoScaleOffset] _SampleTexture2D_1cdad0a82aa24dcebf2ae0b3feaf659b_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
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
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}