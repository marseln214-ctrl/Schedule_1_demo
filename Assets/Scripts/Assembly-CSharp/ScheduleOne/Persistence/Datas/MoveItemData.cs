using System;

namespace ScheduleOne.Persistence.Datas
{
	[Serializable]
	public class MoveItemData
	{
		public string GrabbedItemID;

		public int GrabbedItemQuantity;

		public string SourceGUID;

		public string DestinationGUID;

		public MoveItemData(string grabbedItemID, int grabbedItemQuantity, Guid sourceGUID, Guid destinationGUID)
		{
			GrabbedItemID = grabbedItemID;
			GrabbedItemQuantity = grabbedItemQuantity;
			SourceGUID = sourceGUID.ToString();
			DestinationGUID = destinationGUID.ToString();
		}
	}
}
