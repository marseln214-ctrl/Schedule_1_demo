using UnityEngine;

namespace ScheduleOne.Persistence.Datas
{
	public class TrashBagData : TrashItemData
	{
		public TrashContentData Contents;

		public TrashBagData(string trashID, string guid, Vector3 position, Quaternion rotation, TrashContentData contents)
			: base(trashID, guid, position, rotation)
		{
			Contents = contents;
		}
	}
}
