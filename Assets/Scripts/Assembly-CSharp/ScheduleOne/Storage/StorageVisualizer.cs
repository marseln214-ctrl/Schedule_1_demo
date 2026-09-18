using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Storage
{
	public class StorageVisualizer : MonoBehaviour
	{
		[Header("References")]
		public StorageGrid[] StorageGrids;

		public Transform ItemContainer;

		[Header("Settings")]
		[Tooltip("Should storage visuals be fully recalculated when item(s) are removed?")]
		public bool FullRefreshOnItemRemoved;

		protected List<ItemSlot> itemSlots = new List<ItemSlot>();

		protected int totalFootprintCapacity;

		protected Dictionary<StorableItemInstance, List<StoredItem>> activeStoredItems = new Dictionary<StorableItemInstance, List<StoredItem>>();

		public bool BlockRefreshes;

		protected bool updateVisuals;

		protected virtual void Awake()
		{
			for (int i = 0; i < StorageGrids.Length; i++)
			{
				totalFootprintCapacity += StorageGrids[i].GetTotalFootprintSize();
			}
			RefreshVisuals();
		}

		protected virtual void FixedUpdate()
		{
			if (updateVisuals)
			{
				updateVisuals = false;
				if (!BlockRefreshes)
				{
					RefreshVisuals();
				}
			}
		}

		public void AddSlot(ItemSlot slot, bool update = false)
		{
			if (!itemSlots.Contains(slot))
			{
				itemSlots.Add(slot);
				slot.onItemDataChanged = (Action)Delegate.Combine(slot.onItemDataChanged, (Action)delegate
				{
					updateVisuals = true;
				});
			}
			if (update)
			{
				updateVisuals = true;
			}
		}

		public Dictionary<StorableItemInstance, int> GetVisualRepresentation()
		{
			return StorageVisualizationUtility.GetVisualRepresentation(GetContentsDictionary(), totalFootprintCapacity);
		}

		public virtual void RefreshVisuals()
		{
			Dictionary<StorableItemInstance, int> visualRepresentation = GetVisualRepresentation();
			List<StorableItemInstance> list = visualRepresentation.Keys.ToList();
			List<StorableItemInstance> list2 = activeStoredItems.Keys.ToList();
			for (int i = 0; i < list2.Count; i++)
			{
				int quantityRequirement = 0;
				if (visualRepresentation.ContainsKey(list2[i]))
				{
					quantityRequirement = visualRepresentation[list2[i]];
				}
				DestroyExcessStoredItems(list2[i], quantityRequirement);
			}
			int num = 0;
			for (int j = 0; j < list.Count; j++)
			{
				num += EnsureSufficientStoredItems(list[j], visualRepresentation[list[j]]).Count;
			}
			List<StoredItem> list3 = new List<StoredItem>();
			if (num > 0 || num == 0 || FullRefreshOnItemRemoved)
			{
				foreach (StorableItemInstance item in list)
				{
					for (int k = 0; k < activeStoredItems[item].Count; k++)
					{
						activeStoredItems[item][k].ClearFootprintOccupancy();
					}
				}
				foreach (StorableItemInstance item2 in list)
				{
					List<StoredItem> list4 = activeStoredItems[item2];
					int num2 = list4[0].FootprintX * list4[0].FootprintY;
					List<StoredItem> list5 = new List<StoredItem>();
					list5.AddRange(list4);
					foreach (StoredItem item3 in list4)
					{
						bool flag = false;
						for (int l = 0; l < StorageGrids.Length; l++)
						{
							if (StorageGrids[l].freeTiles.Count >= num2 && StorageGrids[l].TryFitItem(item3.FootprintX, item3.FootprintY, new List<Coordinate>(), out var originCoordinate, out var rotation))
							{
								item3.InitializeStoredItem(item2, StorageGrids[l], originCoordinate, rotation);
								list5.Remove(item3);
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							break;
						}
					}
					list3.AddRange(list5);
				}
			}
			if (list3.Count > 0)
			{
				Console.LogWarning("Failed to fit " + list3.Count + " stored items into the storage entity. Deleting them.");
				for (int m = 0; m < list3.Count; m++)
				{
					UnityEngine.Object.Destroy(list3[m].gameObject);
				}
			}
		}

		private List<StoredItem> EnsureSufficientStoredItems(StorableItemInstance item, int quantityRequirement)
		{
			int num = 0;
			if (activeStoredItems.ContainsKey(item))
			{
				num = activeStoredItems[item].Count;
			}
			List<StoredItem> list = new List<StoredItem>();
			if (num < quantityRequirement)
			{
				if (!activeStoredItems.ContainsKey(item))
				{
					activeStoredItems.Add(item, new List<StoredItem>());
				}
				int num2 = quantityRequirement - num;
				for (int i = 0; i < num2; i++)
				{
					StoredItem component = UnityEngine.Object.Instantiate(item.StoredItem, (ItemContainer != null) ? ItemContainer : base.transform).GetComponent<StoredItem>();
					component.transform.localScale = Vector3.one;
					activeStoredItems[item].Add(component);
					list.Add(component);
					Collider[] componentsInChildren = component.GetComponentsInChildren<Collider>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						componentsInChildren[j].enabled = false;
					}
				}
			}
			return list;
		}

		private void DestroyExcessStoredItems(StorableItemInstance item, int quantityRequirement)
		{
			int num = 0;
			if (activeStoredItems.ContainsKey(item))
			{
				num = activeStoredItems[item].Count;
			}
			if (num > quantityRequirement)
			{
				int num2 = num - quantityRequirement;
				for (int i = 0; i < num2; i++)
				{
					activeStoredItems[item][activeStoredItems[item].Count - 1].DestroyStoredItem();
					activeStoredItems[item].RemoveAt(activeStoredItems[item].Count - 1);
				}
			}
		}

		public Dictionary<StorableItemInstance, int> GetContentsDictionary()
		{
			Dictionary<StorableItemInstance, int> dictionary = new Dictionary<StorableItemInstance, int>();
			for (int i = 0; i < itemSlots.Count; i++)
			{
				if (itemSlots[i].ItemInstance != null && itemSlots[i].ItemInstance is StorableItemInstance && itemSlots[i].Quantity > 0 && !dictionary.ContainsKey(itemSlots[i].ItemInstance as StorableItemInstance))
				{
					dictionary.Add(itemSlots[i].ItemInstance as StorableItemInstance, itemSlots[i].Quantity);
				}
			}
			return dictionary;
		}

		protected void QueueRefresh()
		{
			updateVisuals = true;
		}
	}
}
