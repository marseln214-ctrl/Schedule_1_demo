using System.Collections.Generic;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Storage
{
	public class StorageGrid : MonoBehaviour
	{
		public static float gridSize = 0.25f;

		public List<StorageTile> storageTiles = new List<StorageTile>();

		public List<StorageTile> freeTiles = new List<StorageTile>();

		public List<CoordinateStorageTilePair> coordinateStorageTilePairs = new List<CoordinateStorageTilePair>();

		protected Dictionary<Coordinate, StorageTile> coordinateToTile = new Dictionary<Coordinate, StorageTile>();

		protected virtual void Awake()
		{
			ProcessCoordinateTilePairs();
			freeTiles.AddRange(storageTiles);
		}

		private void ProcessCoordinateTilePairs()
		{
			foreach (CoordinateStorageTilePair coordinateStorageTilePair in coordinateStorageTilePairs)
			{
				coordinateToTile.Add(coordinateStorageTilePair.coord, coordinateStorageTilePair.tile);
			}
		}

		public void RegisterTile(StorageTile tile)
		{
			storageTiles.Add(tile);
			CoordinateStorageTilePair item = default(CoordinateStorageTilePair);
			item.coord = new Coordinate(tile.x, tile.y);
			item.tile = tile;
			coordinateStorageTilePairs.Add(item);
		}

		public void DeregisterTile(StorageTile tile)
		{
			storageTiles.Remove(tile);
			for (int i = 0; i < coordinateStorageTilePairs.Count; i++)
			{
				if (coordinateStorageTilePairs[i].tile == tile)
				{
					coordinateStorageTilePairs.RemoveAt(i);
					i--;
					break;
				}
			}
		}

		public bool IsItemPositionValid(StorageTile primaryTile, FootprintTile primaryFootprintTile, StoredItem item)
		{
			foreach (CoordinateStorageFootprintTilePair coordinateFootprintTilePair in item.CoordinateFootprintTilePairs)
			{
				Coordinate matchedCoordinate = GetMatchedCoordinate(coordinateFootprintTilePair.tile);
				if (!IsGridPositionValid(matchedCoordinate, coordinateFootprintTilePair.tile))
				{
					return false;
				}
			}
			return true;
		}

		public Coordinate GetMatchedCoordinate(FootprintTile tileToMatch)
		{
			Vector3 vector = base.transform.InverseTransformPoint(tileToMatch.transform.position);
			return new Coordinate(Mathf.RoundToInt(vector.x / gridSize), Mathf.RoundToInt(vector.z / gridSize));
		}

		public bool IsGridPositionValid(Coordinate gridCoord, FootprintTile tile)
		{
			if (!coordinateToTile.ContainsKey(gridCoord))
			{
				return false;
			}
			if (coordinateToTile[gridCoord].occupant != null)
			{
				return false;
			}
			return true;
		}

		public StorageTile GetTile(Coordinate coord)
		{
			for (int i = 0; i < coordinateStorageTilePairs.Count; i++)
			{
				if (coordinateStorageTilePairs[i].coord.Equals(coord))
				{
					return coordinateStorageTilePairs[i].tile;
				}
			}
			return null;
		}

		public int GetUserEndCapacity()
		{
			int actualY = GetActualY();
			int num = coordinateStorageTilePairs.Count / actualY;
			return (actualY - 1) * (num - 1);
		}

		public int GetActualY()
		{
			int result = 0;
			int num = 0;
			while (num < coordinateStorageTilePairs.Count)
			{
				if (coordinateStorageTilePairs[num].coord.x == 0)
				{
					num++;
					num++;
					continue;
				}
				result = num;
				break;
			}
			return result;
		}

		public int GetActualX()
		{
			return coordinateStorageTilePairs.Count / GetActualY();
		}

		public int GetTotalFootprintSize()
		{
			return coordinateStorageTilePairs.Count;
		}

		public bool TryFitItem(int sizeX, int sizeY, List<Coordinate> lockedCoordinates, out Coordinate originCoordinate, out float rotation)
		{
			foreach (CoordinateStorageTilePair coordinateStorageTilePair in coordinateStorageTilePairs)
			{
				if (coordinateStorageTilePair.tile.occupant != null)
				{
					continue;
				}
				originCoordinate = coordinateStorageTilePair.coord;
				bool flag = true;
				rotation = 0f;
				for (int i = 0; i < sizeX; i++)
				{
					for (int j = 0; j < sizeY; j++)
					{
						Coordinate coordinate = new Coordinate(coordinateStorageTilePair.tile.x + i, coordinateStorageTilePair.tile.y + j);
						for (int k = 0; k < lockedCoordinates.Count; k++)
						{
							if (coordinate.Equals(lockedCoordinates[k]))
							{
								flag = false;
							}
						}
						StorageTile tile = GetTile(coordinate);
						if (tile == null || tile.occupant != null)
						{
							flag = false;
						}
					}
				}
				if (flag)
				{
					return true;
				}
				flag = true;
				rotation = 90f;
				for (int l = 0; l < sizeX; l++)
				{
					for (int m = 0; m < sizeY; m++)
					{
						Coordinate coordinate2 = new Coordinate(coordinateStorageTilePair.tile.x + m, coordinateStorageTilePair.tile.y - l);
						for (int n = 0; n < lockedCoordinates.Count; n++)
						{
							if (coordinate2.Equals(lockedCoordinates[n]))
							{
								flag = false;
							}
						}
						StorageTile tile2 = GetTile(coordinate2);
						if (tile2 == null || tile2.occupant != null)
						{
							flag = false;
						}
					}
				}
				if (flag)
				{
					return true;
				}
			}
			originCoordinate = new Coordinate(0, 0);
			rotation = 0f;
			return false;
		}
	}
}
