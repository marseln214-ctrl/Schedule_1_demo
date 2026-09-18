using System;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas
{
	[Serializable]
	public class SaveData
	{
		public string DataType = string.Empty;

		public int DataVersion;

		public string GameVersion = string.Empty;

		public SaveData()
		{
			DataType = GetType().Name;
			DataVersion = GetDataVersion();
			GameVersion = Application.version;
		}

		protected virtual int GetDataVersion()
		{
			return 0;
		}

		public virtual string GetJson(bool prettyPrint = true)
		{
			if (DataType == string.Empty)
			{
				Console.LogError(GetType()?.ToString() + " GetJson() called but has no data type set!");
			}
			return JsonUtility.ToJson(this, prettyPrint);
		}
	}
}
