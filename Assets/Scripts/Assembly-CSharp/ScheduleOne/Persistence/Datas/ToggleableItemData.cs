using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas
{
	public class ToggleableItemData : GridItemData
	{
		public bool IsOn;

		public ToggleableItemData(Guid guid, ItemInstance item, int loadOrder, ScheduleOne.Tiles.Grid grid, Vector2 originCoordinate, int rotation, bool isOn)
			: base(guid, item, loadOrder, grid, originCoordinate, rotation)
		{
			IsOn = isOn;
		}
	}
}
