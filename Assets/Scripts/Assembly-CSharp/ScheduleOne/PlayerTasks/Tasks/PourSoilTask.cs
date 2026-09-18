using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.ObjectScripts.Soil;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.PlayerTasks.Tasks
{
	public class PourSoilTask : PourIntoPotTask
	{
		private PourableSoil soil;

		private Collider HoveredTopCollider;

		public PourSoilTask(Pot _pot, ItemInstance _itemInstance, Pourable _pourablePrefab)
			: base(_pot, _itemInstance, _pourablePrefab)
		{
			base.CurrentInstruction = "Click and drag to cut soil bag";
			soil = pourable as PourableSoil;
			soil.onOpened.AddListener(base.RemoveItem);
		}

		public override void Update()
		{
			base.Update();
			if (soil.IsOpen)
			{
				base.CurrentInstruction = "Pour soil into pot (" + Mathf.FloorToInt(pot.SoilLevel / pot.SoilCapacity * 100f) + "%)";
			}
			UpdateHover();
			UpdateCursor();
			if (HoveredTopCollider != null && GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && soil.TopColliders.IndexOf(HoveredTopCollider) == soil.currentCut)
			{
				soil.Cut();
			}
		}

		public override void StopTask()
		{
			pot.PushSoilDataToServer();
			base.StopTask();
		}

		protected override void UpdateCursor()
		{
			if (soil.IsOpen)
			{
				base.UpdateCursor();
			}
			else if (HoveredTopCollider != null && soil.TopColliders.IndexOf(HoveredTopCollider) == soil.currentCut)
			{
				Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Scissors);
			}
			else
			{
				Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
			}
		}

		private void UpdateHover()
		{
			HoveredTopCollider = GetHoveredTopCollider();
		}

		private Collider GetHoveredTopCollider()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(3f, out var hit, Singleton<InteractionManager>.Instance.Interaction_SearchMask) && soil.TopColliders.Contains(hit.collider))
			{
				return hit.collider;
			}
			return null;
		}
	}
}
