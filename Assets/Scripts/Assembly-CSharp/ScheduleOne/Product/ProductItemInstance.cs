using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Serializing.Helping;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.Packaging;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Properties;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Product
{
	[Serializable]
	public class ProductItemInstance : QualityItemInstance
	{
		public string PackagingID = string.Empty;

		[CodegenExclude]
		private PackagingDefinition packaging;

		[CodegenExclude]
		public PackagingDefinition AppliedPackaging
		{
			get
			{
				if (packaging == null && PackagingID != string.Empty)
				{
					packaging = Registry.GetItem(PackagingID) as PackagingDefinition;
					if (packaging == null)
					{
						Console.LogError("Failed to load packaging with ID (" + PackagingID + ")");
					}
				}
				return packaging;
			}
		}

		[CodegenExclude]
		public int Amount
		{
			get
			{
				if (!(AppliedPackaging != null))
				{
					return 1;
				}
				return AppliedPackaging.Quantity;
			}
		}

		public override string Name => base.Name + ((packaging == null) ? " (Unpackaged)" : string.Empty);

		[CodegenExclude]
		public override Equippable Equippable => GetEquippable();

		[CodegenExclude]
		public override StoredItem StoredItem => GetStoredItem();

		[CodegenExclude]
		public override Sprite Icon => GetIcon();

		public ProductItemInstance()
		{
		}

		public ProductItemInstance(ItemDefinition definition, int quantity, EQuality quality, PackagingDefinition _packaging = null)
			: base(definition, quantity, quality)
		{
			packaging = _packaging;
			if (packaging != null)
			{
				PackagingID = packaging.ID;
			}
			else
			{
				PackagingID = string.Empty;
			}
		}

		public override bool CanStackWith(ItemInstance other, bool checkQuantities = true)
		{
			if (!(other is ProductItemInstance))
			{
				return false;
			}
			if ((other as ProductItemInstance).AppliedPackaging != null)
			{
				if (AppliedPackaging == null)
				{
					return false;
				}
				if ((other as ProductItemInstance).AppliedPackaging.ID != AppliedPackaging.ID)
				{
					return false;
				}
			}
			else if (AppliedPackaging != null)
			{
				return false;
			}
			return base.CanStackWith(other, checkQuantities);
		}

		public override ItemInstance GetCopy(int overrideQuantity = -1)
		{
			int quantity = Quantity;
			if (overrideQuantity != -1)
			{
				quantity = overrideQuantity;
			}
			return new ProductItemInstance(base.Definition, quantity, Quality, AppliedPackaging);
		}

		public virtual void SetPackaging(PackagingDefinition def)
		{
			packaging = def;
			if (packaging != null)
			{
				PackagingID = packaging.ID;
			}
			else
			{
				PackagingID = string.Empty;
			}
			if (onDataChanged != null)
			{
				onDataChanged();
			}
		}

		private Equippable GetEquippable()
		{
			if (AppliedPackaging != null)
			{
				return AppliedPackaging.Equippable_Filled;
			}
			return base.Equippable;
		}

		private StoredItem GetStoredItem()
		{
			if (AppliedPackaging != null)
			{
				return AppliedPackaging.StoredItem_Filled;
			}
			return base.StoredItem;
		}

		public virtual void SetupPackagingVisuals(FilledPackagingVisuals visuals)
		{
			visuals.ResetVisuals();
		}

		private Sprite GetIcon()
		{
			if (AppliedPackaging != null)
			{
				return Singleton<ProductIconManager>.Instance.GetIcon(ID, AppliedPackaging.ID);
			}
			return base.Icon;
		}

		public override ItemData GetItemData()
		{
			return new ProductItemData(ID, Quantity, Quality.ToString(), PackagingID);
		}

		public virtual float GetAddictiveness()
		{
			return (base.Definition as ProductDefinition).GetAddictiveness();
		}

		public float GetSimilarity(ProductDefinition definition, EQuality quality)
		{
			ProductDefinition productDefinition = base.Definition as ProductDefinition;
			float num = 0f;
			if (definition.DrugType == productDefinition.DrugType)
			{
				num = 0.4f;
			}
			int num2 = 0;
			for (int i = 0; i < definition.Properties.Count; i++)
			{
				if (productDefinition.HasProperty(definition.Properties[i]))
				{
					num2++;
				}
			}
			for (int j = 0; j < productDefinition.Properties.Count; j++)
			{
				if (!definition.HasProperty(productDefinition.Properties[j]))
				{
					num2--;
				}
			}
			float num3 = Mathf.Clamp01((float)num2 / (float)productDefinition.Properties.Count) * 0.3f;
			float num4 = Mathf.Clamp((float)Quality / (float)quality, 0f, 1f) * 0.3f;
			return Mathf.Clamp01(num + num3 + num4);
		}

		public virtual void ApplyEffectsToNPC(NPC npc)
		{
			List<ScheduleOne.Properties.Property> list = new List<ScheduleOne.Properties.Property>();
			list.AddRange((base.Definition as ProductDefinition).Properties);
			list = list.OrderBy((ScheduleOne.Properties.Property x) => x.Tier).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].ApplyToNPC(npc);
			}
		}

		public virtual void ClearEffectsFromNPC(NPC npc)
		{
			List<ScheduleOne.Properties.Property> list = new List<ScheduleOne.Properties.Property>();
			list.AddRange((base.Definition as ProductDefinition).Properties);
			list = list.OrderBy((ScheduleOne.Properties.Property x) => x.Tier).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].ClearFromNPC(npc);
			}
		}

		public virtual void ApplyEffectsToPlayer(Player player)
		{
			List<ScheduleOne.Properties.Property> list = new List<ScheduleOne.Properties.Property>();
			list.AddRange((base.Definition as ProductDefinition).Properties);
			list = list.OrderBy((ScheduleOne.Properties.Property x) => x.Tier).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].ApplyToPlayer(player);
			}
		}

		public virtual void ClearEffectsFromPlayer(Player Player)
		{
			List<ScheduleOne.Properties.Property> list = new List<ScheduleOne.Properties.Property>();
			list.AddRange((base.Definition as ProductDefinition).Properties);
			list = list.OrderBy((ScheduleOne.Properties.Property x) => x.Tier).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].ClearFromPlayer(Player);
			}
		}

		public override float GetMonetaryValue()
		{
			if (definition == null)
			{
				Console.LogWarning("ProductItemInstance.GetMonetaryValue() - Definition is null");
				return 0f;
			}
			return (definition as ProductDefinition).MarketValue * (float)Quantity;
		}
	}
}
