using System;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas
{
	[Serializable]
	public class SerializedSaveData
	{
		[NonSerialized]
		public static string _DataType;

		public string DataType = _DataType;

		[NonSerialized]
		public static int _DataVersion;

		public int DataVersion = _DataVersion;

		public string Version => Application.version;
	}
}
