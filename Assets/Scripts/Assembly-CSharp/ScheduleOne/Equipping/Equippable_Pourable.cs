using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.PlayerTasks.Tasks;
using UnityEngine;

namespace ScheduleOne.Equipping
{
	public class Equippable_Pourable : Equippable_Viewmodel
	{
		[Header("Pourable settings")]
		public float InteractionRange = 2.5f;

		public Pourable PourablePrefab;

		public virtual string InteractionLabel { get; set; } = "Pour";


		protected override void Update()
		{
			base.Update();
			if (Singleton<TaskManager>.Instance.currentTask != null || PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0 || !PlayerSingleton<PlayerCamera>.Instance.LookRaycast(InteractionRange, out var hit, Singleton<InteractionManager>.Instance.Interaction_SearchMask))
			{
				return;
			}
			Pot componentInParent = hit.collider.GetComponentInParent<Pot>();
			if (componentInParent == null)
			{
				return;
			}
			string reason = string.Empty;
			if (CanPour(componentInParent, out reason))
			{
				if (componentInParent.PlayerUserObject != null)
				{
					componentInParent.ConfigureInteraction("In use by other player", InteractableObject.EInteractableState.Invalid);
					return;
				}
				if (componentInParent.NPCUserObject != null)
				{
					componentInParent.ConfigureInteraction("In use by workers", InteractableObject.EInteractableState.Invalid);
					return;
				}
				componentInParent.ConfigureInteraction(InteractionLabel, InteractableObject.EInteractableState.Default);
				if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
				{
					StartPourTask(componentInParent);
				}
			}
			else if (reason != string.Empty)
			{
				componentInParent.ConfigureInteraction(reason, InteractableObject.EInteractableState.Invalid);
			}
			else
			{
				componentInParent.ConfigureInteraction(string.Empty, InteractableObject.EInteractableState.Disabled);
			}
		}

		protected virtual void StartPourTask(Pot pot)
		{
			new PourIntoPotTask(pot, itemInstance, PourablePrefab);
		}

		protected virtual bool CanPour(Pot pot, out string reason)
		{
			reason = string.Empty;
			return true;
		}
	}
}
