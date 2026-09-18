using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence.Loaders
{
	public class TrashContainerLoader : GridItemLoader
	{
		public override string ItemType => typeof(TrashContainerData).Name;

		public override void Load(string mainPath)
		{
			GridItem gridItem = LoadAndCreate(mainPath);
			if (gridItem == null)
			{
				Console.LogWarning("Failed to load grid item");
				return;
			}
			TrashContainerData data = GetData<TrashContainerData>(mainPath);
			if (data == null)
			{
				Console.LogWarning("Failed to load toggleableitem data");
				return;
			}
			TrashContainerItem trashContainerItem = gridItem as TrashContainerItem;
			if (trashContainerItem != null)
			{
				trashContainerItem.Container.Content.LoadFromData(data.ContentData);
			}
		}
	}
}
