using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class FleeBehaviour : Behaviour
	{
		public const float FLEE_DIST_MIN = 20f;

		public const float FLEE_DIST_MAX = 40f;

		public const float FLEE_SPEED = 0.7f;

		private Vector3 currentFleeTarget = Vector3.zero;

		private float nextVO;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public NetworkObject EntityToFlee { get; private set; }

		[ObserversRpc(RunLocally = true)]
		public void SetEntityToFlee(NetworkObject entity)
		{
			RpcWriter___Observers_SetEntityToFlee_3323014238(entity);
			RpcLogic___SetEntityToFlee_3323014238(entity);
		}

		protected override void Begin()
		{
			base.Begin();
			StartFlee();
			EVOLineType lineType = ((Random.Range(0, 2) == 0) ? EVOLineType.Scared : EVOLineType.Concerned);
			base.Npc.PlayVO(lineType);
		}

		protected override void Resume()
		{
			base.Resume();
			StartFlee();
		}

		protected override void End()
		{
			base.End();
			Stop();
			base.Npc.Avatar.EmotionManager.RemoveEmotionOverride("fleeing");
		}

		protected override void Pause()
		{
			base.Pause();
			Stop();
		}

		private void StartFlee()
		{
			Flee();
			base.Npc.Avatar.EmotionManager.AddEmotionOverride("Scared", "fleeing");
			base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("fleeing", 2, 0.7f));
			nextVO = Time.time + Random.Range(5f, 15f);
		}

		public override void ActiveMinPass()
		{
			base.ActiveMinPass();
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (EntityToFlee == null)
			{
				End();
				return;
			}
			if (!base.Npc.Movement.IsMoving && Vector3.Distance(base.transform.position, currentFleeTarget) < 3f)
			{
				End_Networked(null);
				Disable_Networked(null);
				return;
			}
			Vector3 from = EntityToFlee.transform.position - base.transform.position;
			from.y = 0f;
			if (Vector3.Angle(from, base.Npc.Movement.Agent.desiredVelocity) < 30f)
			{
				Console.Log("Fleeing entity is in front, finding new flee position");
				Flee();
			}
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (Time.time > nextVO)
			{
				EVOLineType lineType = ((Random.Range(0, 2) == 0) ? EVOLineType.Scared : EVOLineType.Concerned);
				base.Npc.PlayVO(lineType);
				nextVO = Time.time + Random.Range(5f, 15f);
			}
		}

		private void Stop()
		{
			base.Npc.Movement.Stop();
			base.Npc.Movement.SpeedController.RemoveSpeedControl("fleeing");
		}

		private void Flee()
		{
			Vector3 destination = (currentFleeTarget = GetFleePosition());
			base.Npc.Movement.SetDestination(destination);
		}

		public Vector3 GetFleePosition()
		{
			int num = 0;
			float num2 = 0f;
			while (true)
			{
				if (EntityToFlee == null)
				{
					return Vector3.zero;
				}
				Vector3 vector = base.transform.position - EntityToFlee.transform.position;
				vector.y = 0f;
				vector = Quaternion.AngleAxis(num2, Vector3.up) * vector;
				float num3 = Random.Range(20f, 40f);
				if (Physics.Raycast(base.transform.position + vector.normalized * num3 + Vector3.up * 10f, Vector3.down, out var hitInfo, 20f, LayerMask.GetMask("Default")) && NavMeshUtility.SamplePosition(hitInfo.point, out var hit, 2f, -1))
				{
					return hit.position;
				}
				if (num > 10)
				{
					break;
				}
				num2 += 15f;
				num++;
			}
			Console.LogWarning("Failed to find a valid flee position, returning current position");
			return base.transform.position;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_SetEntityToFlee_3323014238);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_SetEntityToFlee_3323014238(NetworkObject entity)
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
				writer.WriteNetworkObject(entity);
				SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetEntityToFlee_3323014238(NetworkObject entity)
		{
			EntityToFlee = entity;
		}

		private void RpcReader___Observers_SetEntityToFlee_3323014238(PooledReader PooledReader0, Channel channel)
		{
			NetworkObject entity = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetEntityToFlee_3323014238(entity);
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
