using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class VehicleLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (TryLoadFile(mainPath, "Vehicle", out var contents))
			{
				VehicleData vehicleData = null;
				try
				{
					vehicleData = JsonUtility.FromJson<VehicleData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
				}
				if (vehicleData != null)
				{
					NetworkSingleton<VehicleManager>.Instance.LoadVehicle(vehicleData, mainPath, playerOwned: true);
				}
			}
		}
	}
}
