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
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Management;
using UnityEngine;

namespace ScheduleOne.Employees
{
	public class Chemist : Employee, IConfigurable
	{
		public const int MAX_ASSIGNED_STATIONS = 3;

		[Header("References")]
		public Sprite typeIcon;

		[SerializeField]
		protected ConfigurationReplicator configReplicator;

		[Header("Behaviours")]
		public StartChemistryStationBehaviour StartChemistryStationBehaviour;

		public StartLabOvenBehaviour StartLabOvenBehaviour;

		public FinishLabOvenBehaviour FinishLabOvenBehaviour;

		public StartCauldronBehaviour StartCauldronBehaviour;

		public StartMixingStationBehaviour StartMixingStationBehaviour;

		[Header("UI")]
		public ChemistUIElement WorldspaceUIPrefab;

		public Transform uiPoint;

		[CompilerGenerated]
		[SyncVar]
		public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted;

		public EntityConfiguration Configuration => configuration;

		protected ChemistConfiguration configuration { get; set; }

		public ConfigurationReplicator ConfigReplicator => configReplicator;

		public EConfigurableType ConfigurableType => EConfigurableType.Chemist;

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

		protected override void AssignProperty(ScheduleOne.Property.Property prop)
		{
			base.AssignProperty(prop);
			prop.Configurables.Add(this);
			configuration = new ChemistConfiguration(configReplicator, this, this);
			CreateWorldspaceUI();
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

		protected override void UpdateBehaviour()
		{
			base.UpdateBehaviour();
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (AnyWorkInProgress())
			{
				MarkIsWorking();
			}
			else if (!(GetBed() == null) && base.PaidForToday)
			{
				if (configuration.TotalStations == 0)
				{
					SubmitNoWorkReason("I haven't been assigned any stations", "You can use your management clipboards to assign stations to me.");
					SetIdle(idle: true);
				}
				else if (InstanceFinder.IsServer)
				{
					TryStartNewTask();
				}
			}
		}

		private void TryStartNewTask()
		{
			List<LabOven> labOvensReadyToFinish = GetLabOvensReadyToFinish();
			if (labOvensReadyToFinish.Count > 0)
			{
				FinishLabOven(labOvensReadyToFinish[0]);
				return;
			}
			List<LabOven> labOvensReadyToStart = GetLabOvensReadyToStart();
			if (labOvensReadyToStart.Count > 0)
			{
				StartLabOven(labOvensReadyToStart[0]);
				return;
			}
			List<ChemistryStation> chemistryStationsReadyToStart = GetChemistryStationsReadyToStart();
			if (chemistryStationsReadyToStart.Count > 0)
			{
				StartChemistryStation(chemistryStationsReadyToStart[0]);
				return;
			}
			List<Cauldron> cauldronsReadyToStart = GetCauldronsReadyToStart();
			if (cauldronsReadyToStart.Count > 0)
			{
				StartCauldron(cauldronsReadyToStart[0]);
				return;
			}
			List<MixingStation> mixingStationsReadyToStart = GetMixingStationsReadyToStart();
			if (mixingStationsReadyToStart.Count > 0)
			{
				StartMixingStation(mixingStationsReadyToStart[0]);
				return;
			}
			List<LabOven> labOvensReadyToMove = GetLabOvensReadyToMove();
			if (labOvensReadyToMove.Count > 0)
			{
				MoveItemBehaviour.Initialize((labOvensReadyToMove[0].Configuration as LabOvenConfiguration).DestinationRoute, labOvensReadyToMove[0].OutputSlot.ItemInstance.ID);
				MoveItemBehaviour.Enable_Networked(null);
				return;
			}
			List<ChemistryStation> chemStationsReadyToMove = GetChemStationsReadyToMove();
			if (chemStationsReadyToMove.Count > 0)
			{
				MoveItemBehaviour.Initialize((chemStationsReadyToMove[0].Configuration as ChemistryStationConfiguration).DestinationRoute, chemStationsReadyToMove[0].OutputSlot.ItemInstance.ID);
				MoveItemBehaviour.Enable_Networked(null);
				return;
			}
			List<Cauldron> cauldronsReadyToMove = GetCauldronsReadyToMove();
			if (cauldronsReadyToMove.Count > 0)
			{
				MoveItemBehaviour.Initialize((cauldronsReadyToMove[0].Configuration as CauldronConfiguration).DestinationRoute, cauldronsReadyToMove[0].OutputSlot.ItemInstance.ID);
				MoveItemBehaviour.Enable_Networked(null);
				return;
			}
			List<MixingStation> mixStationsReadyToMove = GetMixStationsReadyToMove();
			if (mixStationsReadyToMove.Count > 0)
			{
				MoveItemBehaviour.Initialize((mixStationsReadyToMove[0].Configuration as MixingStationConfiguration).DestinationRoute, mixStationsReadyToMove[0].OutputSlot.ItemInstance.ID);
				MoveItemBehaviour.Enable_Networked(null);
			}
			else
			{
				SubmitNoWorkReason("There's nothing for me to do right now.", string.Empty);
				SetIdle(idle: true);
			}
		}

		private bool AnyWorkInProgress()
		{
			if (StartChemistryStationBehaviour.Active)
			{
				return true;
			}
			if (StartLabOvenBehaviour.Active)
			{
				return true;
			}
			if (FinishLabOvenBehaviour.Active)
			{
				return true;
			}
			if (MoveItemBehaviour.Active)
			{
				return true;
			}
			return false;
		}

		protected override bool ShouldIdle()
		{
			if (configuration.Stations.SelectedObjects.Count == 0)
			{
				return true;
			}
			return base.ShouldIdle();
		}

		private void StartChemistryStation(ChemistryStation station)
		{
			StartChemistryStationBehaviour.SetTargetStation(station);
			StartChemistryStationBehaviour.Enable_Networked(null);
		}

		private void StartCauldron(Cauldron cauldron)
		{
			StartCauldronBehaviour.AssignStation(cauldron);
			StartCauldronBehaviour.Enable_Networked(null);
		}

		private void StartLabOven(LabOven oven)
		{
			StartLabOvenBehaviour.SetTargetOven(oven);
			StartLabOvenBehaviour.Enable_Networked(null);
		}

		private void FinishLabOven(LabOven oven)
		{
			FinishLabOvenBehaviour.SetTargetOven(oven);
			FinishLabOvenBehaviour.Enable_Networked(null);
		}

		private void StartMixingStation(MixingStation station)
		{
			StartMixingStationBehaviour.AssignStation(station);
			StartMixingStationBehaviour.Enable_Networked(null);
		}

		public override BedItem GetBed()
		{
			return configuration.bedItem;
		}

		public List<LabOven> GetLabOvensReadyToFinish()
		{
			List<LabOven> list = new List<LabOven>();
			foreach (LabOven labOven in configuration.LabOvens)
			{
				if (!((IUsable)labOven).IsInUse && labOven.CurrentOperation != null && labOven.IsReadyForHarvest() && labOven.CanOutputSpaceFitCurrentOperation())
				{
					list.Add(labOven);
				}
			}
			return list;
		}

		public List<LabOven> GetLabOvensReadyToStart()
		{
			List<LabOven> list = new List<LabOven>();
			foreach (LabOven labOven in configuration.LabOvens)
			{
				if (!((IUsable)labOven).IsInUse && labOven.CurrentOperation == null && labOven.IsReadyToStart())
				{
					list.Add(labOven);
				}
			}
			return list;
		}

		public List<ChemistryStation> GetChemistryStationsReadyToStart()
		{
			List<ChemistryStation> list = new List<ChemistryStation>();
			foreach (ChemistryStation chemStation in configuration.ChemStations)
			{
				if (!((IUsable)chemStation).IsInUse && chemStation.CurrentCookOperation == null)
				{
					StationRecipe selectedRecipe = (chemStation.Configuration as ChemistryStationConfiguration).Recipe.SelectedRecipe;
					if (!(selectedRecipe == null) && chemStation.HasIngredientsForRecipe(selectedRecipe))
					{
						list.Add(chemStation);
					}
				}
			}
			return list;
		}

		public List<Cauldron> GetCauldronsReadyToStart()
		{
			List<Cauldron> list = new List<Cauldron>();
			foreach (Cauldron cauldron in configuration.Cauldrons)
			{
				if (!((IUsable)cauldron).IsInUse && cauldron.RemainingCookTime <= 0 && cauldron.GetState() == Cauldron.EState.Ready)
				{
					list.Add(cauldron);
				}
			}
			return list;
		}

		public List<MixingStation> GetMixingStationsReadyToStart()
		{
			List<MixingStation> list = new List<MixingStation>();
			foreach (MixingStation mixStation in configuration.MixStations)
			{
				if (!((IUsable)mixStation).IsInUse && mixStation.CanStartMix() && mixStation.CurrentMixOperation == null && !((float)mixStation.GetMixQuantity() < (mixStation.Configuration as MixingStationConfiguration).StartThrehold.Value))
				{
					list.Add(mixStation);
				}
			}
			return list;
		}

		protected List<LabOven> GetLabOvensReadyToMove()
		{
			List<LabOven> list = new List<LabOven>();
			foreach (LabOven labOven in configuration.LabOvens)
			{
				ItemSlot outputSlot = labOven.OutputSlot;
				if (outputSlot.Quantity != 0 && MoveItemBehaviour.IsTransitRouteValid((labOven.Configuration as LabOvenConfiguration).DestinationRoute, outputSlot.ItemInstance.ID))
				{
					list.Add(labOven);
				}
			}
			return list;
		}

		protected List<ChemistryStation> GetChemStationsReadyToMove()
		{
			List<ChemistryStation> list = new List<ChemistryStation>();
			foreach (ChemistryStation chemStation in configuration.ChemStations)
			{
				ItemSlot outputSlot = chemStation.OutputSlot;
				if (outputSlot.Quantity != 0 && MoveItemBehaviour.IsTransitRouteValid((chemStation.Configuration as ChemistryStationConfiguration).DestinationRoute, outputSlot.ItemInstance.ID))
				{
					list.Add(chemStation);
				}
			}
			return list;
		}

		protected List<Cauldron> GetCauldronsReadyToMove()
		{
			List<Cauldron> list = new List<Cauldron>();
			foreach (Cauldron cauldron in configuration.Cauldrons)
			{
				ItemSlot outputSlot = cauldron.OutputSlot;
				if (outputSlot.Quantity != 0 && MoveItemBehaviour.IsTransitRouteValid((cauldron.Configuration as CauldronConfiguration).DestinationRoute, outputSlot.ItemInstance.ID))
				{
					list.Add(cauldron);
				}
			}
			return list;
		}

		protected List<MixingStation> GetMixStationsReadyToMove()
		{
			List<MixingStation> list = new List<MixingStation>();
			foreach (MixingStation mixStation in configuration.MixStations)
			{
				ItemSlot outputSlot = mixStation.OutputSlot;
				if (outputSlot.Quantity != 0 && MoveItemBehaviour.IsTransitRouteValid((mixStation.Configuration as MixingStationConfiguration).DestinationRoute, outputSlot.ItemInstance.ID))
				{
					list.Add(mixStation);
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
			ChemistUIElement component = Object.Instantiate(WorldspaceUIPrefab, assignedProperty.WorldspaceUIContainer).GetComponent<ChemistUIElement>();
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
			return new ChemistData(ID, base.AssignedProperty.PropertyCode, FirstName, LastName, base.IsMale, base.AppearanceIndex, base.transform.position, base.transform.rotation, base.GUID, base.PaidForToday, MoveItemBehaviour.GetSaveData()).GetJson();
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
			if (!NetworkInitialize___EarlyScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>(this, 2u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, CurrentPlayerConfigurer);
				RegisterServerRpc(37u, RpcReader___Server_SetConfigurer_3323014238);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EEmployees_002EChemist);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted = true;
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

		public virtual bool ReadSyncVar___ScheduleOne_002EEmployees_002EChemist(PooledReader PooledReader0, byte UInt321, bool Boolean2)
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
