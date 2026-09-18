using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Packaging;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product.Packaging;
using UnityEngine;

namespace ScheduleOne.Product
{
	[Serializable]
	public class CocaineInstance : ProductItemInstance
	{
		public CocaineInstance()
		{
		}

		public CocaineInstance(ItemDefinition definition, int quantity, EQuality quality, PackagingDefinition packaging = null)
			: base(definition, quantity, quality, packaging)
		{
		}

		public override ItemInstance GetCopy(int overrideQuantity = -1)
		{
			int quantity = Quantity;
			if (overrideQuantity != -1)
			{
				quantity = overrideQuantity;
			}
			return new CocaineInstance(base.Definition, quantity, Quality, base.AppliedPackaging);
		}

		public override void SetupPackagingVisuals(FilledPackagingVisuals visuals)
		{
			base.SetupPackagingVisuals(visuals);
			if (visuals == null)
			{
				Console.LogError("CocaineInstance: visuals is null!");
				return;
			}
			CocaineDefinition cocaineDefinition = base.Definition as CocaineDefinition;
			if (cocaineDefinition == null)
			{
				Console.LogError("CocaineInstance: definition is null! Type: " + base.Definition);
				return;
			}
			MeshRenderer[] rockMeshes = visuals.cocaineVisuals.RockMeshes;
			for (int i = 0; i < rockMeshes.Length; i++)
			{
				rockMeshes[i].material = cocaineDefinition.RockMaterial;
			}
			visuals.cocaineVisuals.Container.gameObject.SetActive(value: true);
		}

		public override ItemData GetItemData()
		{
			return new CocaineData(base.Definition.ID, Quantity, Quality.ToString(), PackagingID);
		}
	}
}
