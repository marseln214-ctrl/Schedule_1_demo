using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiquidVolumeFX
{
	[ExecuteInEditMode]
	[HelpURL("https://kronnect.com/support")]
	[AddComponentMenu("Effects/Liquid Volume")]
	[DisallowMultipleComponent]
	public class LiquidVolume : MonoBehaviour
	{
		private struct MeshCache
		{
			public Vector3[] verticesSorted;

			public Vector3[] verticesUnsorted;

			public int[] indices;
		}

		private delegate float MeshVolumeCalcFunction(float level01, float yExtent);

		private static class ShaderParams
		{
			public static int PointLightInsideAtten = Shader.PropertyToID("_PointLightInsideAtten");

			public static int PointLightColorArray = Shader.PropertyToID("_PointLightColor");

			public static int PointLightPositionArray = Shader.PropertyToID("_PointLightPosition");

			public static int PointLightCount = Shader.PropertyToID("_PointLightCount");

			public static int GlossinessInt = Shader.PropertyToID("_GlossinessInternal");

			public static int DoubleSidedBias = Shader.PropertyToID("_DoubleSidedBias");

			public static int BackDepthBias = Shader.PropertyToID("_BackDepthBias");

			public static int Muddy = Shader.PropertyToID("_Muddy");

			public static int Alpha = Shader.PropertyToID("_Alpha");

			public static int AlphaCombined = Shader.PropertyToID("_AlphaCombined");

			public static int SparklingIntensity = Shader.PropertyToID("_SparklingIntensity");

			public static int SparklingThreshold = Shader.PropertyToID("_SparklingThreshold");

			public static int DepthAtten = Shader.PropertyToID("_DeepAtten");

			public static int SmokeColor = Shader.PropertyToID("_SmokeColor");

			public static int SmokeAtten = Shader.PropertyToID("_SmokeAtten");

			public static int SmokeSpeed = Shader.PropertyToID("_SmokeSpeed");

			public static int SmokeHeightAtten = Shader.PropertyToID("_SmokeHeightAtten");

			public static int SmokeRaySteps = Shader.PropertyToID("_SmokeRaySteps");

			public static int LiquidRaySteps = Shader.PropertyToID("_LiquidRaySteps");

			public static int FlaskBlurIntensity = Shader.PropertyToID("_FlaskBlurIntensity");

			public static int FoamColor = Shader.PropertyToID("_FoamColor");

			public static int FoamRaySteps = Shader.PropertyToID("_FoamRaySteps");

			public static int FoamDensity = Shader.PropertyToID("_FoamDensity");

			public static int FoamWeight = Shader.PropertyToID("_FoamWeight");

			public static int FoamBottom = Shader.PropertyToID("_FoamBottom");

			public static int FoamTurbulence = Shader.PropertyToID("_FoamTurbulence");

			public static int RefractTex = Shader.PropertyToID("_RefractTex");

			public static int FlaskThickness = Shader.PropertyToID("_FlaskThickness");

			public static int Size = Shader.PropertyToID("_Size");

			public static int Scale = Shader.PropertyToID("_Scale");

			public static int Center = Shader.PropertyToID("_Center");

			public static int SizeWorld = Shader.PropertyToID("_SizeWorld");

			public static int DepthAwareOffset = Shader.PropertyToID("_DepthAwareOffset");

			public static int Turbulence = Shader.PropertyToID("_Turbulence");

			public static int TurbulenceSpeed = Shader.PropertyToID("_TurbulenceSpeed");

			public static int MurkinessSpeed = Shader.PropertyToID("_MurkinessSpeed");

			public static int Color1 = Shader.PropertyToID("_Color1");

			public static int Color2 = Shader.PropertyToID("_Color2");

			public static int EmissionColor = Shader.PropertyToID("_EmissionColor");

			public static int LightColor = Shader.PropertyToID("_LightColor");

			public static int LightDir = Shader.PropertyToID("_LightDir");

			public static int LevelPos = Shader.PropertyToID("_LevelPos");

			public static int UpperLimit = Shader.PropertyToID("_UpperLimit");

			public static int LowerLimit = Shader.PropertyToID("_LowerLimit");

			public static int FoamMaxPos = Shader.PropertyToID("_FoamMaxPos");

			public static int CullMode = Shader.PropertyToID("_CullMode");

			public static int ZTestMode = Shader.PropertyToID("_ZTestMode");

			public static int NoiseTex = Shader.PropertyToID("_NoiseTex");

			public static int NoiseTexUnwrapped = Shader.PropertyToID("_NoiseTexUnwrapped");

			public static int GlobalRefractionTexture = Shader.PropertyToID("_VLGrabBlurTexture");

			public static int RotationMatrix = Shader.PropertyToID("_Rot");

			public static int QueueOffset = Shader.PropertyToID("_QueueOffset");

			public static int PreserveSpecular = Shader.PropertyToID("_BlendModePreserveSpecular");
		}

		public static bool FORCE_GLES_COMPATIBILITY = false;

		[SerializeField]
		private TOPOLOGY _topology;

		[SerializeField]
		private DETAIL _detail = DETAIL.Default;

		[SerializeField]
		[Range(0f, 1f)]
		private float _level = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _levelMultiplier = 1f;

		[SerializeField]
		[Tooltip("Uses directional light color")]
		private bool _useLightColor;

		[SerializeField]
		[Tooltip("Uses directional light direction for day/night cycle")]
		private bool _useLightDirection;

		[SerializeField]
		private Light _directionalLight;

		[SerializeField]
		[ColorUsage(true)]
		private Color _liquidColor1 = new Color(0f, 1f, 0f, 0.1f);

		[SerializeField]
		[Range(0.1f, 4.85f)]
		private float _liquidScale1 = 1f;

		[SerializeField]
		[ColorUsage(true)]
		private Color _liquidColor2 = new Color(1f, 0f, 0f, 0.3f);

		[SerializeField]
		[Range(2f, 4.85f)]
		private float _liquidScale2 = 5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _alpha = 1f;

		[SerializeField]
		[ColorUsage(true)]
		private Color _emissionColor = new Color(0f, 0f, 0f);

		[SerializeField]
		private bool _ditherShadows = true;

		[SerializeField]
		[Range(0f, 1f)]
		private float _murkiness = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _turbulence1 = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _turbulence2 = 0.2f;

		[SerializeField]
		private float _frecuency = 1f;

		[SerializeField]
		[Range(0f, 2f)]
		private float _speed = 1f;

		[SerializeField]
		[Range(0f, 5f)]
		private float _sparklingIntensity = 0.1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _sparklingAmount = 0.2f;

		[SerializeField]
		[Range(0f, 10f)]
		private float _deepObscurance = 2f;

		[SerializeField]
		[ColorUsage(true)]
		private Color _foamColor = new Color(1f, 1f, 1f, 0.65f);

		[SerializeField]
		[Range(0.01f, 1f)]
		private float _foamScale = 0.2f;

		[SerializeField]
		[Range(0f, 0.1f)]
		private float _foamThickness = 0.04f;

		[SerializeField]
		[Range(-1f, 1f)]
		private float _foamDensity = 0.5f;

		[SerializeField]
		[Range(4f, 100f)]
		private float _foamWeight = 10f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _foamTurbulence = 1f;

		[SerializeField]
		private bool _foamVisibleFromBottom = true;

		[SerializeField]
		private bool _smokeEnabled = true;

		[ColorUsage(true)]
		[SerializeField]
		private Color _smokeColor = new Color(0.7f, 0.7f, 0.7f, 0.25f);

		[SerializeField]
		[Range(0.01f, 1f)]
		private float _smokeScale = 0.25f;

		[SerializeField]
		[Range(0f, 10f)]
		private float _smokeBaseObscurance = 2f;

		[SerializeField]
		[Range(0f, 10f)]
		private float _smokeHeightAtten;

		[SerializeField]
		[Range(0f, 20f)]
		private float _smokeSpeed = 5f;

		[SerializeField]
		private bool _fixMesh;

		public Mesh originalMesh;

		public Vector3 originalPivotOffset;

		[SerializeField]
		private Vector3 _pivotOffset;

		[SerializeField]
		private bool _limitVerticalRange;

		[SerializeField]
		[Range(0f, 1.5f)]
		private float _upperLimit = 1.5f;

		[SerializeField]
		[Range(-1.5f, 1.5f)]
		private float _lowerLimit = -1.5f;

		[SerializeField]
		private int _subMeshIndex = -1;

		[SerializeField]
		private Material _flaskMaterial;

		[SerializeField]
		[Range(0f, 1f)]
		private float _flaskThickness = 0.03f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _glossinessInternal = 0.3f;

		[SerializeField]
		private bool _scatteringEnabled;

		[SerializeField]
		[Range(1f, 16f)]
		private int _scatteringPower = 5;

		[SerializeField]
		[Range(0f, 10f)]
		private float _scatteringAmount = 0.3f;

		[SerializeField]
		private bool _refractionBlur = true;

		[SerializeField]
		[Range(0f, 1f)]
		private float _blurIntensity = 0.75f;

		[SerializeField]
		private int _liquidRaySteps = 10;

		[SerializeField]
		private int _foamRaySteps = 7;

		[SerializeField]
		private int _smokeRaySteps = 5;

		[SerializeField]
		private Texture2D _bumpMap;

		[SerializeField]
		[Range(0f, 1f)]
		private float _bumpStrength = 1f;

		[SerializeField]
		[Range(0f, 10f)]
		private float _bumpDistortionScale = 1f;

		[SerializeField]
		private Vector2 _bumpDistortionOffset;

		[SerializeField]
		private Texture2D _distortionMap;

		[SerializeField]
		private Texture2D _texture;

		[SerializeField]
		private Vector2 _textureScale = Vector2.one;

		[SerializeField]
		private Vector2 _textureOffset;

		[SerializeField]
		[Range(0f, 10f)]
		private float _distortionAmount = 1f;

		[SerializeField]
		private bool _depthAware;

		[SerializeField]
		private float _depthAwareOffset;

		[SerializeField]
		private bool _irregularDepthDebug;

		[SerializeField]
		private bool _depthAwareCustomPass;

		[SerializeField]
		private bool _depthAwareCustomPassDebug;

		[SerializeField]
		[Range(0f, 5f)]
		private float _doubleSidedBias;

		[SerializeField]
		private float _backDepthBias;

		[SerializeField]
		private LEVEL_COMPENSATION _rotationLevelCompensation;

		[SerializeField]
		private bool _ignoreGravity;

		[SerializeField]
		private bool _reactToForces;

		[SerializeField]
		private Vector3 _extentsScale = Vector3.one;

		[SerializeField]
		[Range(1f, 3f)]
		private int _noiseVariation = 1;

		[SerializeField]
		private bool _allowViewFromInside;

		[SerializeField]
		private bool _debugSpillPoint;

		[SerializeField]
		private int _renderQueue = 3001;

		[SerializeField]
		private Cubemap _reflectionTexture;

		[SerializeField]
		[Range(0.1f, 5f)]
		private float _physicsMass = 1f;

		[SerializeField]
		[Range(0f, 0.2f)]
		private float _physicsAngularDamp = 0.02f;

		private const int SHADER_KEYWORD_DEPTH_AWARE_INDEX = 0;

		private const int SHADER_KEYWORD_DEPTH_AWARE_CUSTOM_PASS_INDEX = 1;

		private const int SHADER_KEYWORD_IGNORE_GRAVITY_INDEX = 2;

		private const int SHADER_KEYWORD_NON_AABB_INDEX = 3;

		private const int SHADER_KEYWORD_TOPOLOGY_INDEX = 4;

		private const int SHADER_KEYWORD_REFRACTION_INDEX = 5;

		private const string SHADER_KEYWORD_DEPTH_AWARE = "LIQUID_VOLUME_DEPTH_AWARE";

		private const string SHADER_KEYWORD_DEPTH_AWARE_CUSTOM_PASS = "LIQUID_VOLUME_DEPTH_AWARE_PASS";

		private const string SHADER_KEYWORD_NON_AABB = "LIQUID_VOLUME_NON_AABB";

		private const string SHADER_KEYWORD_IGNORE_GRAVITY = "LIQUID_VOLUME_IGNORE_GRAVITY";

		private const string SHADER_KEYWORD_SPHERE = "LIQUID_VOLUME_SPHERE";

		private const string SHADER_KEYWORD_CUBE = "LIQUID_VOLUME_CUBE";

		private const string SHADER_KEYWORD_CYLINDER = "LIQUID_VOLUME_CYLINDER";

		private const string SHADER_KEYWORD_IRREGULAR = "LIQUID_VOLUME_IRREGULAR";

		private const string SHADER_KEYWORD_FP_RENDER_TEXTURE = "LIQUID_VOLUME_FP_RENDER_TEXTURES";

		private const string SHADER_KEYWORD_USE_REFRACTION = "LIQUID_VOLUME_USE_REFRACTION";

		private const string SPILL_POINT_GIZMO = "SpillPointGizmo";

		[NonSerialized]
		public Material liqMat;

		private Material liqMatSimple;

		private Material liqMatDefaultNoFlask;

		private Mesh mesh;

		[NonSerialized]
		public Renderer mr;

		private static readonly List<Material> mrSharedMaterials = new List<Material>();

		private Vector3 lastPosition;

		private Vector3 lastScale;

		private Quaternion lastRotation;

		private string[] shaderKeywords;

		private bool camInside;

		private float lastDistanceToCam;

		private DETAIL currentDetail;

		private Vector4 turb;

		private Vector4 shaderTurb;

		private float turbulenceSpeed;

		private float murkinessSpeed;

		private float liquidLevelPos;

		private bool shouldUpdateMaterialProperties;

		private int currentNoiseVariation;

		private float levelMultipled;

		private Texture2D noise3DUnwrapped;

		private Texture3D[] noise3DTex;

		private Color[][] colors3D;

		private Vector3[] verticesUnsorted;

		private Vector3[] verticesSorted;

		private static Vector3[] rotatedVertices;

		private int[] verticesIndices;

		private float volumeRef;

		private float lastLevelVolumeRef;

		private Vector3 inertia;

		private Vector3 lastAvgVelocity;

		private float angularVelocity;

		private float angularInertia;

		private float turbulenceDueForces;

		private Quaternion liquidRot;

		private float prevThickness;

		private GameObject spillPointGizmo;

		private static string[] defaultContainerNames = new string[6] { "GLASS", "CONTAINER", "BOTTLE", "POTION", "FLASK", "LIQUID" };

		private Color[] pointLightColorBuffer;

		private Vector4[] pointLightPositionBuffer;

		private int lastPointLightCount;

		private static readonly Dictionary<Mesh, MeshCache> meshCache = new Dictionary<Mesh, MeshCache>();

		private readonly List<Vector3> verts = new List<Vector3>();

		private readonly List<Vector3> cutPoints = new List<Vector3>();

		private Vector3 cutPlaneCenter;

		[SerializeField]
		private Mesh fixedMesh;

		public TOPOLOGY topology
		{
			get
			{
				return _topology;
			}
			set
			{
				if (_topology != value)
				{
					_topology = value;
					UpdateMaterialProperties();
				}
			}
		}

		public DETAIL detail
		{
			get
			{
				return _detail;
			}
			set
			{
				if (_detail != value)
				{
					_detail = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float level
		{
			get
			{
				return _level;
			}
			set
			{
				if (_level != value)
				{
					_level = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float levelMultiplier
		{
			get
			{
				return _levelMultiplier;
			}
			set
			{
				if (_levelMultiplier != value)
				{
					_levelMultiplier = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool useLightColor
		{
			get
			{
				return _useLightColor;
			}
			set
			{
				if (_useLightColor != value)
				{
					_useLightColor = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool useLightDirection
		{
			get
			{
				return _useLightDirection;
			}
			set
			{
				if (_useLightDirection != value)
				{
					_useLightDirection = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Light directionalLight
		{
			get
			{
				return _directionalLight;
			}
			set
			{
				if (_directionalLight != value)
				{
					_directionalLight = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Color liquidColor1
		{
			get
			{
				return _liquidColor1;
			}
			set
			{
				if (_liquidColor1 != value)
				{
					_liquidColor1 = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float liquidScale1
		{
			get
			{
				return _liquidScale1;
			}
			set
			{
				if (_liquidScale1 != value)
				{
					_liquidScale1 = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Color liquidColor2
		{
			get
			{
				return _liquidColor2;
			}
			set
			{
				if (_liquidColor2 != value)
				{
					_liquidColor2 = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float liquidScale2
		{
			get
			{
				return _liquidScale2;
			}
			set
			{
				if (_liquidScale2 != value)
				{
					_liquidScale2 = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float alpha
		{
			get
			{
				return _alpha;
			}
			set
			{
				if (_alpha != Mathf.Clamp01(value))
				{
					_alpha = Mathf.Clamp01(value);
					UpdateMaterialProperties();
				}
			}
		}

		public Color emissionColor
		{
			get
			{
				return _emissionColor;
			}
			set
			{
				if (_emissionColor != value)
				{
					_emissionColor = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool ditherShadows
		{
			get
			{
				return _ditherShadows;
			}
			set
			{
				if (_ditherShadows != value)
				{
					_ditherShadows = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float murkiness
		{
			get
			{
				return _murkiness;
			}
			set
			{
				if (_murkiness != value)
				{
					_murkiness = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float turbulence1
		{
			get
			{
				return _turbulence1;
			}
			set
			{
				if (_turbulence1 != value)
				{
					_turbulence1 = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float turbulence2
		{
			get
			{
				return _turbulence2;
			}
			set
			{
				if (_turbulence2 != value)
				{
					_turbulence2 = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float frecuency
		{
			get
			{
				return _frecuency;
			}
			set
			{
				if (_frecuency != value)
				{
					_frecuency = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float speed
		{
			get
			{
				return _speed;
			}
			set
			{
				if (_speed != value)
				{
					_speed = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float sparklingIntensity
		{
			get
			{
				return _sparklingIntensity;
			}
			set
			{
				if (_sparklingIntensity != value)
				{
					_sparklingIntensity = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float sparklingAmount
		{
			get
			{
				return _sparklingAmount;
			}
			set
			{
				if (_sparklingAmount != value)
				{
					_sparklingAmount = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float deepObscurance
		{
			get
			{
				return _deepObscurance;
			}
			set
			{
				if (_deepObscurance != value)
				{
					_deepObscurance = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Color foamColor
		{
			get
			{
				return _foamColor;
			}
			set
			{
				if (_foamColor != value)
				{
					_foamColor = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float foamScale
		{
			get
			{
				return _foamScale;
			}
			set
			{
				if (_foamScale != value)
				{
					_foamScale = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float foamThickness
		{
			get
			{
				return _foamThickness;
			}
			set
			{
				if (_foamThickness != value)
				{
					_foamThickness = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float foamDensity
		{
			get
			{
				return _foamDensity;
			}
			set
			{
				if (_foamDensity != value)
				{
					_foamDensity = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float foamWeight
		{
			get
			{
				return _foamWeight;
			}
			set
			{
				if (_foamWeight != value)
				{
					_foamWeight = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float foamTurbulence
		{
			get
			{
				return _foamTurbulence;
			}
			set
			{
				if (_foamTurbulence != value)
				{
					_foamTurbulence = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool foamVisibleFromBottom
		{
			get
			{
				return _foamVisibleFromBottom;
			}
			set
			{
				if (_foamVisibleFromBottom != value)
				{
					_foamVisibleFromBottom = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool smokeEnabled
		{
			get
			{
				return _smokeEnabled;
			}
			set
			{
				if (_smokeEnabled != value)
				{
					_smokeEnabled = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Color smokeColor
		{
			get
			{
				return _smokeColor;
			}
			set
			{
				if (_smokeColor != value)
				{
					_smokeColor = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float smokeScale
		{
			get
			{
				return _smokeScale;
			}
			set
			{
				if (_smokeScale != value)
				{
					_smokeScale = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float smokeBaseObscurance
		{
			get
			{
				return _smokeBaseObscurance;
			}
			set
			{
				if (_smokeBaseObscurance != value)
				{
					_smokeBaseObscurance = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float smokeHeightAtten
		{
			get
			{
				return _smokeHeightAtten;
			}
			set
			{
				if (_smokeHeightAtten != value)
				{
					_smokeHeightAtten = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float smokeSpeed
		{
			get
			{
				return _smokeSpeed;
			}
			set
			{
				if (_smokeSpeed != value)
				{
					_smokeSpeed = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool fixMesh
		{
			get
			{
				return _fixMesh;
			}
			set
			{
				if (_fixMesh != value)
				{
					_fixMesh = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Vector3 pivotOffset
		{
			get
			{
				return _pivotOffset;
			}
			set
			{
				if (_pivotOffset != value)
				{
					_pivotOffset = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool limitVerticalRange
		{
			get
			{
				return _limitVerticalRange;
			}
			set
			{
				if (_limitVerticalRange != value)
				{
					_limitVerticalRange = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float upperLimit
		{
			get
			{
				return _upperLimit;
			}
			set
			{
				if (_upperLimit != value)
				{
					_upperLimit = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float lowerLimit
		{
			get
			{
				return _lowerLimit;
			}
			set
			{
				if (_lowerLimit != value)
				{
					_lowerLimit = value;
					UpdateMaterialProperties();
				}
			}
		}

		public int subMeshIndex
		{
			get
			{
				return _subMeshIndex;
			}
			set
			{
				if (_subMeshIndex != value)
				{
					_subMeshIndex = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Material flaskMaterial
		{
			get
			{
				return _flaskMaterial;
			}
			set
			{
				if (_flaskMaterial != value)
				{
					_flaskMaterial = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float flaskThickness
		{
			get
			{
				return _flaskThickness;
			}
			set
			{
				if (_flaskThickness != value)
				{
					_flaskThickness = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float glossinessInternal
		{
			get
			{
				return _glossinessInternal;
			}
			set
			{
				if (_glossinessInternal != value)
				{
					_glossinessInternal = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool scatteringEnabled
		{
			get
			{
				return _scatteringEnabled;
			}
			set
			{
				if (_scatteringEnabled != value)
				{
					_scatteringEnabled = value;
					UpdateMaterialProperties();
				}
			}
		}

		public int scatteringPower
		{
			get
			{
				return _scatteringPower;
			}
			set
			{
				if (_scatteringPower != value)
				{
					_scatteringPower = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float scatteringAmount
		{
			get
			{
				return _scatteringAmount;
			}
			set
			{
				if (_scatteringAmount != value)
				{
					_scatteringAmount = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool refractionBlur
		{
			get
			{
				return _refractionBlur;
			}
			set
			{
				if (_refractionBlur != value)
				{
					_refractionBlur = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float blurIntensity
		{
			get
			{
				return _blurIntensity;
			}
			set
			{
				if (_blurIntensity != Mathf.Clamp01(value))
				{
					_blurIntensity = Mathf.Clamp01(value);
					UpdateMaterialProperties();
				}
			}
		}

		public int liquidRaySteps
		{
			get
			{
				return _liquidRaySteps;
			}
			set
			{
				if (_liquidRaySteps != value)
				{
					_liquidRaySteps = value;
					UpdateMaterialProperties();
				}
			}
		}

		public int foamRaySteps
		{
			get
			{
				return _foamRaySteps;
			}
			set
			{
				if (_foamRaySteps != value)
				{
					_foamRaySteps = value;
					UpdateMaterialProperties();
				}
			}
		}

		public int smokeRaySteps
		{
			get
			{
				return _smokeRaySteps;
			}
			set
			{
				if (_smokeRaySteps != value)
				{
					_smokeRaySteps = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Texture2D bumpMap
		{
			get
			{
				return _bumpMap;
			}
			set
			{
				if (_bumpMap != value)
				{
					_bumpMap = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float bumpStrength
		{
			get
			{
				return _bumpStrength;
			}
			set
			{
				if (_bumpStrength != value)
				{
					_bumpStrength = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float bumpDistortionScale
		{
			get
			{
				return _bumpDistortionScale;
			}
			set
			{
				if (_bumpDistortionScale != value)
				{
					_bumpDistortionScale = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Vector2 bumpDistortionOffset
		{
			get
			{
				return _bumpDistortionOffset;
			}
			set
			{
				if (_bumpDistortionOffset != value)
				{
					_bumpDistortionOffset = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Texture2D distortionMap
		{
			get
			{
				return _distortionMap;
			}
			set
			{
				if (_distortionMap != value)
				{
					_distortionMap = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Texture2D texture
		{
			get
			{
				return _texture;
			}
			set
			{
				if (_texture != value)
				{
					_texture = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Vector2 textureScale
		{
			get
			{
				return _textureScale;
			}
			set
			{
				if (_textureScale != value)
				{
					_textureScale = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Vector2 textureOffset
		{
			get
			{
				return _textureOffset;
			}
			set
			{
				if (_textureOffset != value)
				{
					_textureOffset = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float distortionAmount
		{
			get
			{
				return _distortionAmount;
			}
			set
			{
				if (_distortionAmount != value)
				{
					_distortionAmount = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool depthAware
		{
			get
			{
				return _depthAware;
			}
			set
			{
				if (_depthAware != value)
				{
					_depthAware = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float depthAwareOffset
		{
			get
			{
				return _depthAwareOffset;
			}
			set
			{
				if (_depthAwareOffset != value)
				{
					_depthAwareOffset = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool irregularDepthDebug
		{
			get
			{
				return _irregularDepthDebug;
			}
			set
			{
				if (_irregularDepthDebug != value)
				{
					_irregularDepthDebug = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool depthAwareCustomPass
		{
			get
			{
				return _depthAwareCustomPass;
			}
			set
			{
				if (_depthAwareCustomPass != value)
				{
					_depthAwareCustomPass = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool depthAwareCustomPassDebug
		{
			get
			{
				return _depthAwareCustomPassDebug;
			}
			set
			{
				if (_depthAwareCustomPassDebug != value)
				{
					_depthAwareCustomPassDebug = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float doubleSidedBias
		{
			get
			{
				return _doubleSidedBias;
			}
			set
			{
				if (_doubleSidedBias != value)
				{
					_doubleSidedBias = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float backDepthBias
		{
			get
			{
				return _backDepthBias;
			}
			set
			{
				if (value < 0f)
				{
					value = 0f;
				}
				if (_backDepthBias != value)
				{
					_backDepthBias = value;
					UpdateMaterialProperties();
				}
			}
		}

		public LEVEL_COMPENSATION rotationLevelCompensation
		{
			get
			{
				return _rotationLevelCompensation;
			}
			set
			{
				if (_rotationLevelCompensation != value)
				{
					_rotationLevelCompensation = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool ignoreGravity
		{
			get
			{
				return _ignoreGravity;
			}
			set
			{
				if (_ignoreGravity != value)
				{
					_ignoreGravity = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool reactToForces
		{
			get
			{
				return _reactToForces;
			}
			set
			{
				if (_reactToForces != value)
				{
					_reactToForces = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Vector3 extentsScale
		{
			get
			{
				return _extentsScale;
			}
			set
			{
				if (_extentsScale != value)
				{
					_extentsScale = value;
					UpdateMaterialProperties();
				}
			}
		}

		public int noiseVariation
		{
			get
			{
				return _noiseVariation;
			}
			set
			{
				if (_noiseVariation != value)
				{
					_noiseVariation = value;
					UpdateMaterialProperties();
				}
			}
		}

		public bool allowViewFromInside
		{
			get
			{
				return _allowViewFromInside;
			}
			set
			{
				if (_allowViewFromInside != value)
				{
					_allowViewFromInside = value;
					lastDistanceToCam = -1f;
					CheckInsideOut();
				}
			}
		}

		public bool debugSpillPoint
		{
			get
			{
				return _debugSpillPoint;
			}
			set
			{
				if (_debugSpillPoint != value)
				{
					_debugSpillPoint = value;
				}
			}
		}

		public int renderQueue
		{
			get
			{
				return _renderQueue;
			}
			set
			{
				if (_renderQueue != value)
				{
					_renderQueue = value;
					UpdateMaterialProperties();
				}
			}
		}

		public Cubemap reflectionTexture
		{
			get
			{
				return _reflectionTexture;
			}
			set
			{
				if (_reflectionTexture != value)
				{
					_reflectionTexture = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float physicsMass
		{
			get
			{
				return _physicsMass;
			}
			set
			{
				if (_physicsMass != value)
				{
					_physicsMass = value;
					UpdateMaterialProperties();
				}
			}
		}

		public float physicsAngularDamp
		{
			get
			{
				return _physicsAngularDamp;
			}
			set
			{
				if (_physicsAngularDamp != value)
				{
					_physicsAngularDamp = value;
					UpdateMaterialProperties();
				}
			}
		}

		public static bool useFPRenderTextures => true;

		public float liquidSurfaceYPosition => liquidLevelPos;

		public event PropertiesChangedEvent onPropertiesChanged;

		private void OnEnable()
		{
			if (base.gameObject.activeInHierarchy)
			{
				levelMultipled = _level * _levelMultiplier;
				turb.z = 1f;
				turbulenceDueForces = 0f;
				turbulenceSpeed = 1f;
				liquidRot = base.transform.rotation;
				currentDetail = _detail;
				currentNoiseVariation = -1;
				lastPosition = base.transform.position;
				lastRotation = base.transform.rotation;
				lastScale = base.transform.localScale;
				prevThickness = _flaskThickness;
				if (_depthAwareCustomPass && base.transform.parent == null)
				{
					_depthAwareCustomPass = false;
				}
				UpdateMaterialPropertiesNow();
				if (!Application.isPlaying)
				{
					shouldUpdateMaterialProperties = true;
				}
			}
		}

		private void Reset()
		{
			if (mesh == null)
			{
				return;
			}
			if (mesh.vertexCount == 24)
			{
				topology = TOPOLOGY.Cube;
				return;
			}
			Renderer component = GetComponent<Renderer>();
			if (component == null)
			{
				if (mesh.bounds.extents.y > mesh.bounds.extents.x)
				{
					topology = TOPOLOGY.Cylinder;
				}
			}
			else if (component.bounds.extents.y > component.bounds.extents.x)
			{
				topology = TOPOLOGY.Cylinder;
				if (!Application.isPlaying && base.transform.rotation.eulerAngles != Vector3.zero && (mesh.bounds.extents.y <= mesh.bounds.extents.x || mesh.bounds.extents.y <= mesh.bounds.extents.z))
				{
					Debug.LogWarning("Intrinsic model rotation detected. Consider using the Bake Transform and/or Center Pivot options in Advanced section.");
				}
			}
		}

		private void OnDestroy()
		{
			RestoreOriginalMesh();
			liqMat = null;
			if (liqMatSimple != null)
			{
				UnityEngine.Object.DestroyImmediate(liqMatSimple);
				liqMatSimple = null;
			}
			if (liqMatDefaultNoFlask != null)
			{
				UnityEngine.Object.DestroyImmediate(liqMatDefaultNoFlask);
				liqMatDefaultNoFlask = null;
			}
			if (noise3DTex != null)
			{
				for (int i = 0; i < noise3DTex.Length; i++)
				{
					Texture3D texture3D = noise3DTex[i];
					if (texture3D != null && texture3D.name.Contains("Clone"))
					{
						UnityEngine.Object.DestroyImmediate(texture3D);
						noise3DTex[i] = null;
					}
				}
			}
			LiquidVolumeDepthPrePassRenderFeature.RemoveLiquidFromBackRenderers(this);
			LiquidVolumeDepthPrePassRenderFeature.RemoveLiquidFromFrontRenderers(this);
		}

		private void RenderObject()
		{
			bool flag = base.gameObject.activeInHierarchy && base.enabled;
			if (shouldUpdateMaterialProperties || !Application.isPlaying)
			{
				shouldUpdateMaterialProperties = false;
				UpdateMaterialPropertiesNow();
			}
			if (flag && _allowViewFromInside)
			{
				CheckInsideOut();
			}
			UpdateAnimations();
			if (!flag || _topology != TOPOLOGY.Irregular)
			{
				LiquidVolumeDepthPrePassRenderFeature.RemoveLiquidFromBackRenderers(this);
			}
			else if (_topology == TOPOLOGY.Irregular)
			{
				LiquidVolumeDepthPrePassRenderFeature.AddLiquidToBackRenderers(this);
			}
			if (base.transform.parent != null)
			{
				GetComponentInParent<Renderer>();
				if (!flag || !_depthAwareCustomPass)
				{
					LiquidVolumeDepthPrePassRenderFeature.RemoveLiquidFromFrontRenderers(this);
				}
				else if (_depthAwareCustomPass)
				{
					LiquidVolumeDepthPrePassRenderFeature.AddLiquidToFrontRenderers(this);
				}
			}
			if (_debugSpillPoint)
			{
				UpdateSpillPointGizmo();
			}
		}

		public void OnWillRenderObject()
		{
			RenderObject();
		}

		private void FixedUpdate()
		{
			turbulenceSpeed += Time.deltaTime * 3f * _speed;
			liqMat.SetFloat(ShaderParams.TurbulenceSpeed, turbulenceSpeed * 4f);
			murkinessSpeed += Time.deltaTime * 0.05f * (shaderTurb.x + shaderTurb.y);
			liqMat.SetFloat(ShaderParams.MurkinessSpeed, murkinessSpeed);
		}

		private void OnDidApplyAnimationProperties()
		{
			shouldUpdateMaterialProperties = true;
		}

		public void ClearMeshCache()
		{
			meshCache.Clear();
		}

		private void ReadVertices()
		{
			if (mesh == null)
			{
				return;
			}
			if (!meshCache.TryGetValue(mesh, out var value))
			{
				if (!mesh.isReadable)
				{
					Debug.LogError("Mesh " + mesh.name + " is not readable. Please select your mesh and enable the Read/Write Enabled option.");
				}
				verticesUnsorted = mesh.vertices;
				verticesIndices = mesh.triangles;
				int num = verticesUnsorted.Length;
				if (verticesSorted == null || verticesSorted.Length != num)
				{
					verticesSorted = new Vector3[num];
				}
				Array.Copy(verticesUnsorted, verticesSorted, num);
				Array.Sort(verticesSorted, vertexComparer);
				value.verticesUnsorted = verticesUnsorted;
				value.indices = verticesIndices;
				value.verticesSorted = verticesSorted;
				if (meshCache.Count > 64)
				{
					ClearMeshCache();
				}
				meshCache[mesh] = value;
			}
			else
			{
				verticesUnsorted = value.verticesUnsorted;
				verticesIndices = value.indices;
				verticesSorted = value.verticesSorted;
			}
		}

		private int vertexComparer(Vector3 v0, Vector3 v1)
		{
			if (v1.y < v0.y)
			{
				return -1;
			}
			if (v1.y > v0.y)
			{
				return 1;
			}
			return 0;
		}

		private void UpdateAnimations()
		{
			switch (topology)
			{
			case TOPOLOGY.Sphere:
				if (base.transform.localScale.y != base.transform.localScale.x || base.transform.localScale.z != base.transform.localScale.x)
				{
					base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.x, base.transform.localScale.x);
				}
				break;
			case TOPOLOGY.Cylinder:
				if (base.transform.localScale.z != base.transform.localScale.x)
				{
					base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, base.transform.localScale.x);
				}
				break;
			}
			if (liqMat != null)
			{
				Vector3 lhs = Vector3.right;
				Quaternion rotation = base.transform.rotation;
				if (_reactToForces)
				{
					Quaternion b = base.transform.rotation;
					float deltaTime = Time.deltaTime;
					if (Application.isPlaying && deltaTime > 0f)
					{
						Vector3 vector = (base.transform.position - lastPosition) / deltaTime;
						Vector3 vector2 = vector - lastAvgVelocity;
						lastAvgVelocity = vector;
						inertia += vector;
						float num = Mathf.Max(vector2.magnitude / _physicsMass - _physicsAngularDamp * 150f * deltaTime, 0f);
						angularInertia += num;
						angularVelocity += angularInertia;
						if (angularVelocity > 0f)
						{
							angularInertia -= Mathf.Abs(angularVelocity) * deltaTime * _physicsMass;
						}
						else if (angularVelocity < 0f)
						{
							angularInertia += Mathf.Abs(angularVelocity) * deltaTime * _physicsMass;
						}
						float num2 = 1f - _physicsAngularDamp;
						angularInertia *= num2;
						inertia *= num2;
						float angle = Mathf.Clamp(angularVelocity, -90f, 90f);
						float magnitude = inertia.magnitude;
						if (magnitude > 0f)
						{
							lhs = inertia / magnitude;
						}
						Vector3 axis = Vector3.Cross(lhs, Vector3.down);
						b = Quaternion.AngleAxis(angle, axis);
						float num3 = Mathf.Abs(angularInertia) + Mathf.Abs(angularVelocity);
						turbulenceDueForces = Mathf.Min(0.5f / _physicsMass, turbulenceDueForces + num3 / 1000f);
						turbulenceDueForces *= num2;
					}
					else
					{
						turbulenceDueForces = 0f;
					}
					if (_topology == TOPOLOGY.Sphere)
					{
						liquidRot = Quaternion.Lerp(liquidRot, b, 0.1f);
						rotation = liquidRot;
					}
				}
				else if (turbulenceDueForces > 0f)
				{
					turbulenceDueForces *= 0.1f;
				}
				Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
				liqMat.SetMatrix(ShaderParams.RotationMatrix, matrix4x.inverse);
				if (_topology != 0)
				{
					float x = lhs.x;
					lhs.x += (lhs.z - lhs.x) * 0.25f;
					lhs.z += (x - lhs.z) * 0.25f;
				}
				turb.z = lhs.x;
				turb.w = lhs.z;
			}
			bool flag = base.transform.rotation != lastRotation;
			if (_reactToForces || flag || base.transform.position != lastPosition || base.transform.localScale != lastScale)
			{
				UpdateLevels(flag);
			}
		}

		public void UpdateMaterialProperties()
		{
			if (Application.isPlaying)
			{
				shouldUpdateMaterialProperties = true;
			}
			else
			{
				UpdateMaterialPropertiesNow();
			}
		}

		private void UpdateMaterialPropertiesNow()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			DETAIL dETAIL = _detail;
			if ((uint)dETAIL <= 1u)
			{
				if (liqMatSimple == null)
				{
					liqMatSimple = UnityEngine.Object.Instantiate(Resources.Load<Material>("Materials/LiquidVolumeSimple"));
				}
				liqMat = liqMatSimple;
			}
			else
			{
				if (liqMatDefaultNoFlask == null)
				{
					liqMatDefaultNoFlask = UnityEngine.Object.Instantiate(Resources.Load<Material>("Materials/LiquidVolumeDefaultNoFlask"));
				}
				liqMat = liqMatDefaultNoFlask;
			}
			if (_flaskMaterial == null)
			{
				_flaskMaterial = UnityEngine.Object.Instantiate(Resources.Load<Material>("Materials/Flask"));
			}
			if (liqMat == null)
			{
				return;
			}
			CheckMeshDisplacement();
			if (currentDetail != _detail)
			{
				currentDetail = _detail;
			}
			UpdateLevels();
			if (mr == null)
			{
				return;
			}
			mr.GetSharedMaterials(mrSharedMaterials);
			int count = mrSharedMaterials.Count;
			if (_subMeshIndex < 0)
			{
				for (int i = 0; i < defaultContainerNames.Length; i++)
				{
					if (_subMeshIndex >= 0)
					{
						break;
					}
					for (int j = 0; j < count; j++)
					{
						if (mrSharedMaterials[j] != null && mrSharedMaterials[j] != _flaskMaterial && mrSharedMaterials[j].name.ToUpper().Contains(defaultContainerNames[i]))
						{
							_subMeshIndex = j;
							break;
						}
					}
				}
			}
			if (_subMeshIndex < 0)
			{
				_subMeshIndex = 0;
			}
			if (count > 1 && _subMeshIndex >= 0 && _subMeshIndex < count)
			{
				mrSharedMaterials[_subMeshIndex] = liqMat;
			}
			else
			{
				mrSharedMaterials.Clear();
				mrSharedMaterials.Add(liqMat);
			}
			if (_flaskMaterial != null)
			{
				bool flag = _detail.usesFlask();
				if (flag && !mrSharedMaterials.Contains(_flaskMaterial))
				{
					for (int k = 0; k < mrSharedMaterials.Count; k++)
					{
						if (mrSharedMaterials[k] == null)
						{
							mrSharedMaterials[k] = _flaskMaterial;
							flag = false;
						}
					}
					if (flag)
					{
						mrSharedMaterials.Add(_flaskMaterial);
					}
				}
				else if (!flag && mrSharedMaterials.Contains(_flaskMaterial))
				{
					mrSharedMaterials.Remove(_flaskMaterial);
				}
				_flaskMaterial.SetFloat(ShaderParams.QueueOffset, _renderQueue - 3000);
				_flaskMaterial.SetFloat(ShaderParams.PreserveSpecular, 0f);
			}
			mr.sharedMaterials = mrSharedMaterials.ToArray();
			liqMat.SetColor(ShaderParams.Color1, ApplyGlobalAlpha(_liquidColor1));
			liqMat.SetColor(ShaderParams.Color2, ApplyGlobalAlpha(_liquidColor2));
			liqMat.SetColor(ShaderParams.EmissionColor, _emissionColor);
			if (_useLightColor && _directionalLight != null)
			{
				Color color = _directionalLight.color;
				liqMat.SetColor(ShaderParams.LightColor, color);
			}
			else
			{
				liqMat.SetColor(ShaderParams.LightColor, Color.white);
			}
			if (_useLightDirection && _directionalLight != null)
			{
				liqMat.SetVector(ShaderParams.LightDir, -_directionalLight.transform.forward);
			}
			else
			{
				liqMat.SetVector(ShaderParams.LightDir, Vector3.up);
			}
			int num = _scatteringPower;
			float z = _scatteringAmount;
			if (!_scatteringEnabled)
			{
				num = 0;
				z = 0f;
			}
			liqMat.SetVector(ShaderParams.GlossinessInt, new Vector4((1f - _glossinessInternal) * 96f + 1f, Mathf.Pow(2f, num), z, _glossinessInternal));
			liqMat.SetFloat(ShaderParams.DoubleSidedBias, _doubleSidedBias);
			liqMat.SetFloat(ShaderParams.BackDepthBias, 0f - _backDepthBias);
			liqMat.SetFloat(ShaderParams.Muddy, _murkiness);
			liqMat.SetFloat(ShaderParams.Alpha, _alpha);
			float num2 = _alpha * Mathf.Clamp01((_liquidColor1.a + _liquidColor2.a) * 4f);
			if (_ditherShadows)
			{
				liqMat.SetFloat(ShaderParams.AlphaCombined, num2);
			}
			else
			{
				liqMat.SetFloat(ShaderParams.AlphaCombined, (num2 > 0f) ? 1000f : 0f);
			}
			liqMat.SetFloat(ShaderParams.SparklingIntensity, _sparklingIntensity * 250f);
			liqMat.SetFloat(ShaderParams.SparklingThreshold, 1f - _sparklingAmount);
			liqMat.SetFloat(ShaderParams.DepthAtten, _deepObscurance);
			Color value = ApplyGlobalAlpha(_smokeColor);
			int num3 = _smokeRaySteps;
			if (!_smokeEnabled)
			{
				value.a = 0f;
				num3 = 1;
			}
			liqMat.SetColor(ShaderParams.SmokeColor, value);
			liqMat.SetFloat(ShaderParams.SmokeAtten, _smokeBaseObscurance);
			liqMat.SetFloat(ShaderParams.SmokeHeightAtten, _smokeHeightAtten);
			liqMat.SetFloat(ShaderParams.SmokeSpeed, _smokeSpeed);
			liqMat.SetFloat(ShaderParams.SmokeRaySteps, num3);
			liqMat.SetFloat(ShaderParams.LiquidRaySteps, _liquidRaySteps);
			liqMat.SetColor(ShaderParams.FoamColor, ApplyGlobalAlpha(_foamColor));
			liqMat.SetFloat(ShaderParams.FoamRaySteps, (!(_foamThickness > 0f)) ? 1 : _foamRaySteps);
			liqMat.SetFloat(ShaderParams.FoamDensity, (_foamThickness > 0f) ? _foamDensity : (-1f));
			liqMat.SetFloat(ShaderParams.FoamWeight, _foamWeight);
			liqMat.SetFloat(ShaderParams.FoamBottom, _foamVisibleFromBottom ? 1f : 0f);
			liqMat.SetFloat(ShaderParams.FoamTurbulence, _foamTurbulence);
			if (_noiseVariation != currentNoiseVariation)
			{
				currentNoiseVariation = _noiseVariation;
				if (noise3DTex == null || noise3DTex.Length != 4)
				{
					noise3DTex = new Texture3D[4];
				}
				if (noise3DTex[currentNoiseVariation] == null)
				{
					noise3DTex[currentNoiseVariation] = Resources.Load<Texture3D>("Textures/Noise3D" + currentNoiseVariation);
				}
				Texture3D texture3D = noise3DTex[currentNoiseVariation];
				if (texture3D != null)
				{
					liqMat.SetTexture(ShaderParams.NoiseTex, texture3D);
				}
			}
			liqMat.renderQueue = _renderQueue;
			UpdateInsideOut();
			if (_topology == TOPOLOGY.Irregular && prevThickness != _flaskThickness)
			{
				prevThickness = _flaskThickness;
			}
			this.onPropertiesChanged?.Invoke(this);
		}

		private Color ApplyGlobalAlpha(Color originalColor)
		{
			return new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * _alpha);
		}

		private void GetRenderer()
		{
			MeshFilter component = GetComponent<MeshFilter>();
			if (component != null)
			{
				mesh = component.sharedMesh;
				mr = GetComponent<MeshRenderer>();
				return;
			}
			SkinnedMeshRenderer component2 = GetComponent<SkinnedMeshRenderer>();
			if (component2 != null)
			{
				mesh = component2.sharedMesh;
				mr = component2;
			}
		}

		private void UpdateLevels(bool updateShaderKeywords = true)
		{
			_level = Mathf.Clamp01(_level);
			levelMultipled = _level * _levelMultiplier;
			if (liqMat == null)
			{
				return;
			}
			if (mesh == null)
			{
				GetRenderer();
				ReadVertices();
			}
			else if (mr == null)
			{
				GetRenderer();
			}
			if (mesh == null || mr == null)
			{
				return;
			}
			Vector4 value = new Vector4(mesh.bounds.extents.x * 2f * base.transform.lossyScale.x, mesh.bounds.extents.y * 2f * base.transform.lossyScale.y, mesh.bounds.extents.z * 2f * base.transform.lossyScale.z, 0f);
			value.x *= _extentsScale.x;
			value.y *= _extentsScale.y;
			value.z *= _extentsScale.z;
			float num = Mathf.Max(value.x, value.z);
			Vector3 vector = (_ignoreGravity ? new Vector3(value.x * 0.5f, value.y * 0.5f, value.z * 0.5f) : mr.bounds.extents);
			vector *= 1f - _flaskThickness;
			vector.x *= _extentsScale.x;
			vector.y *= _extentsScale.y;
			vector.z *= _extentsScale.z;
			float num2;
			if (_upperLimit < 1f && !_ignoreGravity)
			{
				float y = base.transform.TransformPoint(Vector3.up * vector.y).y;
				num2 = Mathf.Max(base.transform.TransformPoint(Vector3.up * (vector.y * _upperLimit)).y - y, 0f);
			}
			else
			{
				num2 = 0f;
			}
			float num3 = levelMultipled;
			if (_rotationLevelCompensation != 0 && !_ignoreGravity && num3 > 0f)
			{
				MeshVolumeCalcFunction meshVolumeCalcFunction;
				int num4;
				if (_rotationLevelCompensation == LEVEL_COMPENSATION.Fast)
				{
					meshVolumeCalcFunction = GetMeshVolumeUnderLevelFast;
					num4 = 8;
				}
				else
				{
					meshVolumeCalcFunction = GetMeshVolumeUnderLevel;
					num4 = 10;
				}
				if (lastLevelVolumeRef != num3)
				{
					lastLevelVolumeRef = num3;
					if (_topology == TOPOLOGY.Cylinder)
					{
						float num5 = value.x * 0.5f;
						float num6 = value.y * num3;
						volumeRef = MathF.PI * num5 * num5 * num6;
					}
					else
					{
						Quaternion rotation = base.transform.rotation;
						base.transform.rotation = Quaternion.identity;
						float num7 = (_ignoreGravity ? (value.y * 0.5f) : mr.bounds.extents.y);
						num7 *= 1f - _flaskThickness;
						num7 *= _extentsScale.y;
						RotateVertices();
						volumeRef = meshVolumeCalcFunction(num3, num7);
						base.transform.rotation = rotation;
					}
				}
				RotateVertices();
				float num8 = num3;
				float num9 = float.MaxValue;
				float num10 = Mathf.Clamp01(num3 + 0.5f);
				float num11 = Mathf.Clamp01(num3 - 0.5f);
				for (int i = 0; i < 12; i++)
				{
					num3 = (num11 + num10) * 0.5f;
					float num12 = meshVolumeCalcFunction(num3, vector.y);
					float num13 = Mathf.Abs(volumeRef - num12);
					if (num13 < num9)
					{
						num9 = num13;
						num8 = num3;
					}
					if (num12 < volumeRef)
					{
						num11 = num3;
						continue;
					}
					if (i >= num4)
					{
						break;
					}
					num10 = num3;
				}
				num3 = num8 * _levelMultiplier;
			}
			else if (levelMultipled <= 0f)
			{
				num3 = -0.001f;
			}
			liquidLevelPos = mr.bounds.center.y - vector.y;
			liquidLevelPos += vector.y * 2f * num3 + num2;
			liqMat.SetFloat(ShaderParams.LevelPos, liquidLevelPos);
			float num14 = mesh.bounds.extents.y * _extentsScale.y * _upperLimit;
			liqMat.SetFloat(ShaderParams.UpperLimit, _limitVerticalRange ? num14 : float.MaxValue);
			float num15 = mesh.bounds.extents.y * _extentsScale.y * _lowerLimit;
			liqMat.SetFloat(ShaderParams.LowerLimit, _limitVerticalRange ? num15 : float.MinValue);
			float num16 = ((levelMultipled <= 0f || levelMultipled >= 1f) ? 0f : 1f);
			UpdateTurbulence();
			float value2 = mr.bounds.center.y - vector.y + (num2 + vector.y * 2f * (num3 + _foamThickness)) * num16;
			liqMat.SetFloat(ShaderParams.FoamMaxPos, value2);
			Vector4 value3 = new Vector4(1f - _flaskThickness, 1f - _flaskThickness * num / value.z, 1f - _flaskThickness * num / value.z, 0f);
			liqMat.SetVector(ShaderParams.FlaskThickness, value3);
			value.w = value.x * 0.5f * value3.x;
			value.x = Vector3.Distance(mr.bounds.max, mr.bounds.min);
			liqMat.SetVector(ShaderParams.Size, value);
			float num17 = value.y * 0.5f * (1f - _flaskThickness * num / value.y);
			liqMat.SetVector(ShaderParams.Scale, new Vector4(_smokeScale / num17, _foamScale / num17, _liquidScale1 / num17, _liquidScale2 / num17));
			liqMat.SetVector(ShaderParams.Center, base.transform.position);
			if (shaderKeywords == null || shaderKeywords.Length != 6)
			{
				shaderKeywords = new string[6];
			}
			for (int j = 0; j < shaderKeywords.Length; j++)
			{
				shaderKeywords[j] = null;
			}
			if (_depthAware)
			{
				shaderKeywords[0] = "LIQUID_VOLUME_DEPTH_AWARE";
				liqMat.SetFloat(ShaderParams.DepthAwareOffset, _depthAwareOffset);
			}
			if (_depthAwareCustomPass)
			{
				shaderKeywords[1] = "LIQUID_VOLUME_DEPTH_AWARE_PASS";
			}
			if (_reactToForces && _topology == TOPOLOGY.Sphere)
			{
				shaderKeywords[2] = "LIQUID_VOLUME_IGNORE_GRAVITY";
			}
			else if (_ignoreGravity)
			{
				shaderKeywords[2] = "LIQUID_VOLUME_IGNORE_GRAVITY";
			}
			else if (base.transform.rotation.eulerAngles != Vector3.zero)
			{
				shaderKeywords[3] = "LIQUID_VOLUME_NON_AABB";
			}
			switch (_topology)
			{
			case TOPOLOGY.Sphere:
				shaderKeywords[4] = "LIQUID_VOLUME_SPHERE";
				break;
			case TOPOLOGY.Cube:
				shaderKeywords[4] = "LIQUID_VOLUME_CUBE";
				break;
			case TOPOLOGY.Cylinder:
				shaderKeywords[4] = "LIQUID_VOLUME_CYLINDER";
				break;
			default:
				shaderKeywords[4] = "LIQUID_VOLUME_IRREGULAR";
				break;
			}
			if (_refractionBlur && _detail.allowsRefraction())
			{
				liqMat.SetFloat(ShaderParams.FlaskBlurIntensity, _blurIntensity * (_refractionBlur ? 1f : 0f));
				shaderKeywords[5] = "LIQUID_VOLUME_USE_REFRACTION";
			}
			if (updateShaderKeywords)
			{
				liqMat.shaderKeywords = shaderKeywords;
			}
			lastPosition = base.transform.position;
			lastScale = base.transform.localScale;
			lastRotation = base.transform.rotation;
		}

		private void RotateVertices()
		{
			int num = verticesUnsorted.Length;
			if (rotatedVertices == null || rotatedVertices.Length != num)
			{
				rotatedVertices = new Vector3[num];
			}
			for (int i = 0; i < num; i++)
			{
				rotatedVertices[i] = base.transform.TransformPoint(verticesUnsorted[i]);
			}
		}

		private float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 zeroPoint)
		{
			p1.x -= zeroPoint.x;
			p1.y -= zeroPoint.y;
			p1.z -= zeroPoint.z;
			p2.x -= zeroPoint.x;
			p2.y -= zeroPoint.y;
			p2.z -= zeroPoint.z;
			p3.x -= zeroPoint.x;
			p3.y -= zeroPoint.y;
			p3.z -= zeroPoint.z;
			float num = p3.x * p2.y * p1.z;
			float num2 = p2.x * p3.y * p1.z;
			float num3 = p3.x * p1.y * p2.z;
			float num4 = p1.x * p3.y * p2.z;
			float num5 = p2.x * p1.y * p3.z;
			float num6 = p1.x * p2.y * p3.z;
			return 1f / 6f * (0f - num + num2 + num3 - num4 - num5 + num6);
		}

		public float GetMeshVolumeUnderLevelFast(float level01, float yExtent)
		{
			float num = mr.bounds.center.y - yExtent;
			num += yExtent * 2f * level01;
			return GetMeshVolumeUnderLevelWSFast(num);
		}

		public float GetMeshVolumeWSFast()
		{
			return GetMeshVolumeUnderLevelWSFast(float.MaxValue);
		}

		public float GetMeshVolumeUnderLevelWSFast(float level)
		{
			Vector3 center = mr.bounds.center;
			float num = 0f;
			for (int i = 0; i < verticesIndices.Length; i += 3)
			{
				Vector3 p = rotatedVertices[verticesIndices[i]];
				Vector3 p2 = rotatedVertices[verticesIndices[i + 1]];
				Vector3 p3 = rotatedVertices[verticesIndices[i + 2]];
				if (p.y > level)
				{
					p.y = level;
				}
				if (p2.y > level)
				{
					p2.y = level;
				}
				if (p3.y > level)
				{
					p3.y = level;
				}
				num += SignedVolumeOfTriangle(p, p2, p3, center);
			}
			return Mathf.Abs(num);
		}

		private Vector3 ClampVertexToSlicePlane(Vector3 p, Vector3 q, float level)
		{
			Vector3 normalized = (q - p).normalized;
			float num = p.y - level;
			return p + normalized * num / (0f - normalized.y);
		}

		public float GetMeshVolumeUnderLevel(float level01, float yExtent)
		{
			float num = mr.bounds.center.y - yExtent;
			num += yExtent * 2f * level01;
			return GetMeshVolumeUnderLevelWS(num);
		}

		public float GetMeshVolumeWS()
		{
			return GetMeshVolumeUnderLevelWS(float.MaxValue);
		}

		public float GetMeshVolumeUnderLevelWS(float level)
		{
			Vector3 center = mr.bounds.center;
			cutPlaneCenter = Vector3.zero;
			cutPoints.Clear();
			verts.Clear();
			int num = verticesIndices.Length;
			for (int i = 0; i < num; i += 3)
			{
				Vector3 vector = rotatedVertices[verticesIndices[i]];
				Vector3 vector2 = rotatedVertices[verticesIndices[i + 1]];
				Vector3 vector3 = rotatedVertices[verticesIndices[i + 2]];
				if (vector.y > level && vector2.y > level && vector3.y > level)
				{
					continue;
				}
				if (vector.y < level && vector2.y > level && vector3.y > level)
				{
					vector2 = ClampVertexToSlicePlane(vector2, vector, level);
					vector3 = ClampVertexToSlicePlane(vector3, vector, level);
					cutPoints.Add(vector2);
					cutPoints.Add(vector3);
					cutPlaneCenter += vector2;
					cutPlaneCenter += vector3;
				}
				else if (vector2.y < level && vector.y > level && vector3.y > level)
				{
					vector = ClampVertexToSlicePlane(vector, vector2, level);
					vector3 = ClampVertexToSlicePlane(vector3, vector2, level);
					cutPoints.Add(vector);
					cutPoints.Add(vector3);
					cutPlaneCenter += vector;
					cutPlaneCenter += vector3;
				}
				else if (vector3.y < level && vector.y > level && vector2.y > level)
				{
					vector = ClampVertexToSlicePlane(vector, vector3, level);
					vector2 = ClampVertexToSlicePlane(vector2, vector3, level);
					cutPoints.Add(vector);
					cutPoints.Add(vector2);
					cutPlaneCenter += vector;
					cutPlaneCenter += vector2;
				}
				else
				{
					if (vector.y > level && vector2.y < level && vector3.y < level)
					{
						Vector3 vector4 = ClampVertexToSlicePlane(vector, vector2, level);
						Vector3 vector5 = ClampVertexToSlicePlane(vector, vector3, level);
						verts.Add(vector4);
						verts.Add(vector2);
						verts.Add(vector3);
						verts.Add(vector5);
						verts.Add(vector4);
						verts.Add(vector3);
						cutPoints.Add(vector4);
						cutPoints.Add(vector5);
						cutPlaneCenter += vector4;
						cutPlaneCenter += vector5;
						continue;
					}
					if (vector2.y > level && vector.y < level && vector3.y < level)
					{
						Vector3 vector6 = ClampVertexToSlicePlane(vector2, vector, level);
						Vector3 vector7 = ClampVertexToSlicePlane(vector2, vector3, level);
						verts.Add(vector);
						verts.Add(vector6);
						verts.Add(vector3);
						verts.Add(vector6);
						verts.Add(vector7);
						verts.Add(vector3);
						cutPoints.Add(vector6);
						cutPoints.Add(vector7);
						cutPlaneCenter += vector6;
						cutPlaneCenter += vector7;
						continue;
					}
					if (vector3.y > level && vector.y < level && vector2.y < level)
					{
						Vector3 vector8 = ClampVertexToSlicePlane(vector3, vector, level);
						Vector3 vector9 = ClampVertexToSlicePlane(vector3, vector2, level);
						verts.Add(vector8);
						verts.Add(vector);
						verts.Add(vector2);
						verts.Add(vector9);
						verts.Add(vector8);
						verts.Add(vector2);
						cutPoints.Add(vector8);
						cutPoints.Add(vector9);
						cutPlaneCenter += vector8;
						cutPlaneCenter += vector9;
						continue;
					}
				}
				verts.Add(vector);
				verts.Add(vector2);
				verts.Add(vector3);
			}
			int count = cutPoints.Count;
			if (cutPoints.Count >= 3)
			{
				cutPlaneCenter /= (float)count;
				cutPoints.Sort(PolygonSortOnPlane);
				for (int j = 0; j < count; j++)
				{
					Vector3 item = cutPoints[j];
					Vector3 item2 = ((j != count - 1) ? cutPoints[j + 1] : cutPoints[0]);
					verts.Add(cutPlaneCenter);
					verts.Add(item);
					verts.Add(item2);
				}
			}
			int count2 = verts.Count;
			float num2 = 0f;
			for (int k = 0; k < count2; k += 3)
			{
				num2 += SignedVolumeOfTriangle(verts[k], verts[k + 1], verts[k + 2], center);
			}
			return Mathf.Abs(num2);
		}

		private int PolygonSortOnPlane(Vector3 p1, Vector3 p2)
		{
			float num = Mathf.Atan2(p1.x - cutPlaneCenter.x, p1.z - cutPlaneCenter.z);
			float num2 = Mathf.Atan2(p2.x - cutPlaneCenter.x, p2.z - cutPlaneCenter.z);
			if (num < num2)
			{
				return -1;
			}
			if (num > num2)
			{
				return 1;
			}
			return 0;
		}

		private void UpdateTurbulence()
		{
			if (!(liqMat == null))
			{
				float num = ((levelMultipled > 0f) ? 1f : 0f);
				float num2 = ((camInside && _allowViewFromInside) ? 0f : 1f);
				turb.x = _turbulence1 * num * num2;
				turb.y = Mathf.Max(_turbulence2, turbulenceDueForces) * num * num2;
				shaderTurb = turb;
				shaderTurb.z *= MathF.PI * _frecuency * 4f;
				shaderTurb.w *= MathF.PI * _frecuency * 4f;
				liqMat.SetVector(ShaderParams.Turbulence, shaderTurb);
			}
		}

		private void CheckInsideOut()
		{
			Camera current = Camera.current;
			if (current == null || mr == null)
			{
				if (!_allowViewFromInside)
				{
					UpdateInsideOut();
				}
				return;
			}
			Vector3 vector = current.transform.position + current.transform.forward * current.nearClipPlane;
			float sqrMagnitude = (vector - base.transform.position).sqrMagnitude;
			if (sqrMagnitude != lastDistanceToCam)
			{
				lastDistanceToCam = sqrMagnitude;
				bool flag = false;
				switch (_topology)
				{
				case TOPOLOGY.Cube:
					flag = PointInAABB(vector);
					break;
				case TOPOLOGY.Cylinder:
					flag = PointInCylinder(vector);
					break;
				default:
				{
					float num = mesh.bounds.extents.x * 2f;
					flag = (vector - base.transform.position).sqrMagnitude < num * num;
					break;
				}
				}
				if (flag != camInside)
				{
					camInside = flag;
					UpdateInsideOut();
				}
			}
		}

		private bool PointInAABB(Vector3 point)
		{
			point = base.transform.InverseTransformPoint(point);
			Vector3 extents = mesh.bounds.extents;
			if (point.x < extents.x && point.x > 0f - extents.x && point.y < extents.y && point.y > 0f - extents.y && point.z < extents.z && point.z > 0f - extents.z)
			{
				return true;
			}
			return false;
		}

		private bool PointInCylinder(Vector3 point)
		{
			point = base.transform.InverseTransformPoint(point);
			Vector3 extents = mesh.bounds.extents;
			if (point.x < extents.x && point.x > 0f - extents.x && point.y < extents.y && point.y > 0f - extents.y && point.z < extents.z && point.z > 0f - extents.z)
			{
				point.y = 0f;
				Vector3 position = base.transform.position;
				position.y = 0f;
				return (point - position).sqrMagnitude < extents.x * extents.x;
			}
			return false;
		}

		private void UpdateInsideOut()
		{
			if (liqMat == null)
			{
				return;
			}
			if (_allowViewFromInside && camInside)
			{
				liqMat.SetInt(ShaderParams.CullMode, 1);
				liqMat.SetInt(ShaderParams.ZTestMode, 8);
				if (_flaskMaterial != null)
				{
					_flaskMaterial.SetInt(ShaderParams.CullMode, 1);
					_flaskMaterial.SetInt(ShaderParams.ZTestMode, 8);
				}
			}
			else
			{
				liqMat.SetInt(ShaderParams.CullMode, 2);
				liqMat.SetInt(ShaderParams.ZTestMode, 4);
				if (_flaskMaterial != null)
				{
					_flaskMaterial.SetInt(ShaderParams.CullMode, 2);
					_flaskMaterial.SetInt(ShaderParams.ZTestMode, 4);
				}
			}
			UpdateTurbulence();
		}

		public bool GetSpillPoint(out Vector3 spillPosition, float apertureStart = 1f)
		{
			float spillAmount;
			return GetSpillPoint(out spillPosition, out spillAmount, apertureStart);
		}

		public bool GetSpillPoint(out Vector3 spillPosition, out float spillAmount, float apertureStart = 1f, LEVEL_COMPENSATION rotationCompensation = LEVEL_COMPENSATION.None)
		{
			spillPosition = Vector3.zero;
			spillAmount = 0f;
			if (mesh == null || verticesSorted == null || levelMultipled <= 0f)
			{
				return false;
			}
			float num = float.MinValue;
			for (int i = 0; i < verticesSorted.Length; i++)
			{
				Vector3 vector = verticesSorted[i];
				if (vector.y > num)
				{
					num = vector.y;
				}
			}
			float num2 = num * apertureStart * 0.99f;
			Vector3 vector2 = base.transform.position;
			bool flag = false;
			float num3 = float.MaxValue;
			for (int j = 0; j < verticesSorted.Length; j++)
			{
				Vector3 position = verticesSorted[j];
				if (position.y < num2)
				{
					break;
				}
				position = base.transform.TransformPoint(position);
				if (position.y < liquidLevelPos && position.y < num3)
				{
					num3 = position.y;
					vector2 = position;
					flag = true;
				}
			}
			if (!flag)
			{
				return false;
			}
			spillPosition = vector2;
			switch (rotationCompensation)
			{
			case LEVEL_COMPENSATION.Accurate:
				spillAmount = GetMeshVolumeUnderLevelWS(liquidLevelPos) - GetMeshVolumeUnderLevelWS(vector2.y);
				break;
			case LEVEL_COMPENSATION.Fast:
				spillAmount = GetMeshVolumeUnderLevelWSFast(liquidLevelPos) - GetMeshVolumeUnderLevelWSFast(vector2.y);
				break;
			default:
				spillAmount = (liquidLevelPos - vector2.y) / (mr.bounds.extents.y * 2f);
				break;
			}
			return true;
		}

		private void UpdateSpillPointGizmo()
		{
			if (!_debugSpillPoint)
			{
				if (spillPointGizmo != null)
				{
					UnityEngine.Object.DestroyImmediate(spillPointGizmo.gameObject);
					spillPointGizmo = null;
				}
				return;
			}
			if (spillPointGizmo == null)
			{
				Transform transform = base.transform.Find("SpillPointGizmo");
				if (transform != null)
				{
					UnityEngine.Object.DestroyImmediate(transform.gameObject);
				}
				spillPointGizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				spillPointGizmo.name = "SpillPointGizmo";
				spillPointGizmo.transform.SetParent(base.transform, worldPositionStays: true);
				Collider component = spillPointGizmo.GetComponent<Collider>();
				if (component != null)
				{
					UnityEngine.Object.DestroyImmediate(component);
				}
				MeshRenderer component2 = spillPointGizmo.GetComponent<MeshRenderer>();
				if (component2 != null)
				{
					component2.sharedMaterial = UnityEngine.Object.Instantiate(component2.sharedMaterial);
					component2.sharedMaterial.hideFlags = HideFlags.DontSave;
					component2.sharedMaterial.color = Color.yellow;
				}
			}
			if (GetSpillPoint(out var spillPosition))
			{
				spillPointGizmo.transform.position = spillPosition;
				if (mesh != null)
				{
					Vector3 vector = mesh.bounds.extents * 0.2f;
					float num = ((vector.x > vector.y) ? vector.x : vector.z);
					num = ((num > vector.z) ? num : vector.z);
					spillPointGizmo.transform.localScale = new Vector3(num, num, num);
				}
				else
				{
					spillPointGizmo.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
				}
				spillPointGizmo.SetActive(value: true);
			}
			else
			{
				spillPointGizmo.SetActive(value: false);
			}
		}

		public void BakeRotation()
		{
			if (base.transform.localRotation == base.transform.rotation)
			{
				return;
			}
			MeshFilter component = GetComponent<MeshFilter>();
			Mesh sharedMesh = component.sharedMesh;
			if (!(sharedMesh == null))
			{
				sharedMesh = UnityEngine.Object.Instantiate(sharedMesh);
				Vector3[] vertices = sharedMesh.vertices;
				Vector3 localScale = base.transform.localScale;
				Vector3 localPosition = base.transform.localPosition;
				base.transform.localScale = Vector3.one;
				Transform parent = base.transform.parent;
				if (parent != null)
				{
					base.transform.SetParent(null, worldPositionStays: false);
				}
				for (int i = 0; i < vertices.Length; i++)
				{
					vertices[i] = base.transform.TransformVector(vertices[i]);
				}
				sharedMesh.vertices = vertices;
				sharedMesh.RecalculateBounds();
				sharedMesh.RecalculateNormals();
				component.sharedMesh = sharedMesh;
				if (parent != null)
				{
					base.transform.SetParent(parent, worldPositionStays: false);
					base.transform.localPosition = localPosition;
				}
				base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				base.transform.localScale = localScale;
				RefreshMeshAndCollider();
			}
		}

		public void CenterPivot()
		{
			CenterPivot(Vector3.zero);
		}

		public void CenterPivot(Vector3 offset)
		{
			MeshFilter component = GetComponent<MeshFilter>();
			Mesh sharedMesh = component.sharedMesh;
			if (!(sharedMesh == null))
			{
				sharedMesh = UnityEngine.Object.Instantiate(sharedMesh);
				sharedMesh.name = component.sharedMesh.name;
				Vector3[] vertices = sharedMesh.vertices;
				Vector3 zero = Vector3.zero;
				for (int i = 0; i < vertices.Length; i++)
				{
					zero += vertices[i];
				}
				zero /= (float)vertices.Length;
				zero += offset;
				for (int j = 0; j < vertices.Length; j++)
				{
					vertices[j] -= zero;
				}
				sharedMesh.vertices = vertices;
				sharedMesh.RecalculateBounds();
				component.sharedMesh = sharedMesh;
				fixedMesh = sharedMesh;
				Vector3 localScale = base.transform.localScale;
				zero.x *= localScale.x;
				zero.y *= localScale.y;
				zero.z *= localScale.z;
				base.transform.localPosition += zero;
				RefreshMeshAndCollider();
			}
		}

		public void RefreshMeshAndCollider()
		{
			ClearMeshCache();
			MeshCollider component = GetComponent<MeshCollider>();
			if (component != null)
			{
				Mesh sharedMesh = component.sharedMesh;
				component.sharedMesh = null;
				component.sharedMesh = sharedMesh;
			}
		}

		public void Redraw()
		{
			UpdateMaterialProperties();
		}

		private void CheckMeshDisplacement()
		{
			MeshFilter component = GetComponent<MeshFilter>();
			if (component == null)
			{
				originalMesh = null;
				return;
			}
			Mesh sharedMesh = component.sharedMesh;
			if (sharedMesh == null)
			{
				if (!_fixMesh)
				{
					originalMesh = null;
					return;
				}
				if (fixedMesh != null)
				{
					component.sharedMesh = fixedMesh;
					return;
				}
				if (originalMesh != null)
				{
					component.sharedMesh = originalMesh;
				}
				sharedMesh = component.sharedMesh;
			}
			if (!_fixMesh)
			{
				RestoreOriginalMesh();
				originalMesh = null;
				return;
			}
			if (originalMesh == null || !originalMesh.name.Equals(sharedMesh.name))
			{
				originalMesh = component.sharedMesh;
			}
			if (sharedMesh != originalMesh)
			{
				RestoreOriginalMesh();
			}
			Vector3 localPosition = base.transform.localPosition;
			CenterPivot(_pivotOffset);
			originalPivotOffset = base.transform.localPosition - localPosition;
		}

		private void RestoreOriginalMesh()
		{
			fixedMesh = null;
			if (!(originalMesh == null))
			{
				MeshFilter component = GetComponent<MeshFilter>();
				if (!(component == null))
				{
					component.sharedMesh = originalMesh;
					base.transform.localPosition -= originalPivotOffset;
					RefreshMeshAndCollider();
				}
			}
		}

		public void CopyFrom(LiquidVolume lv)
		{
			if (!(lv == null))
			{
				_allowViewFromInside = lv._allowViewFromInside;
				_alpha = lv._alpha;
				_backDepthBias = lv._backDepthBias;
				_blurIntensity = lv._blurIntensity;
				_bumpDistortionOffset = lv._bumpDistortionOffset;
				_bumpDistortionScale = lv._bumpDistortionScale;
				_bumpMap = lv._bumpMap;
				_bumpStrength = lv._bumpStrength;
				_debugSpillPoint = lv._debugSpillPoint;
				_deepObscurance = lv._deepObscurance;
				_depthAware = lv._depthAware;
				_depthAwareCustomPass = lv._depthAwareCustomPass;
				_depthAwareCustomPassDebug = lv._depthAwareCustomPassDebug;
				_depthAwareOffset = lv._depthAwareOffset;
				_detail = lv._detail;
				_distortionAmount = lv._distortionAmount;
				_distortionMap = lv._distortionMap;
				_ditherShadows = lv._ditherShadows;
				_doubleSidedBias = lv._doubleSidedBias;
				_emissionColor = lv._emissionColor;
				_extentsScale = lv._extentsScale;
				_fixMesh = lv._fixMesh;
				_flaskThickness = lv._flaskThickness;
				_foamColor = lv._foamColor;
				_foamDensity = lv._foamDensity;
				_foamRaySteps = lv._foamRaySteps;
				_foamScale = lv._foamScale;
				_foamThickness = lv._foamThickness;
				_foamTurbulence = lv._foamTurbulence;
				_foamVisibleFromBottom = lv._foamVisibleFromBottom;
				_foamWeight = lv._foamWeight;
				_frecuency = lv._frecuency;
				_ignoreGravity = lv._ignoreGravity;
				_irregularDepthDebug = lv._irregularDepthDebug;
				_level = lv._level;
				_levelMultiplier = lv._levelMultiplier;
				_liquidColor1 = lv._liquidColor1;
				_liquidColor2 = lv._liquidColor2;
				_liquidRaySteps = lv._liquidRaySteps;
				_liquidScale1 = lv._liquidScale1;
				_liquidScale2 = lv._liquidScale2;
				_lowerLimit = lv._lowerLimit;
				_murkiness = lv._murkiness;
				_noiseVariation = lv._noiseVariation;
				_physicsAngularDamp = lv._physicsAngularDamp;
				_physicsMass = lv._physicsMass;
				_pivotOffset = lv._pivotOffset;
				_reactToForces = lv._reactToForces;
				_reflectionTexture = lv._reflectionTexture;
				_refractionBlur = lv._refractionBlur;
				_renderQueue = lv._renderQueue;
				_scatteringAmount = lv._scatteringAmount;
				_scatteringEnabled = lv._scatteringEnabled;
				_scatteringPower = lv._scatteringPower;
				_smokeBaseObscurance = lv._smokeBaseObscurance;
				_smokeColor = lv._smokeColor;
				_smokeEnabled = lv._smokeEnabled;
				_smokeHeightAtten = lv._smokeHeightAtten;
				_smokeRaySteps = lv._smokeRaySteps;
				_smokeScale = lv._smokeScale;
				_smokeSpeed = lv._smokeSpeed;
				_sparklingAmount = lv._sparklingAmount;
				_sparklingIntensity = lv._sparklingIntensity;
				_speed = lv._speed;
				_subMeshIndex = lv._subMeshIndex;
				_texture = lv._texture;
				_textureOffset = lv._textureOffset;
				_textureScale = lv._textureScale;
				_topology = lv._topology;
				_turbulence1 = lv._turbulence1;
				_turbulence2 = lv._turbulence2;
				_upperLimit = lv._upperLimit;
				shouldUpdateMaterialProperties = true;
			}
		}
	}
}
