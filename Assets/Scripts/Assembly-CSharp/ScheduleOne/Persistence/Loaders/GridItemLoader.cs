using System;
using ScheduleOne.Building;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class GridItemLoader : BuildableItemLoader
	{
		public override string ItemType => typeof(GridItemData).Name;

		public override void Load(string mainPath)
		{
			LoadAndCreate(mainPath);
		}

		protected GridItem LoadAndCreate(string mainPath)
		{
			if (TryLoadFile(mainPath, "Data", out var contents))
			{
				GridItemData gridItemData = null;
				try
				{
					gridItemData = JsonUtility.FromJson<GridItemData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
				}
				if (gridItemData != null)
				{
					ItemInstance itemInstance = ItemDeserializer.LoadItem(gridItemData.ItemString);
					if (itemInstance == null)
					{
						return null;
					}
                    ScheduleOne.Tiles.Grid @object = GUIDManager.GetObject<ScheduleOne.Tiles.Grid>(new Guid(gridItemData.GridGUID));
					if (@object == null)
					{
						Console.LogWarning("Failed to find grid for " + gridItemData.GridGUID);
						return null;
					}
					return Singleton<BuildManager>.Instance.CreateGridItem(itemInstance, @object, gridItemData.OriginCoordinate, gridItemData.Rotation, gridItemData.GUID);
				}
			}
			return null;
		}
	}
}
