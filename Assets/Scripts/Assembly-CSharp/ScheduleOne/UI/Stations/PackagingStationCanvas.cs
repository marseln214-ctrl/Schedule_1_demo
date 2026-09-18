using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product.Packaging;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations
{
	public class PackagingStationCanvas : Singleton<PackagingStationCanvas>
	{
		public bool ShowHintOnOpen;

		public bool ShowShiftClickHint;

		public PackagingStation.EMode CurrentMode;

		public Color InstructionWarningColor;

		[Header("References")]
		public Canvas Canvas;

		public GameObject Container;

		public ItemSlotUI PackagingSlotUI;

		public ItemSlotUI ProductSlotUI;

		public ItemSlotUI OutputSlotUI;

		public TextMeshProUGUI InstructionLabel;

		public Image InstructionShadow;

		public Button BeginButton;

		public Animation ModeAnimation;

		public TextMeshProUGUI ButtonLabel;

		public bool isOpen { get; protected set; }

		public PackagingStation PackagingStation { get; protected set; }

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
			if (CurrentMode == PackagingStation.EMode.Package)
			{
				ButtonLabel.text = "PACK";
			}
			else
			{
				ButtonLabel.text = "UNPACK";
			}
			if (BeginButton.interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
			{
				BeginButtonPressed();
				return;
			}
			PackagingStation.EState state = PackagingStation.GetState(CurrentMode);
			if (state == PackagingStation.EState.CanBegin)
			{
				InstructionLabel.enabled = false;
				InstructionShadow.enabled = false;
				BeginButton.interactable = true;
				return;
			}
			if (CurrentMode == PackagingStation.EMode.Package)
			{
				switch (state)
				{
				case PackagingStation.EState.MissingItems:
					InstructionLabel.text = "Drag product + packaging into slots";
					InstructionLabel.color = Color.white;
					break;
				case PackagingStation.EState.InsufficentProduct:
					InstructionLabel.text = "This packaging type requires <color=#FFC73D>" + (PackagingStation.PackagingSlot.ItemInstance.Definition as PackagingDefinition).Quantity + "x</color> product";
					InstructionLabel.color = Color.white;
					break;
				case PackagingStation.EState.OutputSlotFull:
					InstructionLabel.text = "Output slot is full!";
					InstructionLabel.color = InstructionWarningColor;
					break;
				case PackagingStation.EState.Mismatch:
					InstructionLabel.text = "Output slot is full!";
					InstructionLabel.color = InstructionWarningColor;
					break;
				}
			}
			else
			{
				switch (state)
				{
				case PackagingStation.EState.MissingItems:
					InstructionLabel.text = "Drag packaged product into output";
					InstructionLabel.color = Color.white;
					break;
				case PackagingStation.EState.PackageSlotFull:
					InstructionLabel.text = "Unpackaged items won't fit!";
					InstructionLabel.color = InstructionWarningColor;
					break;
				case PackagingStation.EState.ProductSlotFull:
					InstructionLabel.text = "Unpackaged items won't fit!";
					InstructionLabel.color = InstructionWarningColor;
					break;
				}
			}
			InstructionLabel.enabled = true;
			InstructionShadow.enabled = true;
			BeginButton.interactable = false;
		}

		public void SetIsOpen(PackagingStation station, bool open, bool removeUI = true)
		{
			isOpen = open;
			Canvas.enabled = open;
			Container.gameObject.SetActive(open);
			PackagingStation = station;
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				if (open)
				{
					PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
				}
			}
			if (station != null)
			{
				PackagingSlotUI.AssignSlot(station.PackagingSlot);
				ProductSlotUI.AssignSlot(station.ProductSlot);
				OutputSlotUI.AssignSlot(station.OutputSlot);
			}
			else
			{
				PackagingSlotUI.ClearSlot();
				ProductSlotUI.ClearSlot();
				OutputSlotUI.ClearSlot();
			}
			if (open)
			{
				Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
				if (ShowShiftClickHint && station.OutputSlot.Quantity > 0)
				{
					Singleton<HintDisplay>.Instance.ShowHint_20s("<Input_QuickMove><h1> + click</h> an item to quickly move it");
				}
			}
			else if (removeUI)
			{
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			}
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(open);
			if (open)
			{
				if (CurrentMode == PackagingStation.EMode.Package)
				{
					Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), new List<ItemSlot> { station.ProductSlot, station.PackagingSlot, station.OutputSlot });
				}
				else
				{
					Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), new List<ItemSlot> { station.OutputSlot, station.PackagingSlot, station.ProductSlot });
				}
			}
			if (isOpen)
			{
				Singleton<CompassManager>.Instance.SetVisible(visible: false);
			}
		}

		public void BeginButtonPressed()
		{
			if (PackagingStation == null || PackagingStation.GetState(CurrentMode) != 0)
			{
				return;
			}
			if (CurrentMode == PackagingStation.EMode.Unpackage)
			{
				PackagingStation.Unpack();
				Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
				return;
			}
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PackagingStation.StartTask();
			if (ShowHintOnOpen)
			{
				Singleton<HintDisplay>.Instance.ShowHint_20s("When performing tasks at stations, click and drag items to move them.");
			}
			SetIsOpen(null, open: false, removeUI: false);
		}

		private void UpdateSlotPositions()
		{
			if (PackagingStation != null)
			{
				PackagingSlotUI.Rect.position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(PackagingStation.PackagingSlotPosition.position);
				ProductSlotUI.Rect.position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(PackagingStation.ProductSlotPosition.position);
				OutputSlotUI.Rect.position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(PackagingStation.OutputSlotPosition.position);
			}
		}

		public void ToggleMode()
		{
			SetMode((CurrentMode == PackagingStation.EMode.Package) ? PackagingStation.EMode.Unpackage : PackagingStation.EMode.Package);
		}

		public void SetMode(PackagingStation.EMode mode)
		{
			CurrentMode = mode;
			if (mode == PackagingStation.EMode.Package)
			{
				ModeAnimation.Play("Packaging station switch to package");
			}
			else
			{
				ModeAnimation.Play("Packaging station switch to unpackage");
			}
			if (CurrentMode == PackagingStation.EMode.Package)
			{
				Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), new List<ItemSlot> { PackagingStation.ProductSlot, PackagingStation.PackagingSlot, PackagingStation.OutputSlot });
			}
			else
			{
				Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), new List<ItemSlot> { PackagingStation.OutputSlot, PackagingStation.PackagingSlot, PackagingStation.ProductSlot });
			}
		}
	}
}
