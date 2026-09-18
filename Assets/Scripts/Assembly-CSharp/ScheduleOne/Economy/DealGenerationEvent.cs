using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Money;
using ScheduleOne.Product;
using ScheduleOne.Quests;
using ScheduleOne.UI.Phone.Messages;
using UnityEngine;

namespace ScheduleOne.Economy
{
	[Serializable]
	public class DealGenerationEvent
	{
		[Serializable]
		public class DayContainer
		{
			public EDay Day;
		}

		[Header("Settings")]
		public bool Enabled = true;

		public bool CanBeAccepted = true;

		public bool CanBeRejected = true;

		[Header("Timing Settings")]
		public List<DayContainer> ApplicableDays = new List<DayContainer>();

		public int GenerationTime;

		public int GenerationWindowDuration = 60;

		[Header("Products and payment")]
		public ProductList ProductList;

		public float Payment = 100f;

		[Range(0f, 5f)]
		public float RelationshipRequirement = 1f;

		[Header("Messages")]
		[SerializeField]
		private MessageChain[] RequestMessageChains;

		public MessageChain[] ContractAcceptedResponses;

		public MessageChain[] ContractRejectedResponses;

		[Header("Location settings")]
		public DeliveryLocation DeliveryLocation;

		public int PickupScheduleGroup;

		[Header("Window/expiry settings")]
		public QuestWindowConfig DeliveryWindow;

		public bool Expires = true;

		[Tooltip("How many days after being accepted does this contract expire? Exact expiry is adjusted to match window end")]
		[Range(1f, 7f)]
		public int ExpiresAfter = 2;

		public ContractInfo GenerateContractInfo(Customer customer)
		{
			return new ContractInfo(Payment, ProductList, DeliveryLocation.GUID.ToString(), DeliveryWindow, Expires, ExpiresAfter, PickupScheduleGroup, isCounterOffer: false);
		}

		public bool ShouldGenerate(Customer customer)
		{
			if (customer.NPC.RelationData.RelationDelta < RelationshipRequirement)
			{
				return false;
			}
			if (!ApplicableDays.Exists((DayContainer x) => x.Day == NetworkSingleton<TimeManager>.Instance.CurrentDay))
			{
				return false;
			}
			if (!NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(GenerationTime, TimeManager.AddMinutesTo24HourTime(GenerationTime, GenerationWindowDuration)))
			{
				return false;
			}
			return true;
		}

		public MessageChain GetRandomRequestMessage()
		{
			return ProcessMessage(RequestMessageChains[UnityEngine.Random.Range(0, RequestMessageChains.Length - 1)]);
		}

		public MessageChain ProcessMessage(MessageChain messageChain)
		{
			MessageChain messageChain2 = new MessageChain();
			foreach (string message in messageChain.Messages)
			{
				string text = message.Replace("<PRICE>", "<color=#46CB4F>" + MoneyManager.FormatAmount(Payment) + "</color>");
				text = text.Replace("<PRODUCT>", GetProductStringList());
				text = text.Replace("<QUALITY>", GetQualityString());
				text = text.Replace("<LOCATION>", DeliveryLocation.GetDescription());
				text = text.Replace("<WINDOW_START>", TimeManager.Get12HourTime(DeliveryWindow.WindowStartTime));
				text = text.Replace("<WINDOW_END>", TimeManager.Get12HourTime(DeliveryWindow.WindowEndTime));
				messageChain2.Messages.Add(text);
			}
			return messageChain2;
		}

		public MessageChain GetRejectionMessage()
		{
			return ProcessMessage(ContractRejectedResponses[UnityEngine.Random.Range(0, ContractRejectedResponses.Length - 1)]);
		}

		public MessageChain GetAcceptanceMessage()
		{
			return ProcessMessage(ContractAcceptedResponses[UnityEngine.Random.Range(0, ContractAcceptedResponses.Length - 1)]);
		}

		public string GetProductStringList()
		{
			return ProductList.GetCommaSeperatedString();
		}

		public string GetQualityString()
		{
			return ProductList.GetQualityString();
		}
	}
}
