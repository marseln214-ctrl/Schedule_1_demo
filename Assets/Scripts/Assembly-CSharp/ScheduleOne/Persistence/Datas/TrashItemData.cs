using UnityEngine;

namespace ScheduleOne.Persistence.Datas
{
	public class TrashItemData : SaveData
	{
		public string TrashID;

		public string GUID;

		public Vector3 Position;

		public Quaternion Rotation;

		public TrashItemData(string trashID, string guid, Vector3 position, Quaternion rotation)
		{
			TrashID = trashID;
			GUID = guid;
			Position = position;
			Rotation = rotation;
		}
	}
}
