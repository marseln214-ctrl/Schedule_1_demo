using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.ObjectScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Behaviour
{
	public class BagTrashCanBehaviour : Behaviour
	{
		public const float ACTION_MAX_DISTANCE = 2f;

		public const float BAG_TIME = 3f;

		private Coroutine actionCoroutine;

		public UnityEvent onPerfomAction;

		public UnityEvent onPerfomDone;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBagTrashCanBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBagTrashCanBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public TrashContainerItem TargetTrashCan { get; private set; }

		private Cleaner Cleaner => (Cleaner)base.Npc;

		public void SetTargetTrashCan(TrashContainerItem trashCan)
		{
			TargetTrashCan = trashCan;
		}

		protected override void Begin()
		{
			base.Begin();
			StartAction();
		}

		protected override void Resume()
		{
			base.Resume();
			StartAction();
		}

		private void StartAction()
		{
			if (base.Npc.Avatar.CurrentEquippable != null)
			{
				base.Npc.SetEquippable_Return(string.Empty);
			}
		}

		protected override void Pause()
		{
			base.Pause();
			StopAllActions();
		}

		public override void Disable()
		{
			base.Disable();
			if (base.Active)
			{
				End();
			}
		}

		protected override void End()
		{
			base.End();
			StopAllActions();
		}

		private void StopAllActions()
		{
			if (base.Npc.Movement.IsMoving)
			{
				base.Npc.Movement.Stop();
			}
			base.Npc.SetAnimationBool("PatSoil", val: false);
			base.Npc.SetCrouched_Networked(crouched: false);
			if (actionCoroutine != null)
			{
				Singleton<CoroutineService>.Instance.StopCoroutine(actionCoroutine);
				actionCoroutine = null;
			}
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (InstanceFinder.IsServer && !base.Npc.Movement.IsMoving && actionCoroutine == null)
			{
				if (!AreActionConditionsMet(checkAccess: false))
				{
					Disable_Networked(null);
				}
				else if (IsAtDestination())
				{
					PerformAction();
				}
				else
				{
					GoToTarget();
				}
			}
		}

		private void GoToTarget()
		{
			if (!AreActionConditionsMet(checkAccess: true))
			{
				Disable_Networked(null);
			}
			else
			{
				base.Npc.Movement.SetDestination(NavMeshUtility.GetAccessPoint(TargetTrashCan, base.Npc).position);
			}
		}

		[ObserversRpc(RunLocally = true)]
		private void PerformAction()
		{
			RpcWriter___Observers_PerformAction_2166136261();
			RpcLogic___PerformAction_2166136261();
		}

		private bool IsAtDestination()
		{
			return Vector3.Distance(base.Npc.transform.position, TargetTrashCan.transform.position) <= 2f;
		}

		private bool AreActionConditionsMet(bool checkAccess)
		{
			if (TargetTrashCan == null)
			{
				return false;
			}
			if (TargetTrashCan.Container.NormalizedTrashLevel == 0f)
			{
				return false;
			}
			if (checkAccess && !base.Npc.Movement.CanGetTo(TargetTrashCan.transform.position, 2f))
			{
				return false;
			}
			return true;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBagTrashCanBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBagTrashCanBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_PerformAction_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBagTrashCanBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBagTrashCanBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_PerformAction_2166136261()
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
				SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___PerformAction_2166136261()
		{
			if (actionCoroutine == null)
			{
				actionCoroutine = Singleton<CoroutineService>.Instance.StartCoroutine(Action());
			}
			IEnumerator Action()
			{
				if (InstanceFinder.IsServer && !AreActionConditionsMet(checkAccess: false))
				{
					Disable_Networked(null);
				}
				else
				{
					if (InstanceFinder.IsServer)
					{
						base.Npc.Movement.FacePoint(TargetTrashCan.transform.position);
					}
					yield return new WaitForSeconds(0.4f);
					base.Npc.SetAnimationBool("PatSoil", val: true);
					base.Npc.SetCrouched_Networked(crouched: true);
					if (onPerfomAction != null)
					{
						onPerfomAction.Invoke();
					}
					yield return new WaitForSeconds(3f);
					if (InstanceFinder.IsServer && AreActionConditionsMet(checkAccess: false))
					{
						TargetTrashCan.Container.BagTrash();
						if (onPerfomDone != null)
						{
							onPerfomDone.Invoke();
						}
					}
					base.Npc.SetAnimationBool("PatSoil", val: false);
					yield return new WaitForSeconds(0.2f);
					actionCoroutine = null;
					Disable_Networked(null);
				}
			}
		}

		private void RpcReader___Observers_PerformAction_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___PerformAction_2166136261();
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
