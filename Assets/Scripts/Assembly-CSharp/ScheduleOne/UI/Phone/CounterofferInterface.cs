using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Messaging;
using ScheduleOne.Money;
using ScheduleOne.Product;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone
{
	public class CounterofferInterface : MonoBehaviour
	{
		public const int COUNTEROFFER_SUCCESS_XP = 5;

		public const int MinQuantity = 1;

		public int MaxQuantity = 50;

		public const float MinPrice = 1f;

		public const float MaxPrice = 9999f;

		public float IconAlignment = 0.2f;

		public GameObject ProductEntryPrefab;

		[Header("References")]
		public GameObject Container;

		public Text TitleLabel;

		public Button ConfirmButton;

		public Image ProductIcon;

		public Text ProductLabel;

		public RectTransform ProductLabelRect;

		public InputField PriceInput;

		public Text FairPriceLabel;

		public RectTransform EntryContainer;

		private Action<ProductDefinition, int, float> orderConfirmedCallback;

		private ProductDefinition product;

		private int quantity;

		private float price;

		private Dictionary<ProductDefinition, RectTransform> productEntries = new Dictionary<ProductDefinition, RectTransform>();

		private bool mouseUp;

		private MSGConversation conversation;

		public bool IsOpen { get; private set; }

		private void Awake()
		{
		}

		private void Start()
		{
			GameInput.RegisterExitListener(Exit, 4);
			Close();
		}

		private void Update()
		{
			if (EntryContainer.gameObject.activeSelf && GameInput.GetButtonUp(GameInput.ButtonCode.PrimaryClick) && mouseUp)
			{
				EntryContainer.gameObject.SetActive(value: false);
				DisplayProduct(product);
			}
			if (!GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
			{
				mouseUp = true;
			}
		}

		public void Open(ProductDefinition product, int quantity, float price, MSGConversation _conversation, Action<ProductDefinition, int, float> _orderConfirmedCallback)
		{
			IsOpen = true;
			this.product = product;
			this.quantity = Mathf.Clamp(quantity, 1, MaxQuantity);
			this.price = price;
			foreach (ProductDefinition allProduct in NetworkSingleton<ProductManager>.Instance.AllProducts)
			{
				if (!productEntries.ContainsKey(allProduct))
				{
					CreateProductEntry(allProduct);
				}
			}
			conversation = _conversation;
			MSGConversation mSGConversation = conversation;
			mSGConversation.onMessageRendered = (Action)Delegate.Combine(mSGConversation.onMessageRendered, new Action(Close));
			orderConfirmedCallback = _orderConfirmedCallback;
			EntryContainer.gameObject.SetActive(value: false);
			Container.gameObject.SetActive(value: true);
			SetProduct(product);
			PriceInput.text = price.ToString();
		}

		private void CreateProductEntry(ProductDefinition product)
		{
			if (!productEntries.ContainsKey(product))
			{
				RectTransform component = UnityEngine.Object.Instantiate(ProductEntryPrefab, EntryContainer).GetComponent<RectTransform>();
				component.Find("Icon").GetComponent<Image>().sprite = product.Icon;
				component.GetComponent<Button>().onClick.AddListener(delegate
				{
					EntryContainer.gameObject.SetActive(value: false);
					SetProduct(product);
				});
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerEnter;
				entry.callback.AddListener(delegate
				{
					DisplayProduct(product);
				});
				component.gameObject.AddComponent<EventTrigger>().triggers.Add(entry);
				productEntries.Add(product, component);
			}
		}

		public void Close()
		{
			IsOpen = false;
			if (conversation != null)
			{
				MSGConversation mSGConversation = conversation;
				mSGConversation.onMessageRendered = (Action)Delegate.Remove(mSGConversation.onMessageRendered, new Action(Close));
			}
			Container.gameObject.SetActive(value: false);
		}

		public void Exit(ExitAction action)
		{
			if (!action.used && IsOpen)
			{
				action.used = true;
				Close();
			}
		}

		public void Send()
		{
			if (float.TryParse(PriceInput.text, out var result))
			{
				price = result;
			}
			price = Mathf.Clamp(price, 1f, 9999f);
			PriceInput.SetTextWithoutNotify(price.ToString());
			if (orderConfirmedCallback != null)
			{
				orderConfirmedCallback(product, quantity, price);
			}
			Close();
		}

		private void UpdateFairPrice()
		{
			float amount = product.MarketValue * (float)quantity;
			FairPriceLabel.text = "Fair price: " + MoneyManager.FormatAmount(amount);
		}

		private void SetProduct(ProductDefinition newProduct)
		{
			product = newProduct;
			DisplayProduct(newProduct);
			UpdateFairPrice();
		}

		private void DisplayProduct(ProductDefinition tempProduct)
		{
			ProductIcon.sprite = tempProduct.Icon;
			UpdatePriceQuantityLabel(tempProduct.Name);
		}

		public void ChangeQuantity(int change)
		{
			quantity = Mathf.Clamp(quantity + change, 1, MaxQuantity);
			UpdatePriceQuantityLabel(product.Name);
			UpdateFairPrice();
		}

		private void UpdatePriceQuantityLabel(string productName)
		{
			ProductLabel.text = quantity + "x " + productName;
			float value = 0f - ProductLabel.preferredWidth / 2f + 20f;
			ProductLabelRect.anchoredPosition = new Vector2(Mathf.Clamp(value, -120f, float.MaxValue), ProductLabelRect.anchoredPosition.y);
		}

		public void ChangePrice(float change)
		{
			price = Mathf.Clamp(price + change, 1f, 9999f);
			PriceInput.SetTextWithoutNotify(price.ToString());
		}

		public void PriceSubmitted(string value)
		{
			if (float.TryParse(value, out var result))
			{
				price = result;
			}
			else
			{
				price = 0f;
			}
			price = Mathf.Clamp(price, 1f, 9999f);
			PriceInput.SetTextWithoutNotify(price.ToString());
		}

		public void ProductClicked()
		{
			mouseUp = false;
			if (!EntryContainer.gameObject.activeSelf)
			{
				EntryContainer.gameObject.SetActive(value: true);
				return;
			}
			EntryContainer.gameObject.SetActive(value: false);
			DisplayProduct(product);
		}
	}
}
