using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Messaging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages
{
	public class DealWindowSelector : MonoBehaviour
	{
		public const float TIME_ARM_ROTATION_0000 = 0f;

		public const float TIME_ARM_ROTATION_2400 = -360f;

		public const int WINDOW_CUTOFF_MINS = 120;

		public UnityEvent<EDealWindow> OnSelected;

		[Header("References")]
		public GameObject Container;

		public WindowSelectorButton MorningButton;

		public WindowSelectorButton AfternoonButton;

		public WindowSelectorButton NightButton;

		public WindowSelectorButton LateNightButton;

		public RectTransform CurrentTimeArm;

		public Text CurrentTimeLabel;

		private Action<EDealWindow> callback;

		private WindowSelectorButton[] buttons;

		private bool hintShown;

		public bool IsOpen { get; private set; }

		private void Start()
		{
			GameInput.RegisterExitListener(Exit, 4);
			buttons = new WindowSelectorButton[4] { MorningButton, AfternoonButton, NightButton, LateNightButton };
			WindowSelectorButton[] array = buttons;
			foreach (WindowSelectorButton button in array)
			{
				button.OnSelected.AddListener(delegate
				{
					ButtonClicked(button.WindowType);
				});
			}
			SetIsOpen(open: false);
		}

		public void Exit(ExitAction action)
		{
			if (!action.used && IsOpen)
			{
				action.used = true;
				SetIsOpen(open: false);
			}
		}

		public void SetIsOpen(bool open)
		{
			SetIsOpen(open, null);
		}

		public void SetIsOpen(bool open, MSGConversation conversation, Action<EDealWindow> callback = null)
		{
			IsOpen = open;
			if (open)
			{
				UpdateTime();
				UpdateWindowValidity();
				conversation.onMessageRendered = (Action)Delegate.Combine(conversation.onMessageRendered, new Action(Close));
			}
			else
			{
				callback = null;
				if (conversation != null)
				{
					conversation.onMessageRendered = (Action)Delegate.Remove(conversation.onMessageRendered, new Action(Close));
				}
				WindowSelectorButton[] array = buttons;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetHoverIndicator(shown: false);
				}
			}
			if (open && NetworkSingleton<GameManager>.Instance.IsTutorial && !hintShown)
			{
				hintShown = true;
				Singleton<HintDisplay>.Instance.ShowHint_20s("You can complete deals any time within the window you choose. For now, choose the morning window.");
			}
			Container.gameObject.SetActive(open);
			this.callback = callback;
		}

		public void Update()
		{
			if (IsOpen)
			{
				UpdateTime();
				UpdateWindowValidity();
			}
		}

		private void UpdateTime()
		{
			CurrentTimeLabel.text = TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime);
			float t = (float)NetworkSingleton<TimeManager>.Instance.DailyMinTotal / 1440f;
			CurrentTimeArm.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -360f, t));
		}

		private void UpdateWindowValidity()
		{
			if (NetworkSingleton<GameManager>.Instance.IsTutorial)
			{
				MorningButton.SetInteractable(interactable: true);
				AfternoonButton.SetInteractable(interactable: false);
				NightButton.SetInteractable(interactable: false);
				LateNightButton.SetInteractable(interactable: false);
				return;
			}
			int dailyMinTotal = NetworkSingleton<TimeManager>.Instance.DailyMinTotal;
			WindowSelectorButton[] array = buttons;
			foreach (WindowSelectorButton obj in array)
			{
				int num = TimeManager.GetMinSumFrom24HourTime(DealWindowInfo.GetWindowInfo(obj.WindowType).EndTime);
				if (dailyMinTotal > num)
				{
					num += 1440;
				}
				obj.SetInteractable(num - dailyMinTotal > 120);
			}
		}

		private void Close()
		{
			SetIsOpen(open: false);
		}

		private void ButtonClicked(EDealWindow window)
		{
			if (IsOpen)
			{
				if (OnSelected != null)
				{
					OnSelected.Invoke(window);
				}
				if (callback != null)
				{
					callback(window);
				}
				SetIsOpen(open: false);
			}
		}
	}
}
