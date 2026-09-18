using System;
using FishNet.Serializing.Helping;
using ScheduleOne.ItemFramework;
using ScheduleOne.StationFramework;

namespace ScheduleOne.ObjectScripts
{
	[Serializable]
	public class OvenCookOperation
	{
		[CodegenExclude]
		private StorableItemDefinition _itemDefinition;

		[CodegenExclude]
		private StorableItemDefinition _productionDefinition;

		[CodegenExclude]
		private CookableModule _cookable;

		public string IngredientID;

		public EQuality IngredientQuality;

		public int IngredientQuantity = 1;

		public string ProductID;

		public int CookProgress;

		[CodegenExclude]
		private int cookDuration = -1;

		[CodegenExclude]
		public StorableItemDefinition Ingredient
		{
			get
			{
				if (_itemDefinition == null)
				{
					_itemDefinition = Registry.GetItem(IngredientID) as StorableItemDefinition;
				}
				return _itemDefinition;
			}
		}

		[CodegenExclude]
		public StorableItemDefinition Product
		{
			get
			{
				if (_productionDefinition == null)
				{
					_productionDefinition = Registry.GetItem(ProductID) as StorableItemDefinition;
				}
				return _productionDefinition;
			}
		}

		[CodegenExclude]
		public CookableModule Cookable
		{
			get
			{
				if (_cookable == null)
				{
					_cookable = Ingredient.StationItem.GetModule<CookableModule>();
				}
				return _cookable;
			}
		}

		public OvenCookOperation(string ingredientID, EQuality ingredientQuality, int ingredientQuantity, string productID)
		{
			IngredientID = ingredientID;
			IngredientQuality = ingredientQuality;
			IngredientQuantity = ingredientQuantity;
			ProductID = productID;
			CookProgress = 0;
		}

		public OvenCookOperation(string ingredientID, EQuality ingredientQuality, int ingredientQuantity, string productID, int progress)
		{
			IngredientID = ingredientID;
			IngredientQuality = ingredientQuality;
			IngredientQuantity = ingredientQuantity;
			ProductID = productID;
			CookProgress = progress;
		}

		public OvenCookOperation()
		{
		}

		public void UpdateCookProgress(int change)
		{
			CookProgress += change;
		}

		public int GetCookDuration()
		{
			if (cookDuration == -1)
			{
				cookDuration = Ingredient.StationItem.GetModule<CookableModule>().CookTime;
			}
			return cookDuration;
		}

		public ItemInstance GetProductItem(int quantity)
		{
			ItemInstance defaultInstance = Product.GetDefaultInstance(quantity);
			if (defaultInstance is QualityItemInstance)
			{
				(defaultInstance as QualityItemInstance).Quality = IngredientQuality;
			}
			return defaultInstance;
		}

		public bool IsReady()
		{
			return CookProgress >= GetCookDuration();
		}
	}
}
