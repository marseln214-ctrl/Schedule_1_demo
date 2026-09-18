using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using ScheduleOne.UI.Management;
using UnityEngine;

namespace ScheduleOne.Employees
{
	public class Botanist : Employee, IConfigurable
	{
		public float CRITICAL_WATERING_THRESHOLD = 0.1f;

		public float WATERING_THRESHOLD = 0.3f;

		public float TARGET_WATER_LEVEL_MIN = 0.75f;

		public float TARGET_WATER_LEVEL_MAX = 1f;

		public float SOIL_POUR_TIME = 10f;

		public float WATER_POUR_TIME = 10f;

		public float ADDITIVE_POUR_TIME = 10f;

		public float SEED_SOW_TIME = 15f;

		public float HARVEST_TIME = 15f;

		[Header("References")]
		public Sprite typeIcon;

		[SerializeField]
		protected ConfigurationReplicator configReplicator;

		public PotActionBehaviour PotActionBehaviour;

		[Header("UI")]
		public BotanistUIElement WorldspaceUIPrefab;

		public Transform uiPoint;

		[Header("Settings")]
		public int MaxAssignedPots = 8;

		public DialogueContainer NoAssignedStationsDialogue;

		public DialogueContainer UnspecifiedPotsDialogue;

		public DialogueContainer NullDestinationPotsDialogue;

		public DialogueContainer MissingMaterialsDialogue;

		public DialogueContainer NoPotsRequireWorkDialogue;

		[CompilerGenerated]
		[SyncVar]
		public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted;

		public EntityConfiguration Configuration => configuration;

		protected BotanistConfiguration configuration { get; set; }

		public ConfigurationReplicator ConfigReplicator => configReplicator;

		public EConfigurableType ConfigurableType => EConfigurableType.Botanist;

		public WorldspaceUIElement WorldspaceUI { get; set; }

		public NetworkObject CurrentPlayerConfigurer
		{
			get
			{
				return _003CCurrentPlayerConfigurer_003Ek__BackingField;
			}
			set
			{
                _003CCurrentPlayerConfigurer_003Ek__BackingField = value;

                syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value, true);
            }
		}

		public Sprite TypeIcon => typeIcon;

		public Transform Transform => base.transform;

		public Transform UIPoint => uiPoint;

		public bool CanBeSelected => true;

