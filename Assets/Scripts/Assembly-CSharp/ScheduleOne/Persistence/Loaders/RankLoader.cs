using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class RankLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (TryLoadFile(mainPath, out var contents))
			{
				RankData rankData = JsonUtility.FromJson<RankData>(contents);
				if (rankData != null)
				{
					NetworkSingleton<LevelManager>.Instance.SetData(null, (ERank)rankData.Rank, rankData.Tier, rankData.XP, rankData.TotalXP);
				}
			}
		}
	}
}
