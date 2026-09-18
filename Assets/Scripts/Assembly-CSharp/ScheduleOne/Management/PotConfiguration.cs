using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Management
{
	public class PotConfiguration : EntityConfiguration
	{
		public ItemField Seed;

		public ItemField Additive1;

		public ItemField Additive2;

		public ItemField Additive3;

		public NPCField AssignedBotanist;

		public ObjectField Destination;

		public Pot Pot { get; protected set; }

		public TransitRoute DestinationRoute { get; protected set; }

		public PotConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, Pot pot)
			: base(replicator, configurable)
		{
			Pot = pot;
			Seed = new ItemField(this);
			Seed.CanSelectNone = true;
			List<ItemDefinition> options = Singleton<ManagementUtilities>.Instance.SeedDefintions.Cast<ItemDefinition>().ToList();
			Seed.Options = options;
			Seed.onItemChanged.AddListener(delegate
			{
				InvokeChanged();
			});
			List<ItemDefinition> options2 = Singleton<ManagementUtilities>.Instance.AdditiveDefinitions.Cast<ItemDefinition>().ToList();
			Additive1 = new ItemField(this);
			Additive1.CanSelectNone = true;
			Additive1.Options = options2;
			Additive1.onItemChanged.AddListener(delegate
			{
				InvokeChanged();
			});
			Additive2 = new ItemField(this);
			Additive2.CanSelectNone = true;
			Additive2.Options = options2;
			Additive2.onItemChanged.AddListener(delegate
			{
				InvokeChanged();
			});
			Additive3 = new ItemField(this);
			Additive3.CanSelectNone = true;
			Additive3.Options = options2;
			Additive3.onItemChanged.AddListener(delegate
			{
				InvokeChanged();
			});
			AssignedBotanist = new NPCField(this);
			AssignedBotanist.TypeRequirement = typeof(Botanist);
			AssignedBotanist.onNPCChanged.AddListener(delegate
			{
				InvokeChanged();
			});
			Destination = new ObjectField(this);
			Destination.objectFilter = DestinationFilter;
			Destination.onObjectChanged.AddListener(delegate
			{
				InvokeChanged();
			});
			Destination.onObjectChanged.AddListener(DestinationChanged);
			Destination.DrawTransitLine = true;
		}

		public override void Destroy()
		{
			base.Destroy();
			if (AssignedBotanist.SelectedNPC != null)
			{
				((AssignedBotanist.SelectedNPC as Botanist).Configuration as BotanistConfiguration).Pots.RemoveItem(Pot);
			}
			if (DestinationRoute != null)
			{
				DestinationRoute.Destroy();
				DestinationRoute = null;
			}
		}

		private void DestinationChanged(BuildableItem item)
		{
			if (DestinationRoute != null)
			{
				DestinationRoute.Destroy();
				DestinationRoute = null;
			}
			if (Destination.SelectedObject != null)
			{
				DestinationRoute = new TransitRoute(Pot, Destination.SelectedObject as ITransitEntity);
				if (base.IsSelected)
				{
					DestinationRoute.SetVisualsActive(active: true);
				}
			}
			else
			{
				DestinationRoute = null;
			}
		}

		public bool DestinationFilter(BuildableItem obj, out string reason)
		{
			reason = "";
			if (obj is ITransitEntity && (obj as ITransitEntity).Selectable && obj != Pot)
			{
				return true;
			}
			return false;
		}

		public override void Selected()
		{
			base.Selected();
			if (DestinationRoute != null)
			{
				DestinationRoute.SetVisualsActive(active: true);
			}
		}

		public override void Deselected()
		{
			base.Deselected();
			if (DestinationRoute != null)
			{
				DestinationRoute.SetVisualsActive(active: false);
			}
		}

		public override bool ShouldSave()
		{
			if (Seed.SelectedItem != null)
			{
				return true;
			}
			if (Additive1.SelectedItem != null)
			{
				return true;
			}
			if (Additive2.SelectedItem != null)
			{
				return true;
			}
			if (Additive3.SelectedItem != null)
			{
				return true;
			}
			if (AssignedBotanist.SelectedNPC != null)
			{
				return true;
			}
			if (Destination.SelectedObject != null)
			{
				return true;
			}
			return base.ShouldSave();
		}

		public override string GetSaveString()
		{
			return new PotConfigurationData(Seed.GetData(), Additive1.GetData(), Additive2.GetData(), Additive3.GetData(), Destination.GetData()).GetJson();
		}
	}
}
