using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using ScheduleOne.Properties;

namespace ScheduleOne.ObjectScripts
{
	[Serializable]
	public class MixOperation
	{
		public string ProductID;

		public EQuality ProductQuality;

		public string IngredientID;

		public int Quantity;

		public MixOperation(string productID, EQuality productQuality, string ingredientID, int quantity)
		{
			ProductID = productID;
			ProductQuality = productQuality;
			IngredientID = ingredientID;
			Quantity = quantity;
		}

		public MixOperation()
		{
		}

		public EDrugType GetOutput(out List<ScheduleOne.Properties.Property> properties)
		{
			ProductDefinition item = Registry.GetItem<ProductDefinition>(ProductID);
			PropertyItemDefinition item2 = Registry.GetItem<PropertyItemDefinition>(IngredientID);
			properties = PropertyMixCalculator.MixProperties(item.Properties, item2.Properties[0], item.DrugType);
			return item.DrugType;
		}

		public bool IsOutputKnown(out ProductDefinition knownProduct)
		{
			List<ScheduleOne.Properties.Property> properties;
			EDrugType output = GetOutput(out properties);
			knownProduct = NetworkSingleton<ProductManager>.Instance.GetKnownProduct(output, properties);
			return knownProduct != null;
		}
	}
}
