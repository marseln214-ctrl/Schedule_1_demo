using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.TV
{
	public class TVHomeScreen : TVApp
	{
		[Header("References")]
		public TVInterface Interface;

		public TVApp[] Apps;

		public RectTransform AppButtonContainer;

		public RectTransform[] PlayerDisplays;

		public TextMeshProUGUI TimeLabel;

		[Header("Prefabs")]
		public GameObject AppButtonPrefab;

		private bool skipExit;

		protected override void Awake()
		{
			base.Awake();
			TVApp[] apps = Apps;
			foreach (TVApp app in apps)
			{
				app.PreviousScreen = this;
				app.CanvasGroup.alpha = 0f;
				GameObject obj = Object.Instantiate(AppButtonPrefab, AppButtonContainer);
				obj.transform.Find("Icon").GetComponent<Image>().sprite = app.Icon;
				obj.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = app.AppName;
				obj.GetComponent<Button>().onClick.AddListener(delegate
				{
					AppSelected(app);
				});
				app.Close();
			}
			Interface.onPlayerAdded.AddListener(PlayerChange);
			Interface.onPlayerRemoved.AddListener(PlayerChange);
			Close();
		}

		public override void Open()
		{
			base.Open();
			UpdateTimeLabel();
		}

		public override void Close()
		{
			base.Close();
			if (skipExit)
			{
				skipExit = false;
			}
			else
			{
				Interface.Close();
			}
		}

		protected override void ActiveMinPass()
		{
			base.ActiveMinPass();
			UpdateTimeLabel();
		}

		private void UpdateTimeLabel()
		{
			TimeLabel.text = TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime);
		}

		private void AppSelected(TVApp app)
		{
			skipExit = true;
			Close();
			app.Open();
		}

		private void PlayerChange(Player player)
		{
			for (int i = 0; i < PlayerDisplays.Length; i++)
			{
				if (Interface.Players.Count > i)
				{
					PlayerDisplays[i].Find("Name").GetComponent<TextMeshProUGUI>().text = Interface.Players[i].PlayerName;
					PlayerDisplays[i].gameObject.SetActive(value: true);
				}
				else
				{
					PlayerDisplays[i].gameObject.SetActive(value: false);
				}
			}
		}
	}
}
