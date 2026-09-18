using System;
using System.Collections.Generic;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.EntityFramework;
using ScheduleOne.Lighting;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Tiles
{
	[Serializable]
	public class Tile : MonoBehaviour
	{
		public delegate void TileChange(Tile thisTile);

		public static float TileSize = 0.5f;

		public int x;

		public int y;

		[Header("Settings")]
		public float AvailableOffset = 1000f;

		[Header("References")]
		public Grid OwnerGrid;

		public LightExposureNode LightExposureNode;

		[Header("Occupants")]
		public List<GridItem> BuildableOccupants = new List<GridItem>();

		public List<Constructable_GridBased> ConstructableOccupants = new List<Constructable_GridBased>();

		public List<FootprintTile> OccupantTiles = new List<FootprintTile>();

		public TileChange onTileChanged;

		public void InitializePropertyTile(int _x, int _y, float _available_Offset, Grid _ownerGrid)
		{
			x = _x;
			y = _y;
			AvailableOffset = _available_Offset;
			OwnerGrid = _ownerGrid;
		}

		public void AddOccupant(GridItem occ, FootprintTile tile)
		{
			BuildableOccupants.Remove(occ);
			BuildableOccupants.Add(occ);
			OccupantTiles.Remove(tile);
			OccupantTiles.Add(tile);
			if (onTileChanged != null)
			{
				onTileChanged(this);
			}
		}

		public void AddOccupant(Constructable_GridBased occ, FootprintTile tile)
		{
			ConstructableOccupants.Remove(occ);
			ConstructableOccupants.Add(occ);
			OccupantTiles.Remove(tile);
			OccupantTiles.Add(tile);
			if (onTileChanged != null)
			{
				onTileChanged(this);
			}
		}

		public void RemoveOccupant(GridItem occ, FootprintTile tile)
		{
			BuildableOccupants.Remove(occ);
			OccupantTiles.Remove(tile);
			if (onTileChanged != null)
			{
				onTileChanged(this);
			}
		}

		public void RemoveOccupant(Constructable_GridBased occ, FootprintTile tile)
		{
			ConstructableOccupants.Remove(occ);
			OccupantTiles.Remove(tile);
			if (onTileChanged != null)
			{
				onTileChanged(this);
			}
		}

		public virtual bool CanBeBuiltOn()
		{
			if (OwnerGrid.GetComponentInParent<ScheduleOne.Property.Property>() != null && !OwnerGrid.GetComponentInParent<ScheduleOne.Property.Property>().IsOwned)
			{
				return false;
			}
			return true;
		}

		public List<Tile> GetSurroundingTiles()
		{
			List<Tile> list = new List<Tile>();
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					Tile tile = OwnerGrid.GetTile(new Coordinate(x + i - 1, y + j - 1));
					if (tile != null && tile != this && !list.Contains(tile))
					{
						list.Add(tile);
					}
				}
			}
			return list;
		}

		public virtual bool IsIndoorTile()
		{
			return false;
		}

		public void SetVisible(bool vis)
		{
			base.transform.Find("Model").gameObject.SetActive(vis);
		}
	}
}
