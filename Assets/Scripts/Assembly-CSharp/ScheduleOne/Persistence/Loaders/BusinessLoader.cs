using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class BusinessLoader : PropertyLoader
	{
		public override void Load(string mainPath)
		{
			base.Load(mainPath);
			if (TryLoadFile(mainPath, "Business", out var contents))
			{
				BusinessData businessData = null;
				try
				{
					businessData = JsonUtility.FromJson<BusinessData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
				}
				if (businessData != null)
				{
					Singleton<BusinessManager>.Instance.LoadBusiness(businessData, mainPath);
				}
			}
		}
	}
}
