using System;
using System.Collections.Generic;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.Construction;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Construction
{
	public class ConstructionMenu : Singleton<ConstructionMenu>
	{
		[Serializable]
		public class ConstructionMenuCategory
		{
			public string categoryName = "Category";

			public Sprite categoryIcon;

			[HideInInspector]
			public Button button;

			[HideInInspector]
			public RectTransform container;

			[HideInInspector]
			public List<ConstructionMenuListing> listings = new List<ConstructionMenuListing>();
		}

		public class ConstructionMenuListing
		{
			public string ID = string.Empty;

			public float price;

			public ConstructionMenuCategory category;

			public RectTransform entry;

			public bool isSelected;

			public ConstructionMenuListing(string id, float _price, ConstructionMenuCategory _cat)
			{
				ID = id;
				price = _price;
				category = _cat;
				category.listings.Add(this);
				CreateUI();
			}

			private void CreateUI()
			{
				int num = category.listings.IndexOf(this);
				entry = UnityEngine.Object.Instantiate(Singleton<ConstructionMenu>.Instance.listingPrefab, category.container).GetComponent<RectTransform>();
				entry.anchoredPosition = new Vector2((0.5f + (float)num) * entry.sizeDelta.x, entry.anchoredPosition.y);
				entry.Find("Content/Icon").GetComponent<Image>().sprite = Registry.GetConstructable(ID).ConstructableIcon;
				entry.Find("Content/Name").GetComponent<Text>().text = Registry.GetConstructable(ID).ConstructableName;
				entry.Find("Content/Price").GetComponent<Text>().text = MoneyManager.FormatAmount(price);
				entry.GetComponent<Button>().onClick.AddListener(ListingClicked);
			}

			private void ListingClicked()
			{
				if (isSelected)
				{
					Singleton<ConstructionMenu>.Instance.ClearSelectedListing();
					return;
				}
				Singleton<ConstructionMenu>.Instance.ListingClicked(this);
				SetSelected(selected: true);
			}

			public void ListingUnselected()
			{
				SetSelected(selected: false);
			}

			public void SetSelected(bool selected)
			{
				isSelected = selected;
				if (selected)
				{
					entry.Find("Content/Outline").GetComponent<Image>().color = Singleton<ConstructionMenu>.Instance.listingOutlineColor_Selected;
					entry.Find("Content/Name").GetComponent<Text>().color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				}
				else
				{
					entry.Find("Content/Outline").GetComponent<Image>().color = Singleton<ConstructionMenu>.Instance.listingOutlineColor_Unselected;
					entry.Find("Content/Name").GetComponent<Text>().color = new Color32(50, 50, 50, byte.MaxValue);
				}
			}
		}

		public List<ConstructionMenuCategory> categories = new List<ConstructionMenuCategory>();

		[Header("References")]
		[SerializeField]
		protected Canvas canvas;

		[SerializeField]
		protected GraphicRaycaster raycaster;

		[SerializeField]
		protected Transform categoryButtonContainer;

		[SerializeField]
		protected RectTransform categoryContainer;

		[SerializeField]
		protected Text categoryNameDisplay;

		[SerializeField]
		protected RectTransform infoPopup;

		[SerializeField]
		protected TextMeshProUGUI infoPopup_ConstructableName;

		[SerializeField]
		protected EventSystem eventSystem;

		[SerializeField]
		protected Button destroyButton;

		[SerializeField]
		protected Button customizeButton;

		[SerializeField]
		protected Button moveButton;

		[SerializeField]
		protected TextMeshProUGUI infoPopup_Description;

		[Header("Prefabs")]
		[SerializeField]
		protected GameObject categoryButtonPrefab;

		[SerializeField]
		protected GameObject categoryContainerPrefab;

		public GameObject listingPrefab;

		[Header("Settings")]
		[SerializeField]
		protected Color iconColor_Unselected;

		[SerializeField]
		protected Color iconColor_Selected;

		public Color listingOutlineColor_Unselected;

		public Color listingOutlineColor_Selected;

		private ConstructionMenuCategory selectedCategory;

		private ConstructionMenuListing selectedListing;

		private Constructable selectedConstructable;

		public bool isOpen { get; protected set; }

		public Constructable SelectedConstructable => selectedConstructable;

		protected override void Start()
		{
			base.Start();
			SetIsOpen(open: false);
			ConstructionManager constructionManager = Singleton<ConstructionManager>.Instance;
			constructionManager.onConstructionModeEnabled = (Action)Delegate.Combine(constructionManager.onConstructionModeEnabled, (Action)delegate
			{
				SetIsOpen(open: true);
			});
			ConstructionManager constructionManager2 = Singleton<ConstructionManager>.Instance;
			constructionManager2.onConstructionModeDisabled = (Action)Delegate.Combine(constructionManager2.onConstructionModeDisabled, (Action)delegate
			{
				SetIsOpen(open: false);
			});
			ConstructionManager constructionManager3 = Singleton<ConstructionManager>.Instance;
			constructionManager3.onNewConstructableBuilt = (ConstructionManager.ConstructableNotification)Delegate.Combine(constructionManager3.onNewConstructableBuilt, new ConstructionManager.ConstructableNotification(OnConstructableBuilt));
			ConstructionManager constructionManager4 = Singleton<ConstructionManager>.Instance;
			constructionManager4.onConstructableMoved = (ConstructionManager.ConstructableNotification)Delegate.Combine(constructionManager4.onConstructableMoved, new ConstructionManager.ConstructableNotification(SelectConstructable));
			GenerateCategories();
			SelectCategory(categories[0].categoryName);
			SetupListings();
			GameInput.RegisterExitListener(Exit, -1);
		}

		private void Exit(ExitAction exit)
		{
			if (!exit.used && selectedConstructable != null)
			{
				exit.used = true;
				DeselectConstructable();
			}
		}

		protected virtual void Update()
		{
			if (isOpen)
			{
				CheckConstructableSelection();
			}
		}

		private void SetupListings()
		{
			AddListing("small_shed", 2500f, "Multipurpose");
		}

		private void AddListing(string ID, float price, string category)
		{
			if (Registry.GetConstructable(ID) == null)
			{
				Console.LogWarning("ID not valid!");
				return;
			}
			ConstructionMenuCategory constructionMenuCategory = categories.Find((ConstructionMenuCategory x) => x.categoryName.ToLower() == category.ToLower());
			if (constructionMenuCategory == null)
			{
				Console.LogWarning("Category not found!");
			}
			else
			{
				new ConstructionMenuListing(ID, price, constructionMenuCategory);
			}
		}

		private void SetIsOpen(bool open)
		{
			if (open)
			{
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			}
			else
			{
				DeselectConstructable();
				if (PlayerSingleton<PlayerCamera>.InstanceExists)
				{
					PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				}
			}
			isOpen = open;
			canvas.enabled = open;
		}

		private void OnConstructableBuilt(Constructable c)
		{
		}

		public void ClearSelectedListing()
		{
			if (selectedListing != null)
			{
				selectedListing.ListingUnselected();
				selectedListing = null;
				Singleton<ConstructionManager>.Instance.StopConstructableDeploy();
			}
		}

		public void ListingClicked(ConstructionMenuListing listing)
		{
			ClearSelectedListing();
			DeselectConstructable();
			Singleton<ConstructionManager>.Instance.DeployConstructable(listing);
			selectedListing = listing;
		}

		public bool IsHoveringUI()
		{
			List<RaycastResult> list = new List<RaycastResult>();
			PointerEventData pointerEventData = new PointerEventData(eventSystem);
			pointerEventData.position = UnityEngine.Input.mousePosition;
			raycaster.Raycast(pointerEventData, list);
			return list.Count > 0;
		}

		public void MoveButtonPressed()
		{
			if (selectedConstructable != null && selectedConstructable is Constructable_GridBased)
			{
				Singleton<ConstructionManager>.Instance.MoveConstructable(selectedConstructable as Constructable_GridBased);
				DeselectConstructable();
			}
		}

		public void CustomizeButtonPressed()
		{
		}

		public void BulldozeButtonPressed()
		{
			if (selectedConstructable != null)
			{
				Constructable constructable = selectedConstructable;
				if (!selectedConstructable.CanBeDestroyed())
				{
					Console.Log("Can't be destroyed!");
					return;
				}
				DeselectConstructable();
				constructable.DestroyConstructable();
			}
		}

		private void CheckConstructableSelection()
		{
			if (IsHoveringUI() || Singleton<ConstructionManager>.Instance.isDeployingConstructable || !GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
			{
				return;
			}
			Constructable hoveredConstructable = GetHoveredConstructable();
			if (hoveredConstructable != null)
			{
				if (selectedConstructable == hoveredConstructable)
				{
					DeselectConstructable();
				}
				else
				{
					SelectConstructable(hoveredConstructable);
				}
			}
			else if (selectedConstructable != null)
			{
				DeselectConstructable();
			}
		}

		public void SelectConstructable(Constructable c)
		{
			SelectConstructable(c, focusCameraTo: true);
		}

		public void SelectConstructable(Constructable c, bool focusCameraTo)
		{
			if (c.CanBeSelected())
			{
				if (focusCameraTo)
				{
					selectedConstructable = c;
				}
				Singleton<BirdsEyeView>.Instance.SlideCameraOrigin(c.GetCosmeticCenter(), c.GetBoundingBoxLongestSide() * 1.75f);
				infoPopup_ConstructableName.text = c.ConstructableName;
				infoPopup_Description.text = c.ConstructableDescription;
				List<Button> list = new List<Button>();
				if (c is Constructable_GridBased)
				{
					SetButtonInteractable(moveButton, interactable: true, iconColor_Unselected);
					list.Add(moveButton);
				}
				else
				{
					moveButton.gameObject.SetActive(value: false);
				}
				customizeButton.gameObject.SetActive(value: false);
				string reason = string.Empty;
				list.Add(destroyButton);
				if (c.CanBeDestroyed(out reason))
				{
					destroyButton.GetComponent<Tooltip>().text = "Bulldoze";
					SetButtonInteractable(destroyButton, interactable: true, new Color32(byte.MaxValue, 110, 80, byte.MaxValue));
				}
				else
				{
					destroyButton.GetComponent<Tooltip>().text = "Cannot bulldoze (" + reason + ")";
					SetButtonInteractable(destroyButton, interactable: false, new Color32(byte.MaxValue, 110, 80, byte.MaxValue));
				}
				for (int i = 0; i < list.Count; i++)
				{
					list[i].GetComponent<RectTransform>().anchoredPosition = new Vector2((float)(-list.Count) * 50f * 0.5f + 50f * ((float)i + 0.5f), -25f);
					list[i].gameObject.SetActive(value: true);
				}
				if (Singleton<FeaturesManager>.Instance.activeConstructable != selectedConstructable)
				{
					Singleton<FeaturesManager>.Instance.Activate(selectedConstructable);
				}
				infoPopup.gameObject.SetActive(value: true);
			}
		}

		private void SetButtonInteractable(Button b, bool interactable, Color32 iconDefaultColor)
		{
			b.interactable = interactable;
			if (interactable)
			{
				b.transform.Find("Outline/Background/Icon").GetComponent<Image>().color = iconDefaultColor;
			}
			else
			{
				b.transform.Find("Outline/Background/Icon").GetComponent<Image>().color = new Color32(200, 200, 200, byte.MaxValue);
			}
		}

		public void DeselectConstructable()
		{
			selectedConstructable = null;
			infoPopup.gameObject.SetActive(value: false);
			if (Singleton<FeaturesManager>.Instance.isActive)
			{
				Singleton<FeaturesManager>.Instance.Deactivate();
			}
		}

		private Constructable GetHoveredConstructable()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(1000f, out var hit, 1 << LayerMask.NameToLayer("Default")))
			{
				return hit.collider.GetComponentInParent<Constructable>();
			}
			return null;
		}

		private void GenerateCategories()
		{
			for (int i = 0; i < categories.Count; i++)
			{
				Button component = UnityEngine.Object.Instantiate(categoryButtonPrefab, categoryButtonContainer).GetComponent<Button>();
				component.GetComponent<RectTransform>().anchoredPosition = new Vector2((0.5f + (float)(i % 3)) * 50f, (0f - (0.5f + (float)(i / 3))) * 50f);
				component.transform.Find("Outline/Background/Icon").GetComponent<Image>().sprite = categories[i].categoryIcon;
				string catName = categories[i].categoryName;
				component.onClick.AddListener(delegate
				{
					SelectCategory(catName);
				});
				component.GetComponent<Tooltip>().text = categories[i].categoryName;
				categories[i].button = component;
				RectTransform component2 = UnityEngine.Object.Instantiate(categoryContainerPrefab, categoryContainer).GetComponent<RectTransform>();
				component2.name = categories[i].categoryName;
				component2.gameObject.SetActive(value: false);
				categories[i].container = component2;
			}
		}

		public void SelectCategory(string categoryName)
		{
			ClearSelectedListing();
			ConstructionMenuCategory constructionMenuCategory = categories.Find((ConstructionMenuCategory x) => x.categoryName.ToLower() == categoryName.ToLower());
			if (selectedCategory != null)
			{
				selectedCategory.button.transform.Find("Outline/Background/Icon").GetComponent<Image>().color = iconColor_Unselected;
				selectedCategory.button.interactable = true;
				selectedCategory.container.gameObject.SetActive(value: false);
			}
			constructionMenuCategory.button.interactable = false;
			constructionMenuCategory.button.transform.Find("Outline/Background/Icon").GetComponent<Image>().color = iconColor_Selected;
			constructionMenuCategory.container.gameObject.SetActive(value: true);
			categoryNameDisplay.text = constructionMenuCategory.categoryName;
			selectedCategory = constructionMenuCategory;
		}

		public float GetListingPrice(string id)
		{
			for (int i = 0; i < categories.Count; i++)
			{
				for (int j = 0; j < categories[i].listings.Count; j++)
				{
					if (categories[i].listings[j].ID == id)
					{
						return categories[i].listings[j].price;
					}
				}
			}
			Console.LogWarning("Failed to get listing price for ID: " + id);
			return 0f;
		}
	}
}
