using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Building
{
	public class BuildStart_ProceduralGrid : BuildStart_Base
	{
		public override void StartBuilding(ItemInstance itemInstance)
		{
			ProceduralGridItem proceduralGridItem = CreateGhostModel(itemInstance.Definition as BuildableItemDefinition);
			if (!(proceduralGridItem == null))
			{
				Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
				ProceduralGridItem component = proceduralGridItem.GetComponent<ProceduralGridItem>();
				base.gameObject.GetComponent<BuildUpdate_ProceduralGrid>().GhostModel = proceduralGridItem.gameObject;
				base.gameObject.GetComponent<BuildUpdate_ProceduralGrid>().ItemClass = component;
				base.gameObject.GetComponent<BuildUpdate_ProceduralGrid>().ItemInstance = itemInstance;
				Singleton<InputPromptsCanvas>.Instance.LoadModule("building");
				for (int i = 0; i < component.CoordinateFootprintTilePairs.Count; i++)
				{
					component.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.tileDetectionMode = ETileDetectionMode.ProceduralTile;
				}
			}
		}

		protected virtual ProceduralGridItem CreateGhostModel(BuildableItemDefinition itemDefinition)
		{
			itemDefinition.BuiltItem.isGhost = true;
			GameObject gameObject = Object.Instantiate(itemDefinition.BuiltItem.gameObject, base.transform);
			itemDefinition.BuiltItem.isGhost = false;
			ProceduralGridItem component = gameObject.GetComponent<ProceduralGridItem>();
			if (component == null)
			{
				Console.LogWarning("CreateGhostModel: asset path is not a BuildableItem!");
				return null;
			}
			component.enabled = false;
			component.isGhost = true;
			Singleton<BuildManager>.Instance.DisableColliders(gameObject);
			Singleton<BuildManager>.Instance.DisableNavigation(gameObject);
			Singleton<BuildManager>.Instance.DisableNetworking(gameObject);
			component.SetFootprintTileVisiblity(visible: false);
			return component;
		}
	}
}
