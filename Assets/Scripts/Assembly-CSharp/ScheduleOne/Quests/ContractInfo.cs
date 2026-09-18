using System;
using System.Collections.Generic;
using ScheduleOne.Dialogue;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Money;
using ScheduleOne.Product;

namespace ScheduleOne.Quests
{
	[Serializable]
	public class ContractInfo
	{
		public float Payment;

		public ProductList Products;

		public string DeliveryLocationGUID;

		public QuestWindowConfig DeliveryWindow;

		public bool Expires;

		public int ExpiresAfter;

		public int PickupScheduleIndex;

		public bool IsCounterOffer;

		public DeliveryLocation DeliveryLocation { get; private set; }

		public ContractInfo(float payment, ProductList products, string deliveryLocationGUID, QuestWindowConfig deliveryWindow, bool expires, int expiresAfter, int pickupScheduleIndex, bool isCounterOffer)
		{
			Payment = payment;
			Products = products;
			DeliveryLocationGUID = deliveryLocationGUID;
			DeliveryWindow = deliveryWindow;
			Expires = expires;
			ExpiresAfter = expiresAfter;
			PickupScheduleIndex = pickupScheduleIndex;
			IsCounterOffer = isCounterOffer;
			if (GUIDManager.IsGUIDValid(deliveryLocationGUID))
			{
				DeliveryLocation = GUIDManager.GetObject<DeliveryLocation>(new Guid(deliveryLocationGUID));
			}
		}

		public ContractInfo()
		{
		}

		public DialogueChain ProcessMessage(DialogueChain messageChain)
		{
			if (DeliveryLocation == null && GUIDManager.IsGUIDValid(DeliveryLocationGUID))
			{
				DeliveryLocation = GUIDManager.GetObject<DeliveryLocation>(new Guid(DeliveryLocationGUID));
			}
			List<string> list = new List<string>();
			string[] lines = messageChain.Lines;
			for (int i = 0; i < lines.Length; i++)
			{
				string text = lines[i].Replace("<PRICE>", "<color=#46CB4F>" + MoneyManager.FormatAmount(Payment) + "</color>");
				text = text.Replace("<PRODUCT>", Products.GetCommaSeperatedString());
				text = text.Replace("<QUALITY>", Products.GetQualityString());
				text = text.Replace("<LOCATION>", "<b>" + DeliveryLocation.GetDescription() + "</b>");
				text = text.Replace("<WINDOW_START>", TimeManager.Get12HourTime(DeliveryWindow.WindowStartTime));
				text = text.Replace("<WINDOW_END>", TimeManager.Get12HourTime(DeliveryWindow.WindowEndTime));
				list.Add(text);
			}
			return new DialogueChain
			{
				Lines = list.ToArray()
			};
		}
	}
}
