using System;
using System.Collections.Generic;
using ScheduleOne.Building;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class ProceduralGridItemLoader : BuildableItemLoader
	{
		public override string ItemType => typeof(ProceduralGridItemData).Name;

		public override void Load(string mainPath)
		{
			LoadAndCreate(mainPath);
		}

		protected ProceduralGridItem LoadAndCreate(string mainPath)
		{
			if (TryLoadFile(mainPath, "Data", out var contents))
			{
				ProceduralGridItemData proceduralGridItemData = null;
				try
				{
					proceduralGridItemData = JsonUtility.FromJson<ProceduralGridItemData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
				}
				if (proceduralGridItemData != null)
				{
					ItemInstance itemInstance = ItemDeserializer.LoadItem(proceduralGridItemData.ItemString);
					if (itemInstance == null)
					{
						return null;
					}
					List<CoordinateProceduralTilePair> list = new List<CoordinateProceduralTilePair>();
					for (int i = 0; i < proceduralGridItemData.FootprintMatches.Length; i++)
					{
						CoordinateProceduralTilePair item = default(CoordinateProceduralTilePair);
						item.coord = new Coordinate(Mathf.RoundToInt(proceduralGridItemData.FootprintMatches[i].FootprintCoordinate.x), Mathf.RoundToInt(proceduralGridItemData.FootprintMatches[i].FootprintCoordinate.y));
						item.tileIndex = proceduralGridItemData.FootprintMatches[i].TileIndex;
						BuildableItem @object = GUIDManager.GetObject<BuildableItem>(new Guid(proceduralGridItemData.FootprintMatches[i].TileOwnerGUID));
						if (@object == null)
						{
							Debug.LogError("Failed to find tile parent for " + proceduralGridItemData.FootprintMatches[i].TileOwnerGUID);
							return null;
						}
						item.tileParent = @object.NetworkObject;
						list.Add(item);
					}
					return Singleton<BuildManager>.Instance.CreateProceduralGridItem(itemInstance, proceduralGridItemData.Rotation, list, proceduralGridItemData.GUID);
				}
			}
			return null;
		}
	}
}
