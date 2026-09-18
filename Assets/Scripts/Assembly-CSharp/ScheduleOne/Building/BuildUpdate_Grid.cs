using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Building
{
	public class BuildUpdate_Grid : BuildUpdate_Base
	{
		public GameObject GhostModel;

		public GridItem BuildableItemClass;

		public ItemInstance ItemInstance;

		public float CurrentRotation;

		[Header("Settings")]
		public float detectionRange = 6f;

		public LayerMask detectionMask;

		public float rotation_Smoothing = 5f;

		public bool AllowRotation = true;

		protected bool validPosition;

		protected Material currentGhostMaterial;

		protected TileIntersection closestIntersection;

		private float verticalOffset;

		protected virtual void Start()
		{
			LateUpdate();
			if (closestIntersection != null)
			{
				Vector3 forward = closestIntersection.tile.OwnerGrid.transform.forward;
				Vector3 normalized = (PlayerSingleton<PlayerCamera>.Instance.transform.position - BuildableItemClass.BuildPoint.transform.position).normalized;
				normalized.y = 0f;
				float num = Vector3.SignedAngle(forward, normalized, Vector3.up);
				Debug.DrawRay(BuildableItemClass.BuildPoint.transform.position, forward, Color.red, 5f);
				Debug.DrawRay(BuildableItemClass.BuildPoint.transform.position, normalized, Color.green, 5f);
				float num2 = 90f;
				float currentRotation = (float)(int)Mathf.Round(num / num2) * num2;
				CurrentRotation = currentRotation;
			}
		}

		protected virtual void Update()
		{
			CheckRotation();
			if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && validPosition)
			{
				Place();
			}
		}

		protected virtual void LateUpdate()
		{
			validPosition = false;
			GhostModel.transform.up = Vector3.up;
			float holdDistance = BuildableItemClass.HoldDistance;
			float num = (Mathf.Clamp(Vector3.Angle(Vector3.down, PlayerSingleton<PlayerCamera>.Instance.transform.forward), 45f, 90f) - 45f) / 45f;
			float num2 = holdDistance * (1f + num);
			PositionObjectInFrontOfPlayer(num2, sanitizeForward: true, buildPointAsOrigin: true);
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast_ExcludeBuildables(num2, out var hit, detectionMask))
			{
				ApplyRotation();
			}
			if (PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(BuildableItemClass.transform.position + Vector3.up * 0.1f, Vector3.down, 3f, out hit, detectionMask, includeTriggers: false, 0f, 45f))
			{
				GhostModel.transform.position = hit.point - GhostModel.transform.InverseTransformPoint(BuildableItemClass.BuildPoint.transform.position);
			}
			ApplyRotation();
			if ((!Application.isEditor || !Input.GetKey(KeyCode.LeftAlt)) && BuildableItemClass.GetPenetration(out var x, out var z, out var y))
			{
				if (Vector3.Distance(GhostModel.transform.position - GhostModel.transform.right * x, PlayerSingleton<PlayerCamera>.Instance.transform.position) < Vector3.Distance(GhostModel.transform.position - GhostModel.transform.forward * z, PlayerSingleton<PlayerCamera>.Instance.transform.position))
				{
					GhostModel.transform.position -= GhostModel.transform.right * x;
					if (BuildableItemClass.GetPenetration(out x, out z, out y))
					{
						GhostModel.transform.position -= GhostModel.transform.forward * z;
					}
				}
				else
				{
					GhostModel.transform.position -= GhostModel.transform.forward * z;
					if (BuildableItemClass.GetPenetration(out x, out z, out y))
					{
						GhostModel.transform.position -= GhostModel.transform.right * x;
					}
				}
				GhostModel.transform.position -= GhostModel.transform.up * y;
			}
			BuildableItemClass.CalculateFootprintTileIntersections();
			CheckIntersections();
			if (validPosition)
			{
				verticalOffset = Mathf.MoveTowards(verticalOffset, 0f, Time.deltaTime * 1f);
			}
			else
			{
				verticalOffset = Mathf.MoveTowards(verticalOffset, 0.1f, Time.deltaTime * 1f);
			}
			BuildableItemClass.transform.position += Vector3.up * verticalOffset;
			UpdateMaterials();
		}

		protected void PositionObjectInFrontOfPlayer(float dist, bool sanitizeForward, bool buildPointAsOrigin)
		{
			Vector3 forward = PlayerSingleton<PlayerCamera>.Instance.transform.forward;
			if (sanitizeForward)
			{
				forward.y = 0f;
			}
			Vector3 position = PlayerSingleton<PlayerCamera>.Instance.transform.position + forward * dist;
			GhostModel.transform.position = position;
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast_ExcludeBuildables(dist, out var hit, detectionMask))
			{
				GhostModel.transform.position = hit.point;
				if (buildPointAsOrigin && Vector3.Angle(hit.normal, Vector3.up) < 1f)
				{
					GhostModel.transform.position += -GhostModel.transform.InverseTransformPoint(BuildableItemClass.BuildPoint.transform.position);
				}
				else if (BuildableItemClass.MidAirCenterPoint != null)
				{
					GhostModel.transform.position += -GhostModel.transform.InverseTransformPoint(BuildableItemClass.MidAirCenterPoint.transform.position);
				}
			}
			else if (BuildableItemClass.MidAirCenterPoint != null)
			{
				GhostModel.transform.position += -GhostModel.transform.InverseTransformPoint(BuildableItemClass.MidAirCenterPoint.transform.position);
			}
		}

		protected void CheckRotation()
		{
			if (!AllowRotation)
			{
				CurrentRotation = 0f;
				return;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateLeft) && !GameInput.IsTyping)
			{
				CurrentRotation -= 90f;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateRight) && !GameInput.IsTyping)
			{
				CurrentRotation += 90f;
			}
		}

		protected void ApplyRotation()
		{
			GhostModel.transform.rotation = Quaternion.Inverse(BuildableItemClass.BuildPoint.transform.rotation) * GhostModel.transform.rotation;
            ScheduleOne.Tiles.Grid hoveredGrid = GetHoveredGrid();
			float num = CurrentRotation;
			if (hoveredGrid != null)
			{
				num += hoveredGrid.transform.eulerAngles.y;
			}
			GhostModel.transform.Rotate(BuildableItemClass.BuildPoint.up, num);
		}

		private List<TileIntersection> GetRelevantIntersections(FootprintTile tile)
		{
			List<TileIntersection> list = new List<TileIntersection>();
			List<Tile> list2 = new List<Tile>();
			switch (BuildableItemClass.GridType)
			{
			case GridItem.EGridType.All:
				list2 = tile.tileDetector.intersectedTiles;
				break;
			case GridItem.EGridType.IndoorOnly:
				list2 = tile.tileDetector.intersectedIndoorTiles;
				break;
			case GridItem.EGridType.OutdoorOnly:
				list2 = tile.tileDetector.intersectedOutdoorTiles;
				break;
			}
			for (int i = 0; i < list2.Count; i++)
			{
				TileIntersection tileIntersection = new TileIntersection();
				tileIntersection.footprint = tile;
				tileIntersection.tile = list2[i];
				list.Add(tileIntersection);
			}
			return list;
		}

		protected virtual void CheckIntersections()
		{
			List<TileIntersection> list = new List<TileIntersection>();
			for (int i = 0; i < BuildableItemClass.CoordinateFootprintTilePairs.Count; i++)
			{
				list.AddRange(GetRelevantIntersections(BuildableItemClass.CoordinateFootprintTilePairs[i].footprintTile));
			}
			if (list.Count == 0 || (Application.isEditor && Input.GetKey(KeyCode.LeftControl)))
			{
				BuildableItemClass.SetFootprintTileVisiblity(visible: false);
				closestIntersection = null;
				return;
			}
			BuildableItemClass.SetFootprintTileVisiblity(visible: true);
			float num = 100f;
			closestIntersection = null;
			for (int j = 0; j < list.Count; j++)
			{
				if (Vector3.Distance(list[j].tile.transform.position, list[j].footprint.transform.position) < num)
				{
					num = Vector3.Distance(list[j].tile.transform.position, list[j].footprint.transform.position);
					closestIntersection = list[j];
				}
			}
			List<Vector2> list2 = new List<Vector2>();
			GhostModel.transform.position = closestIntersection.tile.transform.position + (GhostModel.transform.position - closestIntersection.footprint.transform.position);
			validPosition = true;
			for (int k = 0; k < BuildableItemClass.CoordinateFootprintTilePairs.Count; k++)
			{
				Coordinate matchedCoordinate = closestIntersection.tile.OwnerGrid.GetMatchedCoordinate(BuildableItemClass.CoordinateFootprintTilePairs[k].footprintTile);
				BuildableItemClass.CoordinateFootprintTilePairs[k].footprintTile.tileAppearance.SetColor(ETileColor.Red);
				if (closestIntersection.tile.OwnerGrid.GetTile(matchedCoordinate) == null)
				{
					validPosition = false;
					continue;
				}
				list2.Add(new Vector2(matchedCoordinate.x, matchedCoordinate.y));
				if (BuildableItemClass.CoordinateFootprintTilePairs[k].footprintTile.AreCornerObstaclesBlocked(closestIntersection.tile.OwnerGrid.GetTile(matchedCoordinate)))
				{
					validPosition = false;
				}
				else if (closestIntersection.tile.OwnerGrid.IsTileValidAtCoordinate(matchedCoordinate, BuildableItemClass.CoordinateFootprintTilePairs[k].footprintTile, BuildableItemClass))
				{
					BuildableItemClass.CoordinateFootprintTilePairs[k].footprintTile.tileAppearance.SetColor(ETileColor.White);
				}
				else
				{
					validPosition = false;
				}
			}
			for (int l = 0; l < BuildableItemClass.CoordinateFootprintTilePairs.Count; l++)
			{
				Coordinate matchedCoordinate2 = closestIntersection.tile.OwnerGrid.GetMatchedCoordinate(BuildableItemClass.CoordinateFootprintTilePairs[l].footprintTile);
				Tile tile = closestIntersection.tile.OwnerGrid.GetTile(matchedCoordinate2);
				if (!(tile != null))
				{
					continue;
				}
				for (int m = 0; m < tile.OccupantTiles.Count; m++)
				{
					for (int n = 0; n < tile.OccupantTiles[m].Corners.Count; n++)
					{
						if (!tile.OccupantTiles[m].Corners[n].obstacleEnabled)
						{
							continue;
						}
						List<Tile> neighbourTiles = tile.OccupantTiles[m].Corners[n].GetNeighbourTiles(tile);
						int num2 = 0;
						foreach (Tile item in neighbourTiles)
						{
							if (list2.Contains(new Vector2(item.x, item.y)))
							{
								num2++;
							}
						}
						if (num2 == 4)
						{
							validPosition = false;
							for (int num3 = 0; num3 < BuildableItemClass.CoordinateFootprintTilePairs.Count; num3++)
							{
								BuildableItemClass.CoordinateFootprintTilePairs[num3].footprintTile.tileAppearance.SetColor(ETileColor.Red);
							}
							return;
						}
					}
				}
			}
		}

		protected void UpdateMaterials()
		{
			Material material = Singleton<BuildManager>.Instance.ghostMaterial_White;
			if (!validPosition)
			{
				material = Singleton<BuildManager>.Instance.ghostMaterial_Red;
			}
			if (currentGhostMaterial != material)
			{
				currentGhostMaterial = material;
				Singleton<BuildManager>.Instance.ApplyMaterial(GhostModel, material);
			}
		}

		protected virtual void Place()
		{
			int rotation = Mathf.RoundToInt(Vector3.SignedAngle(closestIntersection.tile.OwnerGrid.transform.forward, BuildableItemClass.BuildPoint.forward, closestIntersection.tile.OwnerGrid.transform.up));
			Singleton<BuildManager>.Instance.CreateGridItem(ItemInstance.GetCopy(1), closestIntersection.tile.OwnerGrid, GetOriginCoordinate(), rotation);
			PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ChangeQuantity(-1);
			Singleton<BuildManager>.Instance.PlayBuildSound((ItemInstance.Definition as BuildableItemDefinition).BuildSoundType, GhostModel.transform.position);
		}

		private Vector2 GetOriginCoordinate()
		{
			BuildableItemClass.OriginFootprint.tileDetector.CheckIntersections();
			TileIntersection tileIntersection = GetRelevantIntersections(BuildableItemClass.OriginFootprint)[0];
			return new Vector2(tileIntersection.tile.x, tileIntersection.tile.y);
		}

		private ScheduleOne.Tiles.Grid GetHoveredGrid()
		{
			Collider[] array = Physics.OverlapSphere(GhostModel.transform.position, 1.5f, detectionMask);
			for (int i = 0; i < array.Length; i++)
			{
				Tile component = array[i].GetComponent<Tile>();
				if (component != null)
				{
					return component.OwnerGrid;
				}
			}
			return null;
		}
	}
}
