using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Building
{
	public class BuildUpdate_StoredItem : BuildUpdate_Base
	{
		public class StorageTileIntersection
		{
			public FootprintTile footprintTile;

			public StorageTile storageTile;
		}

		public StorableItemInstance itemInstance;

		public GameObject ghostModel;

		public StoredItem storedItemClass;

		protected StorageTileIntersection bestIntersection;

		[Header("Settings")]
		public float detectionRange = 6f;

		public LayerMask detectionMask;

		public float storedItemHoldDistance = 2f;

		public float currentRotation;

		protected bool validPosition;

		protected Material currentGhostMaterial;

		protected bool mouseUpSinceStart;

		protected bool mouseUpSincePlace = true;

		private Vector3 positionDuringLastValidPosition = Vector3.zero;

		protected virtual void Update()
		{
			CheckRotation();
			if (!GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
			{
				mouseUpSinceStart = true;
				mouseUpSincePlace = true;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && validPosition && mouseUpSinceStart)
			{
				Place();
			}
		}

		protected virtual void LateUpdate()
		{
			validPosition = false;
			ghostModel.transform.up = Vector3.up;
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(detectionRange, out var hit, detectionMask, includeTriggers: false))
			{
				ghostModel.transform.position = hit.point - ghostModel.transform.InverseTransformPoint(storedItemClass.buildPoint.transform.position);
			}
			else
			{
				ghostModel.transform.position = PlayerSingleton<PlayerCamera>.Instance.transform.position + PlayerSingleton<PlayerCamera>.Instance.transform.forward * storedItemHoldDistance;
			}
			ApplyRotation();
			storedItemClass.CalculateFootprintTileIntersections();
			CheckGridIntersections();
			if (validPosition)
			{
				positionDuringLastValidPosition = ghostModel.transform.position;
			}
			else if (mouseUpSincePlace)
			{
				Vector3 position = ghostModel.transform.position;
				float num = 0.0625f;
				ghostModel.transform.position = position + ghostModel.transform.right * num;
				storedItemClass.CalculateFootprintTileIntersections();
				CheckGridIntersections();
				if (!validPosition)
				{
					ghostModel.transform.position = position - ghostModel.transform.right * num;
					storedItemClass.CalculateFootprintTileIntersections();
					CheckGridIntersections();
					if (!validPosition)
					{
						ghostModel.transform.position = position + ghostModel.transform.forward * num;
						storedItemClass.CalculateFootprintTileIntersections();
						CheckGridIntersections();
						if (!validPosition)
						{
							ghostModel.transform.position = position - ghostModel.transform.forward * num;
							storedItemClass.CalculateFootprintTileIntersections();
							CheckGridIntersections();
							if (!validPosition)
							{
								ghostModel.transform.position = position;
								storedItemClass.CalculateFootprintTileIntersections();
								CheckGridIntersections();
							}
						}
					}
				}
			}
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
			ghostModel.transform.rotation = Quaternion.Inverse(storedItemClass.buildPoint.transform.rotation) * ghostModel.transform.rotation;
			ghostModel.transform.Rotate(storedItemClass.buildPoint.up, currentRotation);
		}

		protected virtual void CheckGridIntersections()
		{
			List<StorageTileIntersection> list = new List<StorageTileIntersection>();
			for (int i = 0; i < storedItemClass.CoordinateFootprintTilePairs.Count; i++)
			{
				for (int j = 0; j < storedItemClass.CoordinateFootprintTilePairs[i].tile.tileDetector.intersectedStorageTiles.Count; j++)
				{
					StorageTileIntersection storageTileIntersection = new StorageTileIntersection();
					storageTileIntersection.footprintTile = storedItemClass.CoordinateFootprintTilePairs[i].tile;
					storageTileIntersection.storageTile = storedItemClass.CoordinateFootprintTilePairs[i].tile.tileDetector.intersectedStorageTiles[j];
					list.Add(storageTileIntersection);
				}
			}
			if (list.Count == 0)
			{
				storedItemClass.SetFootprintTileVisiblity(visible: false);
				bestIntersection = null;
				return;
			}
			storedItemClass.SetFootprintTileVisiblity(visible: true);
			float num = 100f;
			bestIntersection = null;
			for (int k = 0; k < list.Count; k++)
			{
				if (bestIntersection == null || Vector3.Distance(list[k].footprintTile.transform.position, list[k].storageTile.transform.position) < num)
				{
					num = Vector3.Distance(list[k].footprintTile.transform.position, list[k].storageTile.transform.position);
					bestIntersection = list[k];
				}
			}
			if (bestIntersection != null && (bool)bestIntersection.storageTile.GetComponentInParent<Pallet>())
			{
				Vector3 vector = bestIntersection.storageTile.transform.forward;
				if (Vector3.Angle(base.transform.forward, -bestIntersection.storageTile.transform.forward) < Vector3.Angle(base.transform.forward, vector))
				{
					vector = -bestIntersection.storageTile.transform.forward;
				}
				if (Vector3.Angle(base.transform.forward, bestIntersection.storageTile.transform.right) < Vector3.Angle(base.transform.forward, vector))
				{
					vector = bestIntersection.storageTile.transform.right;
				}
				if (Vector3.Angle(base.transform.forward, -bestIntersection.storageTile.transform.right) < Vector3.Angle(base.transform.forward, vector))
				{
					vector = -bestIntersection.storageTile.transform.right;
				}
				ghostModel.transform.rotation = Quaternion.LookRotation(vector, Vector3.up);
				ghostModel.transform.Rotate(storedItemClass.buildPoint.up, currentRotation);
			}
			ghostModel.transform.position = bestIntersection.storageTile.transform.position - (bestIntersection.footprintTile.transform.position - ghostModel.transform.position);
			validPosition = bestIntersection.storageTile.ownerGrid.IsItemPositionValid(bestIntersection.storageTile, bestIntersection.footprintTile, storedItemClass);
			for (int l = 0; l < storedItemClass.CoordinateFootprintTilePairs.Count; l++)
			{
				Coordinate matchedCoordinate = bestIntersection.storageTile.ownerGrid.GetMatchedCoordinate(storedItemClass.CoordinateFootprintTilePairs[l].tile);
				_ = storedItemClass.CoordinateFootprintTilePairs[l];
				if (bestIntersection.storageTile.ownerGrid.IsGridPositionValid(matchedCoordinate, storedItemClass.CoordinateFootprintTilePairs[l].tile))
				{
					storedItemClass.CoordinateFootprintTilePairs[l].tile.tileAppearance.SetColor(ETileColor.White);
				}
				else
				{
					storedItemClass.CoordinateFootprintTilePairs[l].tile.tileAppearance.SetColor(ETileColor.Red);
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
				Singleton<BuildManager>.Instance.ApplyMaterial(ghostModel, material);
			}
		}

		protected virtual void Place()
		{
			float rotation = Vector3.SignedAngle(bestIntersection.storageTile.ownerGrid.transform.forward, storedItemClass.buildPoint.forward, bestIntersection.storageTile.ownerGrid.transform.up);
			StorableItemInstance item = itemInstance.GetCopy(1) as StorableItemInstance;
			Singleton<BuildManager>.Instance.CreateStoredItem(item, bestIntersection.storageTile.ownerGrid.GetComponentInParent<IStorageEntity>(), bestIntersection.storageTile.ownerGrid, GetOriginCoordinate(), rotation);
			mouseUpSincePlace = false;
			PostPlace();
		}

		protected virtual void PostPlace()
		{
			PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ChangeQuantity(-1);
		}

		protected Vector2 GetOriginCoordinate()
		{
			storedItemClass.OriginFootprint.tileDetector.CheckIntersections();
			return new Vector2(storedItemClass.OriginFootprint.tileDetector.intersectedStorageTiles[0].x, storedItemClass.OriginFootprint.tileDetector.intersectedStorageTiles[0].y);
		}
	}
}
