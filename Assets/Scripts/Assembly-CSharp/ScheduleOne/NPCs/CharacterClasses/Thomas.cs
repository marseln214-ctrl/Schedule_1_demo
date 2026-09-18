using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Messaging;
using ScheduleOne.Money;
using ScheduleOne.NPCs.Schedules;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Datas.Characters;
using ScheduleOne.Product;
using ScheduleOne.Quests;
using ScheduleOne.UI.Handover;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.CharacterClasses
{
	public class Thomas : NPC
	{
		public const int CARTEL_CONTRACT_QUANTITY = 15;

		public const float CARTEL_CONTRACT_PAYMENT = 100f;

		public NPCEvent_LocationDialogue FirstMeetingEvent;

		public NPCEvent_LocationDialogue HandoverEvent;

		public UnityEvent onCartelContractReceived;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted;

		public bool MeetingReminderSent { get; protected set; }

		public bool HandoverReminderSent { get; protected set; }

		protected override void Start()
		{
			base.Start();
			dialogueHandler.onDialogueChoiceChosen.AddListener(DialogueChoiceCallback);
		}

		public void SetFirstMeetingEventActive(bool active)
		{
			FirstMeetingEvent.gameObject.SetActive(active);
		}

		public void SetHandoverEventActive(bool active)
		{
			HandoverEvent.gameObject.SetActive(active);
		}

		public void SendMeetingReminder()
		{
			if (MeetingReminderSent)
			{
				Console.LogWarning("Reminder message already sent");
				return;
			}
			MeetingReminderSent = true;
			base.HasChanged = true;
			Message message = new Message();
			message.text = "Either you haven't read our note or are choosing to ignore it - for your sake I'll assume the former. We have business to discuss at Hyland Manor ASAP. - TB";
			message.sender = Message.ESenderType.Other;
			message.endOfGroup = true;
			base.MSGConversation.SetIsKnown(known: false);
			base.MSGConversation.SendMessage(message);
		}

		public void SendHandoverReminder()
		{
			if (HandoverReminderSent)
			{
				Console.LogWarning("Reminder message already sent");
				return;
			}
			Debug.Log("Sending reminder");
			HandoverReminderSent = true;
			base.HasChanged = true;
			Message message = new Message();
			message.text = "You haven't yet made this week's delivery. There are 24 hours left. Don't make this difficult. - TB";
			message.sender = Message.ESenderType.Other;
			message.endOfGroup = true;
			base.MSGConversation.SendMessage(message);
		}

		public void InitialMeetingComplete()
		{
			base.MSGConversation.SetIsKnown(known: true);
			SetFirstMeetingEventActive(active: false);
		}

		private void DialogueChoiceCallback(string choiceLabel)
		{
			if (choiceLabel == "BEGIN_HANDOVER")
			{
				ProductList productList = new ProductList();
				productList.entries.Add(new ProductList.Entry
				{
					ProductID = "ogkush",
					Quantity = 15,
					Quality = EQuality.Trash
				});
				Contract contract = new GameObject("CartelContract").AddComponent<Contract>();
				contract.transform.SetParent(base.transform);
				contract.SilentlyInitializeContract("Cartel Contract", "Deliver the goods to the cartel", new QuestEntryData[0], string.Empty, base.NetworkObject, 100f, productList, string.Empty, new QuestWindowConfig(), 0, NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.GetDateTime());
				Singleton<HandoverScreen>.Instance.Open(contract, GetComponent<Customer>(), HandoverScreen.EMode.Contract, ProcessItemHandover, null);
			}
		}

		private void ProcessItemHandover(HandoverScreen.EHandoverOutcome outcome, List<ItemInstance> items, float price)
		{
			if (outcome != 0)
			{
				Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: false);
				SetHandoverEventActive(active: false);
				HandoverReminderSent = false;
				base.HasChanged = true;
				GetComponent<Customer>().ContractWellReceived(handoverByPlayer: true);
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(100f);
				if (onCartelContractReceived != null)
				{
					onCartelContractReceived.Invoke();
				}
			}
		}

		public override string GetSaveString()
		{
			return new ThomasData(ID, MeetingReminderSent, HandoverReminderSent).GetJson();
		}

		public override void Load(NPCData data, string containerPath)
		{
			base.Load(data, containerPath);
			if (((ISaveable)this).TryLoadFile(containerPath, "NPC", out string contents))
			{
				ThomasData thomasData = null;
				try
				{
					thomasData = JsonUtility.FromJson<ThomasData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogWarning("Failed to deserialize character data: " + ex.Message);
					return;
				}
				MeetingReminderSent = thomasData.MeetingReminderSent;
				HandoverReminderSent = thomasData.HandoverReminderSent;
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
