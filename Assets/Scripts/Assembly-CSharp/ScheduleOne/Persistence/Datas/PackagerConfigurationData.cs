using System;

namespace ScheduleOne.Persistence.Datas
{
	[Serializable]
	public class PackagerConfigurationData : SaveData
	{
		public ObjectFieldData Bed;

		public ObjectListFieldData Stations;

		public PackagerConfigurationData(ObjectFieldData bed, ObjectListFieldData stations)
		{
			Bed = bed;
			Stations = stations;
		}
	}
}
