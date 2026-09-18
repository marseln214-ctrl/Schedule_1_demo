using System.Collections.Generic;
using ScheduleOne.Building;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tiles;
using ScheduleOne.UI.Construction;
using UnityEngine;

namespace ScheduleOne.Construction.ConstructionMethods
{
	public class ConstructUpdate_OutdoorGrid : ConstructUpdate_Base
	{
		[Header("Settings")]
		public LayerMask detectionMask;

		public Constructable_GridBased ConstructableClass;

		public Transform GhostModel;

		protected bool validPosition;

		public float currentRotation;

		protected Material currentGhostMaterial;

		protected ConstructionManager.WorldIntersection closestIntersection;

		private float listingPrice;

		protected virtual void Start()
		{
			listingPrice = Singleton<ConstructionMenu>.Instance.GetListingPrice(ConstructableClass.PrefabID);
			if (MovedConstructable == null)
			{
				currentRotation = Singleton<ConstructionManager>.Instance.currentProperty.DefaultRotation;
			}
		}

		protected override void Update()
		{
			base.Update();
			CheckRotation();
			if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && validPosition && AreMetaReqsMet() && !Singleton<ConstructionMenu>.Instance.IsHoveringUI())
			{
				if (base.isMoving)
				{
					FinalizeMoveConstructable();
				}
				else
				{
					PlaceNewConstructable();
				}
			}
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			validPosition = false;
			GhostModel.transform.up = Vector3.up;
			if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(500f, out var hit, detectionMask))
			{
				GhostModel.transform.position = hit.point - GhostModel.transform.InverseTransformPoint(ConstructableClass.buildPoint.transform.position);
			}
			ApplyRotation();
			ConstructableClass.CalculateFootprintTileIntersections();
			CheckTileIntersections();
			UpdateMaterials();
		}

		protected void CheckRotation()
		{
			if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateLeft))
			{
				currentRotation -= 90f;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateRight))
			{
				currentRotation += 90f;
			}
		}

		protected void ApplyRotation()
		{
			GhostModel.transform.rotation = Quaternion.Inverse(ConstructableClass.buildPoint.transform.rotation) * GhostModel.transform.rotation;
			GhostModel.transform.Rotate(ConstructableClass.buildPoint.up, currentRotation);
		}

		protected virtual void CheckTileIntersections()
		{
			List<ConstructionManager.WorldIntersection> list = new List<ConstructionManager.WorldIntersection>();
			for (int i = 0; i < ConstructableClass.CoordinateFootprintTilePairs.Count; i++)
			{
				for (int j = 0; j < ConstructableClass.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.intersectedOutdoorTiles.Count; j++)
				{
					ConstructionManager.WorldIntersection worldIntersection = new ConstructionManager.WorldIntersection();
					worldIntersection.footprint = ConstructableClass.CoordinateFootprintTilePairs[i].footprintTile;
					worldIntersection.tile = ConstructableClass.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.intersectedOutdoorTiles[j];
					list.Add(worldIntersection);
				}
			}
			if (list.Count == 0)
			{
				ConstructableClass.SetFootprintTileVisiblity(visible: false);
				return;
			}
			ConstructableClass.SetFootprintTileVisiblity(visible: true);
			_ = closestIntersection;
			float num = 100f;
			closestIntersection = null;
			for (int k = 0; k < list.Count; k++)
			{
				if (Vector3.Distance(list[k].footprint.transform.position, list[k].tile.transform.position) < num)
				{
					num = Vector3.Distance(list[k].footprint.transform.position, list[k].tile.transform.position);
					closestIntersection = list[k];
				}
			}
			List<Vector2> list2 = new List<Vector2>();
			GhostModel.transform.position = closestIntersection.tile.transform.position + (GhostModel.transform.position - closestIntersection.footprint.transform.position);
			if (base.isMoving)
			{
				_ = MovedConstructable;
			}
			validPosition = true;
			for (int l = 0; l < ConstructableClass.CoordinateFootprintTilePairs.Count; l++)
			{
				Coordinate matchedCoordinate = closestIntersection.tile.OwnerGrid.GetMatchedCoordinate(ConstructableClass.CoordinateFootprintTilePairs[l].footprintTile);
				ConstructableClass.CoordinateFootprintTilePairs[l].footprintTile.tileAppearance.SetColor(ETileColor.Red);
				if (closestIntersection.tile.OwnerGrid.GetTile(matchedCoordinate) == null)
				{
					validPosition = false;
					continue;
				}
				list2.Add(new Vector2(matchedCoordinate.x, matchedCoordinate.y));
				if (closestIntersection.tile.OwnerGrid.IsTileValidAtCoordinate(matchedCoordinate, ConstructableClass.CoordinateFootprintTilePairs[l].footprintTile, MovedConstructable))
				{
					ConstructableClass.CoordinateFootprintTilePairs[l].footprintTile.tileAppearance.SetColor(ETileColor.White);
				}
				else
				{
					validPosition = false;
				}
			}
		}

		protected void UpdateMaterials()
		{
			Material material = Singleton<BuildManager>.Instance.ghostMaterial_White;
			if (!validPosition || !AreMetaReqsMet())
			{
				material = Singleton<BuildManager>.Instance.ghostMaterial_Red;
			}
			if (currentGhostMaterial != material)
			{
				currentGhostMaterial = material;
				Singleton<BuildManager>.Instance.ApplyMaterial(GhostModel.gameObject, material);
			}
		}

		private bool AreMetaReqsMet()
		{
			if (base.isMoving)
			{
				return true;
			}
			return NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= listingPrice;
		}

		protected virtual Constructable_GridBased PlaceNewConstructable()
		{
			Constructable_GridBased constructable_GridBased = Singleton<ConstructionManager>.Instance.CreateConstructable_GridBased(ConstructableClass.PrefabID, closestIntersection.tile.OwnerGrid, GetOriginCoordinate(), currentRotation);
			NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction(ConstructableClass.ConstructableName, 0f - listingPrice, 1f, string.Empty);
			if (Singleton<ConstructionManager>.Instance.onNewConstructableBuilt != null)
			{
				Singleton<ConstructionManager>.Instance.onNewConstructableBuilt(constructable_GridBased);
			}
			if (!Input.GetKey(KeyCode.LeftShift))
			{
				Singleton<ConstructionMenu>.Instance.ClearSelectedListing();
			}
			return constructable_GridBased;
		}

		protected virtual void FinalizeMoveConstructable()
		{
			MovedConstructable.RepositionConstructable(closestIntersection.tile.OwnerGrid.GUID, GetOriginCoordinate(), currentRotation);
			Constructable_GridBased movedConstructable = MovedConstructable;
			Singleton<ConstructionManager>.Instance.StopMovingConstructable();
			if (Singleton<ConstructionManager>.Instance.onConstructableMoved != null)
			{
				Singleton<ConstructionManager>.Instance.onConstructableMoved(movedConstructable);
			}
		}

		private Vector2 GetOriginCoordinate()
		{
			ConstructableClass.OriginFootprint.tileDetector.CheckIntersections();
			return new Vector2(ConstructableClass.OriginFootprint.tileDetector.intersectedOutdoorTiles[0].x, ConstructableClass.OriginFootprint.tileDetector.intersectedOutdoorTiles[0].y);
		}
	}
}
