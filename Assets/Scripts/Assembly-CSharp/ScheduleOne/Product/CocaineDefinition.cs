using System;
using System.Collections.Generic;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Properties;
using UnityEngine;

namespace ScheduleOne.Product
{
	[Serializable]
	[CreateAssetMenu(fileName = "CocaineDefinition", menuName = "ScriptableObjects/Item Definitions/CocaineDefinition", order = 1)]
	public class CocaineDefinition : ProductDefinition
	{
		[Header("Materials")]
		public Material RockMaterial;

		public CocaineAppearanceSettings AppearanceSettings { get; private set; }

		public override ItemInstance GetDefaultInstance(int quantity = 1)
		{
			return new CocaineInstance(this, quantity, EQuality.Standard);
		}

		public void Initialize(List<ScheduleOne.Properties.Property> properties, List<EDrugType> drugTypes, CocaineAppearanceSettings _appearance)
		{
			Initialize(properties, drugTypes);
			if (_appearance == null || _appearance.IsUnintialized())
			{
				Console.LogWarning("Coke definition " + Name + " has no or uninitialized appearance settings! Generating new");
				_appearance = GetAppearanceSettings(properties);
			}
			AppearanceSettings = _appearance;
			Console.Log("Initializing weed definition: " + Name);
			RockMaterial = new Material(RockMaterial);
			RockMaterial.color = AppearanceSettings.MainColor;
		}

		public override string GetSaveString()
		{
			string[] array = new string[Properties.Count];
			for (int i = 0; i < Properties.Count; i++)
			{
				array[i] = Properties[i].ID;
			}
			return new CocaineProductData(Name, ID, DrugTypes[0].DrugType, array, AppearanceSettings).GetJson();
		}

		public static CocaineAppearanceSettings GetAppearanceSettings(List<ScheduleOne.Properties.Property> properties)
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
			Color32 a = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			Color32 a2 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			Color32 mainColor = Color32.Lerp(a, list[0], (float)properties[0].Tier * 0.13f);
			Color32 secondaryColor = Color32.Lerp(a2, Color32.Lerp(list[0], list[1], 0.5f), (properties.Count > 1) ? ((float)properties[1].Tier * 0.2f) : 0.5f);
			return new CocaineAppearanceSettings
			{
				MainColor = mainColor,
				SecondaryColor = secondaryColor
			};
		}
	}
}
