using System;
using System.Collections.Generic;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Management
{
	public class CleanerConfiguration : EntityConfiguration
	{
		public ObjectField Bed;

		public ObjectListField Bins;

		public Cleaner cleaner { get; protected set; }

		public List<TrashContainerItem> binItems { get; private set; } = new List<TrashContainerItem>();


		public BedItem bedItem { get; private set; }

		public CleanerConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, Cleaner _cleaner)
			: base(replicator, configurable)
		{
			cleaner = _cleaner;
			Bed = new ObjectField(this);
			Bed.TypeRequirements = new List<Type> { typeof(BedItem) };
			Bed.onObjectChanged.AddListener(BedChanged);
			Bed.objectFilter = BedItem.IsBedValid;
			Bins = new ObjectListField(this);
			Bins.MaxItems = 3;
			Bins.onListChanged.AddListener(delegate
			{
				InvokeChanged();
			});
			Bins.onListChanged.AddListener(AssignedBinsChanged);
			Bins.objectFilter = IsObjValid;
		}

		private bool IsObjValid(BuildableItem obj, out string reason)
		{
			TrashContainerItem trashContainerItem = obj as TrashContainerItem;
			if (trashContainerItem == null)
			{
				reason = string.Empty;
				return true;
			}
			if (!trashContainerItem.UsableByCleaners)
			{
				reason = "This trash can is not usable by cleaners.";
				return false;
			}
			reason = string.Empty;
			return true;
		}

		public void AssignedBinsChanged(List<BuildableItem> objects)
		{
			for (int i = 0; i < binItems.Count; i++)
			{
				if (!objects.Contains(binItems[i]))
				{
					binItems.RemoveAt(i);
					i--;
				}
			}
			for (int j = 0; j < objects.Count; j++)
			{
				if (!binItems.Contains(objects[j] as TrashContainerItem))
				{
					binItems.Add(objects[j] as TrashContainerItem);
				}
			}
		}

		public override bool ShouldSave()
		{
			if (Bed.SelectedObject != null)
			{
				return true;
			}
			if (Bins.SelectedObjects.Count > 0)
			{
				return true;
			}
			return base.ShouldSave();
		}

		public override string GetSaveString()
		{
			return new CleanerConfigurationData(Bed.GetData(), Bins.GetData()).GetJson();
		}

		private void BedChanged(BuildableItem newItem)
		{
			BedItem bedItem = this.bedItem;
			if (bedItem != null)
			{
				bedItem.Bed.SetAssignedEmployee(null);
			}
			this.bedItem = ((newItem != null) ? (newItem as BedItem) : null);
			if (this.bedItem != null)
			{
				this.bedItem.Bed.SetAssignedEmployee(cleaner);
			}
			InvokeChanged();
		}
	}
}
