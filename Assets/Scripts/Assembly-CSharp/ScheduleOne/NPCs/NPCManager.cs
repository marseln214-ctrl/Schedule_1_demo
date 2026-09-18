using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;

namespace ScheduleOne.NPCs
{
	public class NPCManager : NetworkSingleton<NPCManager>, IBaseSaveable, ISaveable
	{
		public static List<NPC> NPCRegistry = new List<NPC>();

		public Transform[] NPCWarpPoints;

		[Header("Employee Prefabs")]
		public GameObject BotanistPrefab;

		public GameObject PackagerPrefab;

		[Header("Prefabs")]
		public NPCPoI NPCPoIPrefab;

		public NPCPoI PotentialCustomerPoIPrefab;

		public NPCPoI PotentialDealerPoIPrefab;

		private NPCsLoader loader = new NPCsLoader();

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted;

		public string SaveFolderName => "NPCs";

		public string SaveFileName => "NPCs";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => true;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCManager_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Start()
		{
			base.Start();
			Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(delegate
			{
				NPCRegistry.Clear();
			});
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public void Update()
		{
		}

		public static NPC GetNPC(string id)
		{
			foreach (NPC item in NPCRegistry)
			{
				if (item.ID.ToLower() == id.ToLower())
				{
					return item;
				}
			}
			return null;
		}

		public static List<NPC> GetNPCsInRegion(EMapRegion region)
		{
			return NPCRegistry.FindAll((NPC x) => x.Region == region);
		}

		public virtual string GetSaveString()
		{
			return string.Empty;
		}

		public List<Transform> GetOrderedDistanceWarpPoints(Vector3 origin)
		{
			return new List<Transform>(NPCWarpPoints).OrderBy((Transform x) => Vector3.SqrMagnitude(x.position - origin)).ToList();
		}

		public virtual List<string> WriteData(string parentFolderPath)
		{
			List<string> list = new List<string>();
			string containerFolder = ((ISaveable)this).GetContainerFolder(parentFolderPath);
			for (int i = 0; i < NPCRegistry.Count; i++)
			{
				if (NPCRegistry[i].ShouldSave())
				{
					new SaveRequest(NPCRegistry[i], containerFolder);
					list.Add(NPCRegistry[i].SaveFolderName);
				}
			}
			return list;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCManager_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			InitializeSaveable();
		}
	}
}
