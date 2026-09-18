using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Management
{
	public class CauldronConfiguration : EntityConfiguration
	{
		public NPCField AssignedChemist;

		public ObjectField Destination;

		public Cauldron Station { get; protected set; }

		public TransitRoute DestinationRoute { get; protected set; }

		public CauldronConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, Cauldron cauldron)
			: base(replicator, configurable)
		{
			Station = cauldron;
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
				DestinationRoute = new TransitRoute(Station, Destination.SelectedObject as ITransitEntity);
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
			if (obj is ITransitEntity && (obj as ITransitEntity).Selectable && obj != Station)
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
			return new CauldronConfigurationData(Destination.GetData()).GetJson();
		}
	}
}
