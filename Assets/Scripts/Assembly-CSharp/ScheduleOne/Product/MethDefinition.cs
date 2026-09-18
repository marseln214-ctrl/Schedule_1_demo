using System;
using System.Collections.Generic;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Properties;
using UnityEngine;

namespace ScheduleOne.Product
{
	[Serializable]
	[CreateAssetMenu(fileName = "MethDefinition", menuName = "ScriptableObjects/Item Definitions/MethDefinition", order = 1)]
	public class MethDefinition : ProductDefinition
	{
		public Material CrystalMaterial;

		public MethAppearanceSettings AppearanceSettings { get; private set; }

		public override ItemInstance GetDefaultInstance(int quantity = 1)
		{
			return new MethInstance(this, quantity, EQuality.Standard);
		}

		public void Initialize(List<ScheduleOne.Properties.Property> properties, List<EDrugType> drugTypes, MethAppearanceSettings _appearance)
		{
			Initialize(properties, drugTypes);
			if (_appearance == null || _appearance.IsUnintialized())
			{
				Console.LogWarning("Meth definition " + Name + " has no or uninitialized appearance settings! Generating new");
				_appearance = GetAppearanceSettings(properties);
			}
			AppearanceSettings = _appearance;
			Console.Log("Initializing meth definition: " + Name);
			CrystalMaterial = new Material(CrystalMaterial);
			CrystalMaterial.color = AppearanceSettings.MainColor;
		}

		public override string GetSaveString()
		{
			string[] array = new string[Properties.Count];
			for (int i = 0; i < Properties.Count; i++)
			{
				array[i] = Properties[i].ID;
			}
			return new MethProductData(Name, ID, DrugTypes[0].DrugType, array, AppearanceSettings).GetJson();
		}

		public static MethAppearanceSettings GetAppearanceSettings(List<ScheduleOne.Properties.Property> properties)
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
			Color32 mainColor = Color32.Lerp(a, list[0], (float)properties[0].Tier * 0.2f);
			Color32 secondaryColor = Color32.Lerp(a2, Color32.Lerp(list[0], list[1], 0.5f), (properties.Count > 1) ? ((float)properties[1].Tier * 0.2f) : 0.5f);
			return new MethAppearanceSettings
			{
				MainColor = mainColor,
				SecondaryColor = secondaryColor
			};
		}
	}
}
