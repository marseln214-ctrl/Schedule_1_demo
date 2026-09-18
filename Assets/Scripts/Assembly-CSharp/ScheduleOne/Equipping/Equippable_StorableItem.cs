using ScheduleOne.Building;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.Equipping
{
	public class Equippable_StorableItem : Equippable
	{
		protected bool isBuildingStoredItem;

		protected bool lookingAtStorageObject;

		protected float rotation;

		protected virtual void Update()
		{
			CheckLookingAtStorageObject();
			if (lookingAtStorageObject)
			{
				if (!isBuildingStoredItem)
				{
					StartBuildingStoredItem();
				}
			}
			else if (isBuildingStoredItem)
			{
				StopBuildingStoredItem();
			}
		}

		protected void CheckLookingAtStorageObject()
		{
			lookingAtStorageObject = false;
		}

		public override void Unequip()
		{
			if (lookingAtStorageObject)
			{
				Singleton<BuildManager>.Instance.StopBuilding();
			}
			base.Unequip();
		}

		protected virtual void StartBuildingStoredItem()
		{
			isBuildingStoredItem = true;
			Singleton<BuildManager>.Instance.StartBuildingStoredItem(itemInstance);
			Singleton<BuildManager>.Instance.currentBuildHandler.GetComponent<BuildUpdate_StoredItem>().currentRotation = rotation;
		}

		protected virtual void StopBuildingStoredItem()
		{
			isBuildingStoredItem = false;
			rotation = Singleton<BuildManager>.Instance.currentBuildHandler.GetComponent<BuildUpdate_StoredItem>().currentRotation;
			Singleton<BuildManager>.Instance.StopBuilding();
		}
	}
}
