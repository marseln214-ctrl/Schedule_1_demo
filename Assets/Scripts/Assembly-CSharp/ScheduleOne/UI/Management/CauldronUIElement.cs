using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;

namespace ScheduleOne.UI.Management
{
	public class CauldronUIElement : WorldspaceUIElement
	{
		public Cauldron AssignedCauldron { get; protected set; }

		public void Initialize(Cauldron cauldron)
		{
			AssignedCauldron = cauldron;
			AssignedCauldron.Configuration.onChanged.AddListener(RefreshUI);
			RefreshUI();
			base.gameObject.SetActive(value: false);
		}

		protected virtual void RefreshUI()
		{
			CauldronConfiguration cauldronConfiguration = AssignedCauldron.Configuration as CauldronConfiguration;
			SetAssignedNPC(cauldronConfiguration.AssignedChemist.SelectedNPC);
		}
	}
}
