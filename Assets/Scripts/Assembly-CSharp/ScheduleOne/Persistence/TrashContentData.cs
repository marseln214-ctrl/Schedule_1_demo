using System;
using System.Collections.Generic;
using ScheduleOne.Trash;

namespace ScheduleOne.Persistence
{
	[Serializable]
	public class TrashContentData
	{
		public string[] TrashIDs;

		public int[] TrashQuantities;

		public TrashContentData()
		{
			TrashIDs = new string[0];
			TrashQuantities = new int[0];
		}

		public TrashContentData(string[] trashIDs, int[] trashQuantities)
		{
			TrashIDs = trashIDs;
			TrashQuantities = trashQuantities;
		}

		public TrashContentData(List<TrashItem> trashItems)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (TrashItem trashItem in trashItems)
			{
				if (!dictionary.ContainsKey(trashItem.ID))
				{
					dictionary.Add(trashItem.ID, 0);
				}
				dictionary[trashItem.ID]++;
			}
			TrashIDs = new string[dictionary.Count];
			TrashQuantities = new int[dictionary.Count];
			int num = 0;
			foreach (KeyValuePair<string, int> item in dictionary)
			{
				TrashIDs[num] = item.Key;
				TrashQuantities[num] = item.Value;
				num++;
			}
		}
	}
}
