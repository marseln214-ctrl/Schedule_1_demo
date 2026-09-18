using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Law;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class LawLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (TryLoadFile(mainPath, out var contents))
			{
				LawData lawData = null;
				try
				{
					lawData = JsonUtility.FromJson<LawData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
				}
				if (lawData != null)
				{
					Singleton<LawController>.Instance.Load(lawData);
				}
			}
		}
	}
}
