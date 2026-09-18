Shader "Shader Graphs/RoofShader" {
	Properties {
		Color_5F67D5F8 ("Color", Vector) = (0.4716981,0.4694731,0.4694731,0)
		Vector2_BE31F382 ("Tiling", Vector) = (1,1,0,0)
		Vector2_1013FC54 ("Offset", Vector) = (0,0,0,0)
		[NoScaleOffset] [Normal] _SampleTexture2D_b4aa1d764931628a98a363db84e837e2_Texture_1_Texture2D ("Texture2D", 2D) = "bump" {}
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