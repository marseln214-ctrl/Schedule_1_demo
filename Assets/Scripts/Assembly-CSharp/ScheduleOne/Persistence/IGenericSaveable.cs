using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence
{
	public interface IGenericSaveable
	{
		Guid GUID { get; }

		void InitializeSaveable()
		{
			Singleton<GenericSaveablesManager>.Instance.RegisterSaveable(this);
		}

		void Load(GenericSaveData data);

		GenericSaveData GetSaveData();
	}
}
