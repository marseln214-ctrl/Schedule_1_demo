using ScheduleOne.DevUtilities;
using ScheduleOne.TV;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class TVPauseScreen : MonoBehaviour
	{
		public TVApp App;

		public bool IsPaused { get; private set; }

		private void Awake()
		{
			GameInput.RegisterExitListener(Exit, 4);
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && IsPaused && App.IsOpen)
			{
				action.used = true;
				Back();
			}
		}

		public void Pause()
		{
			IsPaused = true;
			base.gameObject.SetActive(value: true);
		}

		public void Resume()
		{
			IsPaused = false;
			base.gameObject.SetActive(value: false);
			App.Resume();
		}

		public void Back()
		{
			App.Close();
		}
	}
}
