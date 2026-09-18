using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(PoolManager))]
	[RequireComponent(typeof(ArrayPoolsSettings))]
	[HelpURL("https://curvyeditor.com/doclink/curvyglobalmanager")]
	public class CurvyGlobalManager : DTSingleton<CurvyGlobalManager>
	{
		public static readonly Color DefaultDefaultGizmoColor = new Color(0.71f, 0.71f, 0.71f);

		public static readonly Color DefaultDefaultGizmoSelectionColor = new Color(0.6f, 0.15f, 0.68f);

		public static readonly Color DefaultGizmoOrientationColor = new Color(0.75f, 0.75f, 0.4f);

		public static bool HideManager;

		public static bool SaveGeneratorOutputs = true;

		public static float SceneViewResolution = 0.5f;

		public static Color DefaultGizmoColor = DefaultDefaultGizmoColor;

		public static Color DefaultGizmoSelectionColor = DefaultDefaultGizmoSelectionColor;

		public static CurvyInterpolation DefaultInterpolation = CurvyInterpolation.CatmullRom;

		public static float GizmoControlPointSize = 0.15f;

		public static float GizmoOrientationLength = 1f;

		public static Color GizmoOrientationColor = DefaultGizmoOrientationColor;

		public static int SplineLayer;

		public static CurvySplineGizmos Gizmos = CurvySplineGizmos.Connections | CurvySplineGizmos.Curve;

		private PoolManager poolManager;

		private ComponentPool controlPointPool;

		private ArrayPoolsSettings arrayPoolsSettings;

		public static bool ShowCurveGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Curve) == CurvySplineGizmos.Curve;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Curve;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Curve;
				}
			}
		}

		public static bool ShowConnectionsGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Connections) == CurvySplineGizmos.Connections;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Connections;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Connections;
				}
			}
		}

		public static bool ShowApproximationGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Approximation) == CurvySplineGizmos.Approximation;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Approximation;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Approximation;
				}
			}
		}

		public static bool ShowTangentsGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Tangents) == CurvySplineGizmos.Tangents;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Tangents;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Tangents;
				}
			}
		}

		public static bool ShowOrientationGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Orientation) == CurvySplineGizmos.Orientation;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Orientation;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Orientation;
				}
			}
		}

		public static bool ShowTFsGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.TFs) == CurvySplineGizmos.TFs;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.TFs;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.TFs;
				}
			}
		}

		public static bool ShowRelativeDistancesGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.RelativeDistances) == CurvySplineGizmos.RelativeDistances;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.RelativeDistances;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.RelativeDistances;
				}
			}
		}

		public static bool ShowLabelsGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Labels) == CurvySplineGizmos.Labels;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Labels;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Labels;
				}
			}
		}

		public static bool ShowMetadataGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Metadata) == CurvySplineGizmos.Metadata;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Metadata;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Metadata;
				}
			}
		}

		public static bool ShowBoundsGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Bounds) == CurvySplineGizmos.Bounds;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Bounds;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Bounds;
				}
			}
		}

		public static bool ShowOrientationAnchorsGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.OrientationAnchors) == CurvySplineGizmos.OrientationAnchors;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.OrientationAnchors;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.OrientationAnchors;
				}
			}
		}

		public PoolManager PoolManager
		{
			get
			{
				if (poolManager == null)
				{
					poolManager = GetComponent<PoolManager>();
				}
				return poolManager;
			}
		}

		public ComponentPool ControlPointPool => controlPointPool;

		public ArrayPoolsSettings ArrayPoolsSettings
		{
			get
			{
				if (arrayPoolsSettings == null)
				{
					arrayPoolsSettings = GetComponent<ArrayPoolsSettings>();
				}
				return arrayPoolsSettings;
			}
		}

		public CurvyConnection[] Connections => GetComponentsInChildren<CurvyConnection>();

		[UsedImplicitly]
		[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
		public CurvyConnection[] GetContainingConnections(params CurvySpline[] splines)
		{
			List<CurvyConnection> list = new List<CurvyConnection>();
			List<CurvySpline> list2 = new List<CurvySpline>(splines);
			foreach (CurvySpline item in list2)
			{
				foreach (CurvySplineSegment controlPoints in item.ControlPointsList)
				{
					if (!(controlPoints.Connection != null) || list.Contains(controlPoints.Connection))
					{
						continue;
					}
					bool flag = true;
					foreach (CurvySplineSegment controlPoints2 in controlPoints.Connection.ControlPointsList)
					{
						if (controlPoints2.Spline != null && !list2.Contains(controlPoints2.Spline))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						list.Add(controlPoints.Connection);
					}
				}
			}
			return list.ToArray();
		}

		public override void Awake()
		{
			base.Awake();
			if (!(this == null))
			{
				base.name = "_CurvyGlobal_";
				base.transform.SetAsLastSibling();
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(this);
				}
				poolManager = GetComponent<PoolManager>();
				controlPointPool = poolManager.CreateComponentPool<CurvySplineSegment>(new PoolSettings());
				arrayPoolsSettings = GetComponent<ArrayPoolsSettings>();
				if (arrayPoolsSettings == null)
				{
					arrayPoolsSettings = base.gameObject.AddComponent<ArrayPoolsSettings>();
				}
			}
		}

		[UsedImplicitly]
		private void Start()
		{
			if (HideManager)
			{
				base.gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
			else
			{
				base.gameObject.hideFlags = HideFlags.None;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		[UsedImplicitly]
		private static void InitializeOnLoad()
		{
			DTSingleton<CurvyGlobalManager>.InitializeStaticFields();
		}

		[RuntimeInitializeOnLoadMethod]
		[UsedImplicitly]
		private static void LoadRuntimeSettings()
		{
			if (!PlayerPrefs.HasKey("Curvy_MaxCachePPU"))
			{
				SaveRuntimeSettings();
			}
			SceneViewResolution = DTUtility.GetPlayerPrefs("Curvy_SceneViewResolution", SceneViewResolution);
			HideManager = DTUtility.GetPlayerPrefs("Curvy_HideManager", HideManager);
			DefaultGizmoColor = DTUtility.GetPlayerPrefs("Curvy_DefaultGizmoColor", DefaultGizmoColor);
			DefaultGizmoSelectionColor = DTUtility.GetPlayerPrefs("Curvy_DefaultGizmoSelectionColor", DefaultGizmoColor);
			DefaultInterpolation = DTUtility.GetPlayerPrefs("Curvy_DefaultInterpolation", DefaultInterpolation);
			GizmoControlPointSize = DTUtility.GetPlayerPrefs("Curvy_ControlPointSize", GizmoControlPointSize);
			GizmoOrientationLength = DTUtility.GetPlayerPrefs("Curvy_OrientationLength", GizmoOrientationLength);
			GizmoOrientationColor = DTUtility.GetPlayerPrefs("Curvy_OrientationColor", GizmoOrientationColor);
			Gizmos = DTUtility.GetPlayerPrefs("Curvy_Gizmos", Gizmos);
			SplineLayer = DTUtility.GetPlayerPrefs("Curvy_SplineLayer", SplineLayer);
			SaveGeneratorOutputs = DTUtility.GetPlayerPrefs("Curvy_SaveGeneratorOutputs", SaveGeneratorOutputs);
		}

		public static void SaveRuntimeSettings()
		{
			DTUtility.SetPlayerPrefs("Curvy_SceneViewResolution", SceneViewResolution);
			DTUtility.SetPlayerPrefs("Curvy_HideManager", HideManager);
			DTUtility.SetPlayerPrefs("Curvy_DefaultGizmoColor", DefaultGizmoColor);
			DTUtility.SetPlayerPrefs("Curvy_DefaultGizmoSelectionColor", DefaultGizmoSelectionColor);
			DTUtility.SetPlayerPrefs("Curvy_DefaultInterpolation", DefaultInterpolation);
			DTUtility.SetPlayerPrefs("Curvy_ControlPointSize", GizmoControlPointSize);
			DTUtility.SetPlayerPrefs("Curvy_OrientationLength", GizmoOrientationLength);
			DTUtility.SetPlayerPrefs("Curvy_OrientationColor", GizmoOrientationColor);
			DTUtility.SetPlayerPrefs("Curvy_Gizmos", Gizmos);
			DTUtility.SetPlayerPrefs("Curvy_SplineLayer", SplineLayer);
			DTUtility.SetPlayerPrefs("Curvy_SaveGeneratorOutputs", SaveGeneratorOutputs);
			PlayerPrefs.Save();
		}

		public override void MergeDoubleLoaded(IDTSingleton newInstance)
		{
			base.MergeDoubleLoaded(newInstance);
			CurvyConnection[] connections = (newInstance as CurvyGlobalManager).Connections;
			for (int i = 0; i < connections.Length; i++)
			{
				connections[i].transform.SetParent(base.transform);
			}
		}
	}
}
