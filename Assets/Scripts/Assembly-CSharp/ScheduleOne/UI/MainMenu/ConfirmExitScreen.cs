using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.MainMenu
{
	public class ConfirmExitScreen : MainMenuScreen
	{
		public TextMeshProUGUI TimeSinceSaveLabel;

		private void Update()
		{
			if (!base.IsOpen)
			{
				return;
			}
			float secondsSinceLastSave = Singleton<SaveManager>.Instance.SecondsSinceLastSave;
			if (InstanceFinder.IsServer)
			{
				if (secondsSinceLastSave <= 60f)
				{
					TimeSinceSaveLabel.text = "Last save was " + Mathf.RoundToInt(secondsSinceLastSave) + " seconds ago";
					TimeSinceSaveLabel.color = Color.white;
				}
				else
				{
					int num = Mathf.FloorToInt(secondsSinceLastSave / 60f);
					TimeSinceSaveLabel.text = "Last save was " + num + " minute" + ((num > 1) ? "s" : "") + " ago";
					TimeSinceSaveLabel.color = ((num > 1) ? ((Color)new Color32(byte.MaxValue, 100, 100, byte.MaxValue)) : Color.white);
				}
				TimeSinceSaveLabel.enabled = true;
			}
			else
			{
				TimeSinceSaveLabel.enabled = false;
			}
		}

		public void ConfirmExit()
		{
			Singleton<LoadManager>.Instance.ExitToMenu();
			Close(openPrevious: true);
		}
	}
}
