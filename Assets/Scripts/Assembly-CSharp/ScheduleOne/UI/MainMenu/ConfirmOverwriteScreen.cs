namespace ScheduleOne.UI.MainMenu
{
	public class ConfirmOverwriteScreen : MainMenuScreen
	{
		public SetupScreen SetupScreen;

		private int slotIndex;

		public void Initialize(int index)
		{
			slotIndex = index;
		}

		public void Confirm()
		{
			Close(openPrevious: false);
			SetupScreen.Initialize(slotIndex);
			SetupScreen.Open(closePrevious: false);
		}
	}
}
