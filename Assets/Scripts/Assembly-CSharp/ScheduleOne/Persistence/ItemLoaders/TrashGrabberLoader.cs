using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence.ItemLoaders
{
	public class TrashGrabberLoader : ItemLoader
	{
		public override string ItemType => typeof(TrashGrabberData).Name;

		public override ItemInstance LoadItem(string itemString)
		{
			TrashGrabberData trashGrabberData = LoadData<TrashGrabberData>(itemString);
			if (trashGrabberData == null)
			{
				Console.LogWarning("Failed loading item data from " + itemString);
				return null;
			}
			if (trashGrabberData.ID == string.Empty)
			{
				return null;
			}
			ItemDefinition item = Registry.GetItem(trashGrabberData.ID);
			if (item == null)
			{
				Console.LogWarning("Failed to find item definition for " + trashGrabberData.ID);
				return null;
			}
			TrashGrabberInstance trashGrabberInstance = new TrashGrabberInstance(item, trashGrabberData.Quantity);
			trashGrabberInstance.LoadContentData(trashGrabberData.Content);
			return trashGrabberInstance;
		}
	}
}
