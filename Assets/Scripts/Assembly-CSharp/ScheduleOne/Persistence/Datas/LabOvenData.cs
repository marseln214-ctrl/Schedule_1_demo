using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas
{
	public class LabOvenData : GridItemData
	{
		public ItemSet InputContents;

		public ItemSet OutputContents;

		public string CurrentIngredientID;

		public int CurrentIngredientQuantity;

		public EQuality CurrentIngredientQuality;

		public string CurrentProductID;

		public int CurrentCookProgress;

		public LabOvenData(Guid guid, ItemInstance item, int loadOrder, ScheduleOne.Tiles.Grid grid, Vector2 originCoordinate, int rotation, ItemSet inputContents, ItemSet outputContents, string ingredientID, int currentIngredientQuantity, EQuality ingredientQuality, string productID, int currentCookProgress)
			: base(guid, item, loadOrder, grid, originCoordinate, rotation)
		{
			InputContents = inputContents;
			OutputContents = outputContents;
			CurrentIngredientID = ingredientID;
			CurrentIngredientQuantity = currentIngredientQuantity;
			CurrentIngredientQuality = ingredientQuality;
			CurrentProductID = productID;
			CurrentCookProgress = currentCookProgress;
		}
	}
}
