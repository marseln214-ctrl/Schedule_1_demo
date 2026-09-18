using System.Collections.Generic;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Building
{
	public class BuildUpdate_ProceduralGrid : BuildUpdate_Base
	{
		public class Intersection
		{
			public FootprintTile footprintTile;

			public ProceduralTile procTile;
		}

		public GameObject GhostModel;

		public ProceduralGridItem ItemClass;

		public ItemInstance ItemInstance;

		[Header("Settings")]
		public float detectionRange = 6f;

		public LayerMask detectionMask;

		public float rotation_Smoothing = 5f;

		protected float currentRotation;

		protected bool validPosition;

		protected Material currentGhostMaterial;

		protected Intersection bestIntersection;

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
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(detectionRange, out var hit, detectionMask))
			{
				GhostModel.transform.position = hit.point - GhostModel.transform.InverseTransformPoint(ItemClass.BuildPoint.transform.position);
			}
			else
			{
				GhostModel.transform.position = PlayerSingleton<PlayerCamera>.Instance.transform.position + PlayerSingleton<PlayerCamera>.Instance.transform.forward * ItemClass.HoldDistance;
				if (ItemClass.MidAirCenterPoint != null)
				{
					GhostModel.transform.position += -GhostModel.transform.InverseTransformPoint(ItemClass.MidAirCenterPoint.transform.position);
				}
			}
			ApplyRotation();
			CheckGridIntersections();
			UpdateMaterials();
		}

		protected void CheckRotation()
		{
			if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateLeft) && !GameInput.IsTyping)
			{
				currentRotation -= 90f;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateRight) && !GameInput.IsTyping)
			{
				currentRotation += 90f;
			}
		}

		protected void ApplyRotation()
		{
			GhostModel.transform.rotation = Quaternion.Inverse(ItemClass.BuildPoint.transform.rotation) * GhostModel.transform.rotation;
			ProceduralTile nearbyProcTile = GetNearbyProcTile();
			float num = currentRotation;
			if (nearbyProcTile != null)
			{
				num += nearbyProcTile.transform.eulerAngles.y;
			}
			GhostModel.transform.Rotate(ItemClass.BuildPoint.up, num);
		}

		protected virtual void CheckGridIntersections()
		{
			ItemClass.CalculateFootprintTileIntersections();
			List<Intersection> list = new List<Intersection>();
			for (int i = 0; i < ItemClass.CoordinateFootprintTilePairs.Count; i++)
			{
				for (int j = 0; j < ItemClass.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.intersectedProceduralTiles.Count; j++)
				{
					Intersection intersection = new Intersection();
					intersection.footprintTile = ItemClass.CoordinateFootprintTilePairs[i].footprintTile;
					intersection.procTile = ItemClass.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.intersectedProceduralTiles[j];
					list.Add(intersection);
				}
			}
			if (list.Count == 0)
			{
				ItemClass.SetFootprintTileVisiblity(visible: false);
				return;
			}
			ItemClass.SetFootprintTileVisiblity(visible: true);
			float num = 100f;
			bestIntersection = null;
			for (int k = 0; k < list.Count; k++)
			{
				if (Vector3.Distance(list[k].footprintTile.transform.position, list[k].procTile.transform.position) < num)
				{
					num = Vector3.Distance(list[k].footprintTile.transform.position, list[k].procTile.transform.position);
					bestIntersection = list[k];
				}
			}
			validPosition = true;
			GhostModel.transform.position = bestIntersection.procTile.transform.position - (bestIntersection.footprintTile.transform.position - GhostModel.transform.position);
			ItemClass.CalculateFootprintTileIntersections();
			for (int l = 0; l < ItemClass.CoordinateFootprintTilePairs.Count; l++)
			{
				bool flag = false;
				ProceduralTile closestProceduralTile = ItemClass.CoordinateFootprintTilePairs[l].footprintTile.tileDetector.GetClosestProceduralTile();
				if (IsMatchValid(ItemClass.CoordinateFootprintTilePairs[l].footprintTile, closestProceduralTile))
				{
					flag = true;
				}
				if (flag)
				{
					ItemClass.CoordinateFootprintTilePairs[l].footprintTile.tileAppearance.SetColor(ETileColor.White);
					continue;
				}
				validPosition = false;
				ItemClass.CoordinateFootprintTilePairs[l].footprintTile.tileAppearance.SetColor(ETileColor.Red);
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

		private bool IsMatchValid(FootprintTile footprintTile, ProceduralTile matchedTile)
		{
			if (footprintTile == null || matchedTile == null)
			{
				return false;
			}
			if (Vector3.Distance(matchedTile.transform.position, footprintTile.transform.position) < 0.01f && matchedTile.Occupants.Count == 0 && matchedTile.TileType == ItemClass.ProceduralTileType)
			{
				return true;
			}
			return false;
		}

		protected void Place()
		{
			List<CoordinateProceduralTilePair> list = new List<CoordinateProceduralTilePair>();
			for (int i = 0; i < ItemClass.CoordinateFootprintTilePairs.Count; i++)
			{
				bool flag = false;
				ProceduralTile closestProceduralTile = ItemClass.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.GetClosestProceduralTile();
				if (IsMatchValid(ItemClass.CoordinateFootprintTilePairs[i].footprintTile, closestProceduralTile))
				{
					flag = true;
				}
				if (!flag)
				{
					Console.LogWarning("Invalid placement!");
					return;
				}
				NetworkObject networkObject = closestProceduralTile.ParentBuildableItem.NetworkObject;
				int tileIndex = (closestProceduralTile.ParentBuildableItem as IProceduralTileContainer).ProceduralTiles.IndexOf(closestProceduralTile);
				list.Add(new CoordinateProceduralTilePair
				{
					coord = ItemClass.CoordinateFootprintTilePairs[i].coord,
					tileParent = networkObject,
					tileIndex = tileIndex
				});
			}
			float f = Vector3.SignedAngle(list[0].tile.transform.forward, GhostModel.transform.forward, list[0].tile.transform.up);
			Singleton<BuildManager>.Instance.CreateProceduralGridItem(ItemInstance.GetCopy(1), Mathf.RoundToInt(f), list);
			PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ChangeQuantity(-1);
			Singleton<BuildManager>.Instance.PlayBuildSound((ItemInstance.Definition as BuildableItemDefinition).BuildSoundType, GhostModel.transform.position);
		}

		private ProceduralTile GetNearbyProcTile()
		{
			Collider[] array = Physics.OverlapSphere(GhostModel.transform.position, 1f, detectionMask);
			for (int i = 0; i < array.Length; i++)
			{
				ProceduralTile component = array[i].GetComponent<ProceduralTile>();
				if (component != null)
				{
					return component;
				}
			}
			return null;
		}
	}
}
