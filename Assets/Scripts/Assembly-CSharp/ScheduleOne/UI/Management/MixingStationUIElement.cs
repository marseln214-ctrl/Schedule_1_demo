using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;

namespace ScheduleOne.UI.Management
{
	public class MixingStationUIElement : WorldspaceUIElement
	{
		public MixingStation AssignedStation { get; protected set; }

		public void Initialize(MixingStation station)
		{
			AssignedStation = station;
			AssignedStation.Configuration.onChanged.AddListener(RefreshUI);
			RefreshUI();
			base.gameObject.SetActive(value: false);
		}

		protected virtual void RefreshUI()
		{
			MixingStationConfiguration mixingStationConfiguration = AssignedStation.Configuration as MixingStationConfiguration;
			SetAssignedNPC(mixingStationConfiguration.AssignedChemist.SelectedNPC);
		}
	}
}
