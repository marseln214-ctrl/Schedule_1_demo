using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.Settings
{
	public class ConfirmDisplaySettings : MonoBehaviour
	{
		public const float RevertTime = 15f;

		public TextMeshProUGUI SubtitleLabel;

		private float timeUntilRevert;

		private DisplaySettings oldSettings;

		private DisplaySettings newSettings;

		public bool IsOpen
		{
			get
			{
				if (this != null && base.gameObject != null)
				{
					return base.gameObject.activeSelf;
				}
				return false;
			}
		}

		public void Awake()
		{
			GameInput.RegisterExitListener(Exit, 6);
			base.gameObject.SetActive(value: false);
		}

		public void Open(DisplaySettings _oldSettings, DisplaySettings _newSettings)
		{
			base.gameObject.SetActive(value: true);
			oldSettings = _oldSettings;
			newSettings = _newSettings;
			timeUntilRevert = 15f;
			Update();
		}

		public void Exit(ExitAction action)
		{
			if (!action.used && IsOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				Close(revert: true);
			}
		}

		public void Update()
		{
			timeUntilRevert -= Time.unscaledDeltaTime;
			SubtitleLabel.text = $"Reverting in {timeUntilRevert:0.0} seconds";
			if (timeUntilRevert <= 0f)
			{
				Close(revert: true);
			}
		}

		public void Close(bool revert)
		{
			if (revert)
			{
				Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ApplyDisplaySettings(oldSettings);
				Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings = oldSettings;
				Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings = oldSettings;
			}
			else
			{
				Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteDisplaySettings(newSettings);
			}
			base.transform.parent.gameObject.SetActive(value: false);
			base.transform.parent.gameObject.SetActive(value: true);
			base.gameObject.SetActive(value: false);
		}
	}
}
