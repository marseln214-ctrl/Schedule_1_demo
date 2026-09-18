using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;

namespace ScheduleOne.Property
{
	public class PropertyManager : Singleton<PropertyManager>, IBaseSaveable, ISaveable
	{
		private PropertiesLoader loader = new PropertiesLoader();

		public string SaveFolderName => "Properties";

		public string SaveFileName => "Properties";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => true;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		protected override void Awake()
		{
			base.Awake();
			InitializeSaveable();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public virtual string GetSaveString()
		{
			return string.Empty;
		}

		public virtual List<string> WriteData(string parentFolderPath)
		{
			List<string> list = new List<string>();
			string containerFolder = ((ISaveable)this).GetContainerFolder(parentFolderPath);
			for (int i = 0; i < Property.OwnedProperties.Count; i++)
			{
				try
				{
					if (Property.OwnedProperties[i].ShouldSave())
					{
						new SaveRequest(Property.OwnedProperties[i], containerFolder);
						list.Add(Property.OwnedProperties[i].SaveFolderName);
					}
				}
				catch (Exception ex)
				{
					Console.LogError("Error saving property: " + Property.OwnedProperties[i].PropertyCode + " - " + ex.Message);
					SaveManager.ReportSaveError();
				}
			}
			for (int j = 0; j < Property.UnownedProperties.Count; j++)
			{
				try
				{
					if (Property.UnownedProperties[j].ShouldSave())
					{
						new SaveRequest(Property.UnownedProperties[j], containerFolder);
						list.Add(Property.UnownedProperties[j].SaveFolderName);
					}
				}
				catch (Exception ex2)
				{
					Console.LogError("Error saving property: " + Property.OwnedProperties[j].PropertyCode + " - " + ex2.Message);
					SaveManager.ReportSaveError();
				}
			}
			return list;
		}

		public void LoadProperty(PropertyData propertyData, string containerPath)
		{
			Property property = Property.UnownedProperties.FirstOrDefault((Property p) => p.PropertyCode == propertyData.PropertyCode);
			if (property == null)
			{
				property = Property.OwnedProperties.FirstOrDefault((Property p) => p.PropertyCode == propertyData.PropertyCode);
			}
			if (property == null)
			{
				property = Business.UnownedBusinesses.FirstOrDefault((Business p) => p.PropertyCode == propertyData.PropertyCode);
			}
			if (property == null)
			{
				property = Business.OwnedBusinesses.FirstOrDefault((Business p) => p.PropertyCode == propertyData.PropertyCode);
			}
			if (property == null)
			{
				Console.LogWarning("Property not found for data: " + propertyData.PropertyCode);
			}
			else
			{
				property.Load(propertyData, containerPath);
			}
		}

		public Property GetProperty(string code)
		{
			Property property = Property.UnownedProperties.FirstOrDefault((Property p) => p.PropertyCode == code);
			if (property == null)
			{
				property = Property.OwnedProperties.FirstOrDefault((Property p) => p.PropertyCode == code);
			}
			return property;
		}
	}
}
