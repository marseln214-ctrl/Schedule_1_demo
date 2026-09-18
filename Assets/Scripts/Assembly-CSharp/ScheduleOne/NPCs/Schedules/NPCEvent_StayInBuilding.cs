using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.Map;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules
{
	public class NPCEvent_StayInBuilding : NPCEvent
	{
		public NPCEnterableBuilding Building;

		[Header("Optionally specify door to use. Otherwise closest door will be used.")]
		public StaticDoor Door;

		private bool IsEntering;

		private Coroutine enterRoutine;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted;

		public new string ActionName => "Stay in Building";

		private bool InBuilding => npc.CurrentBuilding == Building;

		public override string GetName()
		{
			if (Building == null)
			{
				return ActionName + " (No building set)";
			}
			return ActionName + " (" + Building.BuildingName + ")";
		}

		public override void Started()
		{
			base.Started();
			if (base.IsActive && InstanceFinder.IsServer)
			{
				SetDestination(GetEntryPoint().position);
			}
		}

		public override void ActiveMinPassed()
		{
			base.ActiveMinPassed();
			if (!base.IsActive || !InstanceFinder.IsServer)
			{
				return;
			}
			if (schedule.DEBUG_MODE)
			{
				Debug.Log("StayInBuilding: ActiveMinPassed");
				Debug.Log("In building: " + InBuilding);
				Debug.Log("Is entering: " + IsEntering);
			}
			if (!InBuilding && !IsEntering && (!npc.Movement.IsMoving || Vector3.Distance(npc.Movement.CurrentDestination, GetEntryPoint().position) > 2f))
			{
				if (Vector3.Distance(npc.transform.position, GetEntryPoint().position) < 0.5f)
				{
					PlayEnterAnimation();
				}
				else if (npc.Movement.CanMove())
				{
					SetDestination(GetEntryPoint().position);
				}
			}
		}

		public override void LateStarted()
		{
			base.LateStarted();
			if (InstanceFinder.IsServer)
			{
				SetDestination(GetEntryPoint().position);
			}
		}

		public override void JumpTo()
		{
			base.JumpTo();
			if (InstanceFinder.IsServer)
			{
				if (npc.Movement.IsMoving)
				{
					npc.Movement.Stop();
				}
				PlayEnterAnimation();
			}
		}

		public override void End()
		{
			base.End();
			CancelEnter();
			if (InBuilding)
			{
				ExitBuilding();
			}
			else
			{
				npc.Movement.Stop();
			}
		}

		public override void Interrupt()
		{
			base.Interrupt();
			CancelEnter();
			if (InBuilding)
			{
				ExitBuilding();
			}
			else
			{
				npc.Movement.Stop();
			}
		}

		public override void Skipped()
		{
			base.Skipped();
		}

		public override void Resume()
		{
			base.Resume();
			if (!InBuilding && InstanceFinder.IsServer)
			{
				SetDestination(GetEntryPoint().position);
			}
		}

		protected override void WalkCallback(NPCMovement.WalkResult result)
		{
			base.WalkCallback(result);
			if (base.IsActive && InstanceFinder.IsServer && (result == NPCMovement.WalkResult.Success || result == NPCMovement.WalkResult.Partial))
			{
				PlayEnterAnimation();
			}
		}

		[ObserversRpc(RunLocally = true)]
		private void PlayEnterAnimation()
		{
			RpcWriter___Observers_PlayEnterAnimation_2166136261();
			RpcLogic___PlayEnterAnimation_2166136261();
		}

		private void CancelEnter()
		{
			IsEntering = false;
			if (enterRoutine != null)
			{
				StopCoroutine(enterRoutine);
			}
		}

		private void EnterBuilding()
		{
			if (InstanceFinder.IsServer)
			{
				npc.EnterBuilding(null, Building.GUID.ToString(), Building.Doors.IndexOf(Building.GetClosestDoor(npc.Movement.FootPosition, useableOnly: true)));
			}
		}

		private void ExitBuilding()
		{
			if (InstanceFinder.IsServer)
			{
				npc.ExitBuilding();
			}
		}

		private Transform GetEntryPoint()
		{
			if (Door != null)
			{
				return Door.AccessPoint;
			}
			if (Building == null)
			{
				return null;
			}
			StaticDoor closestDoor = Building.GetClosestDoor(npc.Movement.FootPosition, useableOnly: true);
			if (closestDoor == null)
			{
				return null;
			}
			return closestDoor.AccessPoint;
		}

		private StaticDoor GetDoor()
		{
			if (Door != null)
			{
				return Door;
			}
			if (Building == null)
			{
				return null;
			}
			if (npc == null)
			{
				return null;
			}
			return Building.GetClosestDoor(npc.Movement.FootPosition, useableOnly: true);
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(0u, RpcReader___Observers_PlayEnterAnimation_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_PlayEnterAnimation_2166136261()
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

		private void RpcLogic___PlayEnterAnimation_2166136261()
		{
			if (!IsEntering)
			{
				enterRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Enter());
			}
			IEnumerator Enter()
			{
				IsEntering = true;
				Transform faceDir = GetDoor().transform;
				yield return new WaitUntil(() => !npc.Movement.IsMoving);
				npc.Movement.FacePoint(faceDir.position);
				float t = 0f;
				while (Vector3.SignedAngle(npc.Avatar.transform.forward, faceDir.position - npc.Avatar.CenterPoint, Vector3.up) > 15f && t < 1f)
				{
					yield return new WaitForEndOfFrame();
					t += Time.deltaTime;
				}
				npc.Avatar.Anim.SetTrigger("GrabItem");
				yield return new WaitForSeconds(0.6f);
				IsEntering = false;
				EnterBuilding();
			}
		}

		private void RpcReader___Observers_PlayEnterAnimation_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___PlayEnterAnimation_2166136261();
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
