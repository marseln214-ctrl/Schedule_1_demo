using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.Persistence;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class Map : Singleton<Map>
	{
		public MapRegionData[] Regions;

		[Header("References")]
		public PoliceStation PoliceStation;

		public MedicalCentre MedicalCentre;

		protected override void Awake()
		{
			base.Awake();
			foreach (EMapRegion region in Enum.GetValues(typeof(EMapRegion)))
			{
				if (Regions == null || Array.Find(Regions, (MapRegionData x) => x.Region == region) == null)
				{
					Console.LogError($"No region data found for {region}");
				}
			}
		}

		protected override void Start()
		{
			base.Start();
			LevelManager levelManager = NetworkSingleton<LevelManager>.Instance;
			levelManager.onRankUp = (Action<FullRank, FullRank>)Delegate.Combine(levelManager.onRankUp, new Action<FullRank, FullRank>(OnRankUp));
		}

		public MapRegionData GetRegionData(EMapRegion region)
		{
			return Array.Find(Regions, (MapRegionData x) => x.Region == region);
		}

		private void OnRankUp(FullRank oldRank, FullRank newRank)
		{
			_ = Singleton<LoadManager>.Instance.IsLoading;
		}
	}
}