		public NetworkObject SyncAccessor__003CCurrentPlayerConfigurer_003Ek__BackingField
		{
			get
			{
				return CurrentPlayerConfigurer;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					CurrentPlayerConfigurer = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SetConfigurer(NetworkObject player)
		{
			RpcWriter___Server_SetConfigurer_3323014238(player);
			RpcLogic___SetConfigurer_3323014238(player);
		}

		protected override void Start()
		{
			base.Start();
		}

		protected override void UpdateBehaviour()
		{
			base.UpdateBehaviour();
			if (PotActionBehaviour.Active)
			{
				MarkIsWorking();
			}
			else if (MoveItemBehaviour.Active)
			{
				MarkIsWorking();
			}
			else
			{
				if (GetBed() == null || !base.PaidForToday)
				{
					return;
				}
				if (configuration.AssignedPots.Count == 0)
				{
					SubmitNoWorkReason("I haven't been assigned any pots", "You can use your management clipboards to assign pots to me.");
					SetIdle(idle: true);
				}
				else
				{
					if (!InstanceFinder.IsServer)
					{
						return;
					}
					Pot potForWatering = GetPotForWatering(CRITICAL_WATERING_THRESHOLD);
					if (potForWatering != null)
					{
						StartAction(potForWatering, PotActionBehaviour.EActionType.Water);
						return;
					}
					Pot potForSoilSour = GetPotForSoilSour();
					if (potForSoilSour != null)
					{
						if (PotActionBehaviour.DoesBotanistHaveMaterialsForTask(this, potForSoilSour, PotActionBehaviour.EActionType.PourSoil))
						{
							StartAction(potForSoilSour, PotActionBehaviour.EActionType.PourSoil);
							return;
						}
						string fix = "Make sure there's soil in my supplies stash.";
						if (configuration.Supplies.SelectedObject == null)
						{
							fix = "Use your management clipboards to assign a supplies stash to me. Then make sure there's soil in it.";
						}
						SubmitNoWorkReason("There are empty pots, but I don't have any soil to pour.", fix);
					}
					List<Pot> potsReadyForSeed = GetPotsReadyForSeed();
					List<Pot> list = FilterPotsForSpecifiedSeed(potsReadyForSeed);
					foreach (Pot item in potsReadyForSeed)
					{
						if (!list.Contains(item))
						{
							SubmitNoWorkReason("There it a pot ready for sowing, but it doesn't have an assigned seed type.", "Use your management clipboard to assign a seed type to it.");
							continue;
						}
						if (PotActionBehaviour.DoesBotanistHaveMaterialsForTask(this, item, PotActionBehaviour.EActionType.SowSeed))
						{
							StartAction(item, PotActionBehaviour.EActionType.SowSeed);
							return;
						}
						string fix2 = "Make sure I have the right seeds in my supplies stash.";
						if (configuration.Supplies.SelectedObject == null)
						{
							fix2 = "Use your management clipboards to assign a supplies stash to me, and make sure it contains the right seeds.";
						}
						SubmitNoWorkReason("There is a pot ready for sowing, but I don't have the right seed for it.", fix2, 1);
					}
					int additiveNumber;
					Pot potsForAdditives = GetPotsForAdditives(out additiveNumber);
					if (potsForAdditives != null && PotActionBehaviour.DoesBotanistHaveMaterialsForTask(this, potsForAdditives, PotActionBehaviour.EActionType.ApplyAdditive, additiveNumber))
					{
						PotActionBehaviour.AdditiveNumber = additiveNumber;
						StartAction(potsForAdditives, PotActionBehaviour.EActionType.ApplyAdditive);
						return;
					}
					foreach (Pot item2 in GetPotsForHarvest())
					{
						if (PotActionBehaviour.DoesPotHaveValidDestination(item2))
						{
							StartAction(item2, PotActionBehaviour.EActionType.Harvest);
							return;
						}
						SubmitNoWorkReason("There is a plant ready for harvest, but it has no assigned destination.", "Use your management clipboard to assign a destination for each of my pots.");
					}
					Pot potForWatering2 = GetPotForWatering(WATERING_THRESHOLD);
					if (potForWatering2 != null)
					{
						StartAction(potForWatering2, PotActionBehaviour.EActionType.Water);
						return;
					}
					SubmitNoWorkReason("There's nothing for me to do right now.", string.Empty);
					SetIdle(idle: true);
				}
			}
		}

		private void StartAction(Pot pot, PotActionBehaviour.EActionType actionType)
		{
			SetIdle(idle: false);
			PotActionBehaviour.Initialize(pot, actionType);
			PotActionBehaviour.Enable_Networked(null);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			SendConfigurationToClient(connection);
		}

		public void SendConfigurationToClient(NetworkConnection conn)
		{
			if (!conn.IsHost)
			{
				Singleton<CoroutineService>.Instance.StartCoroutine(WaitForConfig());
			}
			IEnumerator WaitForConfig()
			{
				yield return new WaitUntil(() => Configuration != null);
				Configuration.ReplicateAllFields(conn);
			}
		}

		protected override void AssignProperty(ScheduleOne.Property.Property prop)
		{
			base.AssignProperty(prop);
			prop.Configurables.Add(this);
			configuration = new BotanistConfiguration(configReplicator, this, this);
			CreateWorldspaceUI();
		}

		public ItemInstance GetItemInSupplies(string id)
		{
			if (configuration.Supplies.SelectedObject == null)
			{
				return null;
			}
			if (!PotActionBehaviour.CanGetToSupplies())
			{
				return null;
			}
			List<ItemSlot> list = new List<ItemSlot>();
			BuildableItem selectedObject = configuration.Supplies.SelectedObject;
			if (selectedObject != null)
			{
				list.AddRange((selectedObject as ITransitEntity).OutputSlots);
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Quantity > 0 && list[i].ItemInstance.ID.ToLower() == id.ToLower())
				{
					return list[i].ItemInstance;
				}
			}
			return null;
		}

		protected override bool ShouldIdle()
		{
			if (configuration.Pots.SelectedObjects.Count == 0)
			{
				return true;
			}
			return base.ShouldIdle();
		}

		public override BedItem GetBed()
		{
			return configuration.bedItem;
		}

		private bool AreThereUnspecifiedPots()
		{
			for (int i = 0; i < configuration.AssignedPots.Count; i++)
			{
				if ((configuration.AssignedPots[i].Configuration as PotConfiguration).Seed.SelectedItem == null)
				{
					return true;
				}
			}
			return false;
		}

		private bool AreThereNullDestinationPots()
		{
			foreach (Pot assignedPot in configuration.AssignedPots)
			{
				if (assignedPot.IsReadyForHarvest(out var _) && (assignedPot.Configuration as PotConfiguration).Destination.SelectedObject == null)
				{
					return true;
				}
			}
			return false;
		}

		private bool IsMissingRequiredMaterials()
		{
			Pot potForSoilSour = GetPotForSoilSour();
			if (potForSoilSour != null && !PotActionBehaviour.DoesBotanistHaveMaterialsForTask(this, potForSoilSour, PotActionBehaviour.EActionType.PourSoil))
			{
				return false;
			}
			List<Pot> potsReadyForSeed = GetPotsReadyForSeed();
			for (int i = 0; i < potsReadyForSeed.Count; i++)
			{
				if (PotActionBehaviour.DoesBotanistHaveMaterialsForTask(this, potsReadyForSeed[i], PotActionBehaviour.EActionType.SowSeed))
				{
					return false;
				}
			}
			return false;
		}

		private Pot GetPotForWatering(float threshold)
		{
			for (int i = 0; i < configuration.AssignedPots.Count; i++)
			{
				if (PotActionBehaviour.CanPotBeWatered(configuration.AssignedPots[i], threshold))
				{
					return configuration.AssignedPots[i];
				}
			}
			return null;
		}

		private Pot GetPotForSoilSour()
		{
			for (int i = 0; i < configuration.AssignedPots.Count; i++)
			{
				if (PotActionBehaviour.CanPotHaveSoilPour(configuration.AssignedPots[i]))
				{
					return configuration.AssignedPots[i];
				}
			}
			return null;
		}

		private List<Pot> GetPotsReadyForSeed()
		{
			List<Pot> list = new List<Pot>();
			for (int i = 0; i < configuration.AssignedPots.Count; i++)
			{
				if (PotActionBehaviour.CanPotHaveSeedSown(configuration.AssignedPots[i], ensureAssignedSeed: false))
				{
					list.Add(configuration.AssignedPots[i]);
				}
			}
			return list;
		}

		private List<Pot> FilterPotsForSpecifiedSeed(List<Pot> pots)
		{
			List<Pot> list = new List<Pot>();
			foreach (Pot pot in pots)
			{
				if ((pot.Configuration as PotConfiguration).Seed.SelectedItem != null)
				{
					list.Add(pot);
				}
			}
			return list;
		}

		private Pot GetPotsForAdditives(out int additiveNumber)
		{
			additiveNumber = -1;
			for (int i = 0; i < configuration.AssignedPots.Count; i++)
			{
				if (PotActionBehaviour.CanPotHaveAdditiveApplied(configuration.AssignedPots[i], out additiveNumber))
				{
					return configuration.AssignedPots[i];
				}
			}
			return null;
		}

		private List<Pot> GetPotsForHarvest()
		{
			List<Pot> list = new List<Pot>();
			for (int i = 0; i < configuration.AssignedPots.Count; i++)
			{
				if (PotActionBehaviour.CanPotBeHarvested(configuration.AssignedPots[i]))
				{
					list.Add(configuration.AssignedPots[i]);
				}
			}
			return list;
		}

		public WorldspaceUIElement CreateWorldspaceUI()
		{
			if (WorldspaceUI != null)
			{
				Console.LogWarning(base.gameObject.name + " already has a worldspace UI element!");
			}
			ScheduleOne.Property.Property assignedProperty = base.AssignedProperty;
			if (assignedProperty == null)
			{
				Console.LogError(assignedProperty?.ToString() + " is not a child of a property!");
				return null;
			}
			BotanistUIElement component = Object.Instantiate(WorldspaceUIPrefab, assignedProperty.WorldspaceUIContainer).GetComponent<BotanistUIElement>();
			component.Initialize(this);
			WorldspaceUI = component;
			return component;
		}

		public void DestroyWorldspaceUI()
		{
			if (WorldspaceUI != null)
			{
				WorldspaceUI.Destroy();
			}
		}

		public override string GetSaveString()
		{
			return new BotanistData(ID, base.AssignedProperty.PropertyCode, FirstName, LastName, base.IsMale, base.AppearanceIndex, base.transform.position, base.transform.rotation, base.GUID, base.PaidForToday, MoveItemBehaviour.GetSaveData()).GetJson();
		}

		public override List<string> WriteData(string parentFolderPath)
		{
			List<string> list = new List<string>();
			if (Configuration.ShouldSave())
			{
				list.Add("Configuration.json");
				((ISaveable)this).WriteSubfile(parentFolderPath, "Configuration", Configuration.GetSaveString());
			}
			list.AddRange(base.WriteData(parentFolderPath));
			return list;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>(this, 2u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, CurrentPlayerConfigurer);
				RegisterServerRpc(37u, RpcReader___Server_SetConfigurer_3323014238);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EEmployees_002EBotanist);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SetConfigurer_3323014238(NetworkObject player)
		{
			if (!base.IsClientInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteNetworkObject(player);
				SendServerRpc(37u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetConfigurer_3323014238(NetworkObject player)
		{
			CurrentPlayerConfigurer = player;
		}

		private void RpcReader___Server_SetConfigurer_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject player = PooledReader0.ReadNetworkObject();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetConfigurer_3323014238(player);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EEmployees_002EBotanist(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 2)
			{
				if (PooledReader0 == null)
				{
					syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.GetValue(calledByUser: true), true);
					return true;
				}
				NetworkObject value = PooledReader0.ReadNetworkObject();
                syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			return false;
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
