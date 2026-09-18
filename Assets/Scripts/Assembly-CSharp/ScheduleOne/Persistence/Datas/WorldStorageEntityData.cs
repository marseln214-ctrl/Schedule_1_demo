using System;

namespace ScheduleOne.Persistence.Datas
{
	[Serializable]
	public class WorldStorageEntityData : SaveData
	{
		public string GUID;

		public ItemSet Contents;

		public WorldStorageEntityData(Guid guid, ItemSet contents)
		{
			GUID = guid.ToString();
			Contents = contents;
		}
	}
}
