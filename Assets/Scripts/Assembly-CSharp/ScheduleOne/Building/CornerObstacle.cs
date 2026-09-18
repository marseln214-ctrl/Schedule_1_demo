using System.Collections.Generic;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Building
{
	public class CornerObstacle : MonoBehaviour
	{
		public bool obstacleEnabled;

		public FootprintTile parentFootprint;

		public Vector2 coordinates = Vector2.zero;

		public List<Tile> GetNeighbourTiles(Tile pairedTile)
		{
			List<Tile> list = new List<Tile>();
			List<Tile> surroundingTiles = pairedTile.GetSurroundingTiles();
			surroundingTiles.Add(pairedTile);
			for (int i = 0; i < surroundingTiles.Count; i++)
			{
				if (Vector3.Distance(surroundingTiles[i].transform.position, base.transform.position) < 0.5f)
				{
					list.Add(surroundingTiles[i]);
				}
			}
			return list;
		}

		private bool ApproxEquals(float a, float b, float precision)
		{
			return Mathf.Abs(a - b) <= precision;
		}
	}
}
