using ScheduleOne.Building;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.DevUtilities;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Construction.ConstructionMethods
{
	public class ConstructStart_OutdoorGrid : ConstructStart_Base
	{
		private Constructable_GridBased constructable;

		private Transform ghostModel;

		public override void StartConstruction(string constructableID, Constructable_GridBased movedConstructable = null)
		{
			base.StartConstruction(constructableID, movedConstructable);
			GenerateGhostModel(constructableID);
			for (int i = 0; i < constructable.CoordinateFootprintTilePairs.Count; i++)
			{
				constructable.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.tileDetectionMode = ETileDetectionMode.OutdoorTile;
			}
			GetComponent<ConstructUpdate_OutdoorGrid>().GhostModel = ghostModel;
			GetComponent<ConstructUpdate_OutdoorGrid>().ConstructableClass = constructable;
			if (movedConstructable != null)
			{
				GetComponent<ConstructUpdate_OutdoorGrid>().currentRotation = movedConstructable.SyncAccessor_Rotation;
			}
		}

		private void GenerateGhostModel(string id)
		{
			GameObject gameObject = Object.Instantiate(Registry.GetConstructable(id).gameObject, base.transform);
			constructable = gameObject.GetComponent<Constructable_GridBased>();
			if (constructable == null)
			{
				Console.LogWarning("CreateGhostModel: asset path is not a Constructable!");
				return;
			}
			constructable.enabled = false;
			constructable.isGhost = true;
			Singleton<BuildManager>.Instance.DisableColliders(gameObject);
			Singleton<BuildManager>.Instance.ApplyMaterial(gameObject, Singleton<BuildManager>.Instance.ghostMaterial_White);
			Singleton<BuildManager>.Instance.DisableNavigation(gameObject);
			Singleton<BuildManager>.Instance.DisableNetworking(gameObject);
			constructable.SetFootprintTileVisiblity(visible: false);
			ghostModel = constructable.gameObject.transform;
		}
	}
}
