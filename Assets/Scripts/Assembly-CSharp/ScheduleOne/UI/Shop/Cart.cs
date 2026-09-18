using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop
{
	public class Cart : MonoBehaviour
	{
		[Header("References")]
		public ShopInterface Shop;

		public Image CartIcon;

		public TextMeshProUGUI ViewCartText;

		public RectTransform CartEntryContainer;

		public TextMeshProUGUI ProblemText;

		public TextMeshProUGUI WarningText;

		public Button BuyButton;

		public RectTransform CartContainer;

		public Image CartArea;

		public TextMeshProUGUI TotalText;

		public Toggle LoadVehicleToggle;

		[Header("Prefabs")]
		public CartEntry EntryPrefab;

		public Dictionary<ShopListing, int> cartDictionary = new Dictionary<ShopListing, int>();

		private Coroutine cartIconBop;

		private List<CartEntry> cartEntries = new List<CartEntry>();

		protected virtual void Start()
		{
			UpdateViewCartText();
			BuyButton.onClick.AddListener(Buy);
		}

		protected virtual void Update()
		{
			if (Shop.IsOpen)
			{
				UpdateEntries();
				UpdateLoadVehicleToggle();
				UpdateTotal();
				UpdateProblem();
			}
		}

		public void AddItem(ShopListing listing, int quantity)
		{
			if (!cartDictionary.ContainsKey(listing))
			{
				cartDictionary.Add(listing, 0);
			}
			cartDictionary[listing] += quantity;
			UpdateViewCartText();
			UpdateEntries();
		}

		public void RemoveItem(ShopListing listing)
		{
			cartDictionary[listing]--;
			if (cartDictionary[listing] <= 0)
			{
				cartDictionary.Remove(listing);
			}
			Shop.RemoveItemSound.Play();
			UpdateProblem();
			UpdateViewCartText();
			UpdateEntries();
			UpdateTotal();
		}

		public void ClearCart()
		{
			cartDictionary.Clear();
			UpdateViewCartText();
			UpdateEntries();
			UpdateTotal();
		}

		public void BopCartIcon()
		{
			if (cartIconBop != null)
			{
				StopCoroutine(cartIconBop);
			}
			cartIconBop = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				Vector3 startScale = Vector3.one;
				Vector3 endScale = Vector3.one * 1.25f;
				float lerpTime = 0.09f;
				for (float j = 0f; j < lerpTime; j += Time.deltaTime)
				{
					CartIcon.transform.localScale = Vector3.Lerp(startScale, endScale, j / lerpTime);
					yield return new WaitForEndOfFrame();
				}
				for (float j = 0f; j < lerpTime; j += Time.deltaTime)
				{
					CartIcon.transform.localScale = Vector3.Lerp(endScale, startScale, j / lerpTime);
					yield return new WaitForEndOfFrame();
				}
				CartIcon.transform.localScale = startScale;
				cartIconBop = null;
			}
		}

		public bool CanPlayerAffordCart()
		{
			float priceSum = GetPriceSum();
			switch (Shop.PaymentType)
			{
			case ShopInterface.EPaymentType.Cash:
				return NetworkSingleton<MoneyManager>.Instance.cashBalance >= priceSum;
			case ShopInterface.EPaymentType.Online:
				return NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= priceSum;
			case ShopInterface.EPaymentType.PreferCash:
				if (!(NetworkSingleton<MoneyManager>.Instance.cashBalance >= priceSum))
				{
					return NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= priceSum;
				}
				return true;
			case ShopInterface.EPaymentType.PreferOnline:
				if (!(NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= priceSum))
				{
					return NetworkSingleton<MoneyManager>.Instance.cashBalance >= priceSum;
				}
				return true;
			default:
				return false;
			}
		}

		public void Buy()
		{
			if (!CanCheckout(out var _))
			{
				return;
			}
			Shop.HandoverItems();
			switch (Shop.PaymentType)
			{
			case ShopInterface.EPaymentType.Cash:
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - GetPriceSum());
				break;
			case ShopInterface.EPaymentType.Online:
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Purchase from " + Shop.ShopName, 0f - GetPriceSum(), 1f, string.Empty);
				break;
			case ShopInterface.EPaymentType.PreferCash:
				if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= GetPriceSum())
				{
					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - GetPriceSum());
				}
				else
				{
					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Purchase from " + Shop.ShopName, 0f - GetPriceSum(), 1f, string.Empty);
				}
				break;
			case ShopInterface.EPaymentType.PreferOnline:
				if (NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= GetPriceSum())
				{
					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Purchase from " + Shop.ShopName, 0f - GetPriceSum(), 1f, string.Empty);
				}
				else
				{
					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - GetPriceSum());
				}
				break;
			}
			ClearCart();
			Shop.CheckoutSound.Play();
			Shop.SetIsOpen(isOpen: false);
			if (Shop.onOrderCompleted != null)
			{
				Shop.onOrderCompleted.Invoke();
			}
		}

		private void UpdateEntries()
		{
			List<ShopListing> list = cartDictionary.Keys.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				CartEntry cartEntry = GetEntry(list[i]);
				if (cartEntry == null)
				{
					cartEntry = Object.Instantiate(EntryPrefab, CartEntryContainer);
					cartEntry.Initialize(this, list[i], cartDictionary[list[i]]);
					cartEntries.Add(cartEntry);
				}
				if (cartEntry.Quantity != cartDictionary[list[i]])
				{
					cartEntry.SetQuantity(cartDictionary[list[i]]);
				}
			}
			for (int j = 0; j < cartEntries.Count; j++)
			{
				if (!cartDictionary.ContainsKey(cartEntries[j].Listing))
				{
					Object.Destroy(cartEntries[j].gameObject);
					cartEntries.RemoveAt(j);
					j--;
				}
			}
		}

		private void UpdateTotal()
		{
			TotalText.text = "Total: <color=#" + ColorUtility.ToHtmlStringRGBA(ListingUI.PriceLabelColor_Normal) + ">" + MoneyManager.FormatAmount(GetPriceSum()) + "</color>";
		}

		private void UpdateProblem()
		{
			string reason;
			bool flag = CanCheckout(out reason);
			BuyButton.interactable = flag && cartDictionary.Count > 0;
			if (flag)
			{
				ProblemText.enabled = false;
			}
			else
			{
				ProblemText.text = reason;
				ProblemText.enabled = true;
			}
			if (GetWarning(out var warning) && !ProblemText.enabled)
			{
				WarningText.text = warning;
				WarningText.enabled = true;
			}
			else
			{
				WarningText.enabled = false;
			}
		}

		private bool CanCheckout(out string reason)
		{
			if (!Shop.WillCartFit())
			{
				if (Shop.DeliveryBays.Length != 0)
				{
					reason = "Order too large";
				}
				else
				{
					reason = "Order won't fit in inventory";
				}
				return false;
			}
			if (!CanPlayerAffordCart())
			{
				if (Shop.PaymentType == ShopInterface.EPaymentType.Cash)
				{
					reason = "Insufficient cash. Visit an ATM to withdraw cash.";
				}
				else if (Shop.PaymentType == ShopInterface.EPaymentType.Online)
				{
					reason = "Insufficient online balance. Visit an ATM to deposit cash.";
				}
				else
				{
					reason = "Insufficient funds";
				}
				return false;
			}
			reason = string.Empty;
			return true;
		}

		private bool GetWarning(out string warning)
		{
			warning = string.Empty;
			if (Shop.GetLoadingBayVehicle() != null && LoadVehicleToggle.isOn)
			{
				List<ItemSlot> itemSlots = Shop.GetLoadingBayVehicle().Storage.ItemSlots;
				if (!Shop.WillCartFit(itemSlots))
				{
					warning = "Vehicle won't fit everything. Some items will be placed on the pallets.";
					return true;
				}
			}
			else
			{
				List<ItemSlot> availableSlots = PlayerSingleton<PlayerInventory>.Instance.hotbarSlots.Cast<ItemSlot>().ToList();
				if (!Shop.WillCartFit(availableSlots))
				{
					warning = "Inventory won't fit everything. Some items will be placed on the pallets.";
					return true;
				}
			}
			return false;
		}

		private void UpdateViewCartText()
		{
			int itemSum = GetItemSum();
			if (itemSum > 0)
			{
				ViewCartText.text = "View Cart (" + itemSum + " item" + ((itemSum > 1) ? "s" : "") + ")";
			}
			else
			{
				ViewCartText.text = "View Cart";
			}
		}

		private void UpdateLoadVehicleToggle()
		{
			LoadVehicleToggle.gameObject.SetActive(Shop.GetLoadingBayVehicle() != null);
		}

		private int GetItemSum()
		{
			int num = 0;
			List<ShopListing> list = cartDictionary.Keys.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				num += cartDictionary[list[i]];
			}
			return num;
		}

		private float GetPriceSum()
		{
			float num = 0f;
			List<ShopListing> list = cartDictionary.Keys.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				num += (float)cartDictionary[list[i]] * list[i].Price;
			}
			return num;
		}

		private CartEntry GetEntry(ShopListing listing)
		{
			return cartEntries.Find((CartEntry x) => x.Listing == listing);
		}

		private bool IsMouseOverMenuArea()
		{
			return RectTransformUtility.RectangleContainsScreenPoint(CartArea.rectTransform, UnityEngine.Input.mousePosition);
		}

		public int GetTotalSlotRequirement()
		{
			ShopListing[] array = cartDictionary.Keys.ToArray();
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				int num2 = cartDictionary[array[i]];
				num += Mathf.CeilToInt((float)num2 / (float)array[i].Item.StackLimit);
			}
			return num;
		}
	}
}
