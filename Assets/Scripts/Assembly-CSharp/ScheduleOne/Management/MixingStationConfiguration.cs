using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Management
{
	public class MixingStationConfiguration : EntityConfiguration
	{
		public NPCField AssignedChemist;

		public ObjectField Destination;

		public NumberField StartThrehold;

		public MixingStation station { get; protected set; }

		public TransitRoute DestinationRoute { get; protected set; }

		public MixingStationConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, MixingStation station)
			: base(replicator, configurable)
		{
			this.station = station;
			AssignedChemist = new NPCField(this);
			AssignedChemist.TypeRequirement = typeof(Chemist);
			AssignedChemist.onNPCChanged.AddListener(delegate
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
			StartThrehold = new NumberField(this);
			StartThrehold.Configure(1f, 10f, wholeNumbers: true);
			StartThrehold.SetValue(1f, network: true);
			StartThrehold.onItemChanged.AddListener(delegate
			{
				InvokeChanged();
			});
		}

		public override void Destroy()
		{
			base.Destroy();
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
				DestinationRoute = new TransitRoute(station, Destination.SelectedObject as ITransitEntity);
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
			if (obj is ITransitEntity && (obj as ITransitEntity).Selectable && obj != station)
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
			if (Destination.SelectedObject != null)
			{
				return true;
			}
			return base.ShouldSave();
		}

		public override string GetSaveString()
		{
			return new MixingStationConfigurationData(Destination.GetData(), StartThrehold.GetData()).GetJson();
		}
	}
}
