using System;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Properties;
using UnityEngine;

namespace ScheduleOne.Product
{
	public class PropertyUtility : Singleton<PropertyUtility>
	{
		[Serializable]
		public class PropertyData
		{
			public EProperty Property;

			public string Name;

			public string Description;

			public Color Color;
		}

		[Serializable]
		public class DrugTypeData
		{
			public EDrugType DrugType;

			public string Name;

			public Color Color;
		}

		public List<PropertyData> PropertyDatas = new List<PropertyData>();

		public List<DrugTypeData> DrugTypeDatas = new List<DrugTypeData>();

		public List<ScheduleOne.Properties.Property> AllProperties = new List<ScheduleOne.Properties.Property>();

		[Header("Test Mixing")]
		public List<ProductDefinition> Products = new List<ProductDefinition>();

		public List<PropertyItemDefinition> Properties = new List<PropertyItemDefinition>();

		private Dictionary<string, ScheduleOne.Properties.Property> PropertiesDict = new Dictionary<string, ScheduleOne.Properties.Property>();

		protected override void Awake()
		{
			base.Awake();
			foreach (ScheduleOne.Properties.Property allProperty in AllProperties)
			{
				PropertiesDict.Add(allProperty.ID, allProperty);
			}
		}

		protected override void Start()
		{
			base.Start();
		}

		[Button]
		public void CreateMissingData()
		{
			foreach (EProperty property in Enum.GetValues(typeof(EProperty)))
			{
				if (PropertyDatas.Find((PropertyData x) => x.Property == property) == null)
				{
					PropertyDatas.Add(new PropertyData
					{
						Property = property,
						Name = property.ToString(),
						Description = "",
						Color = Color.white
					});
				}
			}
		}

		public List<ScheduleOne.Properties.Property> GetProperties(int tier)
		{
			bool excludePostMixingRework = false;
			if (SaveManager.GetVersionNumber(Singleton<MetadataManager>.Instance.CreationVersion) < 27f)
			{
				excludePostMixingRework = true;
			}
			return AllProperties.FindAll((ScheduleOne.Properties.Property x) => x.Tier == tier && (!excludePostMixingRework || x.ImplementedPriorMixingRework));
		}

		public List<ScheduleOne.Properties.Property> GetProperties(List<string> ids)
		{
			List<ScheduleOne.Properties.Property> list = new List<ScheduleOne.Properties.Property>();
			foreach (string id in ids)
			{
				if (AllProperties.FirstOrDefault((ScheduleOne.Properties.Property x) => x.ID == id) == null)
				{
					Console.LogWarning("PropertyUtility: Property ID '" + id + "' not found!");
				}
				else
				{
					list.Add(PropertiesDict[id]);
				}
			}
			return AllProperties.FindAll((ScheduleOne.Properties.Property x) => ids.Contains(x.ID));
		}

		public static PropertyData GetPropertyData(EProperty property)
		{
			return Singleton<PropertyUtility>.Instance.PropertyDatas.Find((PropertyData x) => x.Property == property);
		}

		public static DrugTypeData GetDrugTypeData(EDrugType drugType)
		{
			return Singleton<PropertyUtility>.Instance.DrugTypeDatas.Find((DrugTypeData x) => x.DrugType == drugType);
		}

		public static List<Color32> GetOrderedPropertyColors(List<ScheduleOne.Properties.Property> properties)
		{
			properties.Sort((ScheduleOne.Properties.Property x, ScheduleOne.Properties.Property y) => x.Tier.CompareTo(y.Tier));
			List<Color32> list = new List<Color32>();
			foreach (ScheduleOne.Properties.Property property in properties)
			{
				list.Add(property.ProductColor);
			}
			return list;
		}
	}
}
