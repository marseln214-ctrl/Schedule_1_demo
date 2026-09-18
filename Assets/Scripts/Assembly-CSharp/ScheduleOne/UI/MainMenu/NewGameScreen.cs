using ScheduleOne.Persistence;

namespace ScheduleOne.UI.MainMenu
{
	public class NewGameScreen : MainMenuScreen
	{
		public ConfirmOverwriteScreen ConfirmOverwriteScreen;

		public SetupScreen SetupScreen;

		public void SlotSelected(int slotIndex)
		{
			if (LoadManager.SaveGames[slotIndex] != null)
			{
				ConfirmOverwriteScreen.Initialize(slotIndex);
				ConfirmOverwriteScreen.Open(closePrevious: true);
			}
			else
			{
				SetupScreen.Initialize(slotIndex);
				SetupScreen.Open(closePrevious: false);
				Close(openPrevious: false);
			}
		}
	}
}
