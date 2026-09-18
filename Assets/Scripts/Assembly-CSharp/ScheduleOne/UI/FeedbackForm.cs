using System;
using System.Collections;
using System.IO;
using AeLa.EasyFeedback;
using AeLa.EasyFeedback.FormElements;
using AeLa.EasyFeedback.Utility;
using ScheduleOne.DevUtilities;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class FeedbackForm : AeLa.EasyFeedback.FeedbackForm
	{
		private Coroutine ssCoroutine;

		public CanvasGroup CanvasGroup;

		public Toggle ScreenshotToggle;

		public TMP_InputField SummaryField;

		public TMP_InputField DescriptionField;

		public RectTransform Cog;

		public override void Awake()
		{
			base.Awake();
			ScreenshotToggle.SetIsOnWithoutNotify(IncludeScreenshot);
			ScreenshotToggle.onValueChanged.AddListener(OnScreenshotToggle);
			OnSubmissionSucceeded.AddListener(Clear);
		}

		private void Update()
		{
			Cog.localEulerAngles += new Vector3(0f, 0f, -180f * Time.unscaledDeltaTime);
		}

		public void PrepScreenshot()
		{
			CurrentReport = new Report();
		}

		private void OnScreenshotToggle(bool value)
		{
			IncludeScreenshot = value;
		}

		public void SetFormData(string title)
		{
			if (CurrentReport == null)
			{
				CurrentReport = new Report();
			}
			CurrentReport.Title = title;
			GetComponentInChildren<ReportTitle>().GetComponent<TMP_InputField>().SetTextWithoutNotify(title);
		}

		public override void Submit()
		{
			if (IncludeScreenshot)
			{
				PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0f);
				CanvasGroup.alpha = 0f;
				ssCoroutine = Singleton<CoroutineService>.Instance.StartCoroutine(ScreenshotAndOpenForm());
				Singleton<CoroutineService>.Instance.StartCoroutine(Wait());
			}
			if (File.Exists(Application.persistentDataPath + "/Player-prev.log"))
			{
				try
				{
					byte[] data = File.ReadAllBytes(Application.persistentDataPath + "/Player-prev.log");
					CurrentReport.AttachFile("Player-prev.txt", data);
				}
				catch (Exception ex)
				{
					Console.LogError("Failed to attach Player-prev.txt: " + ex.Message);
				}
			}
			CurrentReport.AddSection("Game Info", 2);
			string text = "Singleplayer";
			if (Singleton<Lobby>.InstanceExists && Singleton<Lobby>.Instance.IsInLobby)
			{
				text = "Multiplayer";
				text = ((!Singleton<Lobby>.Instance.IsHost) ? (text + " (Client)") : (text + " (Host)"));
			}
			CurrentReport["Game Info"].AppendLine("Network Mode: " + text);
			CurrentReport["Game Info"].AppendLine("Player Count: " + Player.PlayerList.Count);
			CurrentReport["Game Info"].AppendLine("Beta Branch: " + GameManager.IS_BETA);
			CurrentReport["Game Info"].AppendLine("Is Demo: " + true);
			CurrentReport["Game Info"].AppendLine("Load History: " + string.Join(", ", LoadManager.LoadHistory));
			Singleton<CoroutineService>.Instance.StartCoroutine(SubmitAsync());
			base.Submit();
			IEnumerator Wait()
			{
				yield return new WaitForEndOfFrame();
				PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0f);
				CanvasGroup.alpha = 1f;
			}
		}

		private void Clear()
		{
			SummaryField.SetTextWithoutNotify(string.Empty);
			DescriptionField.SetTextWithoutNotify(string.Empty);
		}

		private IEnumerator ScreenshotAndOpenForm()
		{
			if (IncludeScreenshot)
			{
				yield return ScreenshotUtil.CaptureScreenshot(ScreenshotCaptureMode, ResizeLargeScreenshots, delegate(byte[] ss)
				{
					CurrentReport.AttachFile("screenshot.png", ss);
				}, delegate(string err)
				{
					OnSubmissionError.Invoke(err);
				});
			}
			EnableForm();
			Form.gameObject.SetActive(value: true);
			OnFormOpened.Invoke();
			ssCoroutine = null;
		}
	}
}
