using System;
using System.Collections.Generic;
using EasyButtons;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.Trash
{
	[RequireComponent(typeof(BoxCollider))]
	public class TrashGenerator : MonoBehaviour, IGUIDRegisterable, ISaveable
	{
		public const float TRASH_GENERATION_FRACTION = 0.2f;

		public const float DEFAULT_TRASH_PER_M2 = 0.015f;

		public static List<TrashGenerator> AllGenerators = new List<TrashGenerator>();

		[Range(1f, 200f)]
		[SerializeField]
		private int MaxTrashCount = 10;

		[SerializeField]
		private List<TrashItem> generatedTrash = new List<TrashItem>();

		[Header("Settings")]
		public LayerMask GroundCheckMask;

		private BoxCollider boxCollider;

		public string StaticGUID = string.Empty;

		public string SaveFolderName => "Generator_" + GUID.ToString().Substring(0, 6);

		public string SaveFileName => "Generator_" + GUID.ToString().Substring(0, 6);

		public Loader Loader => null;

		public bool ShouldSaveUnderFolder => false;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		public Guid GUID { get; protected set; }

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
		}

		private void Awake()
		{
			AllGenerators.Add(this);
		}

		private void Start()
		{
			NetworkSingleton<TimeManager>.Instance._onSleepStart.AddListener(SleepStart);
			boxCollider = GetComponent<BoxCollider>();
			boxCollider.isTrigger = true;
			LayerUtility.SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Invisible"));
			GUID = new Guid(StaticGUID);
			GUIDManager.RegisterObject(this);
			InitializeSaveable();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		private void OnValidate()
		{
			if (string.IsNullOrEmpty(StaticGUID))
			{
				RegenerateGUID();
			}
		}

		private void OnDestroy()
		{
			AllGenerators.Remove(this);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			boxCollider = GetComponent<BoxCollider>();
			Gizmos.DrawWireCube(boxCollider.bounds.center, new Vector3(boxCollider.size.x * base.transform.localScale.x, boxCollider.size.y * base.transform.localScale.y, boxCollider.size.z * base.transform.localScale.z));
		}

		public void AddGeneratedTrash(TrashItem item)
		{
			if (!generatedTrash.Contains(item))
			{
				item.onDestroyed = (Action<TrashItem>)Delegate.Combine(item.onDestroyed, new Action<TrashItem>(RemoveGeneratedTrash));
				generatedTrash.Add(item);
				HasChanged = true;
			}
		}

		public void RemoveGeneratedTrash(TrashItem item)
		{
			item.onDestroyed = (Action<TrashItem>)Delegate.Remove(item.onDestroyed, new Action<TrashItem>(RemoveGeneratedTrash));
			generatedTrash.Remove(item);
			HasChanged = true;
		}

		[Button]
		private void RegenerateGUID()
		{
			StaticGUID = Guid.NewGuid().ToString();
		}

		[Button]
		private void AutoCalculateTrashCount()
		{
			boxCollider = GetComponent<BoxCollider>();
			float num = boxCollider.size.x * base.transform.localScale.x * (boxCollider.size.z * base.transform.localScale.z);
			MaxTrashCount = Mathf.FloorToInt(num * 0.015f);
		}

		[Button]
		private void GenerateMaxTrash()
		{
			GenerateTrash(MaxTrashCount - generatedTrash.Count);
		}

		private void SleepStart()
		{
			if (InstanceFinder.IsServer)
			{
				int num = Mathf.Min(MaxTrashCount - generatedTrash.Count, Mathf.FloorToInt((float)MaxTrashCount * 0.2f));
				if (num > 0)
				{
					GenerateTrash(num);
				}
			}
		}

		private void GenerateTrash(int count)
		{
			Console.Log("Generating " + count + " trash items");
			for (int i = 0; i < count; i++)
			{
				Vector3 vector = new Vector3(UnityEngine.Random.Range(boxCollider.bounds.min.x, boxCollider.bounds.max.x), UnityEngine.Random.Range(boxCollider.bounds.min.y, boxCollider.bounds.max.y), UnityEngine.Random.Range(boxCollider.bounds.min.z, boxCollider.bounds.max.z));
				vector = (Physics.Raycast(vector, Vector3.down, out var hitInfo, 20f, GroundCheckMask) ? hitInfo.point : vector);
				int num = 0;
				NavMeshHit hit;
				while (!NavMeshUtility.SamplePosition(vector, out hit, 1.5f, -1))
				{
					if (num > 10)
					{
						Console.Log("Failed to find a valid position for trash item");
						break;
					}
					vector = new Vector3(UnityEngine.Random.Range(boxCollider.bounds.min.x, boxCollider.bounds.max.x), UnityEngine.Random.Range(boxCollider.bounds.min.y, boxCollider.bounds.max.y), UnityEngine.Random.Range(boxCollider.bounds.min.z, boxCollider.bounds.max.z));
					vector = (Physics.Raycast(vector, Vector3.down, out hitInfo, 20f, GroundCheckMask) ? hitInfo.point : vector);
					num++;
				}
				vector += Vector3.up * 0.5f;
				TrashItem randomGeneratableTrashPrefab = NetworkSingleton<TrashManager>.Instance.GetRandomGeneratableTrashPrefab();
				TrashItem trashItem = NetworkSingleton<TrashManager>.Instance.CreateTrashItem(randomGeneratableTrashPrefab.ID, vector, UnityEngine.Random.rotation);
				trashItem.SetContinuousCollisionDetection();
				AddGeneratedTrash(trashItem);
			}
		}

		public bool ShouldSave()
		{
			return generatedTrash.Count > 0;
		}

		public virtual string GetSaveString()
		{
			return new TrashGeneratorData(GUID.ToString(), generatedTrash.ConvertAll((TrashItem x) => x.GUID.ToString()).ToArray()).GetJson();
		}
	}
}
