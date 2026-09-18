using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Quests;
using ScheduleOne.UI;
using ScheduleOne.UI.Handover;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Behaviour
{
	public class RequestProductBehaviour : Behaviour
	{
		public enum EState
		{
			InitialApproach = 0,
			FollowPlayer = 1
		}

		public const float CONVERSATION_RANGE = 2.5f;

		public const float FOLLOW_MAX_RANGE = 5f;

		public const int MINS_TO_ASK_AGAIN = 90;

		private int minsSinceLastDialogue;

		private DialogueController.GreetingOverride requestGreeting;

		private DialogueController.DialogueChoice acceptRequestChoice;

		private DialogueController.DialogueChoice followChoice;

		private DialogueController.DialogueChoice rejectChoice;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public Player TargetPlayer { get; private set; }

		public EState State { get; private set; }

		private Customer customer => base.Npc.GetComponent<Customer>();

		[ObserversRpc(RunLocally = true)]
		public void AssignTarget(NetworkObject plr)
		{
			RpcWriter___Observers_AssignTarget_3323014238(plr);
			RpcLogic___AssignTarget_3323014238(plr);
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviour_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Begin()
		{
			base.Begin();
			State = EState.InitialApproach;
			requestGreeting.Greeting = base.Npc.dialogueHandler.Database.GetLine(EDialogueModule.Customer, "request_product_initial");
			if (InstanceFinder.IsServer)
			{
				Transform target = NetworkSingleton<NPCManager>.Instance.GetOrderedDistanceWarpPoints(TargetPlayer.transform.position)[1];
				base.Npc.Movement.Warp(target);
				if (base.Npc.isInBuilding)
				{
					base.Npc.ExitBuilding();
				}
				base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.IgnoreCosts);
				base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("requestproduct", 5, 0.4f));
			}
			requestGreeting.ShouldShow = TargetPlayer != null && TargetPlayer.Owner.IsLocalClient;
		}

		protected override void End()
		{
			base.End();
			if (requestGreeting != null)
			{
				requestGreeting.ShouldShow = false;
			}
			if (InstanceFinder.IsServer)
			{
				base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.Humanoid);
				base.Npc.Movement.SpeedController.RemoveSpeedControl("requestproduct");
			}
		}

		public override void Disable()
		{
			base.Disable();
			End();
		}

		public override void ActiveMinPass()
		{
			base.ActiveMinPass();
			if (base.Npc.dialogueHandler.IsPlaying)
			{
				minsSinceLastDialogue = 0;
			}
			minsSinceLastDialogue++;
			if (TargetPlayer.Owner.IsLocalClient)
			{
				if (State == EState.InitialApproach && CanStartDialogue())
				{
					SendStartInitialDialogue();
				}
				if (State == EState.FollowPlayer && minsSinceLastDialogue >= 90 && CanStartDialogue())
				{
					minsSinceLastDialogue = 0;
					SendStartFollowUpDialogue();
				}
			}
			if (!InstanceFinder.IsServer || Singleton<HandoverScreen>.Instance.CurrentCustomer == customer)
			{
				return;
			}
			if (!IsTargetValid(TargetPlayer))
			{
				SendDisable();
			}
			else if (State == EState.InitialApproach)
			{
				if (!IsTargetDestinationValid())
				{
					if (GetNewDestination(out var dest))
					{
						base.Npc.Movement.SetDestination(dest);
					}
					else
					{
						SendDisable();
					}
				}
			}
			else if (State == EState.FollowPlayer && !IsTargetDestinationValid())
			{
				if (GetNewDestination(out var dest2))
				{
					base.Npc.Movement.SetDestination(dest2);
				}
				else
				{
					SendDisable();
				}
			}
		}

		private bool IsTargetDestinationValid()
		{
			if (!base.Npc.Movement.IsMoving)
			{
				return false;
			}
			if (Vector3.Distance(base.Npc.Movement.CurrentDestination, TargetPlayer.transform.position) > ((State == EState.InitialApproach) ? 2.5f : 5f))
			{
				return false;
			}
			if (base.Npc.Movement.Agent.path == null)
			{
				return false;
			}
			return true;
		}

		private bool GetNewDestination(out Vector3 dest)
		{
			dest = TargetPlayer.transform.position;
			if (State == EState.InitialApproach)
			{
				dest += TargetPlayer.transform.forward * 1.5f;
			}
			else if (State == EState.InitialApproach)
			{
				dest += (base.Npc.transform.position - TargetPlayer.transform.position).normalized * 2.5f;
			}
			if (NavMeshUtility.SamplePosition(dest, out var hit, 15f, -1))
			{
				dest = hit.position;
				return true;
			}
			Console.LogError("Failed to find valid destination for RequestProductBehaviour: stopping");
			return false;
		}

		public static bool IsTargetValid(Player player)
		{
			if (player == null)
			{
				return false;
			}
			if (player.IsArrested)
			{
				return false;
			}
			if (!player.Health.IsAlive)
			{
				return false;
			}
			if (player.CrimeData.CurrentPursuitLevel != 0)
			{
				return false;
			}
			if (player.CrimeData.BodySearchPending)
			{
				return false;
			}
			if (player.IsSleeping)
			{
				return false;
			}
			return true;
		}

		public bool CanStartDialogue()
		{
			if (!IsTargetValid(TargetPlayer))
			{
				return false;
			}
			if (!TargetPlayer.Owner.IsLocalClient)
			{
				return false;
			}
			if (Singleton<DialogueCanvas>.Instance.isActive)
			{
				return false;
			}
			if (Vector3.Distance(base.Npc.transform.position, TargetPlayer.transform.position) > 2.5f)
			{
				return false;
			}
			if (Singleton<HandoverScreen>.Instance.IsOpen)
			{
				return false;
			}
			if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
			{
				return false;
			}
			return true;
		}

		private void SetUpDialogue()
		{
			if (requestGreeting == null)
			{
				acceptRequestChoice = new DialogueController.DialogueChoice();
				acceptRequestChoice.ChoiceText = "[Make an offer]";
				acceptRequestChoice.Enabled = true;
				acceptRequestChoice.Conversation = null;
				acceptRequestChoice.onChoosen = new UnityEvent();
				acceptRequestChoice.onChoosen.AddListener(RequestAccepted);
				acceptRequestChoice.shouldShowCheck = DialogueActive;
				base.Npc.dialogueHandler.GetComponent<DialogueController>().AddDialogueChoice(acceptRequestChoice);
				followChoice = new DialogueController.DialogueChoice();
				followChoice.ChoiceText = "Follow me, I need to grab it first";
				followChoice.Enabled = true;
				followChoice.Conversation = null;
				followChoice.onChoosen = new UnityEvent();
				followChoice.onChoosen.AddListener(Follow);
				followChoice.shouldShowCheck = DialogueActive;
				base.Npc.dialogueHandler.GetComponent<DialogueController>().AddDialogueChoice(followChoice);
				rejectChoice = new DialogueController.DialogueChoice();
				rejectChoice.ChoiceText = "Get out of here";
				rejectChoice.Enabled = true;
				rejectChoice.Conversation = null;
				rejectChoice.onChoosen = new UnityEvent();
				rejectChoice.onChoosen.AddListener(RequestRejected);
				rejectChoice.shouldShowCheck = DialogueActive;
				base.Npc.dialogueHandler.GetComponent<DialogueController>().AddDialogueChoice(rejectChoice);
				requestGreeting = new DialogueController.GreetingOverride();
				requestGreeting.Greeting = base.Npc.dialogueHandler.Database.GetLine(EDialogueModule.Customer, "request_product_initial");
				requestGreeting.ShouldShow = false;
				requestGreeting.PlayVO = true;
				requestGreeting.VOType = EVOLineType.Question;
				base.Npc.dialogueHandler.GetComponent<DialogueController>().AddGreetingOverride(requestGreeting);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		private void SendStartInitialDialogue()
		{
			RpcWriter___Server_SendStartInitialDialogue_2166136261();
			RpcLogic___SendStartInitialDialogue_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		private void StartInitialDialogue()
		{
			RpcWriter___Observers_StartInitialDialogue_2166136261();
			RpcLogic___StartInitialDialogue_2166136261();
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		private void SendStartFollowUpDialogue()
		{
			RpcWriter___Server_SendStartFollowUpDialogue_2166136261();
			RpcLogic___SendStartFollowUpDialogue_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		private void StartFollowUpDialogue()
		{
			RpcWriter___Observers_StartFollowUpDialogue_2166136261();
			RpcLogic___StartFollowUpDialogue_2166136261();
		}

		private bool DialogueActive(bool enabled)
		{
			if (base.Active)
			{
				return TargetPlayer.Owner.IsLocalClient;
			}
			return false;
		}

		private void RequestAccepted()
		{
			minsSinceLastDialogue = 0;
			Singleton<HandoverScreen>.Instance.Open(null, customer, HandoverScreen.EMode.Offer, HandoverClosed, customer.GetOfferSuccessChance);
		}

		private void HandoverClosed(HandoverScreen.EHandoverOutcome outcome, List<ItemInstance> items, float askingPrice)
		{
			if (outcome == HandoverScreen.EHandoverOutcome.Cancelled)
			{
				Singleton<DialogueCanvas>.Instance.SkipNextRollout = true;
				Singleton<CoroutineService>.Instance.StartCoroutine(Wait());
				return;
			}
			float offerSuccessChance = customer.GetOfferSuccessChance(items, askingPrice);
			if (Random.value < offerSuccessChance)
			{
				Contract contract = new Contract();
				ProductList productList = new ProductList();
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i] is ProductItemInstance)
					{
						productList.entries.Add(new ProductList.Entry
						{
							ProductID = items[i].ID,
							Quantity = items[i].Quantity,
							Quality = customer.CustomerData.Standards.GetCorrespondingQuality()
						});
					}
				}
				contract.SilentlyInitializeContract("Offer", string.Empty, null, string.Empty, base.Npc.NetworkObject, askingPrice, productList, string.Empty, new QuestWindowConfig(), 0, NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.GetDateTime());
				customer.ProcessHandover(HandoverScreen.EHandoverOutcome.Finalize, contract, items, handoverByPlayer: true, giveBonuses: false);
			}
			else
			{
				Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: true);
				customer.RejectProductRequestOffer();
			}
			SendDisable();
			IEnumerator Wait()
			{
				yield return new WaitForEndOfFrame();
				StartInitialDialogue();
			}
		}

		private void Follow()
		{
			minsSinceLastDialogue = 0;
			State = EState.FollowPlayer;
			base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("requestproduct", 5, 0.6f));
			requestGreeting.Greeting = base.Npc.dialogueHandler.Database.GetLine(EDialogueModule.Customer, "request_product_after_follow");
			base.Npc.dialogueHandler.ShowWorldspaceDialogue("Ok...", 3f);
		}

		private void RequestRejected()
		{
			minsSinceLastDialogue = 0;
			customer.PlayerRejectedProductRequest();
			SendDisable();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_AssignTarget_3323014238);
				RegisterServerRpc(16u, RpcReader___Server_SendStartInitialDialogue_2166136261);
				RegisterObserversRpc(17u, RpcReader___Observers_StartInitialDialogue_2166136261);
				RegisterServerRpc(18u, RpcReader___Server_SendStartFollowUpDialogue_2166136261);
				RegisterObserversRpc(19u, RpcReader___Observers_StartFollowUpDialogue_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_AssignTarget_3323014238(NetworkObject plr)
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
				writer.WriteNetworkObject(plr);
				SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___AssignTarget_3323014238(NetworkObject plr)
		{
			TargetPlayer = ((plr != null) ? plr.GetComponent<Player>() : null);
		}

		private void RpcReader___Observers_AssignTarget_3323014238(PooledReader PooledReader0, Channel channel)
		{
			NetworkObject plr = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___AssignTarget_3323014238(plr);
			}
		}

		private void RpcWriter___Server_SendStartInitialDialogue_2166136261()
		{
			if (!base.IsClientInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				SendServerRpc(16u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendStartInitialDialogue_2166136261()
		{
			StartInitialDialogue();
		}

		private void RpcReader___Server_SendStartInitialDialogue_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendStartInitialDialogue_2166136261();
			}
		}

		private void RpcWriter___Observers_StartInitialDialogue_2166136261()
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
				SendObserversRpc(17u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___StartInitialDialogue_2166136261()
		{
			if (TargetPlayer != null && TargetPlayer.IsOwner && !base.Npc.dialogueHandler.IsPlaying)
			{
				if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
				{
					Singleton<GameInput>.Instance.ExitAll();
				}
				base.Npc.dialogueHandler.GetComponent<DialogueController>().StartGenericDialogue(allowExit: false);
			}
		}

		private void RpcReader___Observers_StartInitialDialogue_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___StartInitialDialogue_2166136261();
			}
		}

		private void RpcWriter___Server_SendStartFollowUpDialogue_2166136261()
		{
			if (!base.IsClientInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				SendServerRpc(18u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendStartFollowUpDialogue_2166136261()
		{
			StartFollowUpDialogue();
		}

		private void RpcReader___Server_SendStartFollowUpDialogue_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendStartFollowUpDialogue_2166136261();
			}
		}

		private void RpcWriter___Observers_StartFollowUpDialogue_2166136261()
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
				SendObserversRpc(19u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___StartFollowUpDialogue_2166136261()
		{
			if (TargetPlayer != null && TargetPlayer.IsOwner && !base.Npc.dialogueHandler.IsPlaying)
			{
				if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
				{
					Singleton<GameInput>.Instance.ExitAll();
				}
				base.Npc.dialogueHandler.GetComponent<DialogueController>().StartGenericDialogue(allowExit: false);
			}
		}

		private void RpcReader___Observers_StartFollowUpDialogue_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___StartFollowUpDialogue_2166136261();
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviour_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			SetUpDialogue();
		}
	}
}
