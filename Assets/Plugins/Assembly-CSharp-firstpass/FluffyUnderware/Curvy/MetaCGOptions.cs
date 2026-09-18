using System;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[HelpURL("https://curvyeditor.com/doclink/metacgoptions")]
	public class MetaCGOptions : CurvyMetadataBase
	{
		[Positive]
		[SerializeField]
		private int m_MaterialID;

		[SerializeField]
		[FieldCondition("ShowUvEdgeOrHardEdge", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		private bool m_HardEdge;

		[Positive(Tooltip = "Max step distance when using optimization")]
		[SerializeField]
		private float m_MaxStepDistance;

		[Section("Extended UV", true, false, 100, HelpURL = "https://curvyeditor.com/doclink/metacgoptions_extendeduv")]
		[FieldCondition("ShowUvEdgeOrHardEdge", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private bool m_UVEdge;

		[Positive]
		[FieldCondition("showExplicitU", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private bool m_ExplicitU;

		[FieldCondition("showFirstU", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[FieldAction("CBSetFirstU", ActionAttribute.ActionEnum.Callback)]
		[Positive]
		[SerializeField]
		private float m_FirstU;

		[FieldCondition("showSecondU", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[Positive]
		[SerializeField]
		private float m_SecondU;

		[SerializeField]
		[HideInInspector]
		private bool uVEdgeUpdated;

		private const int DefaultMaterialId = 0;

		public int MaterialID
		{
			get
			{
				return m_MaterialID;
			}
			set
			{
				int num = Mathf.Max(0, value);
				if (m_MaterialID != num)
				{
					m_MaterialID = num;
					if (base.IsActiveAndEnabled)
					{
						NotifyModification();
					}
				}
			}
		}

		public bool HardEdge
		{
			get
			{
				return m_HardEdge;
			}
			set
			{
				if (m_HardEdge != value)
				{
					m_HardEdge = value;
					if (base.IsActiveAndEnabled)
					{
						NotifyModification();
					}
				}
			}
		}

		public bool CorrectedHardEdge
		{
			get
			{
				if (CanHaveUvEdgeOrHadrdEdge())
				{
					return HardEdge;
				}
				return false;
			}
		}

		public bool UVEdge
		{
			get
			{
				return m_UVEdge;
			}
			set
			{
				if (m_UVEdge != value)
				{
					m_UVEdge = value;
					if (base.IsActiveAndEnabled)
					{
						NotifyModification();
					}
				}
			}
		}

		public bool CorrectedUVEdge
		{
			get
			{
				if (CanHaveUvEdgeOrHadrdEdge())
				{
					return UVEdge;
				}
				return false;
			}
		}

		public bool ExplicitU
		{
			get
			{
				return m_ExplicitU;
			}
			set
			{
				if (m_ExplicitU != value)
				{
					m_ExplicitU = value;
					if (base.IsActiveAndEnabled)
					{
						NotifyModification();
					}
				}
			}
		}

		public float FirstU
		{
			get
			{
				return m_FirstU;
			}
			set
			{
				if (m_FirstU != value)
				{
					m_FirstU = value;
					if (base.IsActiveAndEnabled)
					{
						NotifyModification();
					}
				}
			}
		}

		public float SecondU
		{
			get
			{
				return m_SecondU;
			}
			set
			{
				if (m_SecondU != value)
				{
					m_SecondU = value;
					if (base.IsActiveAndEnabled)
					{
						NotifyModification();
					}
				}
			}
		}

		public float MaxStepDistance
		{
			get
			{
				return m_MaxStepDistance;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_MaxStepDistance != num)
				{
					m_MaxStepDistance = num;
					if (base.IsActiveAndEnabled)
					{
						NotifyModification();
					}
				}
			}
		}

		public bool HasDifferentMaterial
		{
			get
			{
				MetaCGOptions previousData = GetPreviousData<MetaCGOptions>(autoCreate: false);
				return ((!(previousData == null)) ? previousData.MaterialID : 0) != MaterialID;
			}
		}

		private bool ShowUvEdgeOrHardEdge
		{
			get
			{
				if ((bool)base.ControlPoint)
				{
					return CanHaveUvEdgeOrHadrdEdge();
				}
				return false;
			}
		}

		private bool showExplicitU
		{
			get
			{
				if ((bool)base.ControlPoint)
				{
					return !showSecondU;
				}
				return false;
			}
		}

		private bool showFirstU
		{
			get
			{
				if (!ExplicitU)
				{
					return CorrectedUVEdge;
				}
				return true;
			}
		}

		private bool showSecondU => CorrectedUVEdge;

		protected override void OnValidate()
		{
			base.OnValidate();
			if (base.IsActiveAndEnabled)
			{
				NotifyModification();
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			NotifyModification();
		}

		protected override void Awake()
		{
			base.Awake();
			EnsureUVEdgeUpdate();
		}

		[UsedImplicitly]
		[Obsolete("Use ResetProperties instead")]
		public new void Reset()
		{
			base.Reset();
			ResetProperties();
		}

		public float GetDefinedFirstU(float defaultValue)
		{
			if (!CorrectedUVEdge && !ExplicitU)
			{
				return defaultValue;
			}
			return FirstU;
		}

		public float GetDefinedSecondU(float defaultValue)
		{
			if (!CorrectedUVEdge)
			{
				return GetDefinedFirstU(defaultValue);
			}
			return SecondU;
		}

		public void ResetProperties()
		{
			MaterialID = 0;
			HardEdge = false;
			MaxStepDistance = 0f;
			UVEdge = false;
			ExplicitU = false;
			FirstU = 0f;
			SecondU = 0f;
		}

		private void EnsureUVEdgeUpdate()
		{
			if (!uVEdgeUpdated)
			{
				m_UVEdge = m_UVEdge || (HasDifferentMaterial && (FirstU != 0f || SecondU != 0f));
				uVEdgeUpdated = true;
			}
		}

		private bool CanHaveUvEdgeOrHadrdEdge()
		{
			if (!base.Spline.Closed)
			{
				if (base.Spline.FirstVisibleControlPoint != base.ControlPoint)
				{
					return base.Spline.LastVisibleControlPoint != base.ControlPoint;
				}
				return false;
			}
			return true;
		}
	}
}
