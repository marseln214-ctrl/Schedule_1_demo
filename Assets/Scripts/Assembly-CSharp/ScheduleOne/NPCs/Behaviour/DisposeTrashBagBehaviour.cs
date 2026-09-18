using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class DisposeTrashBagBehaviour : Behaviour
	{
		public string TRASH_BAG_ASSET_PATH = "Avatar/Equippables/TrashBag";

		public const float GRAB_MAX_DISTANCE = 2f;

		private TrashContent heldTrash;

		private Coroutine grabRoutine;

		private Coroutine dropRoutine;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public TrashBag TargetBag { get; private set; }

		private Cleaner Cleaner => (Cleaner)base.Npc;

		public void SetTargetBag(TrashBag bag)
		{
			TargetBag = bag;
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
			if (grabRoutine != null)
			{
				Singleton<CoroutineService>.Instance.StopCoroutine(grabRoutine);
				grabRoutine = null;
			}
			if (dropRoutine != null)
			{
				Singleton<CoroutineService>.Instance.StopCoroutine(dropRoutine);
				dropRoutine = null;
			}
			if (base.Npc.Avatar.CurrentEquippable != null && base.Npc.Avatar.CurrentEquippable.AssetPath == TRASH_BAG_ASSET_PATH)
			{
				base.Npc.SetEquippable_Return(string.Empty);
			}
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (!InstanceFinder.IsServer || base.Npc.Movement.IsMoving || grabRoutine != null || dropRoutine != null)
			{
				return;
			}
			if (!AreActionConditionsMet(checkAccess: false))
			{
				Disable_Networked(null);
			}
			else if (heldTrash == null)
			{
				if (IsAtDestination())
				{
					GrabTrash();
				}
				else
				{
					GoToTarget();
				}
			}
			else if (IsAtDestination())
			{
				DropTrash();
			}
			else
			{
				GoToTarget();
			}
		}

		private void GoToTarget()
		{
			if (!AreActionConditionsMet(checkAccess: true))
			{
				Disable_Networked(null);
			}
			else if (heldTrash == null)
			{
				base.Npc.Movement.SetDestination(TargetBag.transform.position);
			}
			else
			{
				base.Npc.Movement.SetDestination(Cleaner.AssignedProperty.DisposalArea.StandPoint.position);
			}
		}

		[ObserversRpc(RunLocally = true)]
		private void GrabTrash()
		{
			RpcWriter___Observers_GrabTrash_2166136261();
			RpcLogic___GrabTrash_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		private void DropTrash()
		{
			RpcWriter___Observers_DropTrash_2166136261();
			RpcLogic___DropTrash_2166136261();
		}

		private bool IsAtDestination()
		{
			if (heldTrash == null)
			{
				return Vector3.Distance(base.Npc.transform.position, TargetBag.transform.position) <= 2f;
			}
			return Vector3.Distance(base.Npc.transform.position, Cleaner.AssignedProperty.DisposalArea.StandPoint.position) <= 2f;
		}

		private bool AreActionConditionsMet(bool checkAccess)
		{
			if (heldTrash == null)
			{
				if (TargetBag == null)
				{
					return false;
				}
				if (TargetBag.Draggable.IsBeingDragged)
				{
					return false;
				}
				if (checkAccess && !base.Npc.Movement.CanGetTo(TargetBag.transform.position, 2f))
				{
					return false;
				}
			}
			else if (checkAccess && !base.Npc.Movement.CanGetTo(Cleaner.AssignedProperty.DisposalArea.StandPoint.position, 2f))
			{
				return false;
			}
			return true;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_GrabTrash_2166136261);
				RegisterObserversRpc(16u, RpcReader___Observers_DropTrash_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_GrabTrash_2166136261()
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

		private void RpcLogic___GrabTrash_2166136261()
		{
			if (grabRoutine == null)
			{
				grabRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Action());
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
						base.Npc.Movement.FacePoint(TargetBag.transform.position);
					}
					yield return new WaitForSeconds(0.3f);
					base.Npc.SetAnimationTrigger("GrabItem");
					if (InstanceFinder.IsServer)
					{
						if (!AreActionConditionsMet(checkAccess: false))
						{
							Disable_Networked(null);
							grabRoutine = null;
							yield break;
						}
						base.Npc.SetEquippable_Networked(null, TRASH_BAG_ASSET_PATH);
						heldTrash = TargetBag.Content;
						TargetBag.DestroyTrash();
					}
					yield return new WaitForSeconds(0.2f);
					grabRoutine = null;
				}
			}
		}

		private void RpcReader___Observers_GrabTrash_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___GrabTrash_2166136261();
			}
		}

		private void RpcWriter___Observers_DropTrash_2166136261()
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
				SendObserversRpc(16u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___DropTrash_2166136261()
		{
			if (dropRoutine == null)
			{
				dropRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Action());
			}
			IEnumerator Action()
			{
				if (InstanceFinder.IsServer && !AreActionConditionsMet(checkAccess: false))
				{
					Disable_Networked(null);
				}
				else
				{
					base.Npc.Movement.FaceDirection(Cleaner.AssignedProperty.DisposalArea.StandPoint.forward);
					yield return new WaitForSeconds(0.5f);
					if (InstanceFinder.IsServer)
					{
						Transform trashDropPoint = Cleaner.AssignedProperty.DisposalArea.TrashDropPoint;
						NetworkSingleton<TrashManager>.Instance.CreateTrashBag("trashbag", trashDropPoint.position, Random.rotation, heldTrash.GetData());
						heldTrash = null;
						base.Npc.SetEquippable_Networked(null, string.Empty);
					}
					yield return new WaitForSeconds(0.2f);
					dropRoutine = null;
					Disable_Networked(null);
				}
			}
		}

		private void RpcReader___Observers_DropTrash_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___DropTrash_2166136261();
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
