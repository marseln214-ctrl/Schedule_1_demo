using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules
{
	public class NPCSignal_UseATM : NPCSignal
	{
		private const float destinationThreshold = 1f;

		public ATM ATM;

		private Coroutine purchaseCoroutine;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted;

		public new string ActionName => "Use ATM";

		public override string GetName()
		{
			return ActionName;
		}

		public override void Started()
		{
			base.Started();
			if (ATM == null)
			{
				Debug.LogWarning("No ATM found for NPC to use");
				End();
			}
			else
			{
				SetDestination(ATM.AccessPoint.position);
			}
		}

		public override void ActiveUpdate()
		{
			base.ActiveUpdate();
			if (purchaseCoroutine != null || npc.Movement.IsMoving)
			{
				return;
			}
			if (IsAtDestination())
			{
				if (purchaseCoroutine == null)
				{
					Purchase();
				}
			}
			else
			{
				SetDestination(ATM.AccessPoint.position);
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
			if (purchaseCoroutine != null)
			{
				Singleton<CoroutineService>.Instance.StopCoroutine(purchaseCoroutine);
				purchaseCoroutine = null;
			}
		}

		public override void Resume()
		{
			base.Resume();
		}

		public override void Skipped()
		{
			base.Skipped();
		}

		private bool IsAtDestination()
		{
			if (ATM == null)
			{
				return false;
			}
			return Vector3.Distance(npc.Movement.FootPosition, ATM.AccessPoint.position) < 1f;
		}

		protected override void WalkCallback(NPCMovement.WalkResult result)
		{
			base.WalkCallback(result);
			if (base.IsActive && result == NPCMovement.WalkResult.Success)
			{
				Purchase();
			}
		}

		[ObserversRpc(RunLocally = true)]
		public void Purchase()
		{
			RpcWriter___Observers_Purchase_2166136261();
			RpcLogic___Purchase_2166136261();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(0u, RpcReader___Observers_Purchase_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_Purchase_2166136261()
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

		public void RpcLogic___Purchase_2166136261()
		{
			if (purchaseCoroutine != null)
			{
				Singleton<CoroutineService>.Instance.StopCoroutine(purchaseCoroutine);
			}
			npc.Movement.FaceDirection(ATM.AccessPoint.forward);
			purchaseCoroutine = Singleton<CoroutineService>.Instance.StartCoroutine(Purchase());
			IEnumerator Purchase()
			{
				if (ATM.IsBroken)
				{
					End();
					purchaseCoroutine = null;
				}
				else
				{
					yield return new WaitForSeconds(2f);
					npc.SetAnimationTrigger_Networked(null, "GrabItem");
					yield return new WaitForSeconds(1f);
					End();
					purchaseCoroutine = null;
				}
			}
		}

		private void RpcReader___Observers_Purchase_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Purchase_2166136261();
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
