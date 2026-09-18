using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.ItemFramework;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.StationFramework
{
	[Serializable]
	[CreateAssetMenu(fileName = "StationRecipe", menuName = "StationFramework/StationRecipe", order = 1)]
	public class StationRecipe : ScriptableObject
	{
		public enum EQualityCalculationMethod
		{
			Additive = 0
		}

		[Serializable]
		public class ItemQuantity
		{
			public ItemDefinition Item;

			public int Quantity = 1;
		}

		[Serializable]
		public class IngredientQuantity
		{
			public List<ItemDefinition> Items = new List<ItemDefinition>();

			public int Quantity = 1;

			public ItemDefinition Item => Items.FirstOrDefault();
		}

		[HideInInspector]
		public bool IsDiscovered;

		public string RecipeTitle;

		public bool Unlocked;

		public List<IngredientQuantity> Ingredients = new List<IngredientQuantity>();

		public ItemQuantity Product;

		public Color FinalLiquidColor = Color.white;

		[Tooltip("The time it takes to cook this recipe in minutes")]
		public int CookTime_Mins = 180;

		[Tooltip("The temperature at which this recipe should be cooked")]
		[Range(0f, 500f)]
		public float CookTemperature = 250f;

		[Range(0f, 100f)]
		public float CookTemperatureTolerance = 25f;

		public EQualityCalculationMethod QualityCalculationMethod;

		public float CookTemperatureLowerBound => CookTemperature - CookTemperatureTolerance;

		public float CookTemperatureUpperBound => CookTemperature + CookTemperatureTolerance;

		public string RecipeID => Product.Quantity + "x" + Product.Item.ID;

		public StorableItemInstance GetProductInstance(List<ItemInstance> ingredients)
		{
			StorableItemInstance storableItemInstance = Product.Item.GetDefaultInstance(Product.Quantity) as StorableItemInstance;
			if (storableItemInstance is QualityItemInstance)
			{
				EQuality quality = CalculateQuality(ingredients);
				(storableItemInstance as QualityItemInstance).Quality = quality;
			}
			return storableItemInstance;
		}

		public StorableItemInstance GetProductInstance(EQuality quality)
		{
			StorableItemInstance storableItemInstance = Product.Item.GetDefaultInstance(Product.Quantity) as StorableItemInstance;
			if (storableItemInstance is QualityItemInstance)
			{
				(storableItemInstance as QualityItemInstance).Quality = quality;
			}
			return storableItemInstance;
		}

		public bool DoIngredientsSuffice(List<ItemInstance> ingredients)
		{
			for (int i = 0; i < Ingredients.Count; i++)
			{
				List<ItemInstance> list = new List<ItemInstance>();
				foreach (ItemDefinition ingredientVariant in Ingredients[i].Items)
				{
					List<ItemInstance> collection = ingredients.Where((ItemInstance x) => x.ID == ingredientVariant.ID).ToList();
					list.AddRange(collection);
				}
				int num = 0;
				for (int j = 0; j < list.Count; j++)
				{
					num += list[j].Quantity;
				}
				if (num < Ingredients[i].Quantity)
				{
					return false;
				}
			}
			return true;
		}

		public EQuality CalculateQuality(List<ItemInstance> ingredients)
		{
			EQuality result = EQuality.Standard;
			if (QualityCalculationMethod == EQualityCalculationMethod.Additive)
			{
				int num = 0;
				for (int i = 0; i < ingredients.Count; i++)
				{
					if (ingredients[i] is QualityItemInstance)
					{
						switch ((ingredients[i] as QualityItemInstance).Quality)
						{
						case EQuality.Trash:
							num -= 2;
							break;
						case EQuality.Poor:
							num--;
							break;
						case EQuality.Standard:
							num = num;
							break;
						case EQuality.Premium:
							num++;
							break;
						case EQuality.Heavenly:
							num += 2;
							break;
						}
					}
				}
				if ((float)num <= -2f)
				{
					result = EQuality.Trash;
				}
				else if ((float)num == -1f)
				{
					result = EQuality.Poor;
				}
				else if ((float)num == 0f)
				{
					result = EQuality.Standard;
				}
				else if ((float)num == 1f)
				{
					result = EQuality.Premium;
				}
				else if ((float)num >= 2f)
				{
					result = EQuality.Heavenly;
				}
			}
			return result;
		}
	}
}
