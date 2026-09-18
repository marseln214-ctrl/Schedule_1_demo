Shader "M_Generic" {
	Properties {
		[HideInInspector] _AlphaCutoff ("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor ("Emission Color", Vector) = (1,1,1,1)
		[ASEBegin] _Albedo ("Albedo", 2D) = "white" {}
		_Metallic ("Metallic", 2D) = "white" {}
		_Mask ("Mask", 2D) = "white" {}
		_Roughness ("Roughness", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_Brightness ("Brightness", Range(0, 3)) = 1.5
		_RoughnessPower ("RoughnessPower", Range(0, 5)) = 0
		[Toggle(_USECOLOR_ON)] _UseColor ("UseColor", Float) = 0
		[ASEEnd] _Color ("Color", Vector) = (1,1,1,0)
		[HideInInspector] _texcoord ("", 2D) = "white" {}
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
	Fallback "Hidden/InternalErrorShader"
	//CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
}