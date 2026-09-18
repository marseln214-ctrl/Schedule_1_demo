using System;
using System.Collections.Generic;
using ScheduleOne.EntityFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.UI.Management;
using UnityEngine.Events;

namespace ScheduleOne.Management
{
	public class ObjectField : ConfigField
	{
		public BuildableItem SelectedObject;

		public UnityEvent<BuildableItem> onObjectChanged = new UnityEvent<BuildableItem>();

		public ObjectSelector.ObjectFilter objectFilter;

		public List<Type> TypeRequirements = new List<Type>();

		public bool DrawTransitLine;

		public ObjectField(EntityConfiguration parentConfig)
			: base(parentConfig)
		{
		}

		public void SetObject(BuildableItem obj, bool network)
		{
			if (!(SelectedObject == obj))
			{
				SelectedObject = obj;
				if (network)
				{
					base.ParentConfig.ReplicateField(this);
				}
				if (onObjectChanged != null)
				{
					onObjectChanged.Invoke(obj);
				}
			}
		}

		public override bool IsValueDefault()
		{
			return SelectedObject == null;
		}

		public void Load(ObjectFieldData data)
		{
			if (data != null && !string.IsNullOrEmpty(data.ObjectGUID))
			{
				BuildableItem @object = GUIDManager.GetObject<BuildableItem>(new Guid(data.ObjectGUID));
				if (@object != null)
				{
					SetObject(@object, network: true);
				}
			}
		}

		public ObjectFieldData GetData()
		{
			return new ObjectFieldData((SelectedObject != null) ? SelectedObject.GUID.ToString() : "");
		}
	}
}
