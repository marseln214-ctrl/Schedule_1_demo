Shader "Shader Graphs/ModularMaster" {
	Properties {
		Color_3AED6903 ("DoorColor", Vector) = (1,1,1,0)
		Vector1_E6803737 ("DoorTintIntensity", Range(0, 1)) = 0
		Color_8CD000C2 ("GarageTintColor", Vector) = (0,0,0,0)
		Vector1_148E1D84 ("GarageDoorTintIntensity", Range(0, 1)) = 0
		Color_D97C66B0 ("OutsideWallColor01", Vector) = (0,0,0,0)
		Vector1_A4AFA520 ("OutsideWallTint01Intensity", Range(0, 1)) = 0
		Color_10A92CEA ("OutsideWallColor02", Vector) = (0,0,0,0)
		Vector1_1C8DC315 ("OutsideWallTint02Intensity", Range(0, 1)) = 0
		Color_3D553EDD ("InsideWallColor", Vector) = (0,0,0,0)
		Vector1_FB5E3C0E ("InsideWallTintIntensity", Range(0, 1)) = 0
		[NoScaleOffset] _SampleTexture2D_66e18675b70ab788acf1f18b0985bd5d_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[NoScaleOffset] _SampleTexture2D_81a0c5030ab1e289b3c98c934d65c009_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[NoScaleOffset] _SampleTexture2D_7fc1a12d575cf681837da6f659141ba6_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[NoScaleOffset] _SampleTexture2D_6e8ba83c7c42b78db99e3db580bb0887_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[NoScaleOffset] _SampleTexture2D_3f76de528809ef88b1e427c307ec9eff_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[NoScaleOffset] _Texture2DAsset_bcf4abe100e60b89920a4aa098415cf8_Out_0_Texture2D ("Texture2D", 2D) = "white" {}
		[NoScaleOffset] [Normal] _SampleTexture2D_64b67b6025b5958d89240ca9589f4d2e_Texture_1_Texture2D ("Texture2D", 2D) = "bump" {}
		[NoScaleOffset] _SampleTexture2D_78ee4d1bfde5558abb57f6d589f2551b_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[NoScaleOffset] _SampleTexture2D_7d49af16e9a88c86b65f6bfafc18ecc1_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
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