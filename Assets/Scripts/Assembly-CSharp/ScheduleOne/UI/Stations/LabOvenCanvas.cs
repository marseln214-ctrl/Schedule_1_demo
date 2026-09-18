using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations
{
	public class LabOvenCanvas : Singleton<LabOvenCanvas>
	{
		[Header("References")]
		public Canvas Canvas;

		public GameObject Container;

		public ItemSlotUI IngredientSlotUI;

		public ItemSlotUI OutputSlotUI;

		public TextMeshProUGUI InstructionLabel;

		public TextMeshProUGUI ErrorLabel;

		public Button BeginButton;

		public RectTransform ProgressContainer;

		public Image IngredientIcon;

		public Image ProgressImg;

		public Image ProductIcon;

		public bool isOpen { get; protected set; }

		public LabOven Oven { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			BeginButton.onClick.AddListener(BeginButtonPressed);
		}

		protected override void Start()
		{
			base.Start();
			SetIsOpen(null, open: false);
		}

		protected virtual void Update()
		{
			if (!isOpen)
			{
				return;
			}
			BeginButton.interactable = CanBegin();
			if (BeginButton.interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
			{
				BeginButtonPressed();
				return;
			}
			if (Oven.CurrentOperation != null)
			{
				ProgressImg.fillAmount = Mathf.Clamp01((float)Oven.CurrentOperation.CookProgress / (float)Oven.CurrentOperation.GetCookDuration());
				if (Oven.CurrentOperation.CookProgress >= Oven.CurrentOperation.GetCookDuration())
				{
					if (DoesOvenOutputHaveSpace())
					{
						InstructionLabel.text = "Ready to collect product";
						InstructionLabel.enabled = true;
						ErrorLabel.enabled = false;
					}
					else
					{
						ErrorLabel.text = "Not enough space in output slot";
						ErrorLabel.enabled = true;
						InstructionLabel.enabled = false;
					}
				}
				else
				{
					InstructionLabel.text = "Cooking in progress...";
					InstructionLabel.enabled = true;
					ErrorLabel.enabled = false;
				}
				return;
			}
			ProgressContainer.gameObject.SetActive(value: false);
			if (Oven.IngredientSlot.ItemInstance != null)
			{
				if (Oven.IsIngredientCookable())
				{
					InstructionLabel.text = "Ready to begin cooking";
					InstructionLabel.enabled = true;
				}
				else
				{
					InstructionLabel.enabled = false;
					ErrorLabel.enabled = true;
					ErrorLabel.text = "Ingredient is not cookable";
				}
			}
			else
			{
				InstructionLabel.text = "Place cookable item in ingredient slot";
				InstructionLabel.enabled = true;
				ErrorLabel.enabled = false;
			}
		}

		public void SetIsOpen(LabOven oven, bool open, bool removeUI = true)
		{
			isOpen = open;
			Canvas.enabled = open;
			Container.gameObject.SetActive(open);
			Oven = oven;
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				if (open)
				{
					PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
				}
			}
			if (oven != null)
			{
				IngredientSlotUI.AssignSlot(oven.IngredientSlot);
				OutputSlotUI.AssignSlot(oven.OutputSlot);
			}
			else
			{
				IngredientSlotUI.ClearSlot();
				OutputSlotUI.ClearSlot();
			}
			if (open)
			{
				RefreshActiveOperation();
				Update();
				Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			}
			else if (removeUI)
			{
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			}
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(open);
			if (open)
			{
				Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), new List<ItemSlot> { Oven.IngredientSlot, Oven.OutputSlot });
			}
			if (isOpen)
			{
				Singleton<CompassManager>.Instance.SetVisible(visible: false);
			}
		}

		public void BeginButtonPressed()
		{
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			if (Oven.CurrentOperation != null)
			{
				new FinalizeLabOven(Oven);
			}
			else if ((Oven.IngredientSlot.ItemInstance.Definition as StorableItemDefinition).StationItem.GetModule<CookableModule>().CookType == CookableModule.ECookableType.Liquid)
			{
				new StartLabOvenTask(Oven);
			}
			else
			{
				new LabOvenSolidTask(Oven);
			}
			SetIsOpen(null, open: false, removeUI: false);
		}

		public bool CanBegin()
		{
			if (Oven == null)
			{
				return false;
			}
			if (Oven.CurrentOperation != null)
			{
				if (Oven.CurrentOperation.CookProgress >= Oven.CurrentOperation.GetCookDuration())
				{
					if (DoesOvenOutputHaveSpace())
					{
						return true;
					}
					return false;
				}
				return false;
			}
			return Oven.IsIngredientCookable();
		}

		private bool DoesOvenOutputHaveSpace()
		{
			return Oven.OutputSlot.GetCapacityForItem(Oven.CurrentOperation.Product.GetDefaultInstance()) >= Oven.CurrentOperation.Cookable.ProductQuantity;
		}

		private void RefreshActiveOperation()
		{
			if (Oven.CurrentOperation != null)
			{
				IngredientIcon.sprite = Oven.CurrentOperation.Ingredient.Icon;
				ProductIcon.sprite = Oven.CurrentOperation.Product.Icon;
			}
		}
	}
}
