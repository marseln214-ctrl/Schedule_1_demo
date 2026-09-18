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
	public class BrickPressCanvas : Singleton<BrickPressCanvas>
	{
		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public ItemSlotUI[] ProductSlotUIs;

		public ItemSlotUI OutputSlotUI;

		public TextMeshProUGUI InstructionLabel;

		public Button BeginButton;

		public bool isOpen { get; protected set; }

		public BrickPress Press { get; protected set; }

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
			if (BeginButton.interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
			{
				BeginButtonPressed();
				return;
			}
			switch (Press.GetState())
			{
			case PackagingStation.EState.CanBegin:
				InstructionLabel.enabled = false;
				BeginButton.interactable = true;
				return;
			case PackagingStation.EState.InsufficentProduct:
				InstructionLabel.text = "Drag 20x product into input slots";
				break;
			case PackagingStation.EState.OutputSlotFull:
				InstructionLabel.text = "Output slot is full!";
				break;
			case PackagingStation.EState.Mismatch:
				InstructionLabel.text = "Output slot is full!";
				break;
			}
			InstructionLabel.enabled = true;
			BeginButton.interactable = false;
		}

		public void SetIsOpen(BrickPress press, bool open, bool removeUI = true)
		{
			isOpen = open;
			Canvas.enabled = open;
			Container.gameObject.SetActive(open);
			Press = press;
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				if (open)
				{
					PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
				}
			}
			if (press != null)
			{
				for (int i = 0; i < ProductSlotUIs.Length; i++)
				{
					ProductSlotUIs[i].AssignSlot(press.InputSlots[i]);
				}
				OutputSlotUI.AssignSlot(press.OutputSlot);
			}
			else
			{
				for (int j = 0; j < ProductSlotUIs.Length; j++)
				{
					ProductSlotUIs[j].ClearSlot();
				}
				OutputSlotUI.ClearSlot();
			}
			if (open)
			{
				Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
				Update();
			}
			else if (removeUI)
			{
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			}
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(open);
			if (open)
			{
				List<ItemSlot> list = new List<ItemSlot>();
				list.AddRange(press.InputSlots);
				list.Add(press.OutputSlot);
				Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), list);
			}
			if (isOpen)
			{
				Singleton<CompassManager>.Instance.SetVisible(visible: false);
			}
		}

		public void BeginButtonPressed()
		{
			if (Press.GetState() == PackagingStation.EState.CanBegin && Press.HasSufficientProduct(out var product))
			{
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
				new UseBrickPress(Press, product);
				SetIsOpen(null, open: false, removeUI: false);
			}
		}
	}
}
