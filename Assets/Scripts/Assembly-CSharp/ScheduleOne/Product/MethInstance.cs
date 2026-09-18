using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Packaging;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product.Packaging;
using UnityEngine;

namespace ScheduleOne.Product
{
	[Serializable]
	public class MethInstance : ProductItemInstance
	{
		public MethInstance()
		{
		}

		public MethInstance(ItemDefinition definition, int quantity, EQuality quality, PackagingDefinition packaging = null)
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
			return new MethInstance(base.Definition, quantity, Quality, base.AppliedPackaging);
		}

		public override void SetupPackagingVisuals(FilledPackagingVisuals visuals)
		{
			base.SetupPackagingVisuals(visuals);
			if (visuals == null)
			{
				Console.LogError("MethInstance: visuals is null!");
				return;
			}
			MethDefinition methDefinition = base.Definition as MethDefinition;
			if (methDefinition == null)
			{
				Console.LogError("MethInstance: definition is null! Type: " + base.Definition);
				return;
			}
			MeshRenderer[] crystalMeshes = visuals.methVisuals.CrystalMeshes;
			for (int i = 0; i < crystalMeshes.Length; i++)
			{
				crystalMeshes[i].material = methDefinition.CrystalMaterial;
			}
			visuals.methVisuals.Container.gameObject.SetActive(value: true);
		}

		public override ItemData GetItemData()
		{
			return new MethData(base.Definition.ID, Quantity, Quality.ToString(), PackagingID);
		}
	}
}
