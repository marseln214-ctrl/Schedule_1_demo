using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class SleepCanvas : Singleton<SleepCanvas>
	{
		public const int MaxSleepTime = 12;

		public const int MinSleepTime = 4;

		private float QueuedMessageDisplayTime;

		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public RectTransform MenuContainer;

		public TextMeshProUGUI CurrentTimeLabel;

		public Button IncreaseButton;

		public Button DecreaseButton;

		public TextMeshProUGUI EndTimeLabel;

		public Button SleepButton;

		public TextMeshProUGUI SleepButtonLabel;

		public Image BlackOverlay;

		public TextMeshProUGUI SleepMessageLabel;

		public CanvasGroup SleepMessageGroup;

		public TextMeshProUGUI TimeLabel;

		public TextMeshProUGUI WakeLabel;

		public TextMeshProUGUI WaitingForHostLabel;

		public UnityEvent onSleepFullyFaded;

		public UnityEvent onSleepEndFade;

		private List<IPostSleepEvent> queuedPostSleepEvents = new List<IPostSleepEvent>();

		public bool IsMenuOpen { get; protected set; }

		public string QueuedSleepMessage { get; protected set; } = string.Empty;


		protected override void Awake()
		{
			base.Awake();
			IncreaseButton.onClick.AddListener(delegate
			{
				ChangeSleepAmount(1);
			});
			DecreaseButton.onClick.AddListener(delegate
			{
				ChangeSleepAmount(-1);
			});
			SleepButton.onClick.AddListener(SleepButtonPressed);
			GameInput.RegisterExitListener(Exit, 1);
			TimeManager.onSleepStart = (Action)Delegate.Combine(TimeManager.onSleepStart, new Action(SleepStart));
			TimeLabel.enabled = false;
			WakeLabel.enabled = false;
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && IsMenuOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				SetIsOpen(open: false);
			}
		}

		public void SetIsOpen(bool open)
		{
			IsMenuOpen = open;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			if (open)
			{
				Update();
				NetworkSingleton<TimeManager>.Instance.SetWakeTime(ClampWakeTime(700));
				UpdateTimeLabels();
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
				PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
				PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
				PlayerSingleton<PlayerMovement>.Instance.canMove = false;
				Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
				Canvas.enabled = true;
				Container.gameObject.SetActive(value: true);
			}
			else
			{
				Player.Local.CurrentBed = null;
				Player.Local.SetReadyToSleep(ready: false);
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
				PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: true, returnToOriginalRotation: false);
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
				PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			}
			MenuContainer.gameObject.SetActive(open);
		}

		public void Update()
		{
			if (IsMenuOpen)
			{
				UpdateHourSetting();
				UpdateTimeLabels();
				UpdateSleepButton();
			}
			if (Canvas.enabled)
			{
				CurrentTimeLabel.text = TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime);
			}
		}

		public void AddPostSleepEvent(IPostSleepEvent postSleepEvent)
		{
			Console.Log("Adding post sleep event: " + postSleepEvent.GetType().Name);
			queuedPostSleepEvents.Add(postSleepEvent);
		}

		private void UpdateHourSetting()
		{
			IncreaseButton.interactable = true;
			DecreaseButton.interactable = true;
		}

		private void UpdateTimeLabels()
		{
			EndTimeLabel.text = TimeManager.Get12HourTime(700f);
		}

		private void UpdateSleepButton()
		{
			if (Player.Local.IsReadyToSleep)
			{
				SleepButtonLabel.text = "Waiting for other players";
			}
			else
			{
				SleepButtonLabel.text = "Sleep";
			}
		}

		private void ChangeSleepAmount(int change)
		{
			int time = TimeManager.AddMinutesTo24HourTime(700, change * 60);
			time = ClampWakeTime(time);
			NetworkSingleton<TimeManager>.Instance.SetWakeTime(time);
			UpdateHourSetting();
			UpdateTimeLabels();
		}

		private int ClampWakeTime(int time)
		{
			int currentTime = NetworkSingleton<TimeManager>.Instance.CurrentTime;
			int time2 = TimeManager.AddMinutesTo24HourTime(currentTime, 60 - currentTime % 100);
			int startTime = TimeManager.AddMinutesTo24HourTime(time2, 240);
			int endTime = TimeManager.AddMinutesTo24HourTime(time2, 720);
			return ClampTime(time, startTime, endTime);
		}

		private int ClampTime(int time, int startTime, int endTime)
		{
			if (endTime > startTime)
			{
				if (time < startTime)
				{
					return startTime;
				}
				if (time > endTime)
				{
					return endTime;
				}
			}
			else if (time < startTime && time > endTime)
			{
				int max = TimeManager.AddMinutesTo24HourTime(endTime, 720);
				if (TimeManager.IsGivenTimeWithinRange(time, endTime, max))
				{
					return endTime;
				}
				return startTime;
			}
			return time;
		}

		private void SleepButtonPressed()
		{
			Player.Local.SetReadyToSleep(!Player.Local.IsReadyToSleep);
		}

		private void SleepStart()
		{
			Player.Local.SetReadyToSleep(ready: false);
			MenuContainer.gameObject.SetActive(value: false);
			IsMenuOpen = false;
			int num = 700;
			WakeLabel.text = "Waking up at " + TimeManager.Get12HourTime(num);
			StartCoroutine(Sleep());
			IEnumerator Sleep()
			{
				BlackOverlay.enabled = true;
				SleepMessageLabel.text = string.Empty;
				NetworkSingleton<TimeManager>.Instance.syncVar___hostDailySummaryDone.SetValue(false, true);
				Singleton<HUD>.Instance.canvas.enabled = false;
				LerpBlackOverlay(1f, 0.5f);
				yield return new WaitForSecondsRealtime(0.5f);
				if (onSleepFullyFaded != null)
				{
					onSleepFullyFaded.Invoke();
				}
				yield return new WaitForSecondsRealtime(0.5f);
				NetworkSingleton<DailySummary>.Instance.Open();
				yield return new WaitUntil(() => !NetworkSingleton<DailySummary>.Instance.IsOpen);
				queuedPostSleepEvents = queuedPostSleepEvents.OrderBy((IPostSleepEvent x) => x.Order).ToList();
				foreach (IPostSleepEvent pse in queuedPostSleepEvents)
				{
					yield return new WaitForSecondsRealtime(0.5f);
					Console.Log("Running post sleep event: " + pse.GetType().Name);
					pse.StartEvent();
					yield return new WaitUntil(() => !pse.IsRunning);
				}
				queuedPostSleepEvents.Clear();
				if (InstanceFinder.IsServer)
				{
					NetworkSingleton<TimeManager>.Instance.syncVar___hostDailySummaryDone.SetValue(true, true);
				}
				else
				{
					WaitingForHostLabel.enabled = true;
					yield return new WaitUntil(() => NetworkSingleton<TimeManager>.Instance.SyncAccessor_hostDailySummaryDone);
					WaitingForHostLabel.enabled = false;
				}
				NetworkSingleton<TimeManager>.Instance.FastForwardToWakeTime();
				TimeLabel.enabled = true;
				if (InstanceFinder.IsServer)
				{
					Singleton<SaveManager>.Instance.DelayedSave();
				}
				yield return new WaitForSecondsRealtime(1f);
				TimeLabel.enabled = false;
				if (onSleepEndFade != null)
				{
					onSleepEndFade.Invoke();
				}
				if (!string.IsNullOrEmpty(QueuedSleepMessage))
				{
					yield return new WaitForSecondsRealtime(0.5f);
					SleepMessageLabel.text = QueuedSleepMessage;
					QueuedSleepMessage = string.Empty;
					SleepMessageGroup.alpha = 0f;
					float lerpTime = 0.5f;
					for (float j = 0f; j < lerpTime; j += Time.deltaTime)
					{
						SleepMessageGroup.alpha = j / lerpTime;
						yield return new WaitForEndOfFrame();
					}
					SleepMessageGroup.alpha = 1f;
					yield return new WaitForSecondsRealtime(QueuedMessageDisplayTime);
					for (float j = 0f; j < lerpTime; j += Time.deltaTime)
					{
						SleepMessageGroup.alpha = 1f - j / lerpTime;
						yield return new WaitForEndOfFrame();
					}
					SleepMessageGroup.alpha = 0f;
					yield return new WaitForSecondsRealtime(0.5f);
				}
				PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: false);
				TimeLabel.enabled = false;
				WakeLabel.enabled = false;
				if (!NetworkSingleton<GameManager>.Instance.IsTutorial)
				{
					Singleton<HUD>.Instance.canvas.enabled = true;
				}
				yield return new WaitForSecondsRealtime(0.1f);
				if (!NetworkSingleton<GameManager>.Instance.IsTutorial)
				{
					SetIsOpen(open: false);
				}
				LerpBlackOverlay(0f, 0.5f);
			}
		}

		private void LerpBlackOverlay(float transparency, float lerpTime)
		{
			if (transparency > 0f)
			{
				BlackOverlay.enabled = true;
			}
			StartCoroutine(Routine());
			IEnumerator Routine()
			{
				Color startColor = BlackOverlay.color;
				Color endColor = new Color(0f, 0f, 0f, transparency);
				for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
				{
					BlackOverlay.color = Color.Lerp(startColor, endColor, i / lerpTime);
					yield return new WaitForEndOfFrame();
				}
				BlackOverlay.color = endColor;
				if (transparency == 0f)
				{
					BlackOverlay.enabled = false;
					Canvas.enabled = false;
					Container.gameObject.SetActive(value: false);
				}
			}
		}

		public void QueueSleepMessage(string message, float displayTime = 3f)
		{
			Console.Log("Queueing sleep message: " + message + " for " + displayTime + " seconds");
			QueuedSleepMessage = message;
			QueuedMessageDisplayTime = displayTime;
		}
	}
}
