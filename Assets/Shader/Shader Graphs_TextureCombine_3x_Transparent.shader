Shader "Shader Graphs/TextureCombine_3x_Transparent" {
	Properties {
		[NoScaleOffset] _Layer_1_Texture ("Layer 1 Texture", 2D) = "white" {}
		_Layer_1_Color ("Layer 1 Color", Vector) = (1,1,1,0)
		[NoScaleOffset] _Layer_2_Texture ("Layer 2 Texture", 2D) = "white" {}
		_Layer_2_Color ("Layer 2 Color", Vector) = (1,1,1,0)
		[NoScaleOffset] _Layer_3_Texture ("Layer 3 Texture", 2D) = "white" {}
		_Layer_3_Color ("Layer 3 Color", Vector) = (1,1,1,0)
		[NoScaleOffset] _Layer_4_Texture ("Layer 4 Texture", 2D) = "white" {}
		_Layer_4_Color ("Layer 4 Color", Vector) = (1,1,1,0)
		[NoScaleOffset] _Layer_5_Texture ("Layer 5 Texture", 2D) = "white" {}
		_Layer_5_Color ("Layer 5 Color", Vector) = (1,1,1,0)
		[NoScaleOffset] _Layer_6_Texture ("Layer 6 Texture", 2D) = "white" {}
		_Layer_6_Color ("Layer 6 Color", Vector) = (1,1,1,0)
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