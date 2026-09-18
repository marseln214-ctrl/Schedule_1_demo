Shader "M_BlendMaster" {
	Properties {
		[HideInInspector] _EmissionColor ("Emission Color", Vector) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff ("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin] _Offset ("Offset", Vector) = (0,0,0,0)
		_Tiling ("Tiling", Vector) = (1,1,0,0)
		[Header(Material_1)] [Header(_________________)] _Mat1_UV ("Mat1_UV", Range(0, 20)) = 1
		_Mat1_BaseColor ("Mat1_BaseColor", Range(0, 5)) = 1
		_Mat1_Roughness ("Mat1_Roughness", Range(0, 2)) = 0.5
		_Mat1_Metallic ("Mat1_Metallic", Range(0, 1)) = 0
		[Toggle(_USECOLOR_ON)] _UseColor ("UseColor", Float) = 0
		_Mat1_Color ("Mat1_Color", Vector) = (1,1,1,0)
		[Toggle(_MAT1_BAKEDNORMAL_ON)] _Mat1_BakedNormal ("Mat1_BakedNormal", Float) = 0
		[Toggle(_MAT1_COLORTEXTURE_ON)] _Mat1_ColorTexture ("Mat1_ColorTexture", Float) = 1
		[Toggle(_MAT1_ROUGHNESSTEXTURE_ON)] _Mat1_RoughnessTexture ("Mat1_RoughnessTexture", Float) = 0
		_Mat1_BakedUV1 ("Mat1_BakedUV1", Range(0, 20)) = 1
		_Mat1_BaseTexture ("Mat1_BaseTexture", 2D) = "white" {}
		_BakedNormal ("BakedNormal", 2D) = "bump" {}
		_Mat1_Base_Normal ("Mat1_Base_Normal", 2D) = "bump" {}
		[Header(Material_2)] [Header(_________________)] _Mat2_UV ("Mat2_UV", Range(0, 20)) = 2
		_Mat2_BaseColor ("Mat2_BaseColor", Range(0, 5)) = 3.75
		_Mat2_Roughness ("Mat2_Roughness", Range(0, 2)) = 0
		_Mat2_Metallic ("Mat2_Metallic", Range(0, 1)) = 0
		[Toggle(_USECOLOR_2_ON)] _UseColor_2 ("UseColor_2", Float) = 0
		_Mat2_Color ("Mat2_Color", Vector) = (1,1,1,0)
		[Toggle(_MAT2_COLORTEXTURE_ON)] _Mat2_ColorTexture ("Mat2_ColorTexture", Float) = 1
		[Toggle(_MAT2_ROUGHNESSTEXTURE_ON)] _Mat2_RoughnessTexture ("Mat2_RoughnessTexture", Float) = 0
		_Mat2_BaseTexture ("Mat2_BaseTexture", 2D) = "white" {}
		_Mat2_Base_Normal ("Mat2_Base_Normal", 2D) = "bump" {}
		_T_VertexBlend_Mask ("T_VertexBlend_Mask", 2D) = "white" {}
		[Header(Vertex Color)] [Header(_____________________)] _VColor_G_Scale ("VColor_G_Scale", Range(0, 10)) = 1.37
		_VColor_Details ("VColor_Details", Range(0, 10)) = 1.1
		_VColor_Power ("VColor_Power", Range(0, 10)) = 1.1
		_Worn_Level ("Worn_Level", Range(0, 1)) = 1.1
		[Toggle(_USE_AO_ON)] _Use_AO ("Use_AO", Float) = 0
		[ASEEnd] _AO ("AO", 2D) = "white" {}
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
	//CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
}