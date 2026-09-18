using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.UI.Stations;

namespace ScheduleOne.Management
{
	public class ChemistryStationConfiguration : EntityConfiguration
	{
		public NPCField AssignedChemist;

		public StationRecipeField Recipe;

		public ObjectField Destination;

		public ChemistryStation Station { get; protected set; }

		public TransitRoute DestinationRoute { get; protected set; }

		public ChemistryStationConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, ChemistryStation station)
			: base(replicator, configurable)
		{
			Station = station;
			AssignedChemist = new NPCField(this);
			AssignedChemist.onNPCChanged.AddListener(delegate
			{
				InvokeChanged();
			});
			Recipe = new StationRecipeField(this);
			Recipe.Options = Singleton<ChemistryStationCanvas>.Instance.Recipes;
			Recipe.onRecipeChanged.AddListener(delegate
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
			return new ChemistryStationConfigurationData(Recipe.GetData(), Destination.GetData()).GetJson();
		}
	}
}
