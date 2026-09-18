using System.Collections.Generic;
using ScheduleOne.EntityFramework;
using UnityEngine;

namespace ScheduleOne.Tiles
{
	public class ProceduralTile : MonoBehaviour
	{
		public enum EProceduralTileType
		{
			Rack = 0
		}

		[Header("Settings")]
		public EProceduralTileType TileType;

		[Header("References")]
		public BuildableItem ParentBuildableItem;

		public FootprintTile MatchedFootprintTile;

		[Header("Occupants")]
		public List<ProceduralGridItem> Occupants = new List<ProceduralGridItem>();

		public List<FootprintTile> OccupantTiles = new List<FootprintTile>();

		protected virtual void Awake()
		{
		}

		public void AddOccupant(FootprintTile footprint, ProceduralGridItem item)
		{
			if (!Occupants.Contains(item))
			{
				Occupants.Add(item);
			}
			if (!OccupantTiles.Contains(footprint))
			{
				OccupantTiles.Add(footprint);
			}
		}

		public void RemoveOccupant(FootprintTile footprint, ProceduralGridItem item)
		{
			if (Occupants.Contains(item))
			{
				Occupants.Remove(item);
			}
			if (OccupantTiles.Contains(footprint))
			{
				OccupantTiles.Remove(footprint);
			}
		}
	}
}
