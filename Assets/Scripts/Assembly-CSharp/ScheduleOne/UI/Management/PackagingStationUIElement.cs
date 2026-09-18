using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;

namespace ScheduleOne.UI.Management
{
	public class PackagingStationUIElement : WorldspaceUIElement
	{
		public PackagingStation AssignedStation { get; protected set; }

		public void Initialize(PackagingStation pack)
		{
			AssignedStation = pack;
			AssignedStation.Configuration.onChanged.AddListener(RefreshUI);
			RefreshUI();
			base.gameObject.SetActive(value: false);
		}

		protected virtual void RefreshUI()
		{
			PackagingStationConfiguration packagingStationConfiguration = AssignedStation.Configuration as PackagingStationConfiguration;
			SetAssignedNPC(packagingStationConfiguration.AssignedPackager.SelectedNPC);
		}
	}
}
