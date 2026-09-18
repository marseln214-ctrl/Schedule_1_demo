using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Employees;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Storage
{
	public interface IStorageEntity
	{
		Transform storedItemContainer { get; }

		Dictionary<StoredItem, Employee> reservedItems { get; }

		List<StoredItem> GetStoredItems();

		List<StorageGrid> GetStorageGrids();

		List<StoredItem> GetStoredItemsByID(string ID)
		{
			List<StoredItem> storedItems = GetStoredItems();
			List<StoredItem> list = new List<StoredItem>();
			for (int i = 0; i < storedItems.Count; i++)
			{
				if (storedItems[i].item.ID == ID)
				{
					list.Add(storedItems[i]);
				}
			}
			return list;
		}

		void ReserveItem(StoredItem item, Employee employee)
		{
			if (IsItemReserved(item))
			{
				if (reservedItems[item] != employee)
				{
					Console.LogWarning("Item already reserved by someone else!");
				}
			}
			else
			{
				reservedItems.Add(item, employee);
				(this as MonoBehaviour).StartCoroutine(ClearReserve(item));
			}
		}

		void DereserveItem(StoredItem item)
		{
			if (reservedItems.ContainsKey(item))
			{
				reservedItems.Remove(item);
			}
		}

		bool IsItemReserved(StoredItem item)
		{
			return reservedItems.ContainsKey(item);
		}

		Employee WhoIsReserving(StoredItem item)
		{
			if (reservedItems.ContainsKey(item))
			{
				return reservedItems[item];
			}
			return null;
		}

		List<StoredItem> GetNonReservedItemsByPrefabID(string prefabID, Employee whosAskin)
		{
			List<StoredItem> storedItemsByID = GetStoredItemsByID(prefabID);
			List<StoredItem> list = new List<StoredItem>();
			for (int i = 0; i < storedItemsByID.Count; i++)
			{
				Employee employee = WhoIsReserving(storedItemsByID[i]);
				if (employee == null || employee == whosAskin)
				{
					list.Add(storedItemsByID[i]);
				}
			}
			return list;
		}

		IEnumerator ClearReserve(StoredItem item)
		{
			yield return new WaitForSeconds(60f);
			if (item != null)
			{
				DereserveItem(item);
			}
		}

		bool TryFitItem(int sizeX, int sizeY, out StorageGrid grid, out Coordinate originCoordinate, out float rotation)
		{
			grid = null;
			originCoordinate = new Coordinate(0, 0);
			rotation = 0f;
			List<StorageGrid> storageGrids = GetStorageGrids();
			for (int i = 0; i < storageGrids.Count; i++)
			{
				grid = storageGrids[i];
				if (storageGrids[i].TryFitItem(sizeX, sizeY, new List<Coordinate>(), out originCoordinate, out rotation))
				{
					return true;
				}
			}
			return false;
		}

		int HowManyCanFit(int sizeX, int sizeY, int limit = int.MaxValue)
		{
			int num = 0;
			List<StorageGrid> storageGrids = GetStorageGrids();
			for (int i = 0; i < storageGrids.Count; i++)
			{
				List<Coordinate> list = new List<Coordinate>();
				Coordinate originCoordinate;
				float rotation;
				while (storageGrids[i].TryFitItem(sizeX, sizeY, list, out originCoordinate, out rotation) && num < limit)
				{
					num++;
					List<CoordinatePair> list2 = Coordinate.BuildCoordinateMatches(originCoordinate, sizeX, sizeY, rotation);
					for (int j = 0; j < list2.Count; j++)
					{
						list.Add(list2[i].coord2);
					}
				}
			}
			return num;
		}
	}
}
