Shader "Universal Render Pipeline/Nature Shaders/Tree Leaves" {
	Properties {
		_AlphaTest ("Alpha Test", Float) = 0
		_AlphaTestThreshold ("Alpha Test Threshold", Range(0, 1)) = 0.5
		[Enum(Tint,0, HSL,1)] _ColorCorrection ("Color Variation", Float) = 0
		_HSL ("Hue, Saturation, Lightness", Vector) = (0.02,0.05,0.1,0)
		_HSLVariation ("Hue, Saturation, Lightness Variation", Vector) = (-0.02,-0.05,-0.1,0)
		_Tint ("Tint", Vector) = (1,1,1,1)
		_TintVariation ("Tint Variation", Vector) = (1,1,1,1)
		_ColorVariationSpread ("Color Variation Spread", Float) = 0.2
		[HideInInspector] _FloatingOriginOffset_Color ("Floating Origin (color)", Vector) = (0,0,0,0)
		[Enum(On, 0, Off, 2)] _DoubleSidedMode ("Double Sided", Float) = 2
		[Enum(Same, 0, Flip, 1)] _DoubleSidedNormalMode ("Double sided normals", Float) = 1
		_VertexNormalStrength ("Vertex Normal Strength", Range(0, 1)) = 1
		[Enum(Off,0, MetallicGloss,1, Packed,2)] _SurfaceMapMethod ("Surface Maps", Float) = 2
		[Toggle] _LinkMapTilingOffset ("Link All Maps", Float) = 1
		[HideInInspector] _MainTex ("MainTex (legacy, use Albedo instead)", 2D) = "white" {}
		_Albedo ("Albedo", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_NormalMapScale ("Normal Map Strength", Range(0, 1)) = 1
		_Glossiness ("Smoothness", Range(0, 1)) = 0.2
		_Metallic ("Metallic", Range(0, 1)) = 0
		_PackedMap ("Packed Map", 2D) = "white" {}
		_MetallicGlossMap ("Metallic Gloss Map", 2D) = "black" {}
		_OcclusionMap ("Occlusion Map", 2D) = "white" {}
		_GlossRemap ("Remap Smoothness", Vector) = (0,1,0,0)
		_OcclusionRemap ("Remap Occlusion", Vector) = (0,1,0,0)
		_EmissionColor ("Color", Vector) = (1,1,1,1)
		_EmissionMap ("Emission", 2D) = "white" {}
		_EmissionIntensity ("Intensity", Float) = 0
		[ToggleOff] _BakedMeshData ("Baked Mesh Data", Float) = 0
		_ObjectHeight ("Object Height", Float) = 0.5
		_ObjectRadius ("Object Radius", Float) = 0.5
		_Wind ("Wind", Float) = 1
		_WindVariation ("Wind Variation", Range(0, 1)) = 0.3
		_WindStrength ("Wind Strength", Range(0, 2)) = 1
		_TurbulenceStrength ("Turbulence Strength", Range(0, 2)) = 1
		_RecalculateWindNormals ("Recalculate Normals", Range(0, 1)) = 0.5
		_WindFade ("Wind Fade", Vector) = (50,20,0,0)
		_TrunkBendFactor ("Trunk Bending", Vector) = (1,0,0,0)
		[ToggleOff] _Translucency ("Translucency", Float) = 0
		[Enum(Add,0,Overlay,1)] _TranslucencyBlendMode ("Blend Mode", Float) = 0
		_TranslucencyStrength ("Translucency Strength", Range(0, 2)) = 1
		_TranslucencyDistortion ("Translucency Distortion", Range(0, 1)) = 0.5
		_TranslucencyScattering ("Translucency Scattering", Range(0, 3)) = 2
		_TranslucencyColor ("Translucency Color", Vector) = (1,1,1,1)
		_TranslucencyAmbient ("Translucency Ambient", Range(0, 1)) = 0.5
		_TranslucencyShadow ("Translucency Shadow", Range(0, 1)) = 0.8
		_ThicknessMap ("Thickness Map", 2D) = "black" {}
		_ThicknessRemap ("Thickness Remap", Vector) = (0,1,0,0)
		[ToggleOff] _Overlay ("Overlay", Float) = 0
		_SampleAlphaOverlay ("Sample Alpha Overlay", Float) = 1
		_SampleColorOverlay ("Sample Color Overlay", Float) = 1
		[Enum(High, 0, Low, 1)] _LightingQuality ("Lighting Quality", Float) = 0
		[ToggleOff] _SpecularHighlights ("Specular Highlights", Float) = 1
		_MotionVectors ("Calculate Motion Vectors", Float) = 1
		_TemporalAntiAliasing ("Temporal Anti-Aliasing", Float) = 0
		[HideInInspector] __strip__lod_crossfade ("lod_crossfade", Float) = 1
		[HideInInspector] __strip__decals ("decals", Float) = 1
		[HideInInspector] __strip__reflection_probe_blending ("reflection_probe_blending", Float) = 1
		[HideInInspector] __strip__reflection_probe_box_projection ("reflection_probe_box_projection", Float) = 1
		[HideInInspector] __strip__light_layers ("light_layers", Float) = 0
		[HideInInspector] __strip__light_cookies ("light_cookies", Float) = 1
		[HideInInspector] __strip__additional_lights ("additional_lights", Float) = 0
		[HideInInspector] __strip__additional_light_shadows ("additional_light_shadows", Float) = 1
		[HideInInspector] __strip__lightmap ("lightmap", Float) = 1
		[HideInInspector] __strip__shadow_mask ("shadow_mask", Float) = 1
		[HideInInspector] __strip__fog ("fog", Float) = 0
		[HideInInspector] __strip__debug ("debug", Float) = 0
		[ToggleUI] _Decals ("Support Decals", Float) = 1
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
	//CustomEditor "VisualDesignCafe.Nature.Materials.Editor.NatureMaterialEditor"
}