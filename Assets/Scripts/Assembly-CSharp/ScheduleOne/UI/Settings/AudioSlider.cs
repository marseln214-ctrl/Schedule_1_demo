using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.UI.Settings
{
	public class AudioSlider : SettingsSlider
	{
		public const float MULTIPLIER = 0.01f;

		public bool Master;

		public EAudioType AudioType = EAudioType.FX;

		protected virtual void Start()
		{
			if (Master)
			{
				slider.SetValueWithoutNotify(Singleton<AudioManager>.Instance.MasterVolume / 0.01f);
			}
			else
			{
				slider.SetValueWithoutNotify(Singleton<AudioManager>.Instance.GetVolume(AudioType, scaled: false) / 0.01f);
			}
		}

		protected override void OnValueChanged(float value)
		{
			base.OnValueChanged(value);
			if (Master)
			{
				Singleton<ScheduleOne.DevUtilities.Settings>.Instance.AudioSettings.MasterVolume = value * 0.01f;
			}
			else
			{
				switch (AudioType)
				{
				case EAudioType.Ambient:
					Singleton<ScheduleOne.DevUtilities.Settings>.Instance.AudioSettings.AmbientVolume = value * 0.01f;
					break;
				case EAudioType.Footsteps:
					Singleton<ScheduleOne.DevUtilities.Settings>.Instance.AudioSettings.FootstepsVolume = value * 0.01f;
					break;
				case EAudioType.FX:
					Singleton<ScheduleOne.DevUtilities.Settings>.Instance.AudioSettings.SFXVolume = value * 0.01f;
					break;
				case EAudioType.UI:
					Singleton<ScheduleOne.DevUtilities.Settings>.Instance.AudioSettings.UIVolume = value * 0.01f;
					break;
				case EAudioType.Music:
					Singleton<ScheduleOne.DevUtilities.Settings>.Instance.AudioSettings.MusicVolume = value * 0.01f;
					break;
				case EAudioType.Voice:
					Singleton<ScheduleOne.DevUtilities.Settings>.Instance.AudioSettings.DialogueVolume = value * 0.01f;
					break;
				}
			}
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ReloadAudioSettings();
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteAudioSettings(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.AudioSettings);
		}
	}
}
