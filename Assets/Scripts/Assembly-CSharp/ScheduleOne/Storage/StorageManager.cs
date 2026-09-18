using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;

namespace ScheduleOne.Storage
{
	public class StorageManager : NetworkSingleton<StorageManager>, IBaseSaveable, ISaveable
	{
		[Header("Prefabs")]
		public GameObject PalletPrefab;

		private StorageLoader loader = new StorageLoader();

		private bool NetworkInitialize___EarlyScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted;

		public string SaveFolderName => "WorldStorageEntities";

		public string SaveFileName => "WorldStorageEntities";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => true;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EStorage_002EStorageManager_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public Pallet CreatePallet(Vector3 position, Quaternion rotation, string initialSlotGuid = "")
		{
			Pallet component = UnityEngine.Object.Instantiate(PalletPrefab).GetComponent<Pallet>();
			component.transform.position = position;
			component.transform.rotation = rotation;
			base.NetworkObject.Spawn(component.gameObject);
			if (GUIDManager.IsGUIDValid(initialSlotGuid))
			{
				PalletSlot @object = GUIDManager.GetObject<PalletSlot>(new Guid(initialSlotGuid));
				if (@object != null)
				{
					component.BindToSlot_Server(@object.GUID);
				}
			}
			return component;
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public virtual string GetSaveString()
		{
			return string.Empty;
		}

		public virtual List<string> WriteData(string parentFolderPath)
		{
			List<string> list = new List<string>();
			string containerFolder = ((ISaveable)this).GetContainerFolder(parentFolderPath);
			for (int i = 0; i < WorldStorageEntity.All.Count; i++)
			{
				if (WorldStorageEntity.All[i].ShouldSave())
				{
					new SaveRequest(WorldStorageEntity.All[i], containerFolder);
					list.Add(WorldStorageEntity.All[i].SaveFileName);
				}
			}
			return list;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EStorage_002EStorageManager_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			InitializeSaveable();
		}
	}
}
