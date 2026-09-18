using ScheduleOne.EntityFramework;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence.Loaders
{
	public class ToggleableItemLoader : GridItemLoader
	{
		public override string ItemType => typeof(ToggleableItemData).Name;

		public override void Load(string mainPath)
		{
			GridItem gridItem = LoadAndCreate(mainPath);
			if (gridItem == null)
			{
				Console.LogWarning("Failed to load grid item");
				return;
			}
			ToggleableItemData data = GetData<ToggleableItemData>(mainPath);
			if (data == null)
			{
				Console.LogWarning("Failed to load toggleableitem data");
				return;
			}
			ToggleableItem toggleableItem = gridItem as ToggleableItem;
			if (toggleableItem != null && data.IsOn)
			{
				toggleableItem.TurnOn();
			}
		}
	}
}
