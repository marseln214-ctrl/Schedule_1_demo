using System.Collections.Generic;
using System.Linq;

namespace ScheduleOne.Storage
{
	public static class StorageVisualizationUtility
	{
		public static Dictionary<StorableItemInstance, int> GetVisualRepresentation(Dictionary<StorableItemInstance, int> inputDictionary, int TotalFootprintSize)
		{
			int num = TotalFootprintSize;
			List<StorableItemInstance> list = inputDictionary.Keys.ToList();
			Dictionary<StorableItemInstance, int> dictionary = new Dictionary<StorableItemInstance, int>();
			while (num > 0 && list.Count > 0)
			{
				List<StorableItemInstance> list2 = new List<StorableItemInstance>();
				list2.AddRange(list);
				foreach (StorableItemInstance item in list2)
				{
					if (item == null || item.StoredItem == null)
					{
						list.Remove(item);
						continue;
					}
					int num2 = item.StoredItem.FootprintY * item.StoredItem.FootprintX;
					if (num < num2)
					{
						list.Remove(item);
						continue;
					}
					if (!dictionary.ContainsKey(item))
					{
						dictionary.Add(item, 0);
					}
					dictionary[item]++;
					num -= num2;
					inputDictionary[item]--;
					if (inputDictionary[item] <= 0)
					{
						list.Remove(item);
					}
				}
			}
			return dictionary;
		}
	}
}
