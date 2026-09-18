using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.FX;
using ScheduleOne.Law;
using ScheduleOne.Map;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using ScheduleOne.Vision;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Police
{
	public class PoliceOfficer : NPC
	{
		public const float DEACTIVATION_TIME = 1f;

		public const float INVESTIGATION_COOLDOWN = 45f;

		public const float INVESTIGATION_MAX_DISTANCE = 8f;

		public const float INVESTIGATION_MIN_VISIBILITY = 0.2f;

		public const float INVESTIGATION_CHECK_INTERVAL = 1f;

		public const float BODY_SEARCH_CHANCE_DEFAULT = 0.1f;

		public const float MIN_CHATTER_INTERVAL = 15f;

		public const float MAX_CHATTER_INTERVAL = 45f;

		public static Action<VisionEventReceipt> OnPoliceVisionEvent;

		public static List<PoliceOfficer> Officers = new List<PoliceOfficer>();

		[CompilerGenerated]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CTargetPlayerNOB_003Ek__BackingField;

		public LandVehicle AssignedVehicle;

		[Header("References")]
		public PursuitBehaviour PursuitBehaviour;

		public VehiclePursuitBehaviour VehiclePursuitBehaviour;

		public BodySearchBehaviour BodySearchBehaviour;

		public CheckpointBehaviour CheckpointBehaviour;

		public FootPatrolBehaviour FootPatrolBehaviour;

		public ProximityCircle ProxCircle;

		public VehiclePatrolBehaviour VehiclePatrolBehaviour;

		public SentryBehaviour SentryBehaviour;

		public PoliceChatterVO ChatterVO;

		[Header("Dialogue")]
		public DialogueContainer CheckpointDialogue;

		[Header("Tools")]
		public AvatarEquippable BatonPrefab;

		public AvatarEquippable TaserPrefab;

		public AvatarEquippable GunPrefab;

		[Header("Settings")]
		public bool AutoDeactivate = true;

		public bool ChatterEnabled = true;

		[Header("Behaviour Settings")]
		[Range(0f, 1f)]
		public float Suspicion = 0.5f;

		[Range(0f, 1f)]
		public float Leniency = 0.5f;

		[Header("Body Search Settings")]
		[Range(0f, 1f)]
		public float BodySearchChance = 0.1f;

		[Range(1f, 10f)]
		public float BodySearchDuration = 5f;

		[HideInInspector]
		public PoliceBelt belt;

		private float timeSinceReadyToPool;

		private float timeSinceOutOfSight;

		private float chatterCountDown;

		private Investigation currentBodySearchInvestigation;

		public SyncVar<NetworkObject> syncVar____003CTargetPlayerNOB_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted;

		//public NetworkObject TargetPlayerNOB
		//{
		//	[CompilerGenerated]
		//	get
		//	{
		//		return SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField;
		//	}
		//	[CompilerGenerated]
		//	protected set
		//	{
		//		syncVar____003CTargetPlayerNOB_003Ek__BackingField.SetValue(value,  true);
		//	}
		//}

        public NetworkObject TargetPlayerNOB
        {
            get
            {
                return _003CTargetPlayerNOB_003Ek__BackingField;
            }
            protected set
            {
                _003CTargetPlayerNOB_003Ek__BackingField = value;

                syncVar____003CTargetPlayerNOB_003Ek__BackingField.SetValue(value, true);
            }
        }

        public NetworkObject SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField
		{
			get
			{
				return TargetPlayerNOB;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					TargetPlayerNOB = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CTargetPlayerNOB_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EPolice_002EPoliceOfficer_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Start()
		{
			base.Start();
			belt = Avatar.GetComponentInChildren<PoliceBelt>();
		}

		protected override void Update()
		{
			base.Update();
			for (int i = 0; i < Player.PlayerList.Count; i++)
			{
				awareness.VisionCone.StateSettings[Player.PlayerList[i]][PlayerVisualState.EVisualState.Wanted].Enabled = SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField == null;
				awareness.VisionCone.StateSettings[Player.PlayerList[i]][PlayerVisualState.EVisualState.Suspicious].Enabled = SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField == null && !Player.PlayerList[i].CrimeData.BodySearchPending && Player.PlayerList[i].CrimeData.TimeSinceLastBodySearch > 30f;
				awareness.VisionCone.StateSettings[Player.PlayerList[i]][PlayerVisualState.EVisualState.DisobeyingCurfew].Enabled = SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField == null;
				awareness.VisionCone.StateSettings[Player.PlayerList[i]][PlayerVisualState.EVisualState.DrugDealing].Enabled = SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField == null;
				awareness.VisionCone.StateSettings[Player.PlayerList[i]][PlayerVisualState.EVisualState.Vandalizing].Enabled = SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField == null;
				awareness.VisionCone.StateSettings[Player.PlayerList[i]][PlayerVisualState.EVisualState.Pickpocketing].Enabled = SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField == null;
				awareness.VisionCone.StateSettings[Player.PlayerList[i]][PlayerVisualState.EVisualState.Brandishing].Enabled = SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField == null;
				awareness.VisionCone.StateSettings[Player.PlayerList[i]][PlayerVisualState.EVisualState.DischargingWeapon].Enabled = SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField == null;
			}
			UpdateBodySearch();
			UpdateChatter();
		}

		protected override void MinPass()
		{
			base.MinPass();
			if (base.CurrentBuilding == null && InstanceFinder.IsServer && AutoDeactivate)
			{
				CheckDeactivation();
			}
		}

		private void CheckDeactivation()
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField != null)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
			if (behaviour.ScheduleManager.ActiveAction != null)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
			if (CheckpointBehaviour.Active)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
			if (FootPatrolBehaviour.Active)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
			if (VehiclePatrolBehaviour.Active)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
			if (BodySearchBehaviour.Active)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
			if (SentryBehaviour.Active)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
			if (!base.IsConscious)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
			if (behaviour.RagdollBehaviour.Active)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
			if (behaviour.GenericDialogueBehaviour.Active)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
			if (behaviour.FacePlayerBehaviour.Active)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
			timeSinceReadyToPool += 5f / 6f;
			if (timeSinceReadyToPool < 1f)
			{
				return;
			}
			if (!movement.IsMoving && Singleton<ScheduleOne.Map.Map>.InstanceExists)
			{
				if (movement.IsAsCloseAsPossible(Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.Doors[0].transform.position, 1f))
				{
					Deactivate();
					return;
				}
				if (movement.CanGetTo(Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.Doors[0].transform.position))
				{
					movement.SetDestination(Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.Doors[0].transform.position);
				}
				else
				{
					Deactivate();
				}
			}
			bool flag = false;
			foreach (Player player in Player.PlayerList)
			{
				if (player.IsPointVisibleToPlayer(Avatar.CenterPoint))
				{
					flag = true;
					break;
				}
				if (AssignedVehicle != null && player.IsPointVisibleToPlayer(AssignedVehicle.transform.position))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				timeSinceReadyToPool += 5f / 6f;
				timeSinceOutOfSight += 5f / 6f;
				if (timeSinceOutOfSight > 1f)
				{
					Deactivate();
				}
			}
			else
			{
				timeSinceOutOfSight = 0f;
			}
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public virtual void BeginFootPursuit_Networked(NetworkObject target, bool includeColleagues = true)
		{
			RpcWriter___Server_BeginFootPursuit_Networked_419679943(target, includeColleagues);
			RpcLogic___BeginFootPursuit_Networked_419679943(target, includeColleagues);
		}

		[ObserversRpc(RunLocally = true)]
		private void BeginFootPursuitTest(string playerCode)
		{
			RpcWriter___Observers_BeginFootPursuitTest_3615296227(playerCode);
			RpcLogic___BeginFootPursuitTest_3615296227(playerCode);
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public virtual void BeginVehiclePursuit_Networked(NetworkObject target, NetworkObject vehicle, bool beginAsSighted)
		{
			RpcWriter___Server_BeginVehiclePursuit_Networked_2261819652(target, vehicle, beginAsSighted);
			RpcLogic___BeginVehiclePursuit_Networked_2261819652(target, vehicle, beginAsSighted);
		}

		[ObserversRpc(RunLocally = true)]
		private void BeginVehiclePursuit(NetworkObject target, NetworkObject vehicle, bool beginAsSighted)
		{
			RpcWriter___Observers_BeginVehiclePursuit_2261819652(target, vehicle, beginAsSighted);
			RpcLogic___BeginVehiclePursuit_2261819652(target, vehicle, beginAsSighted);
		}

		public void BeginBodySearch_LocalPlayer()
		{
			BeginBodySearch_Networked(Player.Local.NetworkObject);
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public virtual void BeginBodySearch_Networked(NetworkObject target)
		{
			RpcWriter___Server_BeginBodySearch_Networked_3323014238(target);
			RpcLogic___BeginBodySearch_Networked_3323014238(target);
		}

		[ObserversRpc(RunLocally = true)]
		private void BeginBodySearch(NetworkObject target)
		{
			RpcWriter___Observers_BeginBodySearch_3323014238(target);
			RpcLogic___BeginBodySearch_3323014238(target);
		}

		[ObserversRpc(RunLocally = true)]
		public virtual void AssignToCheckpoint(CheckpointManager.ECheckpointLocation location)
		{
			RpcWriter___Observers_AssignToCheckpoint_4087078542(location);
			RpcLogic___AssignToCheckpoint_4087078542(location);
		}

		public void UnassignFromCheckpoint()
		{
			CheckpointBehaviour.Disable_Networked(null);
			dialogueHandler.GetComponent<DialogueController>().OverrideContainer = null;
		}

		public void StartFootPatrol(PatrolGroup group, bool warpToStartPoint)
		{
			FootPatrolBehaviour.SetGroup(group);
			FootPatrolBehaviour.Enable_Networked(null);
			if (warpToStartPoint)
			{
				movement.Warp(group.GetDestination(this));
			}
		}

		public void StartVehiclePatrol(VehiclePatrolRoute route, LandVehicle vehicle)
		{
			VehiclePatrolBehaviour.Vehicle = vehicle;
			VehiclePatrolBehaviour.SetRoute(route);
			VehiclePatrolBehaviour.Enable_Networked(null);
		}

		public virtual void AssignToSentryLocation(SentryLocation location)
		{
			SentryBehaviour.AssignLocation(location);
			SentryBehaviour.Enable();
		}

		public void UnassignFromSentryLocation()
		{
			SentryBehaviour.UnassignLocation();
			SentryBehaviour.Disable();
		}

		public void Activate()
		{
			timeSinceReadyToPool = 0f;
			timeSinceOutOfSight = 0f;
			ExitBuilding();
		}

		public void Deactivate()
		{
			if (!InstanceFinder.IsServer)
			{
				Console.LogError("Attempted to deactivate an officer on the client");
				return;
			}
			Console.Log(base.fullName + " returned to station");
			if (AssignedVehicle != null)
			{
				Singleton<CoroutineService>.Instance.StartCoroutine(Wait());
			}
			EnterBuilding(null, Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.GUID.ToString(), 0);
			IEnumerator Wait()
			{
				yield return new WaitUntil(() => !AssignedVehicle.isOccupied);
				AssignedVehicle.DestroyVehicle();
			}
		}

		protected override bool ShouldNoticeGeneralCrime(Player player)
		{
			if (SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField != null)
			{
				return false;
			}
			return base.ShouldNoticeGeneralCrime(player);
		}

		public override bool ShouldSave()
		{
			return false;
		}

		public override string GetNameAddress()
		{
			return "Officer " + LastName;
		}

		private void UpdateChatter()
		{
			chatterCountDown -= Time.deltaTime;
			if (chatterCountDown <= 0f)
			{
				chatterCountDown = UnityEngine.Random.Range(15f, 45f);
				if (ChatterEnabled && ChatterVO.gameObject.activeInHierarchy)
				{
					ChatterVO.Play(EVOLineType.PoliceChatter);
				}
			}
		}

		private void ProcessVisionEvent(VisionEventReceipt visionEventReceipt)
		{
			if (OnPoliceVisionEvent != null)
			{
				OnPoliceVisionEvent(visionEventReceipt);
			}
		}

		public virtual void UpdateBodySearch()
		{
			if (CanInvestigate() && currentBodySearchInvestigation != null)
			{
				UpdateExistingInvestigation();
			}
		}

		private bool CanInvestigate()
		{
			if (VehiclePursuitBehaviour.Active || PursuitBehaviour.Active || BodySearchBehaviour.Active)
			{
				return false;
			}
			if (base.CurrentBuilding != null)
			{
				return false;
			}
			return true;
		}

		private void UpdateExistingInvestigation()
		{
			if (!CanInvestigatePlayer(currentBodySearchInvestigation.Target))
			{
				StopBodySearchInvestigation();
				return;
			}
			Player target = currentBodySearchInvestigation.Target;
			float playerVisibility = awareness.VisionCone.GetPlayerVisibility(target);
			float suspiciousness = target.VisualState.Suspiciousness;
			float num = Mathf.Lerp(0.2f, 2f, suspiciousness);
			float num2 = Mathf.Lerp(0.4f, 1f, playerVisibility);
			float num3 = Mathf.Lerp(1f, 0.05f, Vector3.Distance(Avatar.CenterPoint, target.Avatar.CenterPoint) / 12f);
			float num4 = num2 * num * num3;
			if (Application.isEditor && Input.GetKey(KeyCode.B))
			{
				num4 = 0.5f;
			}
			if (num4 < 0.08f)
			{
				num4 = -0.08f;
			}
			else if (num4 < 0.12f)
			{
				num4 = 0f;
			}
			currentBodySearchInvestigation.ChangeProgress(num4 * Time.deltaTime);
			if (currentBodySearchInvestigation.CurrentProgress >= 1f)
			{
				ConductBodySearch(currentBodySearchInvestigation.Target);
				StopBodySearchInvestigation();
			}
			else if (currentBodySearchInvestigation.CurrentProgress <= -0.1f)
			{
				StopBodySearchInvestigation();
			}
			else if (currentBodySearchInvestigation.CurrentProgress >= 0f)
			{
				float speed = Mathf.Lerp(0.05f, 0f, currentBodySearchInvestigation.CurrentProgress);
				base.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("consideringbodysearch", 5, speed));
				Avatar.LookController.OverrideLookTarget(target.EyePosition.position, 10, currentBodySearchInvestigation.CurrentProgress >= 0.2f);
			}
		}

		private void CheckNewInvestigation()
		{
			if (currentBodySearchInvestigation != null || !CanInvestigate() || BodySearchChance <= 0f)
			{
				return;
			}
			foreach (Player player in Player.PlayerList)
			{
				if (!CanInvestigatePlayer(player) || Vector3.Distance(Avatar.CenterPoint, player.Avatar.CenterPoint) > 8f)
				{
					continue;
				}
				float playerVisibility = awareness.VisionCone.GetPlayerVisibility(player);
				if (!(playerVisibility < 0.2f))
				{
					float suspiciousness = player.VisualState.Suspiciousness;
					float num = Mathf.Lerp(0.2f, 2f, suspiciousness);
					float num2 = Mathf.Lerp(0.4f, 1f, playerVisibility);
					float num3 = Mathf.Lerp(0.5f, 1f, Suspicion);
					float num4 = Mathf.Clamp01(BodySearchChance * num * num2 * num3 * 1f);
					if (UnityEngine.Random.Range(0f, 1f) < num4)
					{
						currentBodySearchInvestigation = new Investigation(player);
						break;
					}
				}
			}
		}

		private void StartBodySearchInvestigation(Player player)
		{
			Console.Log("Starting body search investigation");
			currentBodySearchInvestigation = new Investigation(player);
		}

		private void StopBodySearchInvestigation()
		{
			currentBodySearchInvestigation = null;
			base.Movement.SpeedController.RemoveSpeedControl("consideringbodysearch");
		}

		public void ConductBodySearch(Player player)
		{
			Console.Log("Conducting body search on " + player.PlayerName);
			BodySearchBehaviour.AssignTarget(null, player.NetworkObject);
			BodySearchBehaviour.Enable_Networked(null);
		}

		private bool CanInvestigatePlayer(Player player)
		{
			if (player == null)
			{
				return false;
			}
			if (!player.Health.IsAlive)
			{
				return false;
			}
			if (player.CrimeData.BodySearchPending)
			{
				return false;
			}
			if (player.CrimeData.CurrentPursuitLevel > PlayerCrimeData.EPursuitLevel.None)
			{
				return false;
			}
			if (player.CrimeData.TimeSinceLastBodySearch < 45f)
			{
				return false;
			}
			if (player.IsArrested)
			{
				return false;
			}
			return true;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar____003CTargetPlayerNOB_003Ek__BackingField = new SyncVar<NetworkObject>(this, 1u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, TargetPlayerNOB);
				RegisterServerRpc(34u, RpcReader___Server_BeginFootPursuit_Networked_419679943);
				RegisterObserversRpc(35u, RpcReader___Observers_BeginFootPursuitTest_3615296227);
				RegisterServerRpc(36u, RpcReader___Server_BeginVehiclePursuit_Networked_2261819652);
				RegisterObserversRpc(37u, RpcReader___Observers_BeginVehiclePursuit_2261819652);
				RegisterServerRpc(38u, RpcReader___Server_BeginBodySearch_Networked_3323014238);
				RegisterObserversRpc(39u, RpcReader___Observers_BeginBodySearch_3323014238);
				RegisterObserversRpc(40u, RpcReader___Observers_AssignToCheckpoint_4087078542);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EPolice_002EPoliceOfficer);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar____003CTargetPlayerNOB_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_BeginFootPursuit_Networked_419679943(NetworkObject target, bool includeColleagues = true)
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
				writer.WriteNetworkObject(target);
				writer.WriteBoolean(includeColleagues);
				SendServerRpc(34u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public virtual void RpcLogic___BeginFootPursuit_Networked_419679943(NetworkObject target, bool includeColleagues = true)
		{
			if (target == null)
			{
				Console.LogError("Attempted to begin foot pursuit with null target");
				return;
			}
			BeginFootPursuitTest(target.GetComponent<Player>().PlayerCode);
			if (!InstanceFinder.IsServer || !includeColleagues)
			{
				return;
			}
			if (FootPatrolBehaviour.Enabled && FootPatrolBehaviour.Group != null)
			{
				for (int i = 0; i < FootPatrolBehaviour.Group.Members.Count; i++)
				{
					if (!(FootPatrolBehaviour.Group.Members[i] == this))
					{
						(FootPatrolBehaviour.Group.Members[i] as PoliceOfficer).BeginFootPursuitTest(target.GetComponent<Player>().PlayerCode);
					}
				}
			}
			if (CheckpointBehaviour.Enabled && CheckpointBehaviour.Checkpoint != null)
			{
				for (int j = 0; j < CheckpointBehaviour.Checkpoint.AssignedNPCs.Count; j++)
				{
					if (!(CheckpointBehaviour.Checkpoint.AssignedNPCs[j] == this))
					{
						(CheckpointBehaviour.Checkpoint.AssignedNPCs[j] as PoliceOfficer).BeginFootPursuitTest(target.GetComponent<Player>().PlayerCode);
					}
				}
			}
			if (!SentryBehaviour.Enabled || !(SentryBehaviour.AssignedLocation != null))
			{
				return;
			}
			for (int k = 0; k < SentryBehaviour.AssignedLocation.AssignedOfficers.Count; k++)
			{
				if (!(SentryBehaviour.AssignedLocation.AssignedOfficers[k] == this))
				{
					SentryBehaviour.AssignedLocation.AssignedOfficers[k].BeginFootPursuitTest(target.GetComponent<Player>().PlayerCode);
				}
			}
		}

		private void RpcReader___Server_BeginFootPursuit_Networked_419679943(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject target = PooledReader0.ReadNetworkObject();
			bool includeColleagues = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___BeginFootPursuit_Networked_419679943(target, includeColleagues);
			}
		}

		private void RpcWriter___Observers_BeginFootPursuitTest_3615296227(string playerCode)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteString(playerCode);
				SendObserversRpc(35u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___BeginFootPursuitTest_3615296227(string playerCode)
		{
			TargetPlayerNOB = Player.GetPlayer(playerCode).NetworkObject;
			if (SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField == null)
			{
				Console.LogError("Attempted to begin foot pursuit with null target");
				return;
			}
			PursuitBehaviour.AssignTarget(null, SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField);
			PursuitBehaviour.Enable();
		}

		private void RpcReader___Observers_BeginFootPursuitTest_3615296227(PooledReader PooledReader0, Channel channel)
		{
			string playerCode = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___BeginFootPursuitTest_3615296227(playerCode);
			}
		}

		private void RpcWriter___Server_BeginVehiclePursuit_Networked_2261819652(NetworkObject target, NetworkObject vehicle, bool beginAsSighted)
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
				writer.WriteNetworkObject(target);
				writer.WriteNetworkObject(vehicle);
				writer.WriteBoolean(beginAsSighted);
				SendServerRpc(36u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public virtual void RpcLogic___BeginVehiclePursuit_Networked_2261819652(NetworkObject target, NetworkObject vehicle, bool beginAsSighted)
		{
			BeginVehiclePursuit(target, vehicle, beginAsSighted);
		}

		private void RpcReader___Server_BeginVehiclePursuit_Networked_2261819652(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject target = PooledReader0.ReadNetworkObject();
			NetworkObject vehicle = PooledReader0.ReadNetworkObject();
			bool beginAsSighted = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___BeginVehiclePursuit_Networked_2261819652(target, vehicle, beginAsSighted);
			}
		}

		private void RpcWriter___Observers_BeginVehiclePursuit_2261819652(NetworkObject target, NetworkObject vehicle, bool beginAsSighted)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteNetworkObject(target);
				writer.WriteNetworkObject(vehicle);
				writer.WriteBoolean(beginAsSighted);
				SendObserversRpc(37u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___BeginVehiclePursuit_2261819652(NetworkObject target, NetworkObject vehicle, bool beginAsSighted)
		{
			TargetPlayerNOB = target.GetComponent<Player>().NetworkObject;
			VehiclePursuitBehaviour.vehicle = vehicle.GetComponent<LandVehicle>();
			VehiclePursuitBehaviour.AssignTarget(SyncAccessor__003CTargetPlayerNOB_003Ek__BackingField.GetComponent<Player>());
			if (beginAsSighted)
			{
				VehiclePursuitBehaviour.BeginAsSighted();
			}
			VehiclePursuitBehaviour.Enable();
		}

		private void RpcReader___Observers_BeginVehiclePursuit_2261819652(PooledReader PooledReader0, Channel channel)
		{
			NetworkObject target = PooledReader0.ReadNetworkObject();
			NetworkObject vehicle = PooledReader0.ReadNetworkObject();
			bool beginAsSighted = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___BeginVehiclePursuit_2261819652(target, vehicle, beginAsSighted);
			}
		}

		private void RpcWriter___Server_BeginBodySearch_Networked_3323014238(NetworkObject target)
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
				writer.WriteNetworkObject(target);
				SendServerRpc(38u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public virtual void RpcLogic___BeginBodySearch_Networked_3323014238(NetworkObject target)
		{
			BeginBodySearch(target);
		}

		private void RpcReader___Server_BeginBodySearch_Networked_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject target = PooledReader0.ReadNetworkObject();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___BeginBodySearch_Networked_3323014238(target);
			}
		}

		private void RpcWriter___Observers_BeginBodySearch_3323014238(NetworkObject target)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteNetworkObject(target);
				SendObserversRpc(39u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___BeginBodySearch_3323014238(NetworkObject target)
		{
			TargetPlayerNOB = target.GetComponent<Player>().NetworkObject;
			BodySearchBehaviour.AssignTarget(null, target);
			BodySearchBehaviour.Enable();
		}

		private void RpcReader___Observers_BeginBodySearch_3323014238(PooledReader PooledReader0, Channel channel)
		{
			NetworkObject target = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___BeginBodySearch_3323014238(target);
			}
		}

		private void RpcWriter___Observers_AssignToCheckpoint_4087078542(CheckpointManager.ECheckpointLocation location)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				GeneratedWriters___Internal.Write___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerated(writer, location);
				SendObserversRpc(40u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public virtual void RpcLogic___AssignToCheckpoint_4087078542(CheckpointManager.ECheckpointLocation location)
		{
			movement.Warp(NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(location).transform.position);
			CheckpointBehaviour.SetCheckpoint(location);
			CheckpointBehaviour.Enable();
			dialogueHandler.GetComponent<DialogueController>().OverrideContainer = CheckpointDialogue;
		}

		private void RpcReader___Observers_AssignToCheckpoint_4087078542(PooledReader PooledReader0, Channel channel)
		{
			CheckpointManager.ECheckpointLocation location = GeneratedReaders___Internal.Read___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___AssignToCheckpoint_4087078542(location);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EPolice_002EPoliceOfficer(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 1)
			{
				if (PooledReader0 == null)
				{
					syncVar____003CTargetPlayerNOB_003Ek__BackingField.SetValue(syncVar____003CTargetPlayerNOB_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				NetworkObject value = PooledReader0.ReadNetworkObject();
				syncVar____003CTargetPlayerNOB_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			return false;
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EPolice_002EPoliceOfficer_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			if (!Officers.Contains(this))
			{
				Officers.Add(this);
			}
			PursuitBehaviour.onEnd.AddListener(delegate
			{
				TargetPlayerNOB = null;
			});
			InvokeRepeating("CheckNewInvestigation", 1f, 1f);
			chatterCountDown = UnityEngine.Random.Range(15f, 45f);
			VisionCone visionCone = awareness.VisionCone;
			visionCone.onVisionEventFull = (VisionCone.EventStateChange)Delegate.Combine(visionCone.onVisionEventFull, new VisionCone.EventStateChange(ProcessVisionEvent));
		}
	}
}
