using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace FluffyUnderware.DevTools
{
	[Serializable]
	public class PoolSettings
	{
		[Header("General")]
		[SerializeField]
		[Label("Auto Create Items", "")]
		[Tooltip("Automatically create items when an item is requested and none is available")]
		private bool m_AutoCreate;

		[SerializeField]
		[Label("Auto Enable/Disable Items", "")]
		[Tooltip("Automatically disable objects when entering the pool and enable them when leaving it")]
		private bool m_AutoEnableDisable;

		[Label("Debug mode", "")]
		[Tooltip("Log operations and show pooled objects in the hierarchy")]
		public bool Debug;

		[Header("Item Count Constraints")]
		[Positive]
		[SerializeField]
		[FormerlySerializedAs("m_MinItems")]
		[Tooltip("Minimum number of items in the pool")]
		private int minimumCount;

		[Positive]
		[SerializeField]
		[FormerlySerializedAs("m_Threshold")]
		[Tooltip("Maximum number of items in the pool")]
		private int maximumCount;

		[Positive]
		[SerializeField]
		[FormerlySerializedAs("m_Speed")]
		[Label("Adjustment Interval", "")]
		[Tooltip("Number of seconds between item count adjustments.\r\nIf 0, adjustments are instantaneous.")]
		private float countAdjustmentInterval;

		[SerializeField]
		[FormerlySerializedAs("m_Prewarm")]
		[Label("Initialize Constrained", "")]
		[Tooltip("Initialize the pool with its item count already within the constraints")]
		private bool initializeCountConstrained;

		public bool InitializeCountConstrained
		{
			get
			{
				return initializeCountConstrained;
			}
			set
			{
				initializeCountConstrained = value;
			}
		}

		public bool AutoCreate
		{
			get
			{
				return m_AutoCreate;
			}
			set
			{
				m_AutoCreate = value;
			}
		}

		public bool AutoEnableDisable
		{
			get
			{
				return m_AutoEnableDisable;
			}
			set
			{
				m_AutoEnableDisable = value;
			}
		}

		public int MinimumCount
		{
			get
			{
				return minimumCount;
			}
			set
			{
				minimumCount = Mathf.Max(0, value);
			}
		}

		public int MaximumCount
		{
			get
			{
				return maximumCount;
			}
			set
			{
				maximumCount = Mathf.Max(MinimumCount, value);
			}
		}

		public float CountAdjustmentInterval
		{
			get
			{
				return countAdjustmentInterval;
			}
			set
			{
				countAdjustmentInterval = Mathf.Max(0f, value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use InitializeCountConstrained instead")]
		public bool Prewarm
		{
			get
			{
				return InitializeCountConstrained;
			}
			set
			{
				InitializeCountConstrained = value;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use MinimumCount instead")]
		public int MinItems
		{
			get
			{
				return MinimumCount;
			}
			set
			{
				MinimumCount = value;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use MaximumCount instead")]
		public int Threshold
		{
			get
			{
				return MaximumCount;
			}
			set
			{
				MaximumCount = value;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use CountAdjustmentInterval instead")]
		public float Speed
		{
			get
			{
				return CountAdjustmentInterval;
			}
			set
			{
				CountAdjustmentInterval = value;
			}
		}

		public PoolSettings()
		{
			SetToDefault();
		}

		public PoolSettings(PoolSettings src)
		{
			InitializeCountConstrained = src.InitializeCountConstrained;
			AutoCreate = src.AutoCreate;
			AutoEnableDisable = src.AutoEnableDisable;
			MinimumCount = src.MinimumCount;
			MaximumCount = src.MaximumCount;
			CountAdjustmentInterval = src.CountAdjustmentInterval;
			Debug = src.Debug;
		}

		public void SetToDefault()
		{
			MinimumCount = 0;
			MaximumCount = 50;
			CountAdjustmentInterval = 1f;
			InitializeCountConstrained = true;
			AutoCreate = true;
			AutoEnableDisable = true;
			Debug = false;
			Validate();
		}

		[UsedImplicitly]
		[Obsolete("Use Validate instead")]
		public void OnValidate()
		{
			Validate();
		}

		public void Validate()
		{
			MinimumCount = minimumCount;
			MaximumCount = maximumCount;
			CountAdjustmentInterval = countAdjustmentInterval;
		}
	}
}
