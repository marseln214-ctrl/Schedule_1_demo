using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Tiles
{
	public class TileDetector : MonoBehaviour
	{
		public float detectionRadius = 0.25f;

		public ETileDetectionMode tileDetectionMode;

		public List<Tile> intersectedTiles = new List<Tile>();

		public List<Tile> intersectedOutdoorTiles = new List<Tile>();

		public List<Tile> intersectedIndoorTiles = new List<Tile>();

		public List<StorageTile> intersectedStorageTiles = new List<StorageTile>();

		public List<ProceduralTile> intersectedProceduralTiles = new List<ProceduralTile>();

		public virtual void CheckIntersections(bool sort = true)
		{
			intersectedTiles.Clear();
			intersectedOutdoorTiles.Clear();
			intersectedIndoorTiles.Clear();
			intersectedStorageTiles.Clear();
			intersectedProceduralTiles.Clear();
			LayerMask layerMask = (int)default(LayerMask) | (1 << LayerMask.NameToLayer("Tile"));
			Collider[] array = Physics.OverlapSphere(base.transform.position, detectionRadius, layerMask);
			for (int i = 0; i < array.Length; i++)
			{
				if (tileDetectionMode == ETileDetectionMode.Tile)
				{
					Tile componentInParent = array[i].GetComponentInParent<Tile>();
					if (componentInParent != null && !intersectedTiles.Contains(componentInParent))
					{
						intersectedTiles.Add(componentInParent);
					}
				}
				if (tileDetectionMode == ETileDetectionMode.OutdoorTile)
				{
					Tile componentInParent2 = array[i].GetComponentInParent<Tile>();
					if (componentInParent2 != null && !(componentInParent2 is IndoorTile) && !intersectedOutdoorTiles.Contains(componentInParent2))
					{
						intersectedOutdoorTiles.Add(componentInParent2);
					}
				}
				if (tileDetectionMode == ETileDetectionMode.IndoorTile)
				{
					IndoorTile componentInParent3 = array[i].GetComponentInParent<IndoorTile>();
					if (componentInParent3 != null && !intersectedIndoorTiles.Contains(componentInParent3))
					{
						intersectedIndoorTiles.Add(componentInParent3);
					}
				}
				if (tileDetectionMode == ETileDetectionMode.StorageTile)
				{
					StorageTile componentInParent4 = array[i].GetComponentInParent<StorageTile>();
					if (componentInParent4 != null && !intersectedStorageTiles.Contains(componentInParent4))
					{
						intersectedStorageTiles.Add(componentInParent4);
					}
				}
				if (tileDetectionMode == ETileDetectionMode.ProceduralTile)
				{
					ProceduralTile componentInParent5 = array[i].GetComponentInParent<ProceduralTile>();
					if (componentInParent5 != null && !intersectedProceduralTiles.Contains(componentInParent5))
					{
						intersectedProceduralTiles.Add(componentInParent5);
					}
				}
			}
			if (sort)
			{
				intersectedTiles = OrderList(intersectedTiles);
				intersectedOutdoorTiles = OrderList(intersectedOutdoorTiles);
				intersectedIndoorTiles = OrderList(intersectedIndoorTiles);
				intersectedStorageTiles = OrderList(intersectedStorageTiles);
				intersectedProceduralTiles = OrderList(intersectedProceduralTiles);
			}
		}

		public List<T> OrderList<T>(List<T> list) where T : MonoBehaviour
		{
			return list.OrderBy((T x) => Vector3.Distance(x.transform.position, base.transform.position)).ToList();
		}

		public Tile GetClosestTile()
		{
			Tile result = null;
			float num = 100f;
			for (int i = 0; i < intersectedTiles.Count; i++)
			{
				if (Vector3.Distance(intersectedTiles[i].transform.position, base.transform.position) < num)
				{
					result = intersectedTiles[i];
					num = Vector3.Distance(intersectedTiles[i].transform.position, base.transform.position);
				}
			}
			return result;
		}

		public ProceduralTile GetClosestProceduralTile()
		{
			ProceduralTile result = null;
			float num = 100f;
			for (int i = 0; i < intersectedProceduralTiles.Count; i++)
			{
				if (Vector3.Distance(intersectedProceduralTiles[i].transform.position, base.transform.position) < num)
				{
					result = intersectedProceduralTiles[i];
					num = Vector3.Distance(intersectedProceduralTiles[i].transform.position, base.transform.position);
				}
			}
			return result;
		}
	}
}
