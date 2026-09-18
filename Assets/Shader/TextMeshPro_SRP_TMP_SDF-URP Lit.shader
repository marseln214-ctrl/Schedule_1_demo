Shader "TextMeshPro/SRP/TMP_SDF-URP Lit" {
	Properties {
		[HDR] _FaceColor ("Face Color", Vector) = (1,1,1,1)
		_IsoPerimeter ("Outline Width", Vector) = (0,0,0,0)
		[HDR] _OutlineColor1 ("Outline Color 1", Vector) = (0,1,1,1)
		[HDR] _OutlineColor2 ("Outline Color 2", Vector) = (0.009433985,0.02534519,1,1)
		[HDR] _OutlineColor3 ("Outline Color 3", Vector) = (0,0,0,1)
		_OutlineOffset1 ("Outline Offset 1", Vector) = (0,0,0,0)
		_OutlineOffset2 ("Outline Offset 2", Vector) = (0,0,0,0)
		_OutlineOffset3 ("Outline Offset 3", Vector) = (0,0,0,0)
		[ToggleUI] _OutlineMode ("OutlineMode", Float) = 0
		_Softness ("Softness", Vector) = (0,0,0,0)
		[NoScaleOffset] _FaceTex ("Face Texture", 2D) = "white" {}
		_FaceUVSpeed ("_FaceUVSpeed", Vector) = (0,0,0,0)
		_FaceTex_ST ("_FaceTex_ST", Vector) = (2,2,0,0)
		[NoScaleOffset] _OutlineTex ("Outline Texture", 2D) = "white" {}
		_OutlineTex_ST ("_OutlineTex_ST", Vector) = (1,1,0,0)
		_OutlineUVSpeed ("_OutlineUVSpeed", Vector) = (0,0,0,0)
		_UnderlayColor ("_UnderlayColor", Vector) = (0,0,0,1)
		_UnderlayOffset ("Underlay Offset", Vector) = (0,0,0,0)
		_UnderlayDilate ("Underlay Dilate", Float) = 0
		_UnderlaySoftness ("_UnderlaySoftness", Float) = 0
		[ToggleUI] _BevelType ("Bevel Type", Float) = 0
		_BevelAmount ("Bevel Amount", Range(0, 1)) = 0
		_BevelOffset ("Bevel Offset", Range(-0.5, 0.5)) = 0
		_BevelWidth ("Bevel Width", Range(0, 0.5)) = 0.5
		_BevelRoundness ("Bevel Roundness", Range(0, 1)) = 0
		_BevelClamp ("Bevel Clamp", Range(0, 1)) = 0
		[HDR] _SpecularColor ("Light Color", Vector) = (1,1,1,1)
		_LightAngle ("Light Angle", Range(0, 6.28)) = 0
		_SpecularPower ("Specular Power", Range(0, 4)) = 0
		_Reflectivity ("Reflectivity Power", Range(5, 15)) = 5
		_Diffuse ("Diffuse Shadow", Range(0, 1)) = 0.3
		_Ambient ("Ambient Shadow", Range(0, 1)) = 0.3
		[NoScaleOffset] _MainTex ("_MainTex", 2D) = "white" {}
		_GradientScale ("_GradientScale", Float) = 10
		_ScaleRatioA ("_ScaleRatioA", Float) = 0
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

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}