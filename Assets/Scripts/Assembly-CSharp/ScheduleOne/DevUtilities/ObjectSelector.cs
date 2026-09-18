using System;
using System.Collections.Generic;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.EntityFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class ObjectSelector : Singleton<ObjectSelector>
	{
		[Header("Settings")]
		[SerializeField]
		protected float detectionRange = 5f;

		[SerializeField]
		protected LayerMask detectionMask;

		private List<Type> allowedTypes;

		private List<BuildableItem> selectedObjects = new List<BuildableItem>();

		private List<Constructable> selectedConstructables = new List<Constructable>();

		public Action onClose;

		private int selectionLimit;

		private bool exitOnSelectionLimit;

		private BuildableItem hoveredBuildable;

		private Constructable hoveredConstructable;

		private List<BuildableItem> outlinedObjects = new List<BuildableItem>();

		private List<Constructable> outlinedConstructables = new List<Constructable>();

		public bool isSelecting { get; protected set; }

		protected override void Start()
		{
			base.Start();
			GameInput.RegisterExitListener(Exit, 3);
		}

		protected virtual void Update()
		{
			if (!isSelecting)
			{
				return;
			}
			hoveredBuildable = GetHoveredBuildable();
			hoveredConstructable = GetHoveredConstructable();
			if (hoveredBuildable != null || hoveredConstructable != null)
			{
				Singleton<HUD>.Instance.ShowRadialIndicator(1f);
			}
			if (!GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) || !PlayerSingleton<PlayerCamera>.Instance.LookRaycast(detectionRange, out var hit, detectionMask))
			{
				return;
			}
			if ((bool)hit.collider.GetComponentInParent<BuildableItem>())
			{
				BuildableItem componentInParent = hit.collider.GetComponentInParent<BuildableItem>();
				if (!allowedTypes.Contains(componentInParent.GetType()))
				{
					return;
				}
				if (selectedObjects.Contains(componentInParent))
				{
					Console.Log("Deselected: " + componentInParent.ItemInstance.Name);
					selectedObjects.Remove(componentInParent);
				}
				else if (selectedObjects.Count + selectedConstructables.Count < selectionLimit)
				{
					Console.Log("Selected: " + componentInParent.ItemInstance.Name);
					selectedObjects.Add(componentInParent);
					if (selectedObjects.Count + selectedConstructables.Count >= selectionLimit && exitOnSelectionLimit)
					{
						StopSelecting();
					}
				}
			}
			else
			{
				if (!hit.collider.GetComponentInParent<Constructable>())
				{
					return;
				}
				Constructable componentInParent2 = hit.collider.GetComponentInParent<Constructable>();
				if (!allowedTypes.Contains(componentInParent2.GetType()))
				{
					return;
				}
				if (selectedConstructables.Contains(componentInParent2))
				{
					Console.Log("Deselected: " + componentInParent2.ConstructableName);
					selectedConstructables.Remove(componentInParent2);
				}
				else if (selectedObjects.Count + selectedConstructables.Count < selectionLimit)
				{
					Console.Log("Selected: " + componentInParent2.ConstructableName);
					selectedConstructables.Add(componentInParent2);
					if (selectedObjects.Count + selectedConstructables.Count >= selectionLimit && exitOnSelectionLimit)
					{
						StopSelecting();
					}
				}
			}
		}

		protected virtual void LateUpdate()
		{
			if (!isSelecting)
			{
				return;
			}
			for (int i = 0; i < outlinedObjects.Count; i++)
			{
				outlinedObjects[i].HideOutline();
			}
			for (int j = 0; j < outlinedConstructables.Count; j++)
			{
				outlinedConstructables[j].HideOutline();
			}
			outlinedObjects.Clear();
			outlinedConstructables.Clear();
			for (int k = 0; k < selectedConstructables.Count; k++)
			{
				selectedConstructables[k].ShowOutline(BuildableItem.EOutlineColor.Blue);
				outlinedConstructables.Add(selectedConstructables[k]);
			}
			for (int l = 0; l < selectedObjects.Count; l++)
			{
				selectedObjects[l].ShowOutline(BuildableItem.EOutlineColor.Blue);
				outlinedObjects.Add(selectedObjects[l]);
			}
			if (hoveredBuildable != null)
			{
				if (selectedObjects.Contains(hoveredBuildable))
				{
					hoveredBuildable.ShowOutline(BuildableItem.EOutlineColor.LightBlue);
				}
				else
				{
					hoveredBuildable.ShowOutline(BuildableItem.EOutlineColor.White);
					outlinedObjects.Add(hoveredBuildable);
				}
			}
			if (hoveredConstructable != null)
			{
				if (selectedConstructables.Contains(hoveredConstructable))
				{
					hoveredConstructable.ShowOutline(BuildableItem.EOutlineColor.LightBlue);
					return;
				}
				hoveredConstructable.ShowOutline(BuildableItem.EOutlineColor.White);
				outlinedConstructables.Add(hoveredConstructable);
			}
		}

		private BuildableItem GetHoveredBuildable()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(detectionRange, out var hit, detectionMask, includeTriggers: false, 0.1f))
			{
				BuildableItem componentInParent = hit.collider.GetComponentInParent<BuildableItem>();
				if (componentInParent != null && allowedTypes.Contains(componentInParent.GetType()))
				{
					return componentInParent;
				}
			}
			return null;
		}

		private Constructable GetHoveredConstructable()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(detectionRange, out var hit, detectionMask, includeTriggers: false, 0.1f))
			{
				Constructable componentInParent = hit.collider.GetComponentInParent<Constructable>();
				if (componentInParent != null && allowedTypes.Contains(componentInParent.GetType()))
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

		public void StartSelecting(string selectionTitle, List<Type> _typeRestriction, ref List<BuildableItem> initialSelection_Objects, ref List<Constructable> initalSelection_Constructables, int _selectionLimit, bool _exitOnSelectionLimit)
		{
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			allowedTypes = _typeRestriction;
			selectedObjects = initialSelection_Objects;
			selectedConstructables = initalSelection_Constructables;
			for (int i = 0; i < selectedObjects.Count; i++)
			{
				selectedObjects[i].ShowOutline(BuildableItem.EOutlineColor.White);
				outlinedObjects.Add(selectedObjects[i]);
			}
			for (int j = 0; j < selectedConstructables.Count; j++)
			{
				selectedConstructables[j].ShowOutline(BuildableItem.EOutlineColor.White);
				outlinedConstructables.Add(selectedConstructables[j]);
			}
			selectionLimit = _selectionLimit;
			Singleton<HUD>.Instance.ShowTopScreenText(selectionTitle);
			isSelecting = true;
			exitOnSelectionLimit = _exitOnSelectionLimit;
		}

		public void StopSelecting()
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			allowedTypes = null;
			for (int i = 0; i < outlinedObjects.Count; i++)
			{
				outlinedObjects[i].HideOutline();
			}
			for (int j = 0; j < outlinedConstructables.Count; j++)
			{
				outlinedConstructables[j].HideOutline();
			}
			outlinedObjects.Clear();
			outlinedConstructables.Clear();
			if (onClose != null)
			{
				onClose();
			}
			Singleton<HUD>.Instance.HideTopScreenText();
			isSelecting = false;
		}
	}
}
