using System;
using System.Collections.Generic;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Properties;
using UnityEngine;

namespace ScheduleOne.Product
{
	[Serializable]
	[CreateAssetMenu(fileName = "WeedDefinition", menuName = "ScriptableObjects/Item Definitions/WeedDefinition", order = 1)]
	public class WeedDefinition : ProductDefinition
	{
		[Header("Weed Materials")]
		public Material MainMat;

		public Material SecondaryMat;

		public Material LeafMat;

		public Material StemMat;

		private WeedAppearanceSettings appearance;

		public override ItemInstance GetDefaultInstance(int quantity = 1)
		{
			return new WeedInstance(this, quantity, EQuality.Standard);
		}

		public void Initialize(List<ScheduleOne.Properties.Property> properties, List<EDrugType> drugTypes, WeedAppearanceSettings _appearance)
		{
			Initialize(properties, drugTypes);
			if (_appearance == null || _appearance.IsUnintialized())
			{
				Console.LogWarning("Weed definition " + Name + " has no or uninitialized appearance settings! Generating new");
				_appearance = GetAppearanceSettings(properties);
			}
			appearance = _appearance;
			Console.Log("Initializing weed definition: " + Name);
			MainMat = new Material(MainMat);
			MainMat.color = appearance.MainColor;
			SecondaryMat = new Material(SecondaryMat);
			SecondaryMat.color = appearance.SecondaryColor;
			LeafMat = new Material(LeafMat);
			LeafMat.color = appearance.LeafColor;
			StemMat = new Material(StemMat);
			StemMat.color = appearance.StemColor;
		}

		public override string GetSaveString()
		{
			string[] array = new string[Properties.Count];
			for (int i = 0; i < Properties.Count; i++)
			{
				array[i] = Properties[i].ID;
			}
			return new WeedProductData(Name, ID, DrugTypes[0].DrugType, array, appearance).GetJson();
		}

		public static WeedAppearanceSettings GetAppearanceSettings(List<ScheduleOne.Properties.Property> properties)
		{
			properties.Sort((ScheduleOne.Properties.Property x, ScheduleOne.Properties.Property y) => x.Tier.CompareTo(y.Tier));
			List<Color32> list = new List<Color32>();
			foreach (ScheduleOne.Properties.Property property in properties)
			{
				list.Add(property.ProductColor);
			}
			if (list.Count == 1)
			{
				list.Add(list[0]);
			}
			Color32 a = new Color32(90, 100, 70, byte.MaxValue);
			Color32 a2 = new Color32(120, 120, 80, byte.MaxValue);
			Color32 color = Color32.Lerp(a, list[0], (float)properties[0].Tier * 0.15f);
			Color32 color2 = Color32.Lerp(a2, Color32.Lerp(list[0], list[1], 0.5f), (properties.Count > 1) ? ((float)properties[1].Tier * 0.2f) : 0.5f);
			Color32 a3 = new Color32(0, 0, 0, byte.MaxValue);
			return new WeedAppearanceSettings
			{
				MainColor = color,
				SecondaryColor = color2,
				LeafColor = Color32.Lerp(color, color2, 0.5f),
				StemColor = Color32.Lerp(a3, color, 0.8f)
			};
		}
	}
}
