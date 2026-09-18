using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tiles;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScheduleOne.Storage
{
	public class StoredItem : MonoBehaviour
	{
		[Header("References")]
		public Transform buildPoint;

		public List<CoordinateStorageFootprintTilePair> CoordinateFootprintTilePairs = new List<CoordinateStorageFootprintTilePair>();

		private int footprintX = -1;

		private int footprintY = -1;

		protected InteractableObject intObj;

		protected List<CoordinatePair> coordinatePairs = new List<CoordinatePair>();

		protected float rotation;

		public int xSize;

		public int ySize;

		public StorableItemInstance item { get; protected set; }

		public FootprintTile OriginFootprint => CoordinateFootprintTilePairs[0].tile;

		public int FootprintX
		{
			get
			{
				if (footprintX == -1)
				{
					footprintX = CoordinateFootprintTilePairs.OrderByDescending((CoordinateStorageFootprintTilePair c) => c.coord.x).FirstOrDefault().coord.x + 1;
				}
				return footprintX;
			}
		}

		public int FootprintY
		{
			get
			{
				if (footprintY == -1)
				{
					footprintY = CoordinateFootprintTilePairs.OrderByDescending((CoordinateStorageFootprintTilePair c) => c.coord.y).FirstOrDefault().coord.y + 1;
				}
				return footprintY;
			}
		}

		public IStorageEntity parentStorageEntity { get; protected set; }

		public StorageGrid parentGrid { get; protected set; }

		public List<CoordinatePair> CoordinatePairs => coordinatePairs;

		public float Rotation => rotation;

		public int totalArea => CoordinateFootprintTilePairs.Count;

		public bool canBePickedUp { get; protected set; } = true;


		public string noPickupReason { get; protected set; } = string.Empty;


		protected virtual void Awake()
		{
			MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].shadowCastingMode == ShadowCastingMode.ShadowsOnly)
				{
					componentsInChildren[i].enabled = false;
				}
				else
				{
					componentsInChildren[i].shadowCastingMode = ShadowCastingMode.Off;
				}
			}
		}

		protected virtual void OnValidate()
		{
			if (base.gameObject.layer != LayerMask.NameToLayer("StoredItem"))
			{
				SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("StoredItem"));
			}
			if (CoordinateFootprintTilePairs.Count == 0)
			{
				Debug.LogWarning("StoredItem (" + base.gameObject.name + ") has no CoordinateFootprintTilePairs!");
			}
		}

		public virtual void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
		{
			if (grid == null)
			{
				Console.LogError("InitializeStoredItem: grid is null!");
				DestroyStoredItem();
				return;
			}
			item = _item;
			parentGrid = grid;
			rotation = _rotation;
			SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("StoredItem"));
			coordinatePairs = Coordinate.BuildCoordinateMatches(new Coordinate(_originCoordinate), FootprintX, FootprintY, Rotation);
			RefreshTransform();
			for (int i = 0; i < coordinatePairs.Count; i++)
			{
				StorageTile tile = parentGrid.GetTile(coordinatePairs[i].coord2);
				if (tile == null)
				{
					Console.LogError("Failed to find tile at " + coordinatePairs[i].coord2?.ToString() + " when initializing stored item!");
					DestroyStoredItem();
					return;
				}
				if (tile.occupant != null)
				{
					Console.LogError("InitializeStoredItem: " + coordinatePairs[i].coord2?.ToString() + " is already occupied by " + tile.occupant.item.Name + "! Destroying this StoredItem.");
					DestroyStoredItem();
					return;
				}
				tile.SetOccupant(this);
				grid.freeTiles.Remove(tile);
			}
			intObj = GetComponentInChildren<InteractableObject>();
			if (intObj != null)
			{
				Object.Destroy(intObj);
			}
			SetFootprintTileVisiblity(visible: false);
		}

		private void RefreshTransform()
		{
			FootprintTile tile = GetTile(coordinatePairs[0].coord1);
			StorageTile tile2 = parentGrid.GetTile(coordinatePairs[0].coord2);
			base.transform.rotation = parentGrid.transform.rotation * (Quaternion.Inverse(buildPoint.transform.rotation) * base.transform.rotation);
			base.transform.Rotate(buildPoint.up, rotation);
			base.transform.position = tile2.transform.position - (tile.transform.position - base.transform.position);
		}

		protected virtual void InitializeIntObj()
		{
			intObj = GetComponentInChildren<InteractableObject>();
			if (intObj == null)
			{
				intObj = base.gameObject.AddComponent<InteractableObject>();
			}
			intObj.onHovered.AddListener(Hovered);
			intObj.onInteractStart.AddListener(Interacted);
		}

		public virtual void Destroy_Internal()
		{
			for (int i = 0; i < coordinatePairs.Count; i++)
			{
				parentGrid.GetTile(coordinatePairs[i].coord2).SetOccupant(null);
			}
			if (GetComponentInParent<IStorageEntity>() != null)
			{
				GetComponentInParent<IStorageEntity>().DereserveItem(this);
			}
			Object.Destroy(base.gameObject);
		}

		public void DestroyStoredItem()
		{
			ClearFootprintOccupancy();
			if (this != null && base.gameObject != null)
			{
				Object.Destroy(base.gameObject);
			}
		}

		public void ClearFootprintOccupancy()
		{
			if (parentGrid == null)
			{
				return;
			}
			for (int i = 0; i < coordinatePairs.Count; i++)
			{
				StorageTile tile = parentGrid.GetTile(coordinatePairs[i].coord2);
				if (!(tile == null))
				{
					tile.SetOccupant(null);
					parentGrid.freeTiles.Add(tile);
				}
			}
		}

		public void SetCanBePickedUp(bool _canBePickedUp, string _noPickupReason = "")
		{
			canBePickedUp = _canBePickedUp;
			noPickupReason = _noPickupReason;
		}

		public static void SetLayerRecursively(GameObject go, int layerNumber)
		{
			Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.gameObject.layer != LayerMask.NameToLayer("Grid"))
				{
					transform.gameObject.layer = layerNumber;
				}
			}
		}

		public static List<StoredItem> RemoveReservedItems(List<StoredItem> itemList, Employee allowedReservant)
		{
			return itemList.Where((StoredItem x) => x.parentStorageEntity.WhoIsReserving(x) == null || x.parentStorageEntity.WhoIsReserving(x) == allowedReservant).ToList();
		}

		public virtual GameObject CreateGhostModel(ItemInstance _item, Transform parent)
		{
			return Object.Instantiate(base.gameObject, parent);
		}

		public void SetFootprintTileVisiblity(bool visible)
		{
			for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
			{
				CoordinateFootprintTilePairs[i].tile.tileAppearance.SetVisible(visible);
			}
		}

		public void CalculateFootprintTileIntersections()
		{
			for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
			{
				CoordinateFootprintTilePairs[i].tile.tileDetector.CheckIntersections();
			}
		}

		public FootprintTile GetTile(Coordinate coord)
		{
			for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
			{
				if (CoordinateFootprintTilePairs[i].coord.Equals(coord))
				{
					return CoordinateFootprintTilePairs[i].tile;
				}
			}
			return null;
		}

		public virtual void Hovered()
		{
			if (canBePickedUp)
			{
				if (PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(item))
				{
					intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
					intObj.SetMessage("Pick up <color=#" + ColorUtility.ToHtmlStringRGBA(item.LabelDisplayColor) + ">" + item.Name + "</color>");
				}
				else
				{
					intObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
					intObj.SetMessage("Inventory full");
				}
			}
			else if (noPickupReason != "")
			{
				intObj.SetMessage(noPickupReason);
				intObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			}
			else
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}

		public virtual void Interacted()
		{
			if (canBePickedUp)
			{
				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(item);
				DestroyStoredItem();
			}
		}
	}
}
