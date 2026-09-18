using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations
{
	public class CauldronCanvas : Singleton<CauldronCanvas>
	{
		[Header("References")]
		public Canvas Canvas;

		public GameObject Container;

		public List<ItemSlotUI> IngredientSlotUIs;

		public ItemSlotUI LiquidSlotUI;

		public ItemSlotUI OutputSlotUI;

		public TextMeshProUGUI InstructionLabel;

		public Button BeginButton;

		public bool isOpen { get; protected set; }

		public Cauldron Cauldron { get; protected set; }

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
			if (isOpen)
			{
				if (BeginButton.interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
				{
					BeginButtonPressed();
				}
				switch (Cauldron.GetState())
				{
				case Cauldron.EState.Ready:
					InstructionLabel.enabled = false;
					BeginButton.interactable = true;
					return;
				case Cauldron.EState.Cooking:
					InstructionLabel.text = "Cooking in progress...";
					break;
				case Cauldron.EState.MissingIngredients:
					InstructionLabel.text = "Insert <color=#FFC73D>" + 20 + "x</color> coca leaves and <color=#FFC73D>1x</color> gasoline";
					break;
				case Cauldron.EState.OutputFull:
					InstructionLabel.text = "Output is full";
					break;
				}
				InstructionLabel.enabled = true;
				BeginButton.interactable = false;
			}
		}

		public void SetIsOpen(Cauldron cauldron, bool open, bool removeUI = true)
		{
			isOpen = open;
			Canvas.enabled = open;
			Container.SetActive(open);
			Cauldron = cauldron;
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				if (open)
				{
					PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
				}
			}
			if (cauldron != null)
			{
				for (int i = 0; i < IngredientSlotUIs.Count; i++)
				{
					IngredientSlotUIs[i].AssignSlot(cauldron.IngredientSlots[i]);
				}
				LiquidSlotUI.AssignSlot(Cauldron.LiquidSlot);
				OutputSlotUI.AssignSlot(Cauldron.OutputSlot);
			}
			else
			{
				foreach (ItemSlotUI ingredientSlotUI in IngredientSlotUIs)
				{
					ingredientSlotUI.ClearSlot();
				}
				LiquidSlotUI.ClearSlot();
				OutputSlotUI.ClearSlot();
			}
			if (open)
			{
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
				List<ItemSlot> list = new List<ItemSlot>();
				list.AddRange(cauldron.IngredientSlots);
				list.Add(cauldron.LiquidSlot);
				list.Add(cauldron.OutputSlot);
				Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), list);
			}
			if (isOpen)
			{
				Update();
				Singleton<CompassManager>.Instance.SetVisible(visible: false);
			}
		}

		public void BeginButtonPressed()
		{
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			new CauldronTask(Cauldron);
			SetIsOpen(null, open: false, removeUI: false);
		}
	}
}
