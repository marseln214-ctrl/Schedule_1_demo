using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class LabOvenSolidTask : Task
	{
		public enum EStep
		{
			OpenDoor = 0,
			PlaceItems = 1,
			CloseDoor = 2,
			PressButton = 3
		}

		private ItemInstance ingredient;

		private int ingredientQuantity = 1;

		private StationItem[] stationItems;

		private Draggable[] stationDraggables;

		public LabOven Oven { get; private set; }

		public EStep CurrentStep { get; protected set; }

		public LabOvenSolidTask(LabOven oven)
		{
			Oven = oven;
			ingredientQuantity = Mathf.Min(Oven.IngredientSlot.Quantity, 10);
			stationItems = oven.CreateStationItems(ingredientQuantity);
			stationDraggables = new Draggable[stationItems.Length];
			for (int i = 0; i < stationItems.Length; i++)
			{
				stationDraggables[i] = stationItems[i].GetComponentInChildren<Draggable>();
			}
			ingredient = Oven.IngredientSlot.ItemInstance.GetCopy(ingredientQuantity);
			Oven.IngredientSlot.ChangeQuantity(-ingredientQuantity);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Oven.CameraPosition_PlaceItems.position, Oven.CameraPosition_PlaceItems.rotation, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(65f, 0.2f);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			EnableMultiDragging(oven.ItemContainer, 0.12f);
			oven.Door.SetInteractable(interactable: true);
		}

		public override void Update()
		{
			base.Update();
			CheckProgress();
			base.CurrentInstruction = GetStepInstruction(CurrentStep);
		}

		public override void Success()
		{
			string iD = (ingredient.Definition as StorableItemDefinition).StationItem.GetModule<CookableModule>().Product.ID;
			EQuality ingredientQuality = EQuality.Standard;
			if (ingredient is QualityItemInstance)
			{
				ingredientQuality = (ingredient as QualityItemInstance).Quality;
			}
			Oven.SendCookOperation(new OvenCookOperation(ingredient.ID, ingredientQuality, ingredientQuantity, iD));
			base.Success();
		}

		public override void StopTask()
		{
			base.StopTask();
			if (Outcome != EOutcome.Success)
			{
				Oven.IngredientSlot.AddItem(ingredient);
				Oven.LiquidMesh.gameObject.SetActive(value: false);
			}
			for (int i = 0; i < stationItems.Length; i++)
			{
				stationItems[i].Destroy();
			}
			Oven.ClearDecals();
			Oven.Door.SetPosition(0f);
			Oven.Door.SetInteractable(interactable: false);
			Oven.WireTray.SetPosition(0f);
			Oven.Button.SetInteractable(interactable: false);
			Singleton<LabOvenCanvas>.Instance.SetIsOpen(Oven, open: true);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Oven.CameraPosition_Default.position, Oven.CameraPosition_Default.rotation, 0.2f);
		}

		private void CheckProgress()
		{
			switch (CurrentStep)
			{
			case EStep.OpenDoor:
				CheckStep_OpenDoor();
				break;
			case EStep.PlaceItems:
				CheckStep_PlaceItems();
				break;
			case EStep.CloseDoor:
				CheckStep_CloseDoor();
				break;
			case EStep.PressButton:
				CheckStep_PressButton();
				break;
			}
		}

		private void ProgressStep()
		{
			if (CurrentStep == EStep.PressButton)
			{
				Success();
				return;
			}
			CurrentStep++;
			if (CurrentStep == EStep.PlaceItems)
			{
				Oven.WireTray.SetPosition(1f);
			}
			if (CurrentStep == EStep.CloseDoor)
			{
				Oven.Door.SetInteractable(interactable: true);
				for (int i = 0; i < stationDraggables.Length; i++)
				{
					stationDraggables[i].ClickableEnabled = false;
					Object.Destroy(stationDraggables[i].Rb);
					stationItems[i].transform.SetParent(Oven.SquareTray);
				}
			}
			if (CurrentStep == EStep.PressButton)
			{
				Oven.Button.SetInteractable(interactable: true);
			}
		}

		private void CheckStep_OpenDoor()
		{
			if (Oven.Door.TargetPosition > 0.9f)
			{
				ProgressStep();
				Oven.Door.SetInteractable(interactable: false);
				Oven.Door.SetPosition(1f);
			}
		}

		private void CheckStep_PlaceItems()
		{
			for (int i = 0; i < stationDraggables.Length; i++)
			{
				if (stationDraggables[i].IsHeld || stationDraggables[i].Rb.velocity.magnitude > 0.02f || !Oven.TrayDetectionArea.bounds.Contains(stationDraggables[i].transform.position))
				{
					return;
				}
			}
			ProgressStep();
		}

		private void CheckStep_CloseDoor()
		{
			if (Oven.Door.TargetPosition < 0.05f)
			{
				ProgressStep();
				Oven.Door.SetInteractable(interactable: false);
				Oven.Door.SetPosition(0f);
			}
		}

		private void CheckStep_PressButton()
		{
			if (Oven.Button.Pressed)
			{
				ProgressStep();
			}
		}

		public static string GetStepInstruction(EStep step)
		{
			return step switch
			{
				EStep.OpenDoor => "Open oven door", 
				EStep.PlaceItems => "Place items onto tray", 
				EStep.CloseDoor => "Close oven door", 
				EStep.PressButton => "Start oven", 
				_ => string.Empty, 
			};
		}
	}
}
