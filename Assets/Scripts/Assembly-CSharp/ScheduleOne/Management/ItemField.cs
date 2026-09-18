using System.Collections.Generic;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using UnityEngine.Events;

namespace ScheduleOne.Management
{
	public class ItemField : ConfigField
	{
		public bool CanSelectNone = true;

		public List<ItemDefinition> Options = new List<ItemDefinition>();

		public UnityEvent<ItemDefinition> onItemChanged = new UnityEvent<ItemDefinition>();

		public ItemDefinition SelectedItem { get; protected set; }

		public ItemField(EntityConfiguration parentConfig)
			: base(parentConfig)
		{
		}

		public void SetItem(ItemDefinition item, bool network)
		{
			SelectedItem = item;
			if (network)
			{
				base.ParentConfig.ReplicateField(this);
			}
			if (onItemChanged != null)
			{
				onItemChanged.Invoke(SelectedItem);
			}
		}

		public override bool IsValueDefault()
		{
			return SelectedItem == null;
		}

		public ItemFieldData GetData()
		{
			return new ItemFieldData((SelectedItem != null) ? SelectedItem.ID.ToString() : "");
		}

		public void Load(ItemFieldData data)
		{
			if (data != null && !string.IsNullOrEmpty(data.ItemID))
			{
				ItemDefinition item = Registry.GetItem(data.ItemID);
				if (item != null)
				{
					SetItem(item, network: true);
				}
			}
		}
	}
}
