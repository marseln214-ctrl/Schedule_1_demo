using System;

namespace ScheduleOne
{
	public interface IGUIDRegisterable
	{
		Guid GUID { get; }

		void SetGUID(string guid)
		{
			if (Guid.TryParse(guid, out var result))
			{
				SetGUID(result);
			}
			else
			{
				Console.LogWarning(guid + " is not a valid GUID.");
			}
		}

		void SetGUID(Guid guid);
	}
}
