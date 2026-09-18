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
using ScheduleOne.StationFramework;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class StartLabOvenBehaviour : Behaviour
	{
		public const float POUR_TIME = 5f;

		private Chemist chemist;

		private Coroutine cookRoutine;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public LabOven targetOven { get; private set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviour_Assembly_002DCSharp_002Edll();
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
			if (cookRoutine != null)
			{
				StopCook();
			}
			Disable();
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (cookRoutine != null)
			{
				base.Npc.Avatar.LookController.OverrideLookTarget(targetOven.UIPoint.position, 5);
			}
			else if (InstanceFinder.IsServer && !base.Npc.Movement.IsMoving)
			{
				if (IsAtStation())
				{
					StartCook();
				}
				else
				{
					base.Npc.Movement.SetDestination(GetStationAccessPoint());
				}
			}
		}

		[ObserversRpc(RunLocally = true)]
		private void StartCook()
		{
			RpcWriter___Observers_StartCook_2166136261();
			RpcLogic___StartCook_2166136261();
		}

		private bool CanCookStart()
		{
			if (targetOven == null)
			{
				return false;
			}
			if (((IUsable)targetOven).IsInUse && ((IUsable)targetOven).NPCUserObject != base.Npc.NetworkObject)
			{
				return false;
			}
			if (targetOven.CurrentOperation != null)
			{
				return false;
			}
			if (!targetOven.IsIngredientCookable())
			{
				return false;
			}
			return true;
		}

		private void StopCook()
		{
			targetOven.SetNPCUser(null);
			if (cookRoutine != null)
			{
				StopCoroutine(cookRoutine);
				cookRoutine = null;
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
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_StartCook_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_StartCook_2166136261()
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

		private void RpcLogic___StartCook_2166136261()
		{
			if (cookRoutine == null)
			{
				cookRoutine = StartCoroutine(CookRoutine());
			}
			IEnumerator CookRoutine()
			{
				Console.Log("Starting cook...");
				targetOven.SetNPCUser(base.Npc.NetworkObject);
				base.Npc.Movement.FacePoint(targetOven.transform.position);
				yield return new WaitForSeconds(0.5f);
				if (!CanCookStart())
				{
					StopCook();
					End_Networked(null);
				}
				else
				{
					targetOven.Door.SetPosition(1f);
					yield return new WaitForSeconds(0.5f);
					targetOven.WireTray.SetPosition(1f);
					yield return new WaitForSeconds(5f);
					targetOven.Door.SetPosition(0f);
					yield return new WaitForSeconds(1f);
					ItemInstance itemInstance = targetOven.IngredientSlot.ItemInstance;
					if (itemInstance == null)
					{
						Console.LogWarning("No ingredient in oven!");
						StopCook();
						End_Networked(null);
					}
					else
					{
						int num = 1;
						if ((itemInstance.Definition as StorableItemDefinition).StationItem.GetModule<CookableModule>().CookType == CookableModule.ECookableType.Solid)
						{
							num = Mathf.Min(targetOven.IngredientSlot.Quantity, 10);
						}
						itemInstance.ChangeQuantity(-num);
						string iD = (itemInstance.Definition as StorableItemDefinition).StationItem.GetModule<CookableModule>().Product.ID;
						EQuality ingredientQuality = EQuality.Standard;
						if (itemInstance is QualityItemInstance)
						{
							ingredientQuality = (itemInstance as QualityItemInstance).Quality;
						}
						targetOven.SendCookOperation(new OvenCookOperation(itemInstance.ID, ingredientQuality, num, iD));
						StopCook();
						End_Networked(null);
					}
				}
			}
		}

		private void RpcReader___Observers_StartCook_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___StartCook_2166136261();
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviour_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			chemist = base.Npc as Chemist;
		}
	}
}
