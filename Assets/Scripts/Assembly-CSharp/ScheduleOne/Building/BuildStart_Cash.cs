using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Storage;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Building
{
	public class BuildStart_Cash : BuildStart_StoredItem
	{
		public override void StartBuilding(ItemInstance itemInstance)
		{
			GameObject gameObject = CreateGhostModel(itemInstance as StorableItemInstance);
			if (!(gameObject == null))
			{
				Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
				base.gameObject.GetComponent<BuildUpdate_Cash>().itemInstance = itemInstance as StorableItemInstance;
				base.gameObject.GetComponent<BuildUpdate_Cash>().ghostModel = gameObject;
				base.gameObject.GetComponent<BuildUpdate_Cash>().storedItemClass = gameObject.GetComponent<StoredItem>();
			}
		}
	}
}
