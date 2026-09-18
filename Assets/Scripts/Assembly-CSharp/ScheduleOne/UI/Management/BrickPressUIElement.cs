using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;

namespace ScheduleOne.UI.Management
{
	public class BrickPressUIElement : WorldspaceUIElement
	{
		public BrickPress AssignedPress { get; protected set; }

		public void Initialize(BrickPress press)
		{
			AssignedPress = press;
			AssignedPress.Configuration.onChanged.AddListener(RefreshUI);
			RefreshUI();
			base.gameObject.SetActive(value: false);
		}

		protected virtual void RefreshUI()
		{
			BrickPressConfiguration brickPressConfiguration = AssignedPress.Configuration as BrickPressConfiguration;
			SetAssignedNPC(brickPressConfiguration.AssignedPackager.SelectedNPC);
		}
	}
}
