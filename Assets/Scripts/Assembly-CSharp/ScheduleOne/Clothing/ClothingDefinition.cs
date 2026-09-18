using System;
using System.Collections.Generic;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Clothing
{
	[Serializable]
	[CreateAssetMenu(fileName = "ClothingDefinition", menuName = "ScriptableObjects/ClothingDefinition", order = 1)]
	public class ClothingDefinition : StorableItemDefinition
	{
		public EClothingSlot Slot;

		public EClothingApplicationType ApplicationType;

		public string ClothingAssetPath = "Path/To/Clothing/Asset";

		public bool Colorable = true;

		public List<EClothingSlot> SlotsToBlock = new List<EClothingSlot>();

		public override ItemInstance GetDefaultInstance(int quantity = 1)
		{
			return new ClothingInstance(this, quantity, EClothingColor.White);
		}
	}
}
