using System;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGBoundsGroup : CGWeightedItem
	{
		public enum RotationModeEnum
		{
			Full = 0,
			Direction = 1,
			Horizontal = 2,
			Independent = 3
		}

		[UsedImplicitly]
		[Obsolete("Enum no more used by Curvy. This enum is kept for retro compatibility reasons")]
		private enum DistributionModeEnum
		{
			[UsedImplicitly]
			Parent = 0,
			Self = 1
		}

		[SerializeField]
		private string m_Name;

		[SerializeField]
		[Tooltip("When checked, the group will only be placed when all the group's items can be placed in the space left")]
		private bool m_KeepTogether;

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_SpaceBefore = new FloatRegion
		{
			SimpleValue = true
		};

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_SpaceAfter = new FloatRegion
		{
			SimpleValue = true
		};

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, RegionOptionsPropertyName = "PositionRangeOptions", UseSlider = true, Precision = 3)]
		private FloatRegion m_CrossBase = new FloatRegion(0f);

		[SerializeField]
		[Tooltip("If ticked, the Cross origin for this group will not take into consideration the Cross parameters in the General tab")]
		private bool m_IgnoreModuleCrossBase;

		[SerializeField]
		[Tooltip("When enabled, items will be selected randomly")]
		private bool m_RandomizeItems;

		[IntRegion(UseSlider = false, RegionOptionsPropertyName = "RepeatingGroupsOptions", Options = AttributeOptionsFlags.Compact)]
		[SerializeField]
		[Tooltip("The randomized items are the the ones that have their indices inside this range")]
		private IntRegion m_RepeatingItems;

		[SerializeField]
		[Tooltip("If unchecked, translation will be done in the global/world space")]
		private bool m_RelativeTranslation = true;

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_TranslationX = new FloatRegion(0f);

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_TranslationY = new FloatRegion(0f);

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_TranslationZ = new FloatRegion(0f);

		[SerializeField]
		[Tooltip("How the rotation axes are defined related to the Volume's data\r\n  - Full : Use Volume's direction and orientation\r\n  - Direction : Use Volume's direction only\r\n  - Horizontal : Use Volume's direction only after projecting it on XZ plane\r\n  - Independent : Do not use Volume's data")]
		private RotationModeEnum m_RotationMode;

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_RotationX = new FloatRegion(0f);

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_RotationY = new FloatRegion(0f);

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_RotationZ = new FloatRegion(0f);

		[SerializeField]
		[Tooltip("Whether the scaling is applied equally on all dimensions")]
		private bool m_UniformScaling = true;

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_ScaleX = new FloatRegion(1f);

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_ScaleY = new FloatRegion(1f);

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_ScaleZ = new FloatRegion(1f);

		[SerializeField]
		private List<CGBoundsGroupItem> m_Items = new List<CGBoundsGroupItem>();

		[SerializeField]
		[HideInInspector]
		[UsedImplicitly]
		[Obsolete("Use IgnoreModuleCrossBase instead. This field is kept for retro compatibility reasons")]
		private DistributionModeEnum m_DistributionMode;

		[SerializeField]
		[HideInInspector]
		[UsedImplicitly]
		[Obsolete("Use CrossBase instead. This field is kept for retro compatibility reasons")]
		[FloatRegion(RegionIsOptional = true, RegionOptionsPropertyName = "PositionRangeOptions", UseSlider = true, Precision = 3)]
		private FloatRegion m_PositionOffset = new FloatRegion(0f);

		[SerializeField]
		[HideInInspector]
		[UsedImplicitly]
		[Obsolete("Use TranslationY instead, while setting RelativeTranslation to true. This field is kept for retro compatibility reasons")]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_Height = new FloatRegion(0f);

		[SerializeField]
		[HideInInspector]
		[UsedImplicitly]
		[Obsolete("Use RandomizeItems instead. This field is kept for retro compatibility reasons")]
		private CurvyRepeatingOrderEnum m_RepeatingOrder = CurvyRepeatingOrderEnum.Row;

		[SerializeField]
		[HideInInspector]
		[UsedImplicitly]
		[Obsolete("Use RotationX, RotationY and RotationZ instead. This field is kept for retro compatibility reasons")]
		[VectorEx("", "")]
		private Vector3 m_RotationOffset;

		[SerializeField]
		[HideInInspector]
		[UsedImplicitly]
		[Obsolete("Use RotationX, RotationY and RotationZ instead. This field is kept for retro compatibility reasons")]
		[VectorEx("", "")]
		private Vector3 m_RotationScatter;

		public string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				m_Name = value;
			}
		}

		public bool KeepTogether
		{
			get
			{
				return m_KeepTogether;
			}
			set
			{
				m_KeepTogether = value;
			}
		}

		public FloatRegion SpaceBefore
		{
			get
			{
				return m_SpaceBefore;
			}
			set
			{
				m_SpaceBefore = value;
			}
		}

		public FloatRegion SpaceAfter
		{
			get
			{
				return m_SpaceAfter;
			}
			set
			{
				m_SpaceAfter = value;
			}
		}

		public bool RandomizeItems
		{
			get
			{
				return m_RandomizeItems;
			}
			set
			{
				m_RandomizeItems = value;
			}
		}

		public IntRegion RepeatingItems
		{
			get
			{
				return m_RepeatingItems;
			}
			set
			{
				m_RepeatingItems = value;
			}
		}

		public FloatRegion CrossBase
		{
			get
			{
				return m_CrossBase;
			}
			set
			{
				m_CrossBase = value;
			}
		}

		public bool IgnoreModuleCrossBase
		{
			get
			{
				return m_IgnoreModuleCrossBase;
			}
			set
			{
				m_IgnoreModuleCrossBase = value;
			}
		}

		public RotationModeEnum RotationMode
		{
			get
			{
				return m_RotationMode;
			}
			set
			{
				m_RotationMode = value;
			}
		}

		public FloatRegion RotationX
		{
			get
			{
				return m_RotationX;
			}
			set
			{
				m_RotationX = value;
			}
		}

		public FloatRegion RotationY
		{
			get
			{
				return m_RotationY;
			}
			set
			{
				m_RotationY = value;
			}
		}

		public FloatRegion RotationZ
		{
			get
			{
				return m_RotationZ;
			}
			set
			{
				m_RotationZ = value;
			}
		}

		public bool UniformScaling
		{
			get
			{
				return m_UniformScaling;
			}
			set
			{
				m_UniformScaling = value;
			}
		}

		public FloatRegion ScaleX
		{
			get
			{
				return m_ScaleX;
			}
			set
			{
				m_ScaleX = value;
			}
		}

		public FloatRegion ScaleY
		{
			get
			{
				return m_ScaleY;
			}
			set
			{
				m_ScaleY = value;
			}
		}

		public FloatRegion ScaleZ
		{
			get
			{
				return m_ScaleZ;
			}
			set
			{
				m_ScaleZ = value;
			}
		}

		public bool RelativeTranslation
		{
			get
			{
				return m_RelativeTranslation;
			}
			set
			{
				m_RelativeTranslation = value;
			}
		}

		public FloatRegion TranslationX
		{
			get
			{
				return m_TranslationX;
			}
			set
			{
				m_TranslationX = value;
			}
		}

		public FloatRegion TranslationY
		{
			get
			{
				return m_TranslationY;
			}
			set
			{
				m_TranslationY = value;
			}
		}

		public FloatRegion TranslationZ
		{
			get
			{
				return m_TranslationZ;
			}
			set
			{
				m_TranslationZ = value;
			}
		}

		public List<CGBoundsGroupItem> Items => m_Items;

		public int FirstRepeating
		{
			get
			{
				return m_RepeatingItems.From;
			}
			set
			{
				int num = Mathf.Clamp(value, 0, Mathf.Max(0, ItemCount - 1));
				if (m_RepeatingItems.From != num)
				{
					m_RepeatingItems.From = num;
				}
			}
		}

		public int LastRepeating
		{
			get
			{
				return m_RepeatingItems.To;
			}
			set
			{
				int num = Mathf.Clamp(value, FirstRepeating, Mathf.Max(0, ItemCount - 1));
				if (m_RepeatingItems.To != num)
				{
					m_RepeatingItems.To = num;
				}
			}
		}

		public int ItemCount => Items.Count;

		private RegionOptions<int> RepeatingGroupsOptions => RegionOptions<int>.MinMax(0, Mathf.Max(0, ItemCount - 1));

		private RegionOptions<float> PositionRangeOptions => RegionOptions<float>.MinMax(-1f, 1f);

		public CGBoundsGroup(string name)
		{
			Name = name;
		}

		public static void FillItemBag(WeightedRandom<int> bag, IEnumerable<CGWeightedItem> itemsWeights, int firstItem, int lastItem)
		{
			for (int i = firstItem; i <= lastItem; i++)
			{
				bag.Add(i, (int)(itemsWeights.ElementAt(i).Weight * 10f));
			}
			if (bag.Size == 0)
			{
				bag.Add(firstItem, 1);
			}
		}

		[UsedImplicitly]
		[Obsolete("Method will get removed once the obsolete data will get removed")]
		public void ConvertObsoleteData()
		{
			RandomizeItems = m_RepeatingOrder == CurvyRepeatingOrderEnum.Random;
			IgnoreModuleCrossBase = m_DistributionMode == DistributionModeEnum.Self;
			CrossBase = m_PositionOffset;
			if (m_Height.From != 0f || (!m_Height.SimpleValue && m_Height.To != 0f))
			{
				TranslationY = m_Height;
				RelativeTranslation = true;
			}
			float num = m_RotationOffset.x - m_RotationScatter.x;
			float num2 = m_RotationOffset.x + m_RotationScatter.x;
			RotationX = ((num == num2) ? new FloatRegion(num) : new FloatRegion(num, num2));
			float num3 = m_RotationOffset.y - m_RotationScatter.y;
			float num4 = m_RotationOffset.y + m_RotationScatter.y;
			RotationY = ((num3 == num4) ? new FloatRegion(num3) : new FloatRegion(num3, num4));
			float num5 = m_RotationOffset.z - m_RotationScatter.z;
			float num6 = m_RotationOffset.z + m_RotationScatter.z;
			RotationZ = ((num5 == num6) ? new FloatRegion(num5) : new FloatRegion(num5, num6));
		}
	}
}
