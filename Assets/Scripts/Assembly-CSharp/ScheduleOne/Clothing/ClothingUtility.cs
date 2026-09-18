using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Clothing
{
	public class ClothingUtility : Singleton<ClothingUtility>
	{
		[Serializable]
		public class ColorData
		{
			public EClothingColor ColorType;

			public Color ActualColor;

			public Color LabelColor;
		}

		[Serializable]
		public class ClothingSlotData
		{
			public EClothingSlot Slot;

			public string Name;

			public Sprite Icon;
		}

		public List<ColorData> ColorDataList = new List<ColorData>();

		public List<ClothingSlotData> ClothingSlotDataList = new List<ClothingSlotData>();

		protected override void Awake()
		{
			base.Awake();
			foreach (EClothingColor color in Enum.GetValues(typeof(EClothingColor)))
			{
				if (ColorDataList.Find((ColorData x) => x.ColorType == color) == null)
				{
					Debug.LogError("Color " + color.ToString() + " is missing from the ColorDataList");
				}
			}
		}

		private void OnValidate()
		{
			foreach (EClothingColor color in Enum.GetValues(typeof(EClothingColor)))
			{
				if (ColorDataList.Find((ColorData x) => x.ColorType == color) == null)
				{
					ColorDataList.Add(new ColorData
					{
						ColorType = color,
						ActualColor = Color.white,
						LabelColor = Color.white
					});
				}
			}
			foreach (EClothingSlot slot in Enum.GetValues(typeof(EClothingSlot)))
			{
				if (ClothingSlotDataList.Find((ClothingSlotData x) => x.Slot == slot) == null)
				{
					ClothingSlotDataList.Add(new ClothingSlotData
					{
						Slot = slot,
						Name = slot.ToString(),
						Icon = null
					});
				}
			}
		}

		public ColorData GetColorData(EClothingColor color)
		{
			return ColorDataList.Find((ColorData x) => x.ColorType == color);
		}

		public ClothingSlotData GetSlotData(EClothingSlot slot)
		{
			return ClothingSlotDataList.Find((ClothingSlotData x) => x.Slot == slot);
		}
	}
}
