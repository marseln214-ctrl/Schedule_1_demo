using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.NPCs;
using UnityEngine;

namespace ScheduleOne.Map
{
	[Serializable]
	public class MapRegionData
	{
		public EMapRegion Region;

		public string Name;

		public FullRank RankRequirement;

		public NPC[] StartingNPCs;

		public Sprite RegionSprite;

		public bool IsUnlocked
		{
			get
			{
				if (NetworkSingleton<LevelManager>.InstanceExists)
				{
					return NetworkSingleton<LevelManager>.Instance.GetFullRank() >= RankRequirement;
				}
				return false;
			}
		}
	}
}
