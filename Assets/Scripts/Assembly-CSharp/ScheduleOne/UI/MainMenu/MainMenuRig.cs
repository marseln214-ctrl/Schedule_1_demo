using System.IO;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Tools;
using Steamworks;
using UnityEngine;

namespace ScheduleOne.UI.MainMenu
{
	public class MainMenuRig : MonoBehaviour
	{
		public ScheduleOne.AvatarFramework.Avatar Avatar;

		public BasicAvatarSettings DefaultSettings;

		public CashPile[] CashPiles;

		public void Awake()
		{
			Singleton<LoadManager>.Instance.onSaveInfoLoaded.AddListener(LoadStuff);
		}

		private void LoadStuff()
		{
			bool flag = false;
			if (LoadManager.LastPlayedGame != null)
			{
				string path = Path.Combine(LoadManager.LastPlayedGame.SavePath, "Players", "Player_0");
				if (SteamManager.Initialized)
				{
					path = Path.Combine(LoadManager.LastPlayedGame.SavePath, "Players", "Player_" + SteamUser.GetSteamID().ToString());
				}
				string text = Path.Combine(path, "Appearance.json");
				if (File.Exists(text))
				{
					string json = File.ReadAllText(text);
					BasicAvatarSettings basicAvatarSettings = new BasicAvatarSettings();
					JsonUtility.FromJsonOverwrite(json, basicAvatarSettings);
					Avatar.LoadAvatarSettings(basicAvatarSettings.GetAvatarSettings());
					flag = true;
					Console.Log("Loaded player appearance from " + text);
				}
				float num = LoadManager.LastPlayedGame.Networth;
				for (int i = 0; i < CashPiles.Length; i++)
				{
					float displayedAmount = Mathf.Clamp(num, 0f, 100000f);
					CashPiles[i].SetDisplayedAmount(displayedAmount);
					num -= 100000f;
					if (num <= 0f)
					{
						break;
					}
				}
			}
			if (!flag)
			{
				Avatar.gameObject.SetActive(value: false);
			}
		}
	}
}
