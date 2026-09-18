Shader "LiquidVolume/DefaultNoFlask" {
	Properties {
		[HideInInspector] _Color1 ("Color 1", Vector) = (1,0,0,0.1)
		[HideInInspector] _FoamColor ("Foam Color", Vector) = (1,1,1,0.9)
		[HideInInspector] _Color2 ("Color 2", Vector) = (1,0,0,0.3)
		[HideInInspector] _Glossiness ("Smoothness", Range(0, 1)) = 0.5
		[HideInInspector] _GlossinessInternal ("Internal Smoothness", Vector) = (0.5,180,0.3,1)
		[HideInInspector] _Muddy ("Muddy", Range(0, 1)) = 1
		[HideInInspector] _Turbulence ("Turbulence", Vector) = (1,1,1,0)
		[HideInInspector] _TurbulenceSpeed ("Turbulence Speed", Float) = 1
		[HideInInspector] _SparklingIntensity ("Sparkling Intensity", Range(0, 1)) = 1
		[HideInInspector] _SparklingThreshold ("Sparkling Threshold", Range(0, 1)) = 0.85
		[HideInInspector] _LightColor ("Light Color", Vector) = (1,1,1,1)
		[HideInInspector] _EmissionColor ("Emission Color", Vector) = (0,0,0,1)
		[HideInInspector] _DeepAtten ("Deep Atten", Range(0, 10)) = 2
		[HideInInspector] _LiquidRaySteps ("Liquid Ray Steps", Float) = 10
		[HideInInspector] _SmokeColor ("Smoke Color", Vector) = (0.7,0.7,0.7,0.1)
		[HideInInspector] _SmokeAtten ("Smoke Atten", Range(0, 10)) = 2
		[HideInInspector] _SmokeRaySteps ("Smoke Ray Steps", Float) = 10
		[HideInInspector] _SmokeSpeed ("Smoke Speed", Range(0, 20)) = 5
		[HideInInspector] _SmokeHeightAtten ("Smoke Height Atten", Range(0, 1)) = 0
		[HideInInspector] _NoiseTex2D ("Noise Tex 2D", 2D) = "white" {}
		[HideInInspector] _Noise2Tex ("Noise Tex 2D3D", 2D) = "white" {}
		[HideInInspector] _FoamRaySteps ("Foam Ray Steps", Float) = 15
		[HideInInspector] _FoamWeight ("Foam Weight", Float) = 10
		[HideInInspector] _FoamBottom ("Foam Visible From Bottom", Float) = 1
		[HideInInspector] _FoamTurbulence ("Foam Turbulence", Float) = 1
		[HideInInspector] _Scale ("Scale", Vector) = (0.25,0.2,1,5)
		[HideInInspector] _CullMode ("Cull Mode", Float) = 2
		[HideInInspector] _ZTestMode ("ZTest Mode", Float) = 4
		[HideInInspector] _FoamDensity ("Foam Density", Float) = 1
		[HideInInspector] _AlphaCombined ("Alpha Combined", Float) = 1
		[HideInInspector] _FoamMaxPos ("Foam Max Pos", Float) = 0
		[HideInInspector] _LevelPos ("Level Pos", Float) = 0
		[HideInInspector] _UpperLimit ("Upper Limit", Float) = 1
		[HideInInspector] _LowerLimit ("Lower Limit", Float) = -1
		[HideInInspector] _NoiseTex ("Noise Tex", 3D) = "white" {}
		[HideInInspector] _Center ("Center", Vector) = (1,1,1,1)
		[HideInInspector] _Size ("Size", Vector) = (1,1,1,0.5)
		[HideInInspector] _DoubleSidedBias ("Double Sided Bias", Float) = 0
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
	Fallback "Transparent/VertexLit"
}