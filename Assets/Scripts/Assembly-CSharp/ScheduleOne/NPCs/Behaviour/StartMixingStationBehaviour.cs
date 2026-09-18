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
	public class StartMixingStationBehaviour : Behaviour
	{
		public const float INSERT_INGREDIENT_BASE_TIME = 1f;

		private Chemist chemist;

		private Coroutine startRoutine;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public MixingStation targetStation { get; private set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviour_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public void AssignStation(MixingStation station)
		{
			targetStation = station;
		}

		protected override void End()
		{
			base.End();
			if (startRoutine != null)
			{
				StopCook();
			}
			Disable();
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (startRoutine != null)
			{
				base.Npc.Avatar.LookController.OverrideLookTarget(targetStation.UIPoint.position, 5);
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
			if (targetStation == null)
			{
				return false;
			}
			if (((IUsable)targetStation).IsInUse && ((IUsable)targetStation).NPCUserObject != base.Npc.NetworkObject)
			{
				return false;
			}
			MixingStationConfiguration mixingStationConfiguration = targetStation.Configuration as MixingStationConfiguration;
			if ((float)targetStation.GetMixQuantity() < mixingStationConfiguration.StartThrehold.Value)
			{
				return false;
			}
			return true;
		}

		private void StopCook()
		{
			targetStation.SetNPCUser(null);
			base.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", value: false);
			if (startRoutine != null)
			{
				StopCoroutine(startRoutine);
				startRoutine = null;
			}
		}

		private Vector3 GetStationAccessPoint()
		{
			if (targetStation == null)
			{
				return base.Npc.transform.position;
			}
			return ((ITransitEntity)targetStation).AccessPoints[0].position;
		}

		private bool IsAtStation()
		{
			if (targetStation == null)
			{
				return false;
			}
			return Vector3.Distance(base.Npc.transform.position, GetStationAccessPoint()) < 1f;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_StartCook_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
			if (startRoutine == null)
			{
				startRoutine = StartCoroutine(CookRoutine());
			}
			IEnumerator CookRoutine()
			{
				base.Npc.Movement.FacePoint(targetStation.transform.position);
				yield return new WaitForSeconds(0.5f);
				if (!CanCookStart())
				{
					StopCook();
					End_Networked(null);
				}
				else
				{
					targetStation.SetNPCUser(base.Npc.NetworkObject);
					base.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", value: true);
					QualityItemInstance product = targetStation.ProductSlot.ItemInstance as QualityItemInstance;
					ItemInstance mixer = targetStation.MixerSlot.ItemInstance;
					int mixQuantity = targetStation.GetMixQuantity();
					for (int i = 0; i < mixQuantity; i++)
					{
						yield return new WaitForSeconds(1f);
					}
					if (InstanceFinder.IsServer)
					{
						targetStation.ProductSlot.ChangeQuantity(-mixQuantity);
						targetStation.MixerSlot.ChangeQuantity(-mixQuantity);
						MixOperation operation = new MixOperation(product.ID, product.Quality, mixer.ID, mixQuantity);
						targetStation.SendMixingOperation(operation, 0);
					}
					StopCook();
					End_Networked(null);
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

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviour_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			chemist = base.Npc as Chemist;
		}
	}
}
