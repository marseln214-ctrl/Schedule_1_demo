using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone
{
	public class HomeScreen : PlayerSingleton<HomeScreen>
	{
		[Header("References")]
		[SerializeField]
		protected Canvas canvas;

		[SerializeField]
		protected Text timeText;

		[SerializeField]
		protected RectTransform appIconContainer;

		[Header("Prefabs")]
		[SerializeField]
		protected GameObject appIconPrefab;

		protected List<Button> appIcons = new List<Button>();

		private Coroutine delayedSetOpenRoutine;

		public bool isOpen { get; protected set; } = true;


		protected override void Start()
		{
			base.Start();
			SetIsOpen(o: true);
		}

		public override void OnStartClient(bool IsOwner)
		{
			base.OnStartClient(IsOwner);
			if (IsOwner)
			{
				TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
				timeManager.onMinutePass = (Action)Delegate.Combine(timeManager.onMinutePass, new Action(MinPass));
				Phone phone = PlayerSingleton<Phone>.Instance;
				phone.onPhoneOpened = (Action)Delegate.Combine(phone.onPhoneOpened, new Action(PhoneOpened));
				Phone phone2 = PlayerSingleton<Phone>.Instance;
				phone2.onPhoneClosed = (Action)Delegate.Combine(phone2.onPhoneClosed, new Action(PhoneClosed));
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (NetworkSingleton<TimeManager>.InstanceExists)
			{
				TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
				timeManager.onMinutePass = (Action)Delegate.Remove(timeManager.onMinutePass, new Action(MinPass));
			}
		}

		protected void PhoneOpened()
		{
			if (isOpen)
			{
				SetCanvasActive(a: true);
			}
		}

		protected void PhoneClosed()
		{
			delayedSetOpenRoutine = StartCoroutine(DelayedSetCanvasActive(active: false, 0.25f));
		}

		private IEnumerator DelayedSetCanvasActive(bool active, float delay)
		{
			yield return new WaitForSeconds(delay);
			delayedSetOpenRoutine = null;
			SetCanvasActive(active);
		}

		public void SetIsOpen(bool o)
		{
			isOpen = o;
			SetCanvasActive(o);
		}

		public void SetCanvasActive(bool a)
		{
			if (delayedSetOpenRoutine != null)
			{
				StopCoroutine(delayedSetOpenRoutine);
			}
			canvas.enabled = a;
		}

		protected virtual void Update()
		{
			if (PlayerSingleton<Phone>.Instance.IsOpen && isOpen)
			{
				int num = -1;
				if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
				{
					num = 0;
				}
				else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
				{
					num = 1;
				}
				else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
				{
					num = 2;
				}
				else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4))
				{
					num = 3;
				}
				else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5))
				{
					num = 4;
				}
				else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha6))
				{
					num = 5;
				}
				else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha7))
				{
					num = 6;
				}
				else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha8))
				{
					num = 7;
				}
				else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha9))
				{
					num = 8;
				}
				if (num != -1 && appIcons.Count > num)
				{
					appIcons[num].onClick.Invoke();
				}
			}
		}

		protected virtual void MinPass()
		{
			if (NetworkSingleton<GameManager>.Instance.IsTutorial)
			{
				int num = TimeManager.Get24HourTimeFromMinSum(Mathf.RoundToInt(Mathf.Round((float)NetworkSingleton<TimeManager>.Instance.DailyMinTotal / 60f) * 60f));
				timeText.text = TimeManager.Get12HourTime(num) + " " + NetworkSingleton<TimeManager>.Instance.CurrentDay;
			}
			else
			{
				timeText.text = TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime) + " " + NetworkSingleton<TimeManager>.Instance.CurrentDay;
			}
		}

		public Button GenerateAppIcon<T>(App<T> prog) where T : PlayerSingleton<T>
		{
			RectTransform component = UnityEngine.Object.Instantiate(appIconPrefab, appIconContainer).GetComponent<RectTransform>();
			component.Find("Mask/Image").GetComponent<Image>().sprite = prog.AppIcon;
			component.Find("Label").GetComponent<Text>().text = prog.IconLabel;
			appIcons.Add(component.GetComponent<Button>());
			return component.GetComponent<Button>();
		}
	}
}
