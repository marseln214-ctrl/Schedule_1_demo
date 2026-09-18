Shader "Hidden/VolumetricFog2/DistantFog" {
	Properties {
		[HideInInspector] _MainTex ("Noise Texture", 2D) = "white" {}
		[HideInInspector] _Color ("Color", Vector) = (1,1,1,1)
		[HideInInspector] _DistantFogData ("Distant Fog Data", Vector) = (100,0.1,400,0.5)
		[HideInInspector] _BaseAltitude ("Base Altitude", Float) = 0
		[HideInInspector] _LightColor ("Light Color", Vector) = (1,1,1,1)
		[HideInInspector] _LightDiffusionPower ("Sun Diffusion Power", Range(1, 64)) = 32
		[HideInInspector] _LightDiffusionIntensity ("Sun Diffusion Intensity", Range(0, 1)) = 0.4
		[HideInInspector] _SunDir ("Sun Direction", Vector) = (1,0,0,1)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}