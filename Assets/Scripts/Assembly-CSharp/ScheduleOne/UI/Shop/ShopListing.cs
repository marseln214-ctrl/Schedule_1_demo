using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using UnityEngine;

namespace ScheduleOne.UI.Shop
{
	[Serializable]
	public class ShopListing
	{
		[Serializable]
		public class CategoryInstance
		{
			public EShopCategory Category;
		}

		public string name;

		public StorableItemDefinition Item;

		[Header("Pricing")]
		[SerializeField]
		protected bool OverridePrice;

		[SerializeField]
		protected float OverriddenPrice = 10f;

		[Header("Stock")]
		public int StockQuantity = -1;

		[Header("Settings")]
		public bool EnforceMinimumGameCreationVersion;

		public float MinimumGameCreationVersion = 27f;

		public Action onQuantityChanged;

		public bool IsInStock => true;

		public float Price
		{
			get
			{
				if (!OverridePrice)
				{
					return Item.BasePurchasePrice;
				}
				return OverriddenPrice;
			}
		}

		public int CurrentQuantity { get; protected set; }

		public void Restock()
		{
			CurrentQuantity = StockQuantity;
			if (onQuantityChanged != null)
			{
				onQuantityChanged();
			}
		}

		public virtual bool ShouldShow()
		{
			if (EnforceMinimumGameCreationVersion && SaveManager.GetVersionNumber(Singleton<MetadataManager>.Instance.CreationVersion) < MinimumGameCreationVersion)
			{
				return false;
			}
			return true;
		}

		public virtual bool DoesListingMatchCategoryFilter(EShopCategory category)
		{
			if (category != 0)
			{
				return Item.ShopCategories.Find((CategoryInstance x) => x.Category == category) != null;
			}
			return true;
		}

		public virtual bool DoesListingMatchSearchTerm(string searchTerm)
		{
			return Item.Name.ToLower().Contains(searchTerm.ToLower());
		}
	}
}
