using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.TV
{
	public class TVApp : MonoBehaviour
	{
		public const float SCALE_MIN = 0.67f;

		public const float SCALE_MAX = 1.5f;

		public const float LERP_TIME = 0.12f;

		[Header("Settings")]
		public bool CanClose = true;

		public string AppName;

		public Sprite Icon;

		public bool Pauseable = true;

		[Header("References")]
		public Canvas Canvas;

		[HideInInspector]
		public TVApp PreviousScreen;

		public CanvasGroup CanvasGroup;

		public TVPauseScreen PauseScreen;

		private Coroutine lerpCoroutine;

		public bool IsOpen { get; private set; }

		public bool IsPaused
		{
			get
			{
				if (PauseScreen != null)
				{
					return PauseScreen.IsPaused;
				}
				return false;
			}
		}

		protected virtual void Awake()
		{
			GameInput.RegisterExitListener(Exit, 3);
			CanvasGroup.alpha = 0f;
		}

		private void OnDestroy()
		{
			GameInput.DeregisterExitListener(Exit);
		}

		public virtual void Open()
		{
			IsOpen = true;
			Canvas.gameObject.SetActive(value: true);
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(ActiveMinPass));
			TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
			instance2.onMinutePass = (Action)Delegate.Combine(instance2.onMinutePass, new Action(ActiveMinPass));
			Lerp(1f, 1f);
		}

		public virtual void Close()
		{
			IsOpen = false;
			Canvas.gameObject.SetActive(value: false);
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(ActiveMinPass));
			if (PreviousScreen != null)
			{
				Lerp(0.67f, 0f);
			}
			else
			{
				Lerp(1.5f, 0f);
			}
			if (PreviousScreen != null)
			{
				PreviousScreen.Open();
			}
		}

		public virtual void Resume()
		{
		}

		private void Lerp(float endScale, float endAlpha)
		{
			if (lerpCoroutine != null)
			{
				Singleton<CoroutineService>.Instance.StopCoroutine(lerpCoroutine);
			}
			lerpCoroutine = Singleton<CoroutineService>.Instance.StartCoroutine(Lerp(endScale, endAlpha));
			IEnumerator Lerp(float endScale, float endAlpha)
			{
				if (!(Canvas == null))
				{
					Canvas.gameObject.SetActive(value: true);
					float startScale = Canvas.transform.localScale.x;
					float startAlpha = CanvasGroup.alpha;
					float lerpTime = Mathf.Abs(endScale - startScale) / 0.5f * 0.12f;
					for (float i = 0f; i < lerpTime; i += Time.deltaTime)
					{
						if (Canvas == null)
						{
							yield break;
						}
						Canvas.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, i / lerpTime);
						CanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, i / lerpTime);
						yield return new WaitForEndOfFrame();
					}
					if (Canvas != null)
					{
						Canvas.transform.localScale = Vector3.one * endScale;
						CanvasGroup.alpha = endAlpha;
						if (endAlpha == 0f)
						{
							Canvas.gameObject.SetActive(value: false);
						}
					}
					lerpCoroutine = null;
				}
			}
		}

		protected virtual void ActiveMinPass()
		{
		}

		private void Exit(ExitAction action)
		{
			if (action.used || !IsOpen)
			{
				return;
			}
			if (CanClose || Pauseable)
			{
				action.used = true;
				if (Pauseable && PauseScreen != null)
				{
					TryPause();
				}
				else
				{
					Close();
				}
			}
			else
			{
				PreviousScreen.Open();
			}
		}

		protected virtual void TryPause()
		{
			PauseScreen.Pause();
		}
	}
}
