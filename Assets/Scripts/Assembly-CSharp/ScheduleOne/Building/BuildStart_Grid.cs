using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Building
{
	public class BuildStart_Grid : BuildStart_Base
	{
		public override void StartBuilding(ItemInstance itemInstance)
		{
			GridItem gridItem = CreateGhostModel(itemInstance.Definition as BuildableItemDefinition);
			if (gridItem == null)
			{
				return;
			}
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
			GridItem component = gridItem.GetComponent<GridItem>();
			for (int i = 0; i < component.CoordinateFootprintTilePairs.Count; i++)
			{
				switch (component.GridType)
				{
				case GridItem.EGridType.All:
					component.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.tileDetectionMode = ETileDetectionMode.Tile;
					break;
				case GridItem.EGridType.IndoorOnly:
					component.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.tileDetectionMode = ETileDetectionMode.IndoorTile;
					break;
				case GridItem.EGridType.OutdoorOnly:
					component.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.tileDetectionMode = ETileDetectionMode.OutdoorTile;
					break;
				}
			}
			Singleton<InputPromptsCanvas>.Instance.LoadModule("building");
			base.gameObject.GetComponent<BuildUpdate_Grid>().GhostModel = gridItem.gameObject;
			base.gameObject.GetComponent<BuildUpdate_Grid>().BuildableItemClass = gridItem;
			base.gameObject.GetComponent<BuildUpdate_Grid>().ItemInstance = itemInstance;
		}

		protected virtual GridItem CreateGhostModel(BuildableItemDefinition itemDefinition)
		{
			itemDefinition.BuiltItem.isGhost = true;
			GameObject gameObject = Object.Instantiate(itemDefinition.BuiltItem.gameObject, base.transform);
			itemDefinition.BuiltItem.isGhost = false;
			GridItem component = gameObject.GetComponent<GridItem>();
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
			Singleton<BuildManager>.Instance.DisableCanvases(gameObject);
			ActivateDuringBuild[] componentsInChildren = gameObject.GetComponentsInChildren<ActivateDuringBuild>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.SetActive(value: true);
			}
			component.SetFootprintTileVisiblity(visible: false);
			return component;
		}
	}
}
