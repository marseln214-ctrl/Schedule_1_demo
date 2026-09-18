using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class StartCauldronBehaviour : Behaviour
	{
		public const float START_CAULDRON_TIME = 15f;

		private Coroutine workRoutine;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public Cauldron Station { get; protected set; }

		public bool WorkInProgress { get; protected set; }

		protected override void Begin()
		{
			base.Begin();
			StartWork();
		}

		protected override void Resume()
		{
			base.Resume();
			StartWork();
		}

		protected override void Pause()
		{
			base.Pause();
			if (WorkInProgress)
			{
				StopCauldron();
			}
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
			if (WorkInProgress)
			{
				StopCauldron();
			}
			if (InstanceFinder.IsServer && Station != null && Station.NPCUserObject == base.Npc.NetworkObject)
			{
				Station.SetNPCUser(null);
			}
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (!InstanceFinder.IsServer || WorkInProgress)
			{
				return;
			}
			if (IsStationReady(Station))
			{
				if (!base.Npc.Movement.IsMoving)
				{
					if (IsAtStation())
					{
						BeginCauldron();
					}
					else
					{
						GoToStation();
					}
				}
			}
			else
			{
				Disable_Networked(null);
			}
		}

		private void StartWork()
		{
			if (InstanceFinder.IsServer)
			{
				if (!IsStationReady(Station))
				{
					Console.LogWarning(base.Npc.fullName + " has no station to work with");
					Disable_Networked(null);
				}
				else
				{
					Station.SetNPCUser(base.Npc.NetworkObject);
				}
			}
		}

		public void AssignStation(Cauldron station)
		{
			Station = station;
		}

		public bool IsAtStation()
		{
			return base.Npc.Movement.IsAsCloseAsPossible(Station.StandPoint.position);
		}

		public void GoToStation()
		{
			base.Npc.Movement.SetDestination(Station.StandPoint.position);
		}

		[ObserversRpc(RunLocally = true)]
		public void BeginCauldron()
		{
			RpcWriter___Observers_BeginCauldron_2166136261();
			RpcLogic___BeginCauldron_2166136261();
		}

		private void StopCauldron()
		{
			if (workRoutine != null)
			{
				StopCoroutine(workRoutine);
			}
			WorkInProgress = false;
		}

		public bool IsStationReady(Cauldron station)
		{
			if (station == null)
			{
				return false;
			}
			if (station.GetState() != Cauldron.EState.Ready)
			{
				return false;
			}
			if (((IUsable)station).IsInUse && (station.PlayerUserObject != null || station.NPCUserObject != base.Npc.NetworkObject))
			{
				return false;
			}
			if (!base.Npc.Movement.CanGetTo(station.StandPoint.position))
			{
				return false;
			}
			return true;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_BeginCauldron_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_BeginCauldron_2166136261()
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

		public void RpcLogic___BeginCauldron_2166136261()
		{
			if (!WorkInProgress)
			{
				WorkInProgress = true;
				base.Npc.Movement.FaceDirection(Station.StandPoint.forward);
				workRoutine = StartCoroutine(Package());
			}
			IEnumerator Package()
			{
				yield return new WaitForEndOfFrame();
				base.Npc.Avatar.Anim.SetBool("UseChemistryStation", value: true);
				float packageTime = 15f;
				for (float i = 0f; i < packageTime; i += Time.deltaTime)
				{
					base.Npc.Avatar.LookController.OverrideLookTarget(Station.LinkOriginPoint, 0);
					yield return new WaitForEndOfFrame();
				}
				base.Npc.Avatar.Anim.SetBool("UseChemistryStation", value: false);
				if (InstanceFinder.IsServer)
				{
					EQuality quality = Station.RemoveIngredients();
					Station.StartCookOperation(null, Station.CookTime, quality);
				}
				WorkInProgress = false;
				workRoutine = null;
			}
		}

		private void RpcReader___Observers_BeginCauldron_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___BeginCauldron_2166136261();
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
