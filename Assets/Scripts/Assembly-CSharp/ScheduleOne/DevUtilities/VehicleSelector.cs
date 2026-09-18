using System;
using System.Collections.Generic;
using ScheduleOne.EntityFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class VehicleSelector : Singleton<VehicleSelector>
	{
		[Header("Settings")]
		[SerializeField]
		protected float detectionRange = 5f;

		[SerializeField]
		protected LayerMask detectionMask;

		private List<LandVehicle> selectedVehicles = new List<LandVehicle>();

		public Action onClose;

		private int selectionLimit;

		private bool exitOnSelectionLimit;

		private LandVehicle hoveredVehicle;

		private List<LandVehicle> outlinedVehicles = new List<LandVehicle>();

		private Func<LandVehicle, bool> vehicleFilter;

		public bool isSelecting { get; protected set; }

		protected override void Start()
		{
			base.Start();
			GameInput.RegisterExitListener(Exit, 8);
		}

		protected virtual void Update()
		{
			if (!isSelecting)
			{
				return;
			}
			hoveredVehicle = GetHoveredVehicle();
			if (hoveredVehicle != null)
			{
				Singleton<HUD>.Instance.ShowRadialIndicator(1f);
			}
			if (!GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) || !(hoveredVehicle != null) || (vehicleFilter != null && !vehicleFilter(hoveredVehicle)))
			{
				return;
			}
			if (selectedVehicles.Contains(hoveredVehicle))
			{
				Console.Log("Deselected: " + hoveredVehicle.VehicleName);
				selectedVehicles.Remove(hoveredVehicle);
			}
			else if (selectedVehicles.Count < selectionLimit)
			{
				selectedVehicles.Add(hoveredVehicle);
				if (selectedVehicles.Count >= selectionLimit && exitOnSelectionLimit)
				{
					StopSelecting();
				}
			}
		}

		protected virtual void LateUpdate()
		{
			if (!isSelecting)
			{
				return;
			}
			for (int i = 0; i < outlinedVehicles.Count; i++)
			{
				outlinedVehicles[i].HideOutline();
			}
			outlinedVehicles.Clear();
			for (int j = 0; j < selectedVehicles.Count; j++)
			{
				selectedVehicles[j].ShowOutline(BuildableItem.EOutlineColor.Blue);
				outlinedVehicles.Add(selectedVehicles[j]);
			}
			if (hoveredVehicle != null)
			{
				if (selectedVehicles.Contains(hoveredVehicle))
				{
					hoveredVehicle.ShowOutline(BuildableItem.EOutlineColor.LightBlue);
					return;
				}
				hoveredVehicle.ShowOutline(BuildableItem.EOutlineColor.White);
				outlinedVehicles.Add(hoveredVehicle);
			}
		}

		private LandVehicle GetHoveredVehicle()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(detectionRange, out var hit, detectionMask, includeTriggers: false, 0.1f))
			{
				LandVehicle componentInParent = hit.collider.GetComponentInParent<LandVehicle>();
				if (componentInParent != null && (vehicleFilter == null || vehicleFilter(componentInParent)))
				{
					return componentInParent;
				}
			}
			return null;
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && action.exitType == ExitType.Escape && isSelecting)
			{
				action.used = true;
				StopSelecting();
			}
		}

		public void StartSelecting(string selectionTitle, ref List<LandVehicle> initialSelection, int _selectionLimit, bool _exitOnSelectionLimit, Func<LandVehicle, bool> filter = null)
		{
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			selectedVehicles = initialSelection;
			for (int i = 0; i < selectedVehicles.Count; i++)
			{
				selectedVehicles[i].ShowOutline(BuildableItem.EOutlineColor.White);
				outlinedVehicles.Add(selectedVehicles[i]);
			}
			selectionLimit = _selectionLimit;
			vehicleFilter = filter;
			Singleton<HUD>.Instance.ShowTopScreenText(selectionTitle);
			isSelecting = true;
			exitOnSelectionLimit = _exitOnSelectionLimit;
		}

		public void StopSelecting()
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			vehicleFilter = null;
			for (int i = 0; i < outlinedVehicles.Count; i++)
			{
				outlinedVehicles[i].HideOutline();
			}
			outlinedVehicles.Clear();
			if (onClose != null)
			{
				onClose();
			}
			Singleton<HUD>.Instance.HideTopScreenText();
			isSelecting = false;
		}
	}
}
