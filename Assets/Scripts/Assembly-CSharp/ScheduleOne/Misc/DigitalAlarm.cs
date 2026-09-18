using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using TMPro;
using UnityEngine;

namespace ScheduleOne.Misc
{
	public class DigitalAlarm : MonoBehaviour
	{
		public const float FLASH_FREQUENCY = 4f;

		public MeshRenderer ScreenMesh;

		public int ScreenMeshMaterialIndex;

		public TextMeshPro ScreenText;

		public bool FlashScreen;

		[Header("Settings")]
		public bool DisplayCurrentTime;

		public Material ScreenOffMat;

		public Material ScreenOnMat;

		private void Start()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		private void OnDestroy()
		{
			if (NetworkSingleton<TimeManager>.Instance != null)
			{
				TimeManager instance = NetworkSingleton<TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			}
		}

		public void SetScreenLit(bool lit)
		{
			Material[] materials = ScreenMesh.materials;
			materials[ScreenMeshMaterialIndex] = (lit ? ScreenOnMat : ScreenOffMat);
			ScreenMesh.materials = materials;
		}

		public void DisplayText(string text)
		{
			ScreenText.text = text;
		}

		public void DisplayMinutes(int mins)
		{
			int num = mins / 60;
			mins %= 60;
			DisplayText($"{num:D2}:{mins:D2}");
		}

		private void MinPass()
		{
			if (DisplayCurrentTime)
			{
				DisplayText(TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime, appendDesignator: false));
			}
		}

		private void FixedUpdate()
		{
			if (FlashScreen)
			{
				float num = Mathf.Sin(Time.timeSinceLevelLoad * 4f);
				SetScreenLit(num > 0f);
			}
		}
	}
}
