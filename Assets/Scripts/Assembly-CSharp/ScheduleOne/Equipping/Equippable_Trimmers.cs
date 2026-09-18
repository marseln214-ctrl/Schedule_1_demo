using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;

namespace ScheduleOne.Equipping
{
	public class Equippable_Trimmers : Equippable_Viewmodel
	{
		public bool CanClickAndDrag;

		public AudioSourceController SoundLoopPrefab;

		protected override void Update()
		{
			base.Update();
			if (Singleton<TaskManager>.Instance.currentTask != null || PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0 || !PlayerSingleton<PlayerCamera>.Instance.LookRaycast(5f, out var hit, Singleton<InteractionManager>.Instance.Interaction_SearchMask))
			{
				return;
			}
			Pot componentInParent = hit.collider.GetComponentInParent<Pot>();
			if (!(componentInParent != null))
			{
				return;
			}
			if (componentInParent.IsReadyForHarvest(out var reason))
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
				componentInParent.ConfigureInteraction("Harvest", InteractableObject.EInteractableState.Default, useHighLabelPos: true);
				if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
				{
					new HarvestPlant(componentInParent, CanClickAndDrag, SoundLoopPrefab);
				}
			}
			else
			{
				componentInParent.ConfigureInteraction(reason, InteractableObject.EInteractableState.Invalid);
			}
		}
	}
}
