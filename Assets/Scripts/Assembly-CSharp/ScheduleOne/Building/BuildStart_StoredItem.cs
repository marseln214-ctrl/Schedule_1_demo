using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Storage;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Building
{
	public class BuildStart_StoredItem : BuildStart_Base
	{
		public override void StartBuilding(ItemInstance itemInstance)
		{
			GameObject gameObject = CreateGhostModel(itemInstance as StorableItemInstance);
			if (!(gameObject == null))
			{
				Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
				base.gameObject.GetComponent<BuildUpdate_StoredItem>().itemInstance = itemInstance as StorableItemInstance;
				base.gameObject.GetComponent<BuildUpdate_StoredItem>().ghostModel = gameObject;
				base.gameObject.GetComponent<BuildUpdate_StoredItem>().storedItemClass = gameObject.GetComponent<StoredItem>();
			}
		}

		protected virtual GameObject CreateGhostModel(StorableItemInstance item)
		{
			if (item == null)
			{
				Console.LogError("StoredItem CreateGhostModel called but item is null!");
				return null;
			}
			GameObject gameObject = item.StoredItem.CreateGhostModel(item, base.transform);
			StoredItem component = gameObject.GetComponent<StoredItem>();
			if (component == null)
			{
				Console.LogWarning("CreateGhostModel: asset path is not a storeableItem!");
				return null;
			}
			component.enabled = false;
			Singleton<BuildManager>.Instance.DisableColliders(gameObject);
			Singleton<BuildManager>.Instance.ApplyMaterial(gameObject, Singleton<BuildManager>.Instance.ghostMaterial_White);
			Singleton<BuildManager>.Instance.DisableSpriteRenderers(gameObject);
			component.SetFootprintTileVisiblity(visible: false);
			return gameObject;
		}
	}
}
