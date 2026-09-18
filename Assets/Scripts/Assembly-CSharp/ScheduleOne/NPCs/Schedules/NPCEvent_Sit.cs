using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.AvatarFramework.Animation;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Schedules
{
	public class NPCEvent_Sit : NPCEvent
	{
		public const float DESTINATION_THRESHOLD = 1.5f;

		public AvatarSeatSet SeatSet;

		public bool WarpIfSkipped;

		private bool seated;

		private AvatarSeat targetSeat;

		public UnityEvent onSeated;

		public UnityEvent onStandUp;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted;

		public new string ActionName => "Sit";

		public override string GetName()
		{
			string text = ActionName;
			if (SeatSet == null)
			{
				text += "(no seat assigned)";
			}
			return text;
		}

		public override void Started()
		{
			base.Started();
			seated = false;
			if (IsAtDestination())
			{
				WalkCallback(NPCMovement.WalkResult.Success);
				return;
			}
			targetSeat = SeatSet.GetRandomFreeSeat();
			SetDestination(targetSeat.AccessPoint.position);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (base.IsActive && seated)
			{
				StartAction(connection, SeatSet.Seats.IndexOf(npc.Avatar.Anim.CurrentSeat));
			}
		}

		public override void LateStarted()
		{
			base.LateStarted();
			seated = false;
			if (IsAtDestination())
			{
				WalkCallback(NPCMovement.WalkResult.Success);
				return;
			}
			targetSeat = SeatSet.GetRandomFreeSeat();
			SetDestination(targetSeat.AccessPoint.position);
		}

		public override void ActiveMinPassed()
		{
			base.ActiveMinPassed();
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (schedule.DEBUG_MODE)
			{
				Debug.Log("ActiveMinPassed");
				Debug.Log("Moving: " + npc.Movement.IsMoving);
				Debug.Log("At destination: " + IsAtDestination());
				Debug.Log("Seated: " + seated);
			}
			if (!base.IsActive || npc.Movement.IsMoving)
			{
				return;
			}
			if (IsAtDestination() || seated)
			{
				if (!seated)
				{
					if (!npc.Movement.FaceDirectionInProgress)
					{
						npc.Movement.FaceDirection(targetSeat.SittingPoint.forward);
					}
					if (Vector3.Angle(npc.Movement.transform.forward, targetSeat.SittingPoint.forward) < 10f)
					{
						StartAction(null, SeatSet.Seats.IndexOf(SeatSet.GetRandomFreeSeat()));
					}
				}
				else if (!npc.Movement.FaceDirectionInProgress && Vector3.Angle(npc.Movement.transform.forward, targetSeat.SittingPoint.forward) > 15f)
				{
					npc.Movement.FaceDirection(targetSeat.SittingPoint.forward);
				}
			}
			else
			{
				SetDestination(targetSeat.AccessPoint.position);
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
				targetSeat = SeatSet.GetRandomFreeSeat();
				if (InstanceFinder.IsServer)
				{
					npc.Movement.Warp(targetSeat.AccessPoint.position);
					StartAction(null, SeatSet.Seats.IndexOf(SeatSet.GetRandomFreeSeat()));
				}
				npc.Movement.FaceDirection(targetSeat.AccessPoint.forward, 0f);
			}
		}

		public override void End()
		{
			base.End();
			if (InstanceFinder.IsServer && seated)
			{
				EndAction();
			}
		}

		public override void Interrupt()
		{
			base.Interrupt();
			if (InstanceFinder.IsServer)
			{
				if (npc.Movement.IsMoving)
				{
					npc.Movement.Stop();
				}
				if (seated)
				{
					EndAction();
				}
			}
		}

		public override void Resume()
		{
			base.Resume();
			if (IsAtDestination())
			{
				WalkCallback(NPCMovement.WalkResult.Success);
				return;
			}
			targetSeat = SeatSet.GetRandomFreeSeat();
			SetDestination(targetSeat.AccessPoint.position);
		}

		public override void Skipped()
		{
			base.Skipped();
			if (WarpIfSkipped)
			{
				targetSeat = SeatSet.GetRandomFreeSeat();
				npc.Movement.Warp(targetSeat.AccessPoint.position);
			}
		}

		private bool IsAtDestination()
		{
			if (targetSeat == null)
			{
				return false;
			}
			return Vector3.Distance(npc.Movement.FootPosition, targetSeat.AccessPoint.position) < 1.5f;
		}

		protected override void WalkCallback(NPCMovement.WalkResult result)
		{
			base.WalkCallback(result);
			if (base.IsActive && result == NPCMovement.WalkResult.Success && InstanceFinder.IsServer)
			{
				StartAction(null, SeatSet.Seats.IndexOf(SeatSet.GetRandomFreeSeat()));
			}
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		protected virtual void StartAction(NetworkConnection conn, int seatIndex)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_StartAction_2681120339(conn, seatIndex);
				RpcLogic___StartAction_2681120339(conn, seatIndex);
			}
			else
			{
				RpcWriter___Target_StartAction_2681120339(conn, seatIndex);
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
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(0u, RpcReader___Observers_StartAction_2681120339);
				RegisterTargetRpc(1u, RpcReader___Target_StartAction_2681120339);
				RegisterObserversRpc(2u, RpcReader___Observers_EndAction_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_StartAction_2681120339(NetworkConnection conn, int seatIndex)
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
				writer.WriteInt32(seatIndex);
				SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___StartAction_2681120339(NetworkConnection conn, int seatIndex)
		{
			if (!seated)
			{
				seated = true;
				if (seatIndex >= 0)
				{
					targetSeat = SeatSet.Seats[seatIndex];
				}
				npc.Movement.SetSeat(targetSeat);
				if (onSeated != null)
				{
					onSeated.Invoke();
				}
			}
		}

		private void RpcReader___Observers_StartAction_2681120339(PooledReader PooledReader0, Channel channel)
		{
			int seatIndex = PooledReader0.ReadInt32();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___StartAction_2681120339(null, seatIndex);
			}
		}

		private void RpcWriter___Target_StartAction_2681120339(NetworkConnection conn, int seatIndex)
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
				writer.WriteInt32(seatIndex);
				SendTargetRpc(1u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_StartAction_2681120339(PooledReader PooledReader0, Channel channel)
		{
			int seatIndex = PooledReader0.ReadInt32();
			if (base.IsClientInitialized)
			{
				RpcLogic___StartAction_2681120339(base.LocalConnection, seatIndex);
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
			if (seated)
			{
				seated = false;
				npc.Movement.SetSeat(null);
				if (onStandUp != null)
				{
					onStandUp.Invoke();
				}
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
