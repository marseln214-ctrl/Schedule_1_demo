using ScheduleOne.Interaction;

namespace ScheduleOne.Storage
{
	public class StorageEntityInteractable : InteractableObject
	{
		private StorageEntity StorageEntity;

		private void Awake()
		{
			StorageEntity = GetComponentInParent<StorageEntity>();
			MaxInteractionRange = StorageEntity.MaxAccessDistance;
		}

		public override void Hovered()
		{
			base.Hovered();
			SetInteractableState((!StorageEntity.CanBeOpened()) ? EInteractableState.Disabled : EInteractableState.Default);
		}

		public override void StartInteract()
		{
			base.StartInteract();
			StorageEntity.Open();
		}
	}
}
