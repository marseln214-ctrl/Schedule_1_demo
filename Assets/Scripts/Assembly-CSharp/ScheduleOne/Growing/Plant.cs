using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Trash;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Growing
{
	public class Plant : MonoBehaviour
	{
		[Header("References")]
		public Transform VisualsContainer;

		public PlantGrowthStage[] GrowthStages;

		public Collider Collider;

		public AudioSourceController SnipSound;

		public AudioSourceController DestroySound;

		public ParticleSystem FullyGrownParticles;

		[Header("Settings")]
		public SeedDefinition SeedDefinition;

		public int GrowthTime = 48;

		public float BaseYieldLevel = 0.6f;

		public float BaseQualityLevel = 0.4f;

		public string HarvestTarget = "buds";

		[Header("Trash")]
		public TrashItem PlantScrapPrefab;

		public UnityEvent onGrowthDone;

		[Header("Plant data")]
		public float YieldLevel;

		public float QualityLevel;

		[HideInInspector]
		public List<int> ActiveHarvestables = new List<int>();

		public Pot Pot { get; protected set; }

		public float NormalizedGrowthProgress { get; protected set; }

		public bool IsFullyGrown => NormalizedGrowthProgress >= 1f;

		public PlantGrowthStage FinalGrowthStage => GrowthStages[GrowthStages.Length - 1];

		public virtual void Initialize(NetworkObject pot, float growthProgress = 0f, float yieldLevel = 0f, float qualityLevel = 0f)
		{
			Pot = pot.GetComponent<Pot>();
			if (Pot == null)
			{
				Console.LogWarning("Plant.Initialize: pot is null");
				return;
			}
			if (yieldLevel > 0f)
			{
				YieldLevel = yieldLevel;
			}
			else
			{
				YieldLevel = BaseYieldLevel;
			}
			if (qualityLevel > 0f)
			{
				QualityLevel = qualityLevel;
			}
			else
			{
				QualityLevel = BaseQualityLevel;
			}
			for (int i = 0; i < FinalGrowthStage.GrowthSites.Length; i++)
			{
				SetHarvestableActive(i, active: false);
			}
			SetNormalizedGrowthProgress(growthProgress);
		}

		public virtual void Destroy(bool dropScraps = false)
		{
			DestroySound.transform.SetParent(NetworkSingleton<GameManager>.Instance.Temp.transform);
			DestroySound.PlayOneShot();
			Object.Destroy(DestroySound, 1f);
			if (dropScraps && PlantScrapPrefab != null)
			{
				int num = Random.Range(1, 2);
				for (int i = 0; i < num; i++)
				{
					Vector3 forward = Pot.LeafDropPoint.forward;
					forward += new Vector3(0f, Random.Range(-0.2f, 0.2f), 0f);
					NetworkSingleton<TrashManager>.Instance.CreateTrashItem(PlantScrapPrefab.ID, Pot.LeafDropPoint.position + forward * 0.2f, Random.rotation, forward * 0.5f);
				}
			}
			Object.Destroy(base.gameObject);
		}

		public virtual void MinPass()
		{
			if (!(NormalizedGrowthProgress >= 1f) && !NetworkSingleton<TimeManager>.Instance.IsEndOfDay)
			{
				float num = 1f / ((float)GrowthTime * 60f);
				num *= Pot.GetAdditiveGrowthMultiplier();
				num *= Pot.GetAverageLightExposure(out var growSpeedMultiplier);
				num *= Pot.GrowSpeedMultiplier;
				num *= growSpeedMultiplier;
				if (Pot.NormalizedWaterLevel <= 0f || Pot.NormalizedWaterLevel > 1f)
				{
					num *= 0f;
				}
				SetNormalizedGrowthProgress(NormalizedGrowthProgress + num);
			}
		}

		public virtual void SetNormalizedGrowthProgress(float progress)
		{
			progress = Mathf.Clamp(progress, 0f, 1f);
			float normalizedGrowthProgress = NormalizedGrowthProgress;
			NormalizedGrowthProgress = progress;
			UpdateVisuals();
			if (NormalizedGrowthProgress >= 1f && normalizedGrowthProgress < 1f)
			{
				GrowthDone();
			}
		}

		protected virtual void UpdateVisuals()
		{
			int num = Mathf.FloorToInt(NormalizedGrowthProgress * (float)GrowthStages.Length);
			for (int i = 0; i < GrowthStages.Length; i++)
			{
				GrowthStages[i].gameObject.SetActive(i + 1 == num);
			}
		}

		public virtual void SetHarvestableActive(int index, bool active)
		{
			FinalGrowthStage.GrowthSites[index].gameObject.SetActive(active);
			ActiveHarvestables.Remove(index);
			if (active)
			{
				ActiveHarvestables.Add(index);
			}
		}

		public bool IsHarvestableActive(int index)
		{
			return ActiveHarvestables.Contains(index);
		}

		private void GrowthDone()
		{
			if (InstanceFinder.IsServer)
			{
				if (!Pot.IsSpawned)
				{
					Console.LogError("Pot not spawned!");
					return;
				}
				int count = Mathf.FloorToInt((float)FinalGrowthStage.GrowthSites.Length * YieldLevel);
				foreach (int item in GenerateUniqueIntegers(0, FinalGrowthStage.GrowthSites.Length - 1, count))
				{
					Pot.SendHarvestableActive(item, active: true);
				}
			}
			if (FullyGrownParticles != null)
			{
				FullyGrownParticles.Play();
			}
			if (onGrowthDone != null)
			{
				onGrowthDone.Invoke();
			}
		}

		private List<int> GenerateUniqueIntegers(int min, int max, int count)
		{
			List<int> list = new List<int>();
			if (max - min + 1 < count)
			{
				Debug.LogWarning("Range is too small to generate the requested number of unique integers.");
				return null;
			}
			List<int> list2 = new List<int>();
			for (int i = min; i <= max; i++)
			{
				list2.Add(i);
			}
			for (int j = 0; j < count; j++)
			{
				int index = Random.Range(0, list2.Count);
				list.Add(list2[index]);
				list2.RemoveAt(index);
			}
			return list;
		}

		public void SetVisible(bool vis)
		{
			VisualsContainer.gameObject.SetActive(vis);
		}

		public virtual ItemInstance GetHarvestedProduct(int quantity = 1)
		{
			Console.LogError("Plant.GetHarvestedProduct: This method should be overridden by a subclass.");
			return null;
		}

		public PlantData GetPlantData()
		{
			return new PlantData(SeedDefinition.ID, NormalizedGrowthProgress, YieldLevel, QualityLevel, ActiveHarvestables.ToArray());
		}
	}
}
