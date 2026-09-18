using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.EntityFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.UI.Management;
using UnityEngine.Events;

namespace ScheduleOne.Management
{
	public class ObjectListField : ConfigField
	{
		public List<BuildableItem> SelectedObjects = new List<BuildableItem>();

		public int MaxItems = 1;

		public ObjectSelector.ObjectFilter objectFilter;

		public List<Type> TypeRequirements = new List<Type>();

		public UnityEvent<List<BuildableItem>> onListChanged = new UnityEvent<List<BuildableItem>>();

		public ObjectListField(EntityConfiguration parentConfig)
			: base(parentConfig)
		{
		}

		public void SetList(List<BuildableItem> list, bool network)
		{
			if (!SelectedObjects.SequenceEqual(list))
			{
				SelectedObjects = new List<BuildableItem>();
				SelectedObjects.AddRange(list);
				if (network)
				{
					base.ParentConfig.ReplicateField(this);
				}
				if (onListChanged != null)
				{
					onListChanged.Invoke(list);
				}
			}
		}

		public void AddItem(BuildableItem item)
		{
			if (!SelectedObjects.Contains(item))
			{
				if (SelectedObjects.Count >= MaxItems)
				{
					Console.LogWarning(item.ItemInstance.Name + " cannot be added to " + base.ParentConfig.GetType().Name + " because the maximum number of items has been reached");
					return;
				}
				List<BuildableItem> list = new List<BuildableItem>(SelectedObjects);
				list.Add(item);
				SetList(list, network: true);
			}
		}

		public void RemoveItem(BuildableItem item)
		{
			if (SelectedObjects.Contains(item))
			{
				List<BuildableItem> list = new List<BuildableItem>(SelectedObjects);
				list.Remove(item);
				SetList(list, network: true);
			}
		}

		public override bool IsValueDefault()
		{
			return SelectedObjects.Count == 0;
		}

		public ObjectListFieldData GetData()
		{
			List<string> list = new List<string>();
			for (int i = 0; i < SelectedObjects.Count; i++)
			{
				list.Add(SelectedObjects[i].GUID.ToString());
			}
			return new ObjectListFieldData(list);
		}

		public void Load(ObjectListFieldData data)
		{
			if (data == null)
			{
				return;
			}
			List<BuildableItem> list = new List<BuildableItem>();
			for (int i = 0; i < data.ObjectGUIDs.Count; i++)
			{
				if (!string.IsNullOrEmpty(data.ObjectGUIDs[i]))
				{
					BuildableItem @object = GUIDManager.GetObject<BuildableItem>(new Guid(data.ObjectGUIDs[i]));
					if (@object != null)
					{
						list.Add(@object);
					}
				}
			}
			SetList(list, network: true);
		}
	}
}
