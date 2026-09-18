using System;
using System.Collections.Generic;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Management
{
	public class ChemistConfiguration : EntityConfiguration
	{
		public ObjectField Bed;

		public ObjectListField Stations;

		public List<ChemistryStation> ChemStations = new List<ChemistryStation>();

		public List<LabOven> LabOvens = new List<LabOven>();

		public List<Cauldron> Cauldrons = new List<Cauldron>();

		public List<MixingStation> MixStations = new List<MixingStation>();

		public int TotalStations => ChemStations.Count + LabOvens.Count + Cauldrons.Count + MixStations.Count;

		public Chemist chemist { get; protected set; }

		public BedItem bedItem { get; private set; }

		public ChemistConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, Chemist _chemist)
			: base(replicator, configurable)
		{
			chemist = _chemist;
			Bed = new ObjectField(this);
			Bed.TypeRequirements = new List<Type> { typeof(BedItem) };
			Bed.onObjectChanged.AddListener(BedChanged);
			Bed.objectFilter = BedItem.IsBedValid;
			Stations = new ObjectListField(this);
			Stations.MaxItems = 3;
			Stations.TypeRequirements = new List<Type>
			{
				typeof(ChemistryStation),
				typeof(LabOven),
				typeof(Cauldron),
				typeof(MixingStation)
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
			if (obj is ChemistryStation)
			{
				ChemistryStationConfiguration chemistryStationConfiguration = (obj as ChemistryStation).Configuration as ChemistryStationConfiguration;
				if (chemistryStationConfiguration.AssignedChemist.SelectedNPC != null && chemistryStationConfiguration.AssignedChemist.SelectedNPC != chemist)
				{
					reason = "Already assigned to " + chemistryStationConfiguration.AssignedChemist.SelectedNPC.fullName;
					return false;
				}
				return true;
			}
			if (obj is LabOven)
			{
				LabOvenConfiguration labOvenConfiguration = (obj as LabOven).Configuration as LabOvenConfiguration;
				if (labOvenConfiguration.AssignedChemist.SelectedNPC != null && labOvenConfiguration.AssignedChemist.SelectedNPC != chemist)
				{
					reason = "Already assigned to " + labOvenConfiguration.AssignedChemist.SelectedNPC.fullName;
					return false;
				}
				return true;
			}
			if (obj is Cauldron)
			{
				CauldronConfiguration cauldronConfiguration = (obj as Cauldron).Configuration as CauldronConfiguration;
				if (cauldronConfiguration.AssignedChemist.SelectedNPC != null && cauldronConfiguration.AssignedChemist.SelectedNPC != chemist)
				{
					reason = "Already assigned to " + cauldronConfiguration.AssignedChemist.SelectedNPC.fullName;
					return false;
				}
				return true;
			}
			if (obj is MixingStation)
			{
				MixingStationConfiguration mixingStationConfiguration = (obj as MixingStation).Configuration as MixingStationConfiguration;
				if (mixingStationConfiguration.AssignedChemist.SelectedNPC != null && mixingStationConfiguration.AssignedChemist.SelectedNPC != chemist)
				{
					reason = "Already assigned to " + mixingStationConfiguration.AssignedChemist.SelectedNPC.fullName;
					return false;
				}
				return true;
			}
			return false;
		}

		public void AssignedStationsChanged(List<BuildableItem> objects)
		{
			for (int i = 0; i < ChemStations.Count; i++)
			{
				if (!objects.Contains(ChemStations[i]))
				{
					ChemistryStation chemistryStation = ChemStations[i];
					ChemStations.RemoveAt(i);
					i--;
					if ((chemistryStation.Configuration as ChemistryStationConfiguration).AssignedChemist.SelectedNPC == chemist)
					{
						(chemistryStation.Configuration as ChemistryStationConfiguration).AssignedChemist.SetNPC(null, network: false);
					}
				}
			}
			for (int j = 0; j < LabOvens.Count; j++)
			{
				if (!objects.Contains(LabOvens[j]))
				{
					LabOven labOven = LabOvens[j];
					LabOvens.RemoveAt(j);
					j--;
					if ((labOven.Configuration as LabOvenConfiguration).AssignedChemist.SelectedNPC == chemist)
					{
						(labOven.Configuration as LabOvenConfiguration).AssignedChemist.SetNPC(null, network: false);
					}
				}
			}
			for (int k = 0; k < Cauldrons.Count; k++)
			{
				if (!objects.Contains(Cauldrons[k]))
				{
					Cauldron cauldron = Cauldrons[k];
					Cauldrons.RemoveAt(k);
					k--;
					if ((cauldron.Configuration as CauldronConfiguration).AssignedChemist.SelectedNPC == chemist)
					{
						(cauldron.Configuration as CauldronConfiguration).AssignedChemist.SetNPC(null, network: false);
					}
				}
			}
			for (int l = 0; l < MixStations.Count; l++)
			{
				if (!objects.Contains(MixStations[l]))
				{
					MixingStation mixingStation = MixStations[l];
					MixStations.RemoveAt(l);
					l--;
					if ((mixingStation.Configuration as MixingStationConfiguration).AssignedChemist.SelectedNPC == chemist)
					{
						(mixingStation.Configuration as MixingStationConfiguration).AssignedChemist.SetNPC(null, network: false);
					}
				}
			}
			for (int m = 0; m < objects.Count; m++)
			{
				if (objects[m] is ChemistryStation && !ChemStations.Contains(objects[m] as ChemistryStation))
				{
					ChemistryStation chemistryStation2 = objects[m] as ChemistryStation;
					ChemStations.Add(chemistryStation2);
					if ((chemistryStation2.Configuration as ChemistryStationConfiguration).AssignedChemist.SelectedNPC != chemist)
					{
						(chemistryStation2.Configuration as ChemistryStationConfiguration).AssignedChemist.SetNPC(chemist, network: false);
					}
				}
				if (objects[m] is LabOven && !LabOvens.Contains(objects[m] as LabOven))
				{
					LabOven labOven2 = objects[m] as LabOven;
					LabOvens.Add(labOven2);
					if ((labOven2.Configuration as LabOvenConfiguration).AssignedChemist.SelectedNPC != chemist)
					{
						(labOven2.Configuration as LabOvenConfiguration).AssignedChemist.SetNPC(chemist, network: false);
					}
				}
				if (objects[m] is Cauldron && !Cauldrons.Contains(objects[m] as Cauldron))
				{
					Cauldron cauldron2 = objects[m] as Cauldron;
					Cauldrons.Add(cauldron2);
					if ((cauldron2.Configuration as CauldronConfiguration).AssignedChemist.SelectedNPC != chemist)
					{
						(cauldron2.Configuration as CauldronConfiguration).AssignedChemist.SetNPC(chemist, network: false);
					}
				}
				if (objects[m] is MixingStation && !MixStations.Contains(objects[m] as MixingStation))
				{
					MixingStation mixingStation2 = objects[m] as MixingStation;
					MixStations.Add(mixingStation2);
					if ((mixingStation2.Configuration as MixingStationConfiguration).AssignedChemist.SelectedNPC != chemist)
					{
						(mixingStation2.Configuration as MixingStationConfiguration).AssignedChemist.SetNPC(chemist, network: false);
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
			if (ChemStations.Count > 0)
			{
				return true;
			}
			if (LabOvens.Count > 0)
			{
				return true;
			}
			if (Cauldrons.Count > 0)
			{
				return true;
			}
			return base.ShouldSave();
		}

		public override string GetSaveString()
		{
			return new ChemistConfigurationData(Bed.GetData(), Stations.GetData()).GetJson();
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
				this.bedItem.Bed.SetAssignedEmployee(chemist);
			}
			InvokeChanged();
		}
	}
}
