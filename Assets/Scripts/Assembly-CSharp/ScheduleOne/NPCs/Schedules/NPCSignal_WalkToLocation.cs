using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules
{
	public class NPCSignal_WalkToLocation : NPCSignal
	{
		public Transform Destination;

		public bool FaceDestinationDir = true;

		public float DestinationThreshold = 1f;

		public bool WarpIfSkipped;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted;

		public new string ActionName => "Walk to location";

		public override string GetName()
		{
			return ActionName + " (" + Destination.name + ")";
		}

		public override void Started()
		{
			base.Started();
			SetDestination(Destination.position);
		}

		public override void ActiveUpdate()
		{
			base.ActiveUpdate();
			if (!npc.Movement.IsMoving && !IsAtDestination())
			{
				SetDestination(Destination.position);
			}
		}

		public override void LateStarted()
		{
			base.LateStarted();
		}

		public override void Interrupt()
		{
			base.Interrupt();
			if (npc.Movement.IsMoving)
			{
				npc.Movement.Stop();
			}
		}

		public override void Resume()
		{
			base.Resume();
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
			if (base.IsActive)
			{
				if (result != NPCMovement.WalkResult.Success)
				{
					Debug.LogWarning("NPC walk to location not successful");
					return;
				}
				ReachedDestination();
				End();
			}
		}

		[ObserversRpc]
		private void ReachedDestination()
		{
			RpcWriter___Observers_ReachedDestination_2166136261();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(0u, RpcReader___Observers_ReachedDestination_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_ReachedDestination_2166136261()
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

		private void RpcLogic___ReachedDestination_2166136261()
		{
			if (FaceDestinationDir)
			{
				npc.Movement.FaceDirection(Destination.forward);
			}
		}

		private void RpcReader___Observers_ReachedDestination_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___ReachedDestination_2166136261();
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
