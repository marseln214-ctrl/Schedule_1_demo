using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Quests;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Handover
{
	public class HandoverScreen : Singleton<HandoverScreen>
	{
		public enum EMode
		{
			Contract = 0,
			Sample = 1,
			Offer = 2
		}

		public enum EHandoverOutcome
		{
			Cancelled = 0,
			Finalize = 1
		}

		private enum EItemSource
		{
			Player = 0,
			Vehicle = 1
		}

		public const int CUSTOMER_SLOT_COUNT = 4;

		public const float VEHICLE_MAX_DIST = 20f;

		[Header("Settings")]
		public Gradient SuccessColorMap;

		[Header("References")]
		public Canvas Canvas;

		public GameObject Container;

		public CanvasGroup CanvasGroup;

		public TextMeshProUGUI DescriptionLabel;

		public TextMeshProUGUI CustomerSubtitle;

		public TextMeshProUGUI FavouriteDrugLabel;

		public TextMeshProUGUI FavouritePropertiesLabel;

		public TextMeshProUGUI[] PropertiesEntries;

		public RectTransform[] ExpectationEntries;

		public GameObject NoVehicle;

		public RectTransform VehicleSlotContainer;

		public RectTransform CustomerSlotContainer;

		public TextMeshProUGUI VehicleSubtitle;

		public TextMeshProUGUI SuccessLabel;

		public TextMeshProUGUI ErrorLabel;

		public TextMeshProUGUI WarningLabel;

		public Button DoneButton;

		public RectTransform VehicleContainer;

		public TextMeshProUGUI TitleLabel;

		public HandoverScreenPriceSelector PriceSelector;

		public TextMeshProUGUI FairPriceLabel;

		public Animation TutorialAnimation;

		public RectTransform TutorialContainer;

		public Action<EHandoverOutcome, List<ItemInstance>, float> onHandoverComplete;

		public Func<List<ItemInstance>, float, float> SuccessChanceMethod;

		private ItemSlotUI[] VehicleSlotUIs;

		private ItemSlotUI[] CustomerSlotUIs;

		private ItemSlot[] CustomerSlots = new ItemSlot[4];

		private Dictionary<ItemInstance, EItemSource> OriginalItemLocations = new Dictionary<ItemInstance, EItemSource>();

		private bool ignoreCustomerChangedEvents;

		public Contract CurrentContract { get; protected set; }

		public bool IsOpen { get; protected set; }

		public bool TutorialOpen { get; private set; }

		public EMode Mode { get; protected set; }

		public Customer CurrentCustomer { get; private set; }

		protected override void Start()
		{
			base.Start();
			GameInput.RegisterExitListener(Exit, 8);
			VehicleSlotUIs = VehicleSlotContainer.GetComponentsInChildren<ItemSlotUI>();
			CustomerSlotUIs = CustomerSlotContainer.GetComponentsInChildren<ItemSlotUI>();
			DoneButton.onClick.AddListener(DonePressed);
			for (int i = 0; i < CustomerSlots.Length; i++)
			{
				CustomerSlots[i] = new ItemSlot();
				CustomerSlotUIs[i].AssignSlot(CustomerSlots[i]);
				ItemSlot obj = CustomerSlots[i];
				obj.onItemDataChanged = (Action)Delegate.Combine(obj.onItemDataChanged, new Action(CustomerItemsChanged));
			}
			VehicleSubtitle.text = "This is the vehicle you last drove.\nMust be within " + 20f + " meters.";
			ClearCustomerSlots(returnToOriginals: false);
			PriceSelector.gameObject.SetActive(value: false);
			PriceSelector.onPriceChanged.AddListener(UpdateSuccessChance);
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			IsOpen = false;
		}

		private void Update()
		{
			if (IsOpen && ((Player.Local.CrimeData.CurrentPursuitLevel != 0 && Player.Local.CrimeData.TimeSinceSighted < 5f) || Player.Local.CrimeData.CurrentArrestProgress > 0.01f))
			{
				Close(EHandoverOutcome.Cancelled);
			}
		}

		private void OpenTutorial()
		{
			CanvasGroup.alpha = 0f;
			TutorialOpen = true;
			TutorialContainer.gameObject.SetActive(value: true);
			TutorialAnimation.Play();
		}

		public void CloseTutorial()
		{
			CanvasGroup.alpha = 1f;
			TutorialOpen = false;
			TutorialContainer.gameObject.SetActive(value: false);
		}

		public virtual void Open(Contract contract, Customer customer, EMode mode, Action<EHandoverOutcome, List<ItemInstance>, float> callback, Func<List<ItemInstance>, float, float> successChanceMethod)
		{
			if (mode == EMode.Contract && contract == null)
			{
				Console.LogWarning("Contract is null");
				return;
			}
			CurrentContract = contract;
			CurrentCustomer = customer;
			Mode = mode;
			if (Mode == EMode.Contract)
			{
				TitleLabel.text = "Complete Deal";
			}
			else if (Mode == EMode.Sample)
			{
				TitleLabel.text = "Give Free Sample";
			}
			else if (Mode == EMode.Offer)
			{
				TitleLabel.text = "Offer Deal";
			}
			onHandoverComplete = callback;
			SuccessChanceMethod = successChanceMethod;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			Singleton<CompassManager>.Instance.SetVisible(visible: false);
			List<ItemSlot> allInventorySlots = PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots();
			List<ItemSlot> secondarySlots = new List<ItemSlot>(CustomerSlots);
			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("ItemAmountSelectionTutorialDone"))
			{
				_ = GameManager.IS_TUTORIAL;
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ItemAmountSelectionTutorialDone", true.ToString());
				OpenTutorial();
			}
			else
			{
				Player.Local.VisualState.ApplyState("drugdeal", PlayerVisualState.EVisualState.DrugDealing);
			}
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			if (Mode == EMode.Contract)
			{
				DescriptionLabel.text = customer.NPC.FirstName + " is paying <color=#50E65A>" + MoneyManager.FormatAmount(contract.Payment) + "</color> for:";
				DescriptionLabel.enabled = true;
			}
			else
			{
				DescriptionLabel.enabled = false;
			}
			if (Mode == EMode.Sample)
			{
				EDrugType property = customer.GetOrderedDrugTypes()[0];
				string text = ColorUtility.ToHtmlStringRGB(property.GetColor());
				FavouriteDrugLabel.text = customer.NPC.FirstName + "'s favourite drug: <color=#" + text + ">" + property.ToString() + "</color>";
				FavouriteDrugLabel.enabled = true;
				FavouritePropertiesLabel.text = customer.NPC.FirstName + "'s favourite effects:";
				for (int i = 0; i < PropertiesEntries.Length; i++)
				{
					if (customer.CustomerData.PreferredProperties.Count > i)
					{
						PropertiesEntries[i].text = "•  " + customer.CustomerData.PreferredProperties[i].Name;
						PropertiesEntries[i].color = customer.CustomerData.PreferredProperties[i].LabelColor;
						PropertiesEntries[i].enabled = true;
					}
					else
					{
						PropertiesEntries[i].enabled = false;
					}
				}
				FavouritePropertiesLabel.gameObject.SetActive(value: true);
			}
			else
			{
				FavouriteDrugLabel.enabled = false;
				FavouritePropertiesLabel.gameObject.SetActive(value: false);
			}
			for (int j = 0; j < ExpectationEntries.Length; j++)
			{
				if (contract != null && contract.ProductList.entries.Count > j)
				{
					ExpectationEntries[j].Find("Title").gameObject.GetComponent<TextMeshProUGUI>().text = "<color=#FFC73D>" + contract.ProductList.entries[j].Quantity + "x</color> " + Registry.GetItem(contract.ProductList.entries[j].ProductID).Name;
					ExpectationEntries[j].Find("Star").GetComponent<Image>().color = ItemQuality.GetColor(contract.ProductList.entries[j].Quality);
					ExpectationEntries[j].Find("Star").GetComponent<RectTransform>().anchoredPosition = new Vector2((0f - ExpectationEntries[j].Find("Title").gameObject.GetComponent<TextMeshProUGUI>().preferredWidth) / 2f + 30f, 0f);
					ExpectationEntries[j].gameObject.SetActive(value: true);
				}
				else
				{
					ExpectationEntries[j].gameObject.SetActive(value: false);
				}
			}
			if (Player.Local.LastDrivenVehicle != null && Player.Local.LastDrivenVehicle.Storage != null && Vector3.Distance(Player.Local.LastDrivenVehicle.transform.position, Player.Local.transform.position) < 20f)
			{
				if (Player.Local.LastDrivenVehicle.Storage != null)
				{
					for (int k = 0; k < VehicleSlotUIs.Length; k++)
					{
						ItemSlot itemSlot = null;
						if (k < Player.Local.LastDrivenVehicle.Storage.ItemSlots.Count)
						{
							itemSlot = Player.Local.LastDrivenVehicle.Storage.ItemSlots[k];
						}
						if (itemSlot != null)
						{
							VehicleSlotUIs[k].AssignSlot(itemSlot);
							VehicleSlotUIs[k].gameObject.SetActive(value: true);
							allInventorySlots.Add(itemSlot);
						}
						else
						{
							VehicleSlotUIs[k].gameObject.SetActive(value: false);
						}
					}
				}
				NoVehicle.gameObject.SetActive(value: false);
				VehicleContainer.gameObject.SetActive(value: true);
			}
			else
			{
				NoVehicle.gameObject.SetActive(value: true);
				VehicleContainer.gameObject.SetActive(value: false);
			}
			if (Mode == EMode.Contract)
			{
				CustomerSubtitle.text = "Place the expected products here";
			}
			else if (Mode == EMode.Sample)
			{
				CustomerSubtitle.text = "Place a product here for " + customer.NPC.FirstName + " to try";
			}
			else if (Mode == EMode.Offer)
			{
				CustomerSubtitle.text = "Place product here";
			}
			if (mode == EMode.Offer)
			{
				PriceSelector.gameObject.SetActive(value: true);
				PriceSelector.SetPrice(1f);
			}
			else
			{
				PriceSelector.gameObject.SetActive(value: false);
			}
			RecordOriginalLocations();
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
			Singleton<ItemUIManager>.Instance.EnableQuickMove(allInventorySlots, secondarySlots);
			CustomerItemsChanged();
			Canvas.enabled = true;
			Container.gameObject.SetActive(value: true);
			IsOpen = true;
		}

		public virtual void Close(EHandoverOutcome outcome)
		{
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			List<ItemInstance> list = new List<ItemInstance>();
			if (outcome == EHandoverOutcome.Finalize)
			{
				for (int i = 0; i < CustomerSlots.Length; i++)
				{
					if (CustomerSlots[i].ItemInstance != null)
					{
						list.Add(CustomerSlots[i].ItemInstance);
					}
				}
			}
			Singleton<CompassManager>.Instance.SetVisible(visible: true);
			CurrentContract = null;
			CurrentCustomer = null;
			IsOpen = false;
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			float arg = 0f;
			if (Mode == EMode.Offer)
			{
				PriceSelector.RefreshPrice();
				arg = PriceSelector.Price;
			}
			if (onHandoverComplete != null)
			{
				onHandoverComplete(outcome, list, arg);
			}
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
			Player.Local.VisualState.RemoveState("drugdeal");
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			if (outcome == EHandoverOutcome.Cancelled)
			{
				ClearCustomerSlots(returnToOriginals: true);
			}
		}

		public void DonePressed()
		{
			Close(EHandoverOutcome.Finalize);
		}

		private void RecordOriginalLocations()
		{
			foreach (HotbarSlot hotbarSlot in PlayerSingleton<PlayerInventory>.Instance.hotbarSlots)
			{
				if (hotbarSlot.ItemInstance != null)
				{
					if (OriginalItemLocations.ContainsKey(hotbarSlot.ItemInstance))
					{
						Console.LogWarning("Item already exists in original locations");
					}
					else
					{
						OriginalItemLocations.Add(hotbarSlot.ItemInstance, EItemSource.Player);
					}
				}
			}
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && IsOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				if (TutorialOpen)
				{
					CloseTutorial();
				}
				else
				{
					Close(EHandoverOutcome.Cancelled);
				}
			}
		}

		public void ClearCustomerSlots(bool returnToOriginals)
		{
			ignoreCustomerChangedEvents = true;
			ItemSlot[] customerSlots = CustomerSlots;
			foreach (ItemSlot itemSlot in customerSlots)
			{
				if (itemSlot.ItemInstance != null)
				{
					if (returnToOriginals)
					{
						PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(itemSlot.ItemInstance);
					}
					itemSlot.ClearStoredInstance();
				}
			}
			OriginalItemLocations.Clear();
			ignoreCustomerChangedEvents = false;
			CustomerItemsChanged();
		}

		private void CustomerItemsChanged()
		{
			if (!ignoreCustomerChangedEvents)
			{
				UpdateDoneButton();
				UpdateSuccessChance();
				if (Mode == EMode.Offer)
				{
					float customerItemsValue = GetCustomerItemsValue();
					PriceSelector.SetPrice(customerItemsValue);
					FairPriceLabel.text = "Fair price: " + MoneyManager.FormatAmount(customerItemsValue);
				}
			}
		}

		private void UpdateDoneButton()
		{
			if (GetError(out var err))
			{
				DoneButton.interactable = false;
				ErrorLabel.text = err;
				ErrorLabel.enabled = true;
			}
			else
			{
				DoneButton.interactable = true;
				ErrorLabel.enabled = false;
			}
			if (!ErrorLabel.enabled && GetWarning(out var warning))
			{
				WarningLabel.text = warning;
				WarningLabel.enabled = true;
			}
			else
			{
				WarningLabel.enabled = false;
			}
		}

		private void UpdateSuccessChance()
		{
			if (GetCustomerItems(onlyPackagedProduct: false).Count == 0)
			{
				SuccessLabel.enabled = false;
				return;
			}
			float num = 0f;
			if (Mode == EMode.Sample)
			{
				num = SuccessChanceMethod?.Invoke(GetCustomerItems(), 0f) ?? 0f;
				SuccessLabel.text = Mathf.RoundToInt(num * 100f) + "% chance of success";
				SuccessLabel.color = SuccessColorMap.Evaluate(num);
				SuccessLabel.enabled = true;
			}
			else if (Mode == EMode.Contract)
			{
				if (CurrentContract == null)
				{
					Console.LogWarning("Current contract is null");
					return;
				}
				num = Mathf.Clamp(CurrentContract.GetProductListMatch(GetCustomerItems(), out var _), 0.01f, 1f);
				if (num < 1f)
				{
					SuccessLabel.text = Mathf.RoundToInt(num * 100f) + "% chance of customer accepting";
					SuccessLabel.color = SuccessColorMap.Evaluate(num);
					SuccessLabel.enabled = true;
				}
				else
				{
					SuccessLabel.enabled = false;
				}
			}
			else if (Mode == EMode.Offer)
			{
				float price = PriceSelector.Price;
				num = SuccessChanceMethod?.Invoke(GetCustomerItems(), price) ?? 0f;
				SuccessLabel.text = Mathf.RoundToInt(num * 100f) + "% chance of success";
				SuccessLabel.color = SuccessColorMap.Evaluate(num);
				SuccessLabel.enabled = true;
			}
		}

		private bool GetError(out string err)
		{
			err = string.Empty;
			if (Mode == EMode.Contract && CurrentContract != null)
			{
				if (GetCustomerItemsCount(onlyPackagedProduct: false) == 0)
				{
					err = string.Empty;
					return true;
				}
				if (NetworkSingleton<GameManager>.Instance.IsTutorial && GetCustomerItemsCount() > CurrentContract.ProductList.GetTotalQuantity())
				{
					err = "You are providing more product than required.";
					return true;
				}
			}
			if ((Mode == EMode.Sample || Mode == EMode.Offer) && GetCustomerItemsCount() == 0)
			{
				bool flag = false;
				for (int i = 0; i < CustomerSlots.Length; i++)
				{
					if (CustomerSlots[i].ItemInstance != null && CustomerSlots[i].ItemInstance is ProductItemInstance && (CustomerSlots[i].ItemInstance as ProductItemInstance).AppliedPackaging == null)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					err = "Product must be packaged";
				}
				return true;
			}
			return false;
		}

		private bool GetWarning(out string warning)
		{
			warning = string.Empty;
			if (Mode == EMode.Contract)
			{
				if (CurrentContract != null)
				{
					if (CurrentContract.GetProductListMatch(GetCustomerItems(), out var _) < 1f)
					{
						warning = "Customer expectations not met";
						return true;
					}
					if (GetCustomerItemsCount(onlyPackagedProduct: false) > CurrentContract.ProductList.GetTotalQuantity())
					{
						warning = "You are providing more items than required.";
						return true;
					}
				}
			}
			else if (Mode == EMode.Sample && GetCustomerItemsCount(onlyPackagedProduct: false) > 1)
			{
				warning = "Only 1 sample product is required.";
				return true;
			}
			bool flag = false;
			for (int i = 0; i < CustomerSlots.Length; i++)
			{
				if (CustomerSlots[i].ItemInstance != null && CustomerSlots[i].ItemInstance is ProductItemInstance && (CustomerSlots[i].ItemInstance as ProductItemInstance).AppliedPackaging == null)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				warning = "Product must be packaged";
				return true;
			}
			return false;
		}

		private List<ItemInstance> GetCustomerItems(bool onlyPackagedProduct = true)
		{
			List<ItemInstance> list = new List<ItemInstance>();
			for (int i = 0; i < CustomerSlots.Length; i++)
			{
				if (CustomerSlots[i].ItemInstance != null && (!onlyPackagedProduct || (CustomerSlots[i].ItemInstance is ProductItemInstance productItemInstance && !(productItemInstance.AppliedPackaging == null))))
				{
					list.Add(CustomerSlots[i].ItemInstance);
				}
			}
			return list;
		}

		private float GetCustomerItemsValue()
		{
			float num = 0f;
			foreach (ItemInstance customerItem in GetCustomerItems())
			{
				if (customerItem is ProductItemInstance)
				{
					ProductItemInstance productItemInstance = customerItem as ProductItemInstance;
					num += (productItemInstance.Definition as ProductDefinition).MarketValue * (float)productItemInstance.Quantity * (float)productItemInstance.Amount;
				}
			}
			return num;
		}

		private int GetCustomerItemsCount(bool onlyPackagedProduct = true)
		{
			int num = 0;
			for (int i = 0; i < CustomerSlots.Length; i++)
			{
				if (CustomerSlots[i].ItemInstance == null)
				{
					continue;
				}
				ProductItemInstance productItemInstance = CustomerSlots[i].ItemInstance as ProductItemInstance;
				if (!onlyPackagedProduct || (productItemInstance != null && !(productItemInstance.AppliedPackaging == null)))
				{
					int num2 = 1;
					if (productItemInstance != null)
					{
						num2 = productItemInstance.Amount;
					}
					num += CustomerSlots[i].ItemInstance.Quantity * num2;
				}
			}
			return num;
		}
	}
}
