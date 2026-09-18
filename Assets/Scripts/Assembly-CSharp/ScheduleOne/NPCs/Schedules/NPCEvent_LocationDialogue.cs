using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Dialogue;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules
{
	public class NPCEvent_LocationDialogue : NPCEvent
	{
		public Transform Destination;

		public bool FaceDestinationDir = true;

		public float DestinationThreshold = 1f;

		public bool WarpIfSkipped;

		[Header("Dialogue Settings")]
		public int GreetingOverrideToEnable = -1;

		public int ChoiceToEnable = -1;

		public DialogueContainer DialogueOverride;

		protected bool IsActionStarted;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted;

		public new string ActionName => "Location-based dialogue";

		public override string GetName()
		{
			if (Destination == null)
			{
				return ActionName + " (No destination set)";
			}
			return ActionName + " (" + Destination?.name + ")";
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (base.IsActive && IsActionStarted)
			{
				StartAction(connection);
			}
		}

		public override void Started()
		{
			base.Started();
			if (IsAtDestination())
			{
				WalkCallback(NPCMovement.WalkResult.Success);
			}
			else
			{
				SetDestination(Destination.position);
			}
		}

		public override void ActiveMinPassed()
		{
			base.ActiveMinPassed();
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (npc.Movement.IsMoving)
			{
				if (Vector3.Distance(npc.Movement.CurrentDestination, Destination.position) > DestinationThreshold)
				{
					SetDestination(Destination.position);
				}
			}
			else if (IsAtDestination())
			{
				if (FaceDestinationDir && !npc.Movement.FaceDirectionInProgress && Vector3.Angle(base.transform.forward, Destination.forward) > 5f)
				{
					npc.Movement.FaceDirection(Destination.forward);
				}
			}
			else
			{
				SetDestination(Destination.position);
			}
		}

		public override void LateStarted()
		{
			base.LateStarted();
			if (IsAtDestination())
			{
				WalkCallback(NPCMovement.WalkResult.Success);
			}
			else
			{
				SetDestination(Destination.position);
			}
		}

		public override void JumpTo()
		{
			base.JumpTo();
			if (!IsAtDestination())
			{
				if (npc.Movement.IsMoving)
				{
					npc.Movement.Stop();
				}
				if (InstanceFinder.IsServer)
				{
					npc.Movement.Warp(Destination.position);
					StartAction(null);
				}
			}
		}

		public override void End()
		{
			base.End();
			if (IsActionStarted)
			{
				EndAction();
			}
		}

		public override void Interrupt()
		{
			base.Interrupt();
			if (npc.Movement.IsMoving)
			{
				npc.Movement.Stop();
			}
			if (IsActionStarted)
			{
				EndAction();
			}
		}

		public override void Resume()
		{
			base.Resume();
			if (IsAtDestination())
			{
				WalkCallback(NPCMovement.WalkResult.Success);
			}
			else
			{
				SetDestination(Destination.position);
			}
		}

		public override void Skipped()
		{
			base.Skipped();
			if (WarpIfSkipped)
			{
				npc.Movement.Warp(Destination.position);
			}
		}

		private bool IsAtDestination()
		{
			return Vector3.Distance(npc.Movement.FootPosition, Destination.position) < DestinationThreshold;
		}

		protected override void WalkCallback(NPCMovement.WalkResult result)
		{
			base.WalkCallback(result);
			if (base.IsActive && result == NPCMovement.WalkResult.Success && InstanceFinder.IsServer)
			{
				StartAction(null);
			}
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		protected virtual void StartAction(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_StartAction_328543758(conn);
				RpcLogic___StartAction_328543758(conn);
			}
			else
			{
				RpcWriter___Target_StartAction_328543758(conn);
			}
		}

		[ObserversRpc(RunLocally = true)]
		protected virtual void EndAction()
		{
			RpcWriter___Observers_EndAction_2166136261();
			RpcLogic___EndAction_2166136261();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(0u, RpcReader___Observers_StartAction_328543758);
				RegisterTargetRpc(1u, RpcReader___Target_StartAction_328543758);
				RegisterObserversRpc(2u, RpcReader___Observers_EndAction_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_StartAction_328543758(NetworkConnection conn)
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
				SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___StartAction_328543758(NetworkConnection conn)
		{
			if (IsActionStarted)
			{
				return;
			}
			if (FaceDestinationDir)
			{
				npc.Movement.FaceDirection(Destination.forward);
			}
			IsActionStarted = true;
			DialogueController component = npc.dialogueHandler.GetComponent<DialogueController>();
			if (DialogueOverride != null)
			{
				component.OverrideContainer = DialogueOverride;
				return;
			}
			component.OverrideContainer = null;
			if (component.GreetingOverrides.Count > GreetingOverrideToEnable && GreetingOverrideToEnable >= 0)
			{
				component.GreetingOverrides[GreetingOverrideToEnable].ShouldShow = true;
			}
			if (component.Choices.Count > ChoiceToEnable && ChoiceToEnable >= 0)
			{
				component.Choices[ChoiceToEnable].Enabled = true;
			}
		}

		private void RpcReader___Observers_StartAction_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___StartAction_328543758(null);
			}
		}

		private void RpcWriter___Target_StartAction_328543758(NetworkConnection conn)
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
				SendTargetRpc(1u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_StartAction_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___StartAction_328543758(base.LocalConnection);
			}
		}

		private void RpcWriter___Observers_EndAction_2166136261()
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
				SendObserversRpc(2u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___EndAction_2166136261()
		{
			if (!IsActionStarted)
			{
				return;
			}
			IsActionStarted = false;
			DialogueController component = npc.dialogueHandler.GetComponent<DialogueController>();
			if (DialogueOverride != null)
			{
				component.OverrideContainer = null;
				return;
			}
			if (component.GreetingOverrides.Count > GreetingOverrideToEnable && GreetingOverrideToEnable >= 0)
			{
				component.GreetingOverrides[GreetingOverrideToEnable].ShouldShow = false;
			}
			if (component.Choices.Count > ChoiceToEnable && ChoiceToEnable >= 0)
			{
				component.Choices[ChoiceToEnable].Enabled = false;
			}
		}

		private void RpcReader___Observers_EndAction_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___EndAction_2166136261();
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
