Shader "Shader Graphs/ShaderGraph_Flash" {
	Properties {
		_Flash_Texture ("Flash Texture", 2D) = "white" {}
		_Contrast ("Contrast", Range(1, 15)) = 1
		[HDR] _Flash_Color ("Flash Color", Vector) = (1,1,1,0)
		_Boost ("Boost", Range(0, 100)) = 1
		_Depth_Fade_Length ("Depth Fade Length", Range(0, 1)) = 0.01
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