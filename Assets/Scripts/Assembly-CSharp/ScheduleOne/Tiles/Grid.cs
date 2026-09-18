using System;
using System.Collections.Generic;
using EasyButtons;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.EntityFramework;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Tiles
{
	public class Grid : MonoBehaviour, IGUIDRegisterable
	{
		public static float GridSideLength = 0.5f;

		public List<Tile> Tiles = new List<Tile>();

		public List<CoordinateTilePair> CoordinateTilePairs = new List<CoordinateTilePair>();

		public Transform Container;

		public bool IsStatic;

		public string StaticGUID = string.Empty;

		protected Dictionary<Coordinate, Tile> _coordinateToTile = new Dictionary<Coordinate, Tile>();

		public Guid GUID { get; protected set; }

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
		}

		protected virtual void Awake()
		{
			if (IsStatic)
			{
				if (!GUIDManager.IsGUIDValid(StaticGUID))
				{
					Console.LogError("Static GUID is not valid.");
				}
				((IGUIDRegisterable)this).SetGUID(StaticGUID);
			}
			if (GetComponentInParent<ScheduleOne.Property.Property>() != null && !IsStatic)
			{
				Debug.LogWarning("Grid is a child of a Property, but is not marked as static!");
			}
			SetInvisible();
			ProcessCoordinateDataPairs();
		}

		public virtual void DestroyGrid()
		{
			GridItem[] componentsInChildren = Container.GetComponentsInChildren<GridItem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].DestroyItem();
			}
		}

		private void ProcessCoordinateDataPairs()
		{
			foreach (CoordinateTilePair coordinateTilePair in CoordinateTilePairs)
			{
				_coordinateToTile.Add(coordinateTilePair.coord, coordinateTilePair.tile);
			}
		}

		public void RegisterTile(Tile tile)
		{
			Tiles.Add(tile);
			CoordinateTilePair item = default(CoordinateTilePair);
			item.coord = new Coordinate(tile.x, tile.y);
			item.tile = tile;
			CoordinateTilePairs.Add(item);
		}

		public void DeregisterTile(Tile tile)
		{
			Console.Log("Deregistering tile: " + tile.x + ", " + tile.y);
			Tiles.Remove(tile);
			for (int i = 0; i < CoordinateTilePairs.Count; i++)
			{
				if (CoordinateTilePairs[i].tile == tile)
				{
					CoordinateTilePairs.RemoveAt(i);
					i--;
					break;
				}
			}
		}

		public Coordinate GetMatchedCoordinate(FootprintTile tileToMatch)
		{
			Vector3 vector = base.transform.InverseTransformPoint(tileToMatch.transform.position);
			return new Coordinate(Mathf.RoundToInt(vector.x / GridSideLength), Mathf.RoundToInt(vector.z / GridSideLength));
		}

		public bool IsTileValidAtCoordinate(Coordinate gridCoord, FootprintTile tile, GridItem tileOwner = null)
		{
			if (!_coordinateToTile.ContainsKey(gridCoord))
			{
				return false;
			}
			Tile tile2 = _coordinateToTile[gridCoord];
			if (tile2.ConstructableOccupants.Count > 0)
			{
				return false;
			}
			if (tile2.BuildableOccupants.Count > 0 && (tileOwner == null || !tileOwner.CanShareTileWith(tile2.BuildableOccupants)))
			{
				return false;
			}
			if (tile2.AvailableOffset != 0f && tile.RequiredOffset != 0f && tile2.AvailableOffset < tile.RequiredOffset)
			{
				return false;
			}
			return tile2.CanBeBuiltOn();
		}

		public bool IsTileValidAtCoordinate(Coordinate gridCoord, FootprintTile tile, Constructable_GridBased ignoreConstructable)
		{
			if (!_coordinateToTile.ContainsKey(gridCoord))
			{
				return false;
			}
			Tile tile2 = _coordinateToTile[gridCoord];
			if (tile2.BuildableOccupants.Count > 0)
			{
				return false;
			}
			for (int i = 0; i < tile2.ConstructableOccupants.Count; i++)
			{
				if (tile2.ConstructableOccupants[i] != ignoreConstructable)
				{
					return false;
				}
			}
			if (tile2.AvailableOffset != 0f && tile.RequiredOffset != 0f && tile2.AvailableOffset < tile.RequiredOffset)
			{
				return false;
			}
			return tile2.CanBeBuiltOn();
		}

		public Tile GetTile(Coordinate coord)
		{
			return CoordinateTilePairs.Find((CoordinateTilePair x) => x.coord.Equals(coord)).tile;
		}

		[Button]
		public void SetVisible()
		{
			for (int i = 0; i < CoordinateTilePairs.Count; i++)
			{
				CoordinateTilePairs[i].tile.SetVisible(vis: true);
			}
		}

		[Button]
		public void SetInvisible()
		{
			for (int i = 0; i < CoordinateTilePairs.Count; i++)
			{
				CoordinateTilePairs[i].tile.SetVisible(vis: false);
			}
		}
	}
}
