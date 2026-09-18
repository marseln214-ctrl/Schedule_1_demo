using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class PlayerLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (TryLoadFile(mainPath, "Player", out var contents))
			{
				PlayerData playerData = null;
				try
				{
					playerData = JsonUtility.FromJson<PlayerData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
				}
				if (playerData != null)
				{
					Singleton<PlayerManager>.Instance.LoadPlayer(playerData, mainPath);
				}
			}
		}
	}
}
