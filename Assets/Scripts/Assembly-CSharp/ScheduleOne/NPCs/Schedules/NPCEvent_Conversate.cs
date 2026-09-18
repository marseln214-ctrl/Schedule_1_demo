using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Schedules
{
	public class NPCEvent_Conversate : NPCEvent
	{
		private EVOLineType[] ConversationLines = new EVOLineType[8]
		{
			EVOLineType.Greeting,
			EVOLineType.Question,
			EVOLineType.Surprised,
			EVOLineType.Alerted,
			EVOLineType.Annoyed,
			EVOLineType.Acknowledge,
			EVOLineType.Think,
			EVOLineType.No
		};

		private string[] AnimationTriggers = new string[4] { "ThumbsUp", "DisagreeWave", "Nod", "ConversationGesture1" };

		public const float DESTINATION_THRESHOLD = 1f;

		public const float TIME_BEFORE_WAIT_START = 3f;

		public ConversationLocation Location;

		private bool IsConversating;

		private Coroutine conversateRoutine;

		private bool IsWaiting;

		public UnityEvent OnWaitStart;

		public UnityEvent OnWaitEnd;

		private float timeAtDestination;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted;

		public new string ActionName => "Conversate";

		private Transform StandPoint => Location.GetStandPoint(npc);

		public override string GetName()
		{
			if (Location == null)
			{
				return ActionName + " (No destination set)";
			}
			return ActionName + " (" + Location.gameObject.name + ")";
		}

		protected override void Start()
		{
			base.Start();
			Location.NPCs.Add(npc);
		}

		public override void Started()
		{
			base.Started();
			if (IsAtDestination())
			{
				WalkCallback(NPCMovement.WalkResult.Success);
			}
			else
			{
				SetDestination(StandPoint.position);
			}
		}

		public override void ActiveUpdate()
		{
			base.ActiveUpdate();
			if (!npc.Movement.IsMoving)
			{
				if (IsAtDestination())
				{
					Location.SetNPCReady(npc, ready: true);
					timeAtDestination += Time.deltaTime;
				}
				else
				{
					Location.SetNPCReady(npc, ready: false);
					timeAtDestination = 0f;
					SetDestination(StandPoint.position);
				}
			}
			else
			{
				Location.SetNPCReady(npc, ready: false);
				timeAtDestination = 0f;
			}
		}

		public override void MinPassed()
		{
			base.MinPassed();
			if (InstanceFinder.IsServer)
			{
				if (!IsConversating && timeAtDestination >= 0.1f && CanConversationStart())
				{
					StartConversate();
				}
				if (!IsConversating && !IsWaiting && timeAtDestination >= 3f && !CanConversationStart())
				{
					StartWait();
				}
				if (IsConversating && !CanConversationStart())
				{
					EndConversate();
				}
			}
		}

		public override void LateStarted()
		{
			base.LateStarted();
			if (IsAtDestination())
			{
				WalkCallback(NPCMovement.WalkResult.Success);
			}
			else
			{
				SetDestination(StandPoint.position);
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
				if (InstanceFinder.IsServer)
				{
					npc.Movement.Warp(StandPoint.position);
				}
				npc.Movement.FaceDirection(StandPoint.forward);
			}
		}

		public override void End()
		{
			base.End();
			Location.SetNPCReady(npc, ready: false);
			if (IsWaiting)
			{
				EndWait();
			}
			if (IsConversating)
			{
				EndConversate();
			}
		}

		public override void Interrupt()
		{
			base.Interrupt();
			Location.SetNPCReady(npc, ready: false);
			if (npc.Movement.IsMoving)
			{
				npc.Movement.Stop();
			}
			if (IsWaiting)
			{
				EndWait();
			}
			if (IsConversating)
			{
				EndConversate();
			}
		}

		public override void Resume()
		{
			base.Resume();
			if (IsAtDestination())
			{
				WalkCallback(NPCMovement.WalkResult.Success);
			}
			else
			{
				SetDestination(StandPoint.position);
			}
		}

		private bool IsAtDestination()
		{
			return Vector3.Distance(npc.Movement.FootPosition, StandPoint.position) < 1f;
		}

		private bool CanConversationStart()
		{
			return Location.NPCsReady;
		}

		protected override void WalkCallback(NPCMovement.WalkResult result)
		{
			base.WalkCallback(result);
			if (base.IsActive && result == NPCMovement.WalkResult.Success)
			{
				npc.Movement.FaceDirection(StandPoint.forward);
			}
		}

		[ObserversRpc(RunLocally = true)]
		protected virtual void StartWait()
		{
			RpcWriter___Observers_StartWait_2166136261();
			RpcLogic___StartWait_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		protected virtual void EndWait()
		{
			RpcWriter___Observers_EndWait_2166136261();
			RpcLogic___EndWait_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		protected virtual void StartConversate()
		{
			RpcWriter___Observers_StartConversate_2166136261();
			RpcLogic___StartConversate_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		protected virtual void EndConversate()
		{
			RpcWriter___Observers_EndConversate_2166136261();
			RpcLogic___EndConversate_2166136261();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(0u, RpcReader___Observers_StartWait_2166136261);
				RegisterObserversRpc(1u, RpcReader___Observers_EndWait_2166136261);
				RegisterObserversRpc(2u, RpcReader___Observers_StartConversate_2166136261);
				RegisterObserversRpc(3u, RpcReader___Observers_EndConversate_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_StartWait_2166136261()
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

		protected virtual void RpcLogic___StartWait_2166136261()
		{
			if (!IsWaiting)
			{
				IsWaiting = true;
				if (OnWaitStart != null)
				{
					OnWaitStart.Invoke();
				}
			}
		}

		private void RpcReader___Observers_StartWait_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___StartWait_2166136261();
			}
		}

		private void RpcWriter___Observers_EndWait_2166136261()
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
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___EndWait_2166136261()
		{
			if (IsWaiting)
			{
				IsWaiting = false;
				if (OnWaitEnd != null)
				{
					OnWaitEnd.Invoke();
				}
			}
		}

		private void RpcReader___Observers_EndWait_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___EndWait_2166136261();
			}
		}

		private void RpcWriter___Observers_StartConversate_2166136261()
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

		protected virtual void RpcLogic___StartConversate_2166136261()
		{
			if (!IsConversating)
			{
				if (IsWaiting)
				{
					EndWait();
				}
				IsConversating = true;
				conversateRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				while (IsConversating)
				{
					Random.InitState(npc.fullName.GetHashCode() + (int)Time.time);
					float wait = Random.Range(2f, 8f);
					NPC otherNPC = Location.GetOtherNPC(npc);
					for (float t = 0f; t < wait; t += Time.deltaTime)
					{
						if (!IsConversating)
						{
							yield break;
						}
						npc.Avatar.LookController.OverrideLookTarget(otherNPC.Avatar.LookController.HeadBone.position, 1);
						yield return new WaitForEndOfFrame();
					}
					npc.VoiceOverEmitter.Play(ConversationLines[Random.Range(0, ConversationLines.Length)]);
					npc.Avatar.Anim.SetTrigger(AnimationTriggers[Random.Range(0, AnimationTriggers.Length)]);
				}
			}
		}

		private void RpcReader___Observers_StartConversate_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___StartConversate_2166136261();
			}
		}

		private void RpcWriter___Observers_EndConversate_2166136261()
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
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___EndConversate_2166136261()
		{
			if (IsConversating)
			{
				IsConversating = false;
				timeAtDestination = 0f;
				if (conversateRoutine != null)
				{
					StopCoroutine(conversateRoutine);
				}
			}
		}

		private void RpcReader___Observers_EndConversate_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___EndConversate_2166136261();
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
