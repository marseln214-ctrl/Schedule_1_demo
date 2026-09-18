using ScheduleOne.Building;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.Equipping
{
	public class Equippable_BuildableItem : Equippable_StorableItem
	{
		protected bool isBuilding;

		protected override void Update()
		{
			CheckLookingAtStorageObject();
			if (lookingAtStorageObject && isBuilding)
			{
				isBuilding = false;
				if (Singleton<BuildManager>.Instance.currentBuildHandler.GetComponent<BuildUpdate_Grid>() != null)
				{
					rotation = Singleton<BuildManager>.Instance.currentBuildHandler.GetComponent<BuildUpdate_Grid>().CurrentRotation;
				}
			}
			base.Update();
			if (!lookingAtStorageObject && !isBuilding)
			{
				isBuilding = true;
				Singleton<BuildManager>.Instance.StartBuilding(itemInstance);
				if (Singleton<BuildManager>.Instance.currentBuildHandler.GetComponent<BuildUpdate_Grid>() != null)
				{
					Singleton<BuildManager>.Instance.currentBuildHandler.GetComponent<BuildUpdate_Grid>().CurrentRotation = rotation;
				}
			}
		}

		public override void Unequip()
		{
			if (isBuilding)
			{
				Singleton<BuildManager>.Instance.StopBuilding();
			}
			base.Unequip();
		}
	}
}
