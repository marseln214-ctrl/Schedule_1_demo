using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.Interaction;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;

namespace ScheduleOne.Equipping
{
	public class Equippable_Seed : Equippable_Viewmodel
	{
		public SeedDefinition Seed;

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
			if (componentInParent.CanAcceptSeed(out var reason))
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
				componentInParent.ConfigureInteraction("Sow seed", InteractableObject.EInteractableState.Default);
				if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
				{
					StartSowSeedTask(componentInParent);
				}
			}
			else
			{
				componentInParent.ConfigureInteraction(reason, InteractableObject.EInteractableState.Invalid);
			}
		}

		protected virtual void StartSowSeedTask(Pot pot)
		{
			new SowSeedTask(pot, Seed);
		}
	}
}
