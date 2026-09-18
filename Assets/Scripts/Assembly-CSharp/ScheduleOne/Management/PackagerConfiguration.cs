using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Packaging;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Management
{
	public class PackagerConfiguration : EntityConfiguration
	{
		public ObjectField Bed;

		public ObjectListField Stations;

		public List<PackagingStation> AssignedStations = new List<PackagingStation>();

		public List<BrickPress> AssignedBrickPresses = new List<BrickPress>();

		public int AssignedStationCount => AssignedStations.Count + AssignedBrickPresses.Count;

		public Packager packager { get; protected set; }

		public BedItem bedItem { get; private set; }

		public PackagerConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, Packager _botanist)
			: base(replicator, configurable)
		{
			packager = _botanist;
			Bed = new ObjectField(this);
			Bed.TypeRequirements = new List<Type> { typeof(BedItem) };
			Bed.onObjectChanged.AddListener(BedChanged);
			Bed.objectFilter = BedItem.IsBedValid;
			Stations = new ObjectListField(this);
			Stations.MaxItems = packager.MaxAssignedStations;
			Stations.TypeRequirements = new List<Type>
			{
				typeof(PackagingStation),
				typeof(PackagingStationMk2),
				typeof(BrickPress)
			};
			Stations.onListChanged.AddListener(delegate
			{
				InvokeChanged();
			});
			Stations.onListChanged.AddListener(AssignedStationsChanged);
			Stations.objectFilter = IsStationValid;
		}

		private bool IsStationValid(BuildableItem obj, out string reason)
		{
			reason = string.Empty;
			if (obj is PackagingStation)
			{
				PackagingStationConfiguration packagingStationConfiguration = (obj as PackagingStation).Configuration as PackagingStationConfiguration;
				if (packagingStationConfiguration.AssignedPackager.SelectedNPC != null && packagingStationConfiguration.AssignedPackager.SelectedNPC != packager)
				{
					reason = "Already assigned to " + packagingStationConfiguration.AssignedPackager.SelectedNPC.fullName;
					return false;
				}
				return true;
			}
			if (obj is BrickPress)
			{
				BrickPressConfiguration brickPressConfiguration = (obj as BrickPress).Configuration as BrickPressConfiguration;
				if (brickPressConfiguration.AssignedPackager.SelectedNPC != null && brickPressConfiguration.AssignedPackager.SelectedNPC != packager)
				{
					reason = "Already assigned to " + brickPressConfiguration.AssignedPackager.SelectedNPC.fullName;
					return false;
				}
				return true;
			}
			return false;
		}

		public void AssignedStationsChanged(List<BuildableItem> objects)
		{
			for (int i = 0; i < AssignedStations.Count; i++)
			{
				if (!objects.Contains(AssignedStations[i]))
				{
					PackagingStation packagingStation = AssignedStations[i];
					AssignedStations.RemoveAt(i);
					i--;
					if ((packagingStation.Configuration as PackagingStationConfiguration).AssignedPackager.SelectedNPC == packager)
					{
						(packagingStation.Configuration as PackagingStationConfiguration).AssignedPackager.SetNPC(null, network: false);
					}
				}
			}
			for (int j = 0; j < AssignedBrickPresses.Count; j++)
			{
				if (!objects.Contains(AssignedBrickPresses[j]))
				{
					BrickPress brickPress = AssignedBrickPresses[j];
					AssignedBrickPresses.RemoveAt(j);
					j--;
					if ((brickPress.Configuration as BrickPressConfiguration).AssignedPackager.SelectedNPC == packager)
					{
						(brickPress.Configuration as BrickPressConfiguration).AssignedPackager.SetNPC(null, network: false);
					}
				}
			}
			for (int k = 0; k < objects.Count; k++)
			{
				if (objects[k] is PackagingStation)
				{
					if (!AssignedStations.Contains(objects[k]))
					{
						PackagingStation packagingStation2 = objects[k] as PackagingStation;
						AssignedStations.Add(packagingStation2);
						if ((packagingStation2.Configuration as PackagingStationConfiguration).AssignedPackager.SelectedNPC != packager)
						{
							(packagingStation2.Configuration as PackagingStationConfiguration).AssignedPackager.SetNPC(packager, network: false);
						}
					}
				}
				else if (objects[k] is BrickPress && !AssignedBrickPresses.Contains(objects[k]))
				{
					BrickPress brickPress2 = objects[k] as BrickPress;
					AssignedBrickPresses.Add(brickPress2);
					if ((brickPress2.Configuration as BrickPressConfiguration).AssignedPackager.SelectedNPC != packager)
					{
						(brickPress2.Configuration as BrickPressConfiguration).AssignedPackager.SetNPC(packager, network: false);
					}
				}
			}
		}

		public override bool ShouldSave()
		{
			if (Bed.SelectedObject != null)
			{
				return true;
			}
			if (AssignedStations.Count > 0)
			{
				return true;
			}
			return base.ShouldSave();
		}

		public override string GetSaveString()
		{
			return new PackagerConfigurationData(Bed.GetData(), Stations.GetData()).GetJson();
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
				this.bedItem.Bed.SetAssignedEmployee(packager);
			}
			InvokeChanged();
		}
	}
}
