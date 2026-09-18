Shader "Shader Graphs/Particle Shader" {
	Properties {
		[HDR] Color_B81FB4C8 ("Color 1", Vector) = (0,0,0,0)
		[HDR] Color_79FFAF13 ("Color 2", Vector) = (1,1,1,0)
		[NoScaleOffset] Texture2D_82FEAB2E ("Texture", 2D) = "white" {}
		Vector1_9030F466 ("Seconds Per Texture", Float) = 2
		Vector1_63C98F54 ("Flipbook Width", Float) = 3
		Vector1_5352B0DE ("Flipbook Height", Float) = 3
		Vector1_79F969CA ("Randomize Seconds Per Texture (Amount)", Float) = 0
		[ToggleUI] Boolean_FE64AB82 ("Use Toon Style", Float) = 0
		Vector1_CFB18C85 ("Threshold (Toon Style Only)", Range(0, 1)) = 0.3
		Vector1_4170ECC7 ("Color Steps (Toon Style Only)", Float) = 4
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
		[Toggle] BOOLEAN_DDDED732 ("Smooth Flipbook Transitions", Float) = 0
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