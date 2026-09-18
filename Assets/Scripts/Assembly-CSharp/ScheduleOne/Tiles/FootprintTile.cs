using System.Collections.Generic;
using ScheduleOne.Building;
using ScheduleOne.EntityFramework;
using UnityEngine;

namespace ScheduleOne.Tiles
{
	public class FootprintTile : MonoBehaviour
	{
		public TileAppearance tileAppearance;

		public TileDetector tileDetector;

		public int X;

		public int Y;

		public float RequiredOffset;

		public List<CornerObstacle> Corners = new List<CornerObstacle>();

		public Tile MatchedStandardTile { get; protected set; }

		protected virtual void Awake()
		{
			tileAppearance.SetVisible(visible: false);
		}

		public virtual void Initialize(Tile matchedTile)
		{
			MatchedStandardTile = matchedTile;
		}

		public bool AreCornerObstaclesBlocked(Tile proposedTile)
		{
			if (proposedTile == null)
			{
				return true;
			}
			for (int i = 0; i < Corners.Count; i++)
			{
				if (!Corners[i].obstacleEnabled)
				{
					continue;
				}
				List<Tile> neighbourTiles = Corners[i].GetNeighbourTiles(proposedTile);
				Dictionary<GridItem, int> dictionary = new Dictionary<GridItem, int>();
				for (int j = 0; j < neighbourTiles.Count; j++)
				{
					for (int k = 0; k < neighbourTiles[j].BuildableOccupants.Count; k++)
					{
						if (!dictionary.ContainsKey(neighbourTiles[j].BuildableOccupants[k]))
						{
							dictionary.Add(neighbourTiles[j].BuildableOccupants[k], 1);
						}
						else
						{
							dictionary[neighbourTiles[j].BuildableOccupants[k]]++;
						}
					}
				}
				foreach (GridItem key in dictionary.Keys)
				{
					if (dictionary[key] == neighbourTiles.Count)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
