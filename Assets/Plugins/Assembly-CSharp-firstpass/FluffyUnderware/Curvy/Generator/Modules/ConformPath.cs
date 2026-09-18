using System;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/Conform Path", ModuleName = "Conform Path", Description = "Projects a path")]
	[HelpURL("https://curvyeditor.com/doclink/cgconformpath")]
	public class ConformPath : CGModule, IOnRequestProcessing, IPathProvider
	{
		private const int DefaultMaxDistance = 100;

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path", ModifiesData = true)]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGPath))]
		public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

		[SerializeField]
		[VectorEx("", "")]
		[Tooltip("The direction to raycast in ")]
		private Vector3 m_Direction = new Vector3(0f, -1f, 0f);

		[SerializeField]
		[Tooltip("The maximum raycast distance")]
		private float m_MaxDistance = 100f;

		[SerializeField]
		[Tooltip("Defines an offset shift along the raycast direction")]
		private float m_Offset;

		[SerializeField]
		[Tooltip("If enabled, the entire path is moved to the nearest possible distance. If disabled, each path point is moved individually")]
		private bool m_Warp;

		[SerializeField]
		[Tooltip("The layers to raycast against")]
		private LayerMask m_LayerMask;

		public Vector3 Direction
		{
			get
			{
				return m_Direction;
			}
			set
			{
				if (m_Direction != value)
				{
					m_Direction = value;
					base.Dirty = true;
				}
			}
		}

		public float MaxDistance
		{
			get
			{
				return m_MaxDistance;
			}
			set
			{
				if (m_MaxDistance != value)
				{
					m_MaxDistance = value;
					base.Dirty = true;
				}
			}
		}

		public float Offset
		{
			get
			{
				return m_Offset;
			}
			set
			{
				if (m_Offset != value)
				{
					m_Offset = value;
					base.Dirty = true;
				}
			}
		}

		public bool Warp
		{
			get
			{
				return m_Warp;
			}
			set
			{
				if (m_Warp != value)
				{
					m_Warp = value;
					base.Dirty = true;
				}
			}
		}

		public LayerMask LayerMask
		{
			get
			{
				return m_LayerMask;
			}
			set
			{
				if ((int)m_LayerMask != (int)value)
				{
					m_LayerMask = value;
					base.Dirty = true;
				}
			}
		}

		public bool PathIsClosed
		{
			get
			{
				if (IsConfigured)
				{
					return InPath.SourceSlot().PathProvider.PathIsClosed;
				}
				return false;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.LabelWidth = 80f;
		}

		public override void Reset()
		{
			base.Reset();
			Direction = new Vector3(0f, -1f, 0f);
			MaxDistance = 100f;
			Offset = 0f;
			Warp = false;
			LayerMask = 0;
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			if (!CGModule.GetRequestParameter<CGDataRequestRasterization>(ref requests))
			{
				return Array.Empty<CGData>();
			}
			if ((int)LayerMask == 0)
			{
				UIMessages.Add("Please set a Layer Mask different than Nothing.");
			}
			bool isDataDisposable;
			CGPath data = InPath.GetData<CGPath>(out isDataDisposable, requests);
			if (data == null)
			{
				return Array.Empty<CGData>();
			}
			Conform(data, base.Generator.transform, LayerMask, Direction, Offset, MaxDistance, Warp);
			return new CGData[1] { data };
		}

		public static void Conform(CGPath path, Transform pathTransform, LayerMask layers, Vector3 projectionDirection, float offset, float rayLength, bool warp)
		{
			Conform(pathTransform, path, layers, projectionDirection, offset, rayLength, warp);
		}

		[UsedImplicitly]
		[Obsolete("Use the other override")]
		public static CGPath Conform(Transform pathTransform, CGPath path, LayerMask layers, Vector3 projectionDirection, float offset, float rayLength, bool warp)
		{
			if (path == null)
			{
				return null;
			}
			int count = path.Count;
			if (projectionDirection != Vector3.zero && rayLength > 0f && count > 0)
			{
				RaycastHit hitInfo;
				if (warp)
				{
					float num = float.MaxValue;
					for (int i = 0; i < count; i++)
					{
						if (Physics.Raycast(pathTransform.TransformPoint(path.Positions.Array[i]), projectionDirection, out hitInfo, rayLength, layers) && hitInfo.distance < num)
						{
							num = hitInfo.distance;
						}
					}
					if (num != float.MaxValue)
					{
						Vector3 vector = projectionDirection * (num + offset);
						for (int j = 0; j < path.Count; j++)
						{
							path.Positions.Array[j] += vector;
						}
					}
				}
				else
				{
					for (int k = 0; k < count; k++)
					{
						if (Physics.Raycast(pathTransform.TransformPoint(path.Positions.Array[k]), projectionDirection, out hitInfo, rayLength, layers))
						{
							path.Positions.Array[k] += projectionDirection * (hitInfo.distance + offset);
						}
					}
				}
				path.Recalculate();
			}
			return path;
		}
	}
}
