using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.ItemFramework;
using ScheduleOne.Law;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Product;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class CheckpointBehaviour : Behaviour
	{
		public const float LOOK_TIME = 1.5f;

		private float currentLookTime;

		private bool trunkOpened;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public CheckpointManager.ECheckpointLocation AssignedCheckpoint { get; protected set; }

		public RoadCheckpoint Checkpoint { get; protected set; }

		public bool IsSearching { get; protected set; }

		public LandVehicle CurrentSearchedVehicle { get; protected set; }

		public Player Initiator { get; protected set; }

		private Transform standPoint => Checkpoint.StandPoints[Mathf.Clamp(Checkpoint.AssignedNPCs.IndexOf(base.Npc), 0, Checkpoint.StandPoints.Length - 1)];

		private DialogueDatabase dialogueDatabase => base.Npc.dialogueHandler.Database;

		protected override void Begin()
		{
			base.Begin();
			Checkpoint = NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(AssignedCheckpoint);
			if (!Checkpoint.AssignedNPCs.Contains(base.Npc))
			{
				Checkpoint.AssignedNPCs.Add(base.Npc);
			}
			Checkpoint.onPlayerWalkThrough.AddListener(PlayerWalkedThroughCheckPoint);
		}

		protected override void Resume()
		{
			base.Resume();
			Checkpoint = NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(AssignedCheckpoint);
			if (!Checkpoint.AssignedNPCs.Contains(base.Npc))
			{
				Checkpoint.AssignedNPCs.Add(base.Npc);
			}
			Checkpoint.onPlayerWalkThrough.AddListener(PlayerWalkedThroughCheckPoint);
		}

		protected override void End()
		{
			base.End();
			IsSearching = false;
			if (Checkpoint.AssignedNPCs.Contains(base.Npc))
			{
				Checkpoint.AssignedNPCs.Remove(base.Npc);
			}
			if (CurrentSearchedVehicle != null && trunkOpened)
			{
				CurrentSearchedVehicle.Trunk.SetIsOpen(open: false);
			}
			Checkpoint.onPlayerWalkThrough.RemoveListener(PlayerWalkedThroughCheckPoint);
		}

		protected override void Pause()
		{
			base.Pause();
			IsSearching = false;
			if (Checkpoint.AssignedNPCs.Contains(base.Npc))
			{
				Checkpoint.AssignedNPCs.Remove(base.Npc);
			}
			if (CurrentSearchedVehicle != null && trunkOpened)
			{
				CurrentSearchedVehicle.Trunk.SetIsOpen(open: false);
			}
			Checkpoint.onPlayerWalkThrough.RemoveListener(PlayerWalkedThroughCheckPoint);
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (IsSearching && !base.Npc.Movement.IsMoving && base.Npc.Movement.IsAsCloseAsPossible(GetSearchPoint()))
			{
				if (!CurrentSearchedVehicle.Trunk.IsOpen)
				{
					CurrentSearchedVehicle.Trunk?.SetIsOpen(open: true);
					trunkOpened = true;
				}
			}
			else if (trunkOpened && CurrentSearchedVehicle != null)
			{
				CurrentSearchedVehicle.Trunk?.SetIsOpen(open: false);
			}
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (!IsSearching)
			{
				if (base.Npc.Movement.IsMoving)
				{
					return;
				}
				if (base.Npc.Movement.IsAsCloseAsPossible(standPoint.position))
				{
					if (!base.Npc.Movement.FaceDirectionInProgress)
					{
						base.Npc.Movement.FaceDirection(standPoint.forward);
					}
				}
				else if (base.Npc.Movement.CanMove())
				{
					base.Npc.Movement.SetDestination(standPoint.position);
				}
			}
			else if (!Checkpoint.SearchArea1.vehicles.Contains(CurrentSearchedVehicle) && !Checkpoint.SearchArea2.vehicles.Contains(CurrentSearchedVehicle))
			{
				StopSearch();
			}
			else
			{
				if (base.Npc.Movement.IsMoving)
				{
					return;
				}
				if (base.Npc.Movement.IsAsCloseAsPossible(GetSearchPoint(), 1f))
				{
					if (!base.Npc.Movement.FaceDirectionInProgress)
					{
						base.Npc.Movement.FacePoint(CurrentSearchedVehicle.transform.position);
					}
					currentLookTime += Time.deltaTime;
					if (currentLookTime >= 1.5f)
					{
						ConcludeSearch();
					}
				}
				else
				{
					currentLookTime = 0f;
					if (base.Npc.Movement.CanMove())
					{
						base.Npc.Movement.SetDestination(GetSearchPoint());
					}
				}
			}
		}

		[ObserversRpc(RunLocally = true)]
		public void SetCheckpoint(CheckpointManager.ECheckpointLocation loc)
		{
			RpcWriter___Observers_SetCheckpoint_4087078542(loc);
			RpcLogic___SetCheckpoint_4087078542(loc);
		}

		[ObserversRpc(RunLocally = true)]
		public void SetInitiator(NetworkObject init)
		{
			RpcWriter___Observers_SetInitiator_3323014238(init);
			RpcLogic___SetInitiator_3323014238(init);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void StartSearch(NetworkObject targetVehicle, NetworkObject initiator)
		{
			RpcWriter___Server_StartSearch_3694055493(targetVehicle, initiator);
			RpcLogic___StartSearch_3694055493(targetVehicle, initiator);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void StopSearch()
		{
			RpcWriter___Server_StopSearch_2166136261();
			RpcLogic___StopSearch_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		public void SetIsSearching(bool s)
		{
			RpcWriter___Observers_SetIsSearching_1140765316(s);
			RpcLogic___SetIsSearching_1140765316(s);
		}

		private Vector3 GetSearchPoint()
		{
			return CurrentSearchedVehicle.transform.position - CurrentSearchedVehicle.transform.forward * (CurrentSearchedVehicle.boundingBoxDimensions.z / 2f + 0.75f);
		}

		[ObserversRpc(RunLocally = true)]
		private void ConcludeSearch()
		{
			RpcWriter___Observers_ConcludeSearch_2166136261();
			RpcLogic___ConcludeSearch_2166136261();
		}

		private bool DoesVehicleContainIllicitItems()
		{
			Player.Local.LastDrivenVehicle.Storage.ItemSlots.Select((ItemSlot x) => x.ItemInstance).ToList();
			foreach (ItemSlot itemSlot in Player.Local.LastDrivenVehicle.Storage.ItemSlots)
			{
				if (itemSlot.ItemInstance == null)
				{
					continue;
				}
				if (itemSlot.ItemInstance is ProductItemInstance)
				{
					ProductItemInstance productItemInstance = itemSlot.ItemInstance as ProductItemInstance;
					if (productItemInstance.AppliedPackaging == null || productItemInstance.AppliedPackaging.StealthLevel <= Checkpoint.MaxStealthLevel)
					{
						return true;
					}
				}
				else if (itemSlot.ItemInstance.Definition.legalStatus != 0)
				{
					return true;
				}
			}
			return false;
		}

		private void PlayerWalkedThroughCheckPoint(Player player)
		{
			if (!InstanceFinder.IsServer || player.CrimeData.TimeSinceLastBodySearch < 45f || player.CrimeData.CurrentPursuitLevel != 0 || NetworkSingleton<CurfewManager>.Instance.IsCurrentlyActive || Checkpoint.AssignedNPCs.Count == 0)
			{
				return;
			}
			NPC nPC = null;
			float num = float.MaxValue;
			for (int i = 0; i < Checkpoint.AssignedNPCs.Count; i++)
			{
				float num2 = Vector3.Distance(player.transform.position, Checkpoint.AssignedNPCs[i].Avatar.CenterPoint);
				if (num2 < num)
				{
					num = num2;
					nPC = Checkpoint.AssignedNPCs[i];
				}
			}
			if (!(num > 12f) && !(nPC != base.Npc))
			{
				player.CrimeData.ResetBodysearchCooldown();
				(base.Npc as PoliceOfficer).ConductBodySearch(player);
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_SetCheckpoint_4087078542);
				RegisterObserversRpc(16u, RpcReader___Observers_SetInitiator_3323014238);
				RegisterServerRpc(17u, RpcReader___Server_StartSearch_3694055493);
				RegisterServerRpc(18u, RpcReader___Server_StopSearch_2166136261);
				RegisterObserversRpc(19u, RpcReader___Observers_SetIsSearching_1140765316);
				RegisterObserversRpc(20u, RpcReader___Observers_ConcludeSearch_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_SetCheckpoint_4087078542(CheckpointManager.ECheckpointLocation loc)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerated(writer, loc);
				SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetCheckpoint_4087078542(CheckpointManager.ECheckpointLocation loc)
		{
			AssignedCheckpoint = loc;
		}

		private void RpcReader___Observers_SetCheckpoint_4087078542(PooledReader PooledReader0, Channel channel)
		{
			CheckpointManager.ECheckpointLocation loc = GeneratedReaders___Internal.Read___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetCheckpoint_4087078542(loc);
			}
		}

		private void RpcWriter___Observers_SetInitiator_3323014238(NetworkObject init)
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
				writer.WriteNetworkObject(init);
				SendObserversRpc(16u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetInitiator_3323014238(NetworkObject init)
		{
			Initiator = init.GetComponent<Player>();
		}

		private void RpcReader___Observers_SetInitiator_3323014238(PooledReader PooledReader0, Channel channel)
		{
			NetworkObject init = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetInitiator_3323014238(init);
			}
		}

		private void RpcWriter___Server_StartSearch_3694055493(NetworkObject targetVehicle, NetworkObject initiator)
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
				writer.WriteNetworkObject(targetVehicle);
				writer.WriteNetworkObject(initiator);
				SendServerRpc(17u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___StartSearch_3694055493(NetworkObject targetVehicle, NetworkObject initiator)
		{
			currentLookTime = 0f;
			SetIsSearching(s: true);
			SetInitiator(initiator);
			CurrentSearchedVehicle = targetVehicle.GetComponent<LandVehicle>();
			if (InstanceFinder.IsServer)
			{
				base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("searchingvehicle", 20, 0.15f));
			}
		}

		private void RpcReader___Server_StartSearch_3694055493(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject targetVehicle = PooledReader0.ReadNetworkObject();
			NetworkObject initiator = PooledReader0.ReadNetworkObject();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___StartSearch_3694055493(targetVehicle, initiator);
			}
		}

		private void RpcWriter___Server_StopSearch_2166136261()
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
				SendServerRpc(18u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___StopSearch_2166136261()
		{
			SetIsSearching(s: false);
			if (trunkOpened)
			{
				CurrentSearchedVehicle.Trunk?.SetIsOpen(open: false);
			}
			CurrentSearchedVehicle = null;
			Initiator = null;
			if (InstanceFinder.IsServer)
			{
				base.Npc.Movement.SpeedController.RemoveSpeedControl("searchingvehicle");
			}
		}

		private void RpcReader___Server_StopSearch_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___StopSearch_2166136261();
			}
		}

		private void RpcWriter___Observers_SetIsSearching_1140765316(bool s)
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
				writer.WriteBoolean(s);
				SendObserversRpc(19u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetIsSearching_1140765316(bool s)
		{
			IsSearching = s;
			if (IsSearching)
			{
				base.Npc.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Police, "checkpoint_search_start"), 3f);
			}
		}

		private void RpcReader___Observers_SetIsSearching_1140765316(PooledReader PooledReader0, Channel channel)
		{
			bool s = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetIsSearching_1140765316(s);
			}
		}

		private void RpcWriter___Observers_ConcludeSearch_2166136261()
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
				SendObserversRpc(20u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ConcludeSearch_2166136261()
		{
			if (CurrentSearchedVehicle != null && DoesVehicleContainIllicitItems() && Initiator != null)
			{
				base.Npc.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Police, "checkpoint_items_found"), 3f);
				if (Initiator == Player.Local)
				{
					Player.Local.CrimeData.AddCrime(new TransportingIllicitItems());
					Player.Local.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
					(base.Npc as PoliceOfficer).BeginFootPursuit_Networked(Player.Local.NetworkObject);
				}
			}
			else
			{
				base.Npc.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Police, "checkpoint_all_clear"), 3f);
				if (Checkpoint.SearchArea1.vehicles.Contains(CurrentSearchedVehicle))
				{
					Checkpoint.SetGate1Open(o: true);
				}
				else if (Checkpoint.SearchArea1.vehicles.Contains(CurrentSearchedVehicle))
				{
					Checkpoint.SetGate2Open(o: true);
				}
				else
				{
					Checkpoint.SetGate1Open(o: true);
					Checkpoint.SetGate2Open(o: true);
				}
			}
			StopSearch();
		}

		private void RpcReader___Observers_ConcludeSearch_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ConcludeSearch_2166136261();
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
