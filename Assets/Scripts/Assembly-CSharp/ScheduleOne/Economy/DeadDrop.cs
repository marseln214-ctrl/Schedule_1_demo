using System;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.Storage;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Economy
{
	public class DeadDrop : MonoBehaviour, IGUIDRegisterable
	{
		public static List<DeadDrop> DeadDrops = new List<DeadDrop>();

		public string DeadDropName;

		public string DeadDropDescription;

		public StorageEntity Storage;

		public POI PoI;

		public OptimizedLight Light;

		public string ItemCountVariable = string.Empty;

		[SerializeField]
		protected string BakedGUID = string.Empty;

		public Guid GUID { get; protected set; }

		[Button]
		public void RegenerateGUID()
		{
			BakedGUID = Guid.NewGuid().ToString();
		}

		protected virtual void Awake()
		{
			DeadDrops.Add(this);
			GUID = new Guid(BakedGUID);
			GUIDManager.RegisterObject(this);
		}

		private void OnValidate()
		{
			base.gameObject.name = DeadDropName;
		}

		protected virtual void Start()
		{
			GetComponent<StorageEntity>().StorageEntitySubtitle = DeadDropName;
			PoI.SetMainText("Dead Drop\n(" + DeadDropName + ")");
			UpdateDeadDrop();
			Storage.onContentsChanged.AddListener(UpdateDeadDrop);
		}

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
		}

		public void OnDestroy()
		{
			DeadDrops.Remove(this);
		}

		public static DeadDrop GetRandomEmptyDrop()
		{
			List<DeadDrop> list = DeadDrops.Where((DeadDrop drop) => drop.Storage.ItemCount == 0).ToList();
			if (list.Count == 0)
			{
				return null;
			}
			return list[UnityEngine.Random.Range(0, list.Count)];
		}

		private void UpdateDeadDrop()
		{
			PoI.enabled = false;
			Light.Enabled = Storage.ItemCount > 0;
			if (ItemCountVariable != string.Empty)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(ItemCountVariable, Storage.ItemCount.ToString());
			}
		}
	}
}
