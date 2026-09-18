using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Management
{
	public class BotanistConfiguration : EntityConfiguration
	{
		public ObjectField Bed;

		public ObjectField Supplies;

		public ObjectListField Pots;

		public List<Pot> AssignedPots = new List<Pot>();

		public Botanist botanist { get; protected set; }

		public BedItem bedItem { get; private set; }

		public BotanistConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, Botanist _botanist)
			: base(replicator, configurable)
		{
			botanist = _botanist;
			Bed = new ObjectField(this);
			Bed.TypeRequirements = new List<Type> { typeof(BedItem) };
			Bed.onObjectChanged.AddListener(BedChanged);
			Bed.objectFilter = BedItem.IsBedValid;
			Supplies = new ObjectField(this);
			Supplies.TypeRequirements = new List<Type> { typeof(PlaceableStorageEntity) };
			Supplies.onObjectChanged.AddListener(delegate
			{
				InvokeChanged();
			});
			Pots = new ObjectListField(this);
			Pots.MaxItems = botanist.MaxAssignedPots;
			Pots.TypeRequirements = new List<Type> { typeof(Pot) };
			Pots.onListChanged.AddListener(delegate
			{
				InvokeChanged();
			});
			Pots.onListChanged.AddListener(AssignedPotsChanged);
			Pots.objectFilter = IsPotValid;
		}

		private bool IsPotValid(BuildableItem obj, out string reason)
		{
			Pot pot = obj as Pot;
			if (pot == null)
			{
				reason = "Not a pot";
				return false;
			}
			PotConfiguration potConfiguration = pot.Configuration as PotConfiguration;
			if (potConfiguration.AssignedBotanist.SelectedNPC != null && potConfiguration.AssignedBotanist.SelectedNPC != botanist)
			{
				reason = "Already assigned to " + potConfiguration.AssignedBotanist.SelectedNPC.fullName;
				return false;
			}
			reason = string.Empty;
			return true;
		}

		public void AssignedPotsChanged(List<BuildableItem> objects)
		{
			for (int i = 0; i < AssignedPots.Count; i++)
			{
				if (!objects.Contains(AssignedPots[i]))
				{
					Pot pot = AssignedPots[i];
					AssignedPots.RemoveAt(i);
					i--;
					if ((pot.Configuration as PotConfiguration).AssignedBotanist.SelectedNPC == botanist)
					{
						(pot.Configuration as PotConfiguration).AssignedBotanist.SetNPC(null, network: false);
					}
				}
			}
			for (int j = 0; j < objects.Count; j++)
			{
				if (!AssignedPots.Contains(objects[j]))
				{
					Pot pot2 = objects[j] as Pot;
					AssignedPots.Add(pot2);
					if ((pot2.Configuration as PotConfiguration).AssignedBotanist.SelectedNPC != botanist)
					{
						(pot2.Configuration as PotConfiguration).AssignedBotanist.SetNPC(botanist, network: false);
					}
				}
			}
		}

		public override bool ShouldSave()
		{
			if (AssignedPots.Count > 0)
			{
				return true;
			}
			if (Supplies.SelectedObject != null)
			{
				return true;
			}
			if (Bed.SelectedObject != null)
			{
				return true;
			}
			return base.ShouldSave();
		}

		public override string GetSaveString()
		{
			return new BotanistConfigurationData(Bed.GetData(), Supplies.GetData(), Pots.GetData()).GetJson();
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
				this.bedItem.Bed.SetAssignedEmployee(botanist);
			}
			InvokeChanged();
		}
	}
}
