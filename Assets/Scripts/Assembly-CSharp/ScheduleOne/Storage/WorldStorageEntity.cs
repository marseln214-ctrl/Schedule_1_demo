using System;
using System.Collections.Generic;
using EasyButtons;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;

namespace ScheduleOne.Storage
{
	public class WorldStorageEntity : StorageEntity, IGUIDRegisterable, ISaveable
	{
		public static List<WorldStorageEntity> All = new List<WorldStorageEntity>();

		[SerializeField]
		protected string BakedGUID = string.Empty;

		private bool NetworkInitialize___EarlyScheduleOne_002EStorage_002EWorldStorageEntityAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EStorage_002EWorldStorageEntityAssembly_002DCSharp_002Edll_Excuted;

		public Guid GUID { get; protected set; }

		public string SaveFolderName => "Entity_" + GUID.ToString().Substring(0, 6);

		public string SaveFileName => "Entity_" + GUID.ToString().Substring(0, 6);

		public Loader Loader => null;

		public bool ShouldSaveUnderFolder => false;

		public List<string> LocalExtraFiles { get; set; } = new List<string> { "Contents" };


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		[Button]
		public void RegenerateGUID()
		{
			BakedGUID = Guid.NewGuid().ToString();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EStorage_002EWorldStorageEntity_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			All.Remove(this);
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
		}

		public virtual bool ShouldSave()
		{
			return base.ItemCount > 0;
		}

		public virtual string GetSaveString()
		{
			return new WorldStorageEntityData(GUID, new ItemSet(base.ItemSlots)).GetJson();
		}

		public virtual void Load(WorldStorageEntityData data)
		{
			for (int i = 0; i < data.Contents.Items.Length; i++)
			{
				ItemInstance instance = ItemDeserializer.LoadItem(data.Contents.Items[i]);
				if (base.ItemSlots.Count > i)
				{
					base.ItemSlots[i].SetStoredItem(instance);
				}
			}
		}

		protected override void ContentsChanged()
		{
			base.ContentsChanged();
			HasChanged = true;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EStorage_002EWorldStorageEntityAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EStorage_002EWorldStorageEntityAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EStorage_002EWorldStorageEntityAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EStorage_002EWorldStorageEntityAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EStorage_002EWorldStorageEntity_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			All.Add(this);
			if (!GUIDManager.IsGUIDValid(BakedGUID))
			{
				Console.LogError(base.gameObject.name + "'s baked GUID is not valid! Bad.");
			}
			if (GUIDManager.IsGUIDAlreadyRegistered(new Guid(BakedGUID)))
			{
				Console.LogError(base.gameObject.name + "'s baked GUID is already registered! Bad.", this);
			}
			GUID = new Guid(BakedGUID);
			GUIDManager.RegisterObject(this);
			InitializeSaveable();
		}
	}
}
