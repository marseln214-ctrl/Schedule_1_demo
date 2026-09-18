Shader "Funly/Sky Studio/Skybox/3D Standard" {
	Properties {
		_GradientSkyUpperColor ("Sky Top Color", Vector) = (0.47,0.45,0.75,1)
		_GradientSkyMiddleColor ("Sky Middle Color", Vector) = (1,1,1,1)
		_GradientSkyLowerColor ("Sky Lower Color", Vector) = (0.7,0.53,0.69,1)
		_GradientFadeBegin ("Horizon Fade Begin", Range(-1, 1)) = -0.179
		_GradientFadeEnd ("Horizon Fade End", Range(-1, 1)) = 0.302
		_GradientFadeMiddlePosition ("Horizon Fade Middle Position", Range(0, 1)) = 0.5
		_HorizonScaleFactor ("Star Horizon Scale Factor", Range(0, 1)) = 0.7
		[NoScaleOffset] _MainTex ("Background Cubemap", Cube) = "white" {}
		_StarFadeBegin ("Star Fade Begin", Range(-1, 1)) = 0.067
		_StarFadeEnd ("Star Fade End", Range(-1, 1)) = 0.36
		[NoScaleOffset] _StarBasicCubemap ("Star Basic Cubemap", Cube) = "black" {}
		_StarBasicTwinkleSpeed ("Star Basic Twinkle Speed", Range(0, 10)) = 5
		_StarBasicTwinkleAmount ("Star Basic Twinkle Amount", Range(0, 1)) = 0.75
		_StarBasicOpacity ("Star Basic Opacity", Range(0, 1)) = 1
		_StarBasicTintColor ("Star Basic Tint Color", Vector) = (1,1,1,1)
		_StarBasicExponent ("Star Basic Exponent", Range(0.5, 5)) = 1.2
		[NoScaleOffset] _StarLayer1Tex ("Star 1 Texture", 2D) = "white" {}
		_StarLayer1Color ("Star Layer 1 - Color", Vector) = (1,1,1,1)
		_StarLayer1Density ("Star Layer 1 - Star Density", Range(0, 0.05)) = 0.01
		_StarLayer1MaxRadius ("Star Layer 1 - Star Size", Range(0, 0.1)) = 0.007
		_StarLayer1TwinkleAmount ("Star Layer 1 - Twinkle Amount", Range(0, 1)) = 0.775
		_StarLayer1TwinkleSpeed ("Star Layer 1 - Twinkle Speed", Float) = 2
		_StarLayer1RotationSpeed ("Star Layer 1 - Rotation Speed", Float) = 2
		_StarLayer1EdgeFade ("Star Layer 1 - Edge Feathering", Range(0.0001, 0.9999)) = 0.2
		_StarLayer1HDRBoost ("Star Layer 1 - HDR Bloom Boost", Range(1, 10)) = 1
		_StarLayer1SpriteDimensions ("Star Layer 1 Sprite Dimensions", Vector) = (0,0,0,0)
		_StarLayer1SpriteItemCount ("Star Layer 1 Sprite Total Items", Float) = 1
		_StarLayer1SpriteAnimationSpeed ("Star Layer 1 Sprite Speed", Float) = 1
		[NoScaleOffset] _StarLayer1DataTex ("Star Layer 1 - Data Image", 2D) = "black" {}
		[NoScaleOffset] _StarLayer2Tex ("Star 2 Texture", 2D) = "white" {}
		_StarLayer2Color ("Star Layer 2 - Color", Vector) = (1,0.5,0.96,1)
		_StarLayer2Density ("Star Layer 2 - Star Density", Range(0, 0.05)) = 0.01
		_StarLayer2MaxRadius ("Star Layer 2 - Star Size", Range(0, 0.4)) = 0.014
		_StarLayer2TwinkleAmount ("Star Layer 2 - Twinkle Amount", Range(0, 1)) = 0.875
		_StarLayer2TwinkleSpeed ("Star Layer 2 - Twinkle Speed", Float) = 3
		_StarLayer2RotationSpeed ("Star Layer 2 - Rotation Speed", Float) = 2
		_StarLayer2EdgeFade ("Star Layer 2 - Edge Feathering", Range(0.0001, 0.9999)) = 0.2
		_StarLayer2HDRBoost ("Star Layer 2 - HDR Bloom Boost", Range(1, 10)) = 1
		_StarLayer2SpriteDimensions ("Star Layer 2 Sprite Dimensions", Vector) = (0,0,0,0)
		_StarLayer2SpriteItemCount ("Star Layer 2 Sprite Total Items", Float) = 1
		_StarLayer2SpriteAnimationSpeed ("Star Layer 2 Sprite Speed", Float) = 1
		[NoScaleOffset] _StarLayer2DataTex ("Star Layer 2 - Data Image", 2D) = "black" {}
		[NoScaleOffset] _StarLayer3Tex ("Star 3 Texture", 2D) = "white" {}
		_StarLayer3Color ("Star Layer 3 - Color", Vector) = (0.22,1,0.55,1)
		_StarLayer3Density ("Star Layer 3 - Star Density", Range(0, 0.05)) = 0.01
		_StarLayer3MaxRadius ("Star Layer 3 - Star Size", Range(0, 0.4)) = 0.01
		_StarLayer3TwinkleAmount ("Star Layer 3 - Twinkle Amount", Range(0, 1)) = 0.7
		_StarLayer3TwinkleSpeed ("Star Layer 3 - Twinkle Speed", Float) = 1
		_StarLayer3RotationSpeed ("Star Layer 3 - Rotation Speed", Float) = 2
		_StarLayer3EdgeFade ("Star Layer 3 - Edge Feathering", Range(0.0001, 0.9999)) = 0.2
		_StarLayer3HDRBoost ("Star Layer 3 - HDR Bloom Boost", Range(1, 10)) = 1
		_StarLayer3SpriteDimensions ("Star Layer 3 Sprite Dimensions", Vector) = (0,0,0,0)
		_StarLayer3SpriteItemCount ("Star Layer 3 Sprite Total Items", Float) = 1
		_StarLayer3SpriteAnimationSpeed ("Star Layer 3 Sprite Speed", Float) = 1
		[NoScaleOffset] _StarLayer3DataTex ("Star Layer 3 - Data Image", 2D) = "black" {}
		[NoScaleOffset] _MoonTex ("Moon Texture", 2D) = "white" {}
		_MoonColor ("Moon Color", Vector) = (0.66,0.65,0.55,1)
		_MoonRadius ("Moon Size", Range(0, 1)) = 0.1
		_MoonEdgeFade ("Moon Edge Feathering", Range(0.0001, 0.9999)) = 0.3
		_MoonHDRBoost ("Moon HDR Bloom Boost", Range(1, 10)) = 1
		_MoonSpriteDimensions ("Moon Sprite Dimensions", Vector) = (0,0,0,0)
		_MoonSpriteItemCount ("Moon Sprite Total Items", Float) = 1
		_MoonSpriteAnimationSpeed ("Moon Sprite Speed", Float) = 1
		_MoonPosition ("Moon Position", Vector) = (0,0,0,0)
		_MoonAlpha ("Sun Alpha", Range(0, 1)) = 1
		[NoScaleOffset] _SunTex ("Sun Texture", 2D) = "white" {}
		_SunColor ("Sun Color", Vector) = (0.66,0.65,0.55,1)
		_SunRadius ("Sun Size", Range(0, 1)) = 0.1
		_SunEdgeFade ("Sun Edge Feathering", Range(0.0001, 0.9999)) = 0.3
		_SunHDRBoost ("Sun HDR Bloom Boost", Range(1, 10)) = 1
		_SunSpriteDimensions ("Sun Sprite Dimensions", Vector) = (0,0,0,0)
		_SunSpriteItemCount ("Sun Sprite Total Items", Float) = 1
		_SunSpriteAnimationSpeed ("Sun Sprite Speed", Float) = 1
		_SunPosition ("Sun Position Data", Vector) = (0,0,0,0)
		_SunAlpha ("Sun Alpha", Range(0, 1)) = 1
		[NoScaleOffset] _CloudNoiseTexture ("Cloud Texture", 2D) = "black" {}
		_CloudFadePosition ("Cloud Fade Position", Range(0, 0.97)) = 0.74
		_CloudFadeAmount ("Cloud Fade Amount", Range(0, 1)) = 0.5
		_CloudDensity ("Cloud Density", Range(0, 1)) = 0.25
		_CloudSpeed ("Cloud Speed", Range(0, 1)) = 0.1
		_CloudDirection ("Cloud Direction", Range(0, 6.283)) = 1
		_CloudHeight ("Cloud Height", Range(0, 1)) = 0.5
		_CloudTextureTiling ("Cloud Tiling", Range(0.01, 10)) = 2
		_CloudColor1 ("Cloud 1 Color", Vector) = (1,1,1,1)
		_CloudColor2 ("Cloud 2 Color", Vector) = (0.6,0.6,0.6,1)
		_CloudAlpha ("Cloud Alpha", Range(0, 1)) = 1
		[NoScaleOffset] _CloudCubemapTexture ("Cloud Cubemap", Cube) = "clear" {}
		_CloudCubemapRotationSpeed ("Cloud Cubemap Rotation Speed", Range(-1, 1)) = 0.01
		_CloudCubemapTintColor ("Cloud Cubemap Tint Color", Vector) = (1,1,1,1)
		_CloudCubemapHeight ("Cloud Cubemap Height", Range(-1, 1)) = 0
		[NoScaleOffset] _CloudCubemapDoubleTexture ("Cloud Double Cubemap", Cube) = "clear" {}
		_CloudCubemapDoubleLayerHeight ("Cloud Cubemap Double Layer Offset", Float) = 0
		_CloudCubemapDoubleLayerRotationSpeed ("Cloud Cubemap Double Layer Rotation Speed", Range(-1, 1)) = 0.02
		_CloudCubemapDoubleLayerTintColor ("Cloud Cubemap Double Tint Color", Vector) = (1,1,1,1)
		[NoScaleOffset] _CloudCubemapNormalTexture ("Cloud Cubemap Normal Texture", Cube) = "clear" {}
		_CloudCubemapNormalAmbientIntensity ("Cloud Ambient Light Intensity", Range(0, 1)) = 0.2
		_CloudCubemapNormalRotationSpeed ("Cloud Cubemap Normal Rotation Speed", Range(-1, 1)) = 0.01
		_CloudCubemapNormalLitColor ("Cloud Cubemap Normal Lit Color", Vector) = (1,1,1,1)
		_CloudCubemapNormalShadowColor ("Cloud Cubemap Normal Shadow Color", Vector) = (0,0,0,1)
		_CloudCubemapNormalHeight ("Cloud Cubemap Normal Height", Range(-1, 1)) = 0
		_CloudCubemapNormalToLight ("Cloud Cubemap Light Direction", Vector) = (0,1,0,0)
		[NoScaleOffset] _CloudCubemapNormalDoubleTexture ("Cloud Cubemap Normal Double Cubemap", Cube) = "clear" {}
		_CloudCubemapNormalDoubleLayerHeight ("Cloud Cubemap Normal Double Layer Offset", Float) = 0
		_CloudCubemapNormalDoubleLayerRotationSpeed ("Cloud Cubemap Normal Double Layer Rotation Speed", Range(-1, 1)) = 0.02
		_CloudCubemapNormalDoubleLitColor ("Cloud Cubemap Normal Double Lit Color", Vector) = (1,1,1,1)
		_CloudCubemapNormalDoubleShadowColor ("Cloud Cubemap Normal Double Shadow Color", Vector) = (0,0,0,1)
		_HorizonFogColor ("Fog Color", Vector) = (1,1,1,1)
		_HorizonFogDensity ("Fog Density", Range(0, 1)) = 0.12
		_HorizonFogLength ("Fog Height", Range(0.03, 1)) = 0.1
		_DebugPointsCount ("Debug Points Count", Range(0, 100)) = 0
		_DebugPointRadius ("Debug Point Radius", Range(0, 0.1)) = 0.03
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
	Fallback "Unlit/Color"
	//CustomEditor "DoNotModifyShaderEditor"
}