using System;
using FishNet.Object;
using ScheduleOne.EntityFramework;

namespace ScheduleOne.Tiles
{
	[Serializable]
	public struct CoordinateProceduralTilePair
	{
		public Coordinate coord;

		public NetworkObject tileParent;

		public int tileIndex;

		public ProceduralTile tile => tileParent.GetComponent<IProceduralTileContainer>().ProceduralTiles[tileIndex];
	}
}
