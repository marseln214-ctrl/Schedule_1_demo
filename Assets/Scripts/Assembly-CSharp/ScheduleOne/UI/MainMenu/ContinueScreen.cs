using ScheduleOne.DevUtilities;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using UnityEngine;

namespace ScheduleOne.UI.MainMenu
{
	public class ContinueScreen : MainMenuScreen
	{
		public RectTransform NotHostWarning;

		private void Update()
		{
			if (base.IsOpen)
			{
				NotHostWarning.gameObject.SetActive(!Singleton<Lobby>.Instance.IsHost);
			}
		}

		public void LoadGame(int index)
		{
			if (!Singleton<Lobby>.Instance.IsHost)
			{
				Console.LogWarning("Only the host can start the game.");
			}
			else
			{
				Singleton<LoadManager>.Instance.StartGame(LoadManager.SaveGames[index]);
			}
		}
	}
}
