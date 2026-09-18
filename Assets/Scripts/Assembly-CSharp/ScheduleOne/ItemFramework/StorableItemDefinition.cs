using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.StationFramework;
using ScheduleOne.Storage;
using ScheduleOne.UI.Shop;
using UnityEngine;

namespace ScheduleOne.ItemFramework
{
	[Serializable]
	[CreateAssetMenu(fileName = "StorableItemDefinition", menuName = "ScriptableObjects/StorableItemDefinition", order = 1)]
	public class StorableItemDefinition : ItemDefinition
	{
		[Header("Purchasing")]
		public float BasePurchasePrice = 10f;

		public List<ShopListing.CategoryInstance> ShopCategories = new List<ShopListing.CategoryInstance>();

		public bool RequiresLevelToPurchase;

		public FullRank RequiredRank;

		[Header("Storable Item")]
		public StoredItem StoredItem;

		[Tooltip("Optional station item if this item can be used at a station.")]
		public StationItem StationItem;

		public bool IsPurchasable
		{
			get
			{
				if (RequiresLevelToPurchase)
				{
					return NetworkSingleton<LevelManager>.Instance.GetFullRank() >= RequiredRank;
				}
				return true;
			}
		}

		public override ItemInstance GetDefaultInstance(int quantity = 1)
		{
			return new StorableItemInstance(this, quantity);
		}
	}
}
