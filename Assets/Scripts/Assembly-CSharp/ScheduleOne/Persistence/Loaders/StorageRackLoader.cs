using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence.Loaders
{
	public class StorageRackLoader : GridItemLoader
	{
		public override string ItemType => typeof(PlaceableStorageData).Name;

		public override void Load(string mainPath)
		{
			GridItem gridItem = LoadAndCreate(mainPath);
			if (gridItem == null)
			{
				Console.LogWarning("Failed to load grid item");
				return;
			}
			PlaceableStorageEntity placeableStorageEntity = gridItem as PlaceableStorageEntity;
			if (placeableStorageEntity == null)
			{
				Console.LogWarning("Failed to cast grid item to rack");
				return;
			}
			PlaceableStorageData data = GetData<PlaceableStorageData>(mainPath);
			if (data == null)
			{
				Console.LogWarning("Failed to load storage rack data");
				return;
			}
			for (int i = 0; i < data.Contents.Items.Length; i++)
			{
				ItemInstance instance = ItemDeserializer.LoadItem(data.Contents.Items[i]);
				if (placeableStorageEntity.StorageEntity.ItemSlots.Count > i)
				{
					placeableStorageEntity.StorageEntity.ItemSlots[i].SetStoredItem(instance);
				}
			}
		}
	}
}
