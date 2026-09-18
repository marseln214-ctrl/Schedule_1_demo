using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.Variables;
using ScheduleOne.Vehicles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop
{
	public class ShopInterface : MonoBehaviour
	{
		public enum EPaymentType
		{
			Cash = 0,
			Online = 1,
			PreferCash = 2,
			PreferOnline = 3
		}

		[Header("Settings")]
		public string ShopName = "Shop";

		public EPaymentType PaymentType;

		public bool ShowCurrencyHint;

		[Header("Listings")]
		public List<ShopListing> Listings = new List<ShopListing>();

		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public RectTransform ListingContainer;

		public TextMeshProUGUI StoreNameLabel;

		public Cart Cart;

		public StorageEntity[] DeliveryBays;

		public VehicleDetector LoadingBayDetector;

		public ShopInterfaceDetailPanel DetailPanel;

		public RectTransform QuantityDropdown;

		public VerticalLayoutGroup QuantityDropdownContent;

		public ScrollRect ListingScrollRect;

		[Header("Audio")]
		public AudioSourceController AddItemSound;

		public AudioSourceController RemoveItemSound;

		public AudioSourceController CheckoutSound;

		[Header("Prefabs")]
		public ListingUI ListingUIPrefab;

		public UnityEvent onOrderCompleted;

		[SerializeField]
		private List<CategoryButton> categoryButtons = new List<CategoryButton>();

		private EShopCategory categoryFilter;

		private string searchTerm = string.Empty;

		private List<ListingUI> listingUI = new List<ListingUI>();

		private ListingUI selectedListing;

		private bool dropdownMouseUp;

		public bool IsOpen { get; protected set; }

		protected virtual void Awake()
		{
			foreach (ShopListing listing in Listings)
			{
				CreateListingUI(listing);
			}
			ListingScrollRect.verticalNormalizedPosition = 1f;
			Listings = Listings.OrderBy((ShopListing x) => x.Item.Name).ToList();
			categoryButtons = GetComponentsInChildren<CategoryButton>().ToList();
			StoreNameLabel.text = ShopName;
			ListingContainer.anchoredPosition = Vector2.zero;
		}

		protected virtual void Start()
		{
			RefreshShownItems();
			GameInput.RegisterExitListener(Exit, 7);
			RestockAllListings();
			foreach (ShopListing listing in Listings)
			{
				if (listing.Item.RequiresLevelToPurchase)
				{
					NetworkSingleton<LevelManager>.Instance.AddUnlockable(new Unlockable(listing.Item.RequiredRank, listing.Item.Name, listing.Item.Icon));
				}
			}
			IsOpen = false;
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
		}

		private void OnValidate()
		{
			StoreNameLabel.text = ShopName;
			for (int i = 0; i < Listings.Count; i++)
			{
				if (!(Listings[i].Item == null))
				{
					string text = "(";
					for (int j = 0; j < Listings[i].Item.ShopCategories.Count; j++)
					{
						text = text + Listings[i].Item.ShopCategories[j].Category.ToString() + ", ";
					}
					text += ")";
					Listings[i].name = Listings[i].Item.Name + " ($" + Listings[i].Price + ") " + text;
					if (Listings[i].Item.RequiresLevelToPurchase)
					{
						ShopListing shopListing = Listings[i];
						string text2 = shopListing.name;
						FullRank requiredRank = Listings[i].Item.RequiredRank;
						shopListing.name = text2 + " [Rank " + requiredRank.ToString() + "]";
					}
				}
			}
		}

		protected virtual void Update()
		{
			if (IsOpen && UnityEngine.Input.GetMouseButtonUp(0))
			{
				if (dropdownMouseUp)
				{
					QuantityDropdown.gameObject.SetActive(value: false);
					selectedListing = null;
				}
				else
				{
					dropdownMouseUp = true;
				}
			}
		}

		public virtual void SetIsOpen(bool isOpen)
		{
			IsOpen = isOpen;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(ShopName);
			if (isOpen)
			{
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(ShopName);
				PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
				PlayerSingleton<PlayerMovement>.Instance.canMove = false;
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
				SelectCategory(EShopCategory.All);
				RefreshShownItems();
				ListingScrollRect.verticalNormalizedPosition = 1f;
				ListingScrollRect.content.anchoredPosition = Vector2.zero;
				RefreshUnlockStatus();
				Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
				if (ShowCurrencyHint)
				{
					ShowCurrencyHint = false;
					Singleton<HintDisplay>.Instance.ShowHint("Your <h1>online balance</h> is displayed in the top right corner.", 10f);
					Invoke("Hint", 10.5f);
				}
			}
			else
			{
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
				PlayerSingleton<PlayerMovement>.Instance.canMove = true;
				PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
				DetailPanel.Close();
				QuantityDropdown.gameObject.SetActive(value: false);
			}
			Canvas.enabled = isOpen;
			Container.gameObject.SetActive(isOpen);
		}

		private void Hint()
		{
			Singleton<HintDisplay>.Instance.ShowHint("Most legal shops will only accept <h1>card payments</h>, while most illegal shops only take cash. Visit an <h1>ATM</h> to deposit and withdraw cash.", 20f);
		}

		protected virtual void Exit(ExitAction action)
		{
			if (!action.used && IsOpen)
			{
				action.used = true;
				SetIsOpen(isOpen: false);
			}
		}

		private void CreateListingUI(ShopListing listing)
		{
			ListingUI component = UnityEngine.Object.Instantiate(ListingUIPrefab.gameObject, ListingContainer).GetComponent<ListingUI>();
			component.Initialize(listing);
			ListingUI ui = component;
			component.onClicked = (Action)Delegate.Combine(component.onClicked, (Action)delegate
			{
				ListingClicked(ui);
			});
			component.onDropdownClicked = (Action)Delegate.Combine(component.onDropdownClicked, (Action)delegate
			{
				DropdownClicked(ui);
			});
			component.hoverStart = (Action)Delegate.Combine(component.hoverStart, (Action)delegate
			{
				EntryHovered(ui);
			});
			component.hoverEnd = (Action)Delegate.Combine(component.hoverEnd, new Action(EntryUnhovered));
			listingUI.Add(component);
		}

		public void SelectCategory(EShopCategory category)
		{
			categoryButtons.Find((CategoryButton x) => x.Category == category).Select();
		}

		public virtual void ListingClicked(ListingUI listingUI)
		{
			if (listingUI.Listing.Item.IsPurchasable)
			{
				Cart.AddItem(listingUI.Listing, 1);
				AddItemSound.Play();
			}
		}

		private void ShowCartAnimation(ListingUI listing)
		{
			StartCoroutine(Routine());
			IEnumerator Routine()
			{
				RectTransform iconRect = listing.GetIconCopy(Canvas.GetComponent<RectTransform>());
				iconRect.SetAsLastSibling();
				iconRect.position = listing.Icon.GetComponent<RectTransform>().position;
				Vector3 startPos = iconRect.position;
				Vector2 endPos = Cart.CartIcon.rectTransform.position;
				Vector3 startScale = iconRect.localScale;
				Vector3 endScale = startScale * 0.5f;
				float lerpTime = 0.45f;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					iconRect.position = Vector2.Lerp(startPos, endPos, i / lerpTime);
					iconRect.localScale = Vector3.Lerp(startScale, endScale, i / lerpTime);
					yield return new WaitForEndOfFrame();
				}
				UnityEngine.Object.Destroy(iconRect.gameObject);
				Cart.BopCartIcon();
			}
		}

		public void CategorySelected(EShopCategory category)
		{
			if (category != categoryFilter)
			{
				DeselectCurrentCategory();
				categoryFilter = category;
				RefreshShownItems();
			}
		}

		private void DeselectCurrentCategory()
		{
			categoryButtons.Find((CategoryButton x) => x.Category == categoryFilter).Deselect();
		}

		private void RefreshShownItems()
		{
			for (int i = 0; i < listingUI.Count; i++)
			{
				if (searchTerm != string.Empty)
				{
					listingUI[i].gameObject.SetActive(listingUI[i].Listing.DoesListingMatchSearchTerm(searchTerm));
				}
				else
				{
					listingUI[i].gameObject.SetActive(listingUI[i].Listing.DoesListingMatchCategoryFilter(categoryFilter) && listingUI[i].Listing.ShouldShow());
				}
			}
			for (int j = 0; j < listingUI.Count; j++)
			{
				listingUI[j].transform.SetSiblingIndex(j);
			}
			List<ListingUI> list = listingUI.FindAll((ListingUI x) => !x.Listing.Item.IsPurchasable);
			list.Sort((ListingUI x, ListingUI y) => x.Listing.Item.RequiredRank.CompareTo(y.Listing.Item.RequiredRank));
			for (int k = 0; k < list.Count; k++)
			{
				list[k].transform.SetAsLastSibling();
			}
		}

		private void RefreshUnlockStatus()
		{
			for (int i = 0; i < listingUI.Count; i++)
			{
				listingUI[i].UpdateLockStatus();
			}
		}

		private void RestockAllListings()
		{
			foreach (ShopListing listing in Listings)
			{
				listing.Restock();
			}
		}

		public bool CanCartFitItem(ShopListing listing)
		{
			return true;
		}

		public bool WillCartFit()
		{
			List<ItemSlot> availableSlots = GetAvailableSlots();
			return WillCartFit(availableSlots);
		}

		public bool WillCartFit(List<ItemSlot> availableSlots)
		{
			List<ShopListing> list = Cart.cartDictionary.Keys.ToList();
			List<ItemSlot> list2 = new List<ItemSlot>();
			for (int i = 0; i < list.Count; i++)
			{
				int num = Cart.cartDictionary[list[i]];
				ItemInstance defaultInstance = list[i].Item.GetDefaultInstance();
				for (int j = 0; j < availableSlots.Count; j++)
				{
					if (num <= 0)
					{
						break;
					}
					if (!list2.Contains(availableSlots[j]))
					{
						int capacityForItem = availableSlots[j].GetCapacityForItem(defaultInstance);
						if (capacityForItem > 0)
						{
							list2.Add(availableSlots[j]);
							num -= Mathf.Min(num, capacityForItem);
						}
					}
				}
				if (num > 0)
				{
					return false;
				}
			}
			return true;
		}

		public virtual void HandoverItems()
		{
			List<ItemSlot> availableSlots = GetAvailableSlots();
			List<ShopListing> list = Cart.cartDictionary.Keys.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				NetworkSingleton<VariableDatabase>.Instance.NotifyItemAcquired(list[i].Item.ID, Cart.cartDictionary[list[i]]);
				int num = Cart.cartDictionary[list[i]];
				ItemInstance defaultInstance = list[i].Item.GetDefaultInstance();
				for (int j = 0; j < availableSlots.Count; j++)
				{
					if (num <= 0)
					{
						break;
					}
					int capacityForItem = availableSlots[j].GetCapacityForItem(defaultInstance);
					if (capacityForItem != 0)
					{
						int num2 = Mathf.Min(capacityForItem, num);
						availableSlots[j].AddItem(defaultInstance.GetCopy(num2));
						num -= num2;
					}
				}
				if (num > 0)
				{
					Debug.LogWarning("Failed to handover all items in cart: " + defaultInstance.Name);
				}
			}
		}

		public List<ItemSlot> GetAvailableSlots()
		{
			List<ItemSlot> list = new List<ItemSlot>();
			LandVehicle loadingBayVehicle = GetLoadingBayVehicle();
			if (loadingBayVehicle != null && Cart.LoadVehicleToggle.isOn)
			{
				list.AddRange(loadingBayVehicle.Storage.ItemSlots);
			}
			else
			{
				list.AddRange(PlayerSingleton<PlayerInventory>.Instance.hotbarSlots);
			}
			for (int i = 0; i < DeliveryBays.Length; i++)
			{
				list.AddRange(DeliveryBays[i].ItemSlots);
			}
			return list;
		}

		public LandVehicle GetLoadingBayVehicle()
		{
			if (LoadingBayDetector != null && LoadingBayDetector.closestVehicle != null && LoadingBayDetector.closestVehicle.IsPlayerOwned)
			{
				return LoadingBayDetector.closestVehicle;
			}
			return null;
		}

		public void PlaceItemInDeliveryBay(ItemInstance item)
		{
			int num = item.Quantity;
			StorageEntity[] deliveryBays = DeliveryBays;
			foreach (StorageEntity storageEntity in deliveryBays)
			{
				int num2 = storageEntity.HowManyCanFit(item);
				if (num2 > 0)
				{
					ItemInstance copy = item.GetCopy(Mathf.Min(num, num2));
					storageEntity.InsertItem(copy);
					num -= copy.Quantity;
				}
				if (num <= 0)
				{
					break;
				}
			}
			if (num > 0)
			{
				Console.LogWarning("Could not fit all items in delivery bay!");
			}
		}

		public void QuantitySelected(int amount)
		{
			if (!(selectedListing == null) && selectedListing.Listing.Item.IsPurchasable)
			{
				Cart.AddItem(selectedListing.Listing, amount);
				AddItemSound.Play();
				QuantityDropdown.gameObject.SetActive(value: false);
				selectedListing = null;
			}
		}

		public void OpenQuantityDropdown(ListingUI listing)
		{
			selectedListing = listing;
			if ((listing.transform.position.y - ListingScrollRect.viewport.position.y) / Canvas.scaleFactor < -260f)
			{
				QuantityDropdown.position = listing.TopDropdownAnchor.position;
				QuantityDropdown.anchoredPosition += QuantityDropdown.sizeDelta.y * Vector2.up / 2f;
				QuantityDropdownContent.reverseArrangement = true;
			}
			else
			{
				QuantityDropdown.position = listing.DropdownAnchor.position;
				QuantityDropdown.anchoredPosition -= QuantityDropdown.sizeDelta.y * Vector2.up / 2f;
				QuantityDropdownContent.reverseArrangement = false;
			}
			QuantityDropdown.gameObject.SetActive(value: true);
			QuantityDropdown.SetAsLastSibling();
			dropdownMouseUp = false;
		}

		private void DropdownClicked(ListingUI listing)
		{
			if (selectedListing == listing)
			{
				QuantityDropdown.gameObject.SetActive(value: false);
				selectedListing = null;
			}
			else
			{
				OpenQuantityDropdown(listing);
			}
		}

		private void EntryHovered(ListingUI listing)
		{
			DetailPanel.Open(listing);
		}

		private void EntryUnhovered()
		{
			DetailPanel.Close();
		}
	}
}
