using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;

namespace ScheduleOne.UI.Management
{
	public class LabOvenUIElement : WorldspaceUIElement
	{
		public LabOven AssignedOven { get; protected set; }

		public void Initialize(LabOven oven)
		{
			AssignedOven = oven;
			AssignedOven.Configuration.onChanged.AddListener(RefreshUI);
			RefreshUI();
			base.gameObject.SetActive(value: false);
		}

		protected virtual void RefreshUI()
		{
			LabOvenConfiguration labOvenConfiguration = AssignedOven.Configuration as LabOvenConfiguration;
			SetAssignedNPC(labOvenConfiguration.AssignedChemist.SelectedNPC);
		}
	}
}
