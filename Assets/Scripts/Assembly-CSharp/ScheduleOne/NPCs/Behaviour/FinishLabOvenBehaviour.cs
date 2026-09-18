using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class FinishLabOvenBehaviour : Behaviour
	{
		public const float HARVEST_TIME = 10f;

		private Chemist chemist;

		private Coroutine actionRoutine;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public LabOven targetOven { get; private set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviour_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public void SetTargetOven(LabOven oven)
		{
			targetOven = oven;
		}

		protected override void End()
		{
			base.End();
			targetOven.Door.SetPosition(0f);
			targetOven.ClearShards();
			targetOven.RemoveTrayAnimation.Stop();
			targetOven.ResetSquareTray();
			Disable();
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (actionRoutine != null)
			{
				base.Npc.Avatar.LookController.OverrideLookTarget(targetOven.UIPoint.position, 5);
			}
			else if (InstanceFinder.IsServer && !base.Npc.Movement.IsMoving)
			{
				if (IsAtStation())
				{
					StartAction();
				}
				else
				{
					base.Npc.Movement.SetDestination(GetStationAccessPoint());
				}
			}
		}

		[ObserversRpc(RunLocally = true)]
		private void StartAction()
		{
			RpcWriter___Observers_StartAction_2166136261();
			RpcLogic___StartAction_2166136261();
		}

		private bool CanActionStart()
		{
			if (targetOven == null)
			{
				return false;
			}
			if (((IUsable)targetOven).IsInUse && ((IUsable)targetOven).NPCUserObject != base.Npc.NetworkObject)
			{
				return false;
			}
			if (targetOven.CurrentOperation == null)
			{
				return false;
			}
			if (!targetOven.CurrentOperation.IsReady())
			{
				return false;
			}
			if (!targetOven.CanOutputSpaceFitCurrentOperation())
			{
				return false;
			}
			return true;
		}

		private void StopAction()
		{
			targetOven.SetNPCUser(null);
			base.Npc.SetEquippable_Networked(null, string.Empty);
			base.Npc.SetAnimationBool_Networked(null, "UseHammer", value: false);
			if (actionRoutine != null)
			{
				StopCoroutine(actionRoutine);
				actionRoutine = null;
			}
		}

		private Vector3 GetStationAccessPoint()
		{
			if (targetOven == null)
			{
				return base.Npc.transform.position;
			}
			return ((ITransitEntity)targetOven).AccessPoints[0].position;
		}

		private bool IsAtStation()
		{
			if (targetOven == null)
			{
				return false;
			}
			return Vector3.Distance(base.Npc.transform.position, GetStationAccessPoint()) < 1f;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_StartAction_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_StartAction_2166136261()
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

		private void RpcLogic___StartAction_2166136261()
		{
			if (actionRoutine == null)
			{
				actionRoutine = StartCoroutine(ActionRoutine());
			}
			IEnumerator ActionRoutine()
			{
				Console.Log("Starting harvest...");
				targetOven.SetNPCUser(base.Npc.NetworkObject);
				base.Npc.Movement.FacePoint(targetOven.transform.position);
				yield return new WaitForSeconds(0.5f);
				if (!CanActionStart())
				{
					StopAction();
					End_Networked(null);
				}
				else
				{
					base.Npc.SetEquippable_Networked(null, "Avatar/Equippables/Hammer");
					targetOven.Door.SetPosition(1f);
					targetOven.WireTray.SetPosition(1f);
					yield return new WaitForSeconds(0.5f);
					targetOven.SquareTray.SetParent(targetOven.transform);
					targetOven.RemoveTrayAnimation.Play();
					yield return new WaitForSeconds(0.1f);
					targetOven.Door.SetPosition(0f);
					yield return new WaitForSeconds(1f);
					base.Npc.SetAnimationBool_Networked(null, "UseHammer", value: true);
					yield return new WaitForSeconds(10f);
					base.Npc.SetAnimationBool_Networked(null, "UseHammer", value: false);
					targetOven.Shatter(targetOven.CurrentOperation.Cookable.ProductQuantity, targetOven.CurrentOperation.Cookable.ProductShardPrefab.gameObject);
					yield return new WaitForSeconds(1f);
					ItemInstance productItem = targetOven.CurrentOperation.GetProductItem(targetOven.CurrentOperation.Cookable.ProductQuantity * targetOven.CurrentOperation.IngredientQuantity);
					targetOven.OutputSlot.AddItem(productItem);
					targetOven.SendCookOperation(null);
					StopAction();
					End_Networked(null);
				}
			}
		}

		private void RpcReader___Observers_StartAction_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___StartAction_2166136261();
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviour_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			chemist = base.Npc as Chemist;
		}
	}
}
