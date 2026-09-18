using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI.MainMenu
{
	public class MainMenuScreen : MonoBehaviour
	{
		public const float LERP_TIME = 0.075f;

		public const float LERP_SCALE = 1.25f;

		[Header("Settings")]
		public int ExitInputPriority;

		public bool OpenOnStart;

		[Header("References")]
		public MainMenuScreen PreviousScreen;

		public CanvasGroup Group;

		private RectTransform Rect;

		private Coroutine lerpRoutine;

		public bool IsOpen { get; protected set; }

		protected virtual void Awake()
		{
			Rect = GetComponent<RectTransform>();
			GameInput.RegisterExitListener(Exit, ExitInputPriority);
			if (OpenOnStart)
			{
				Group.alpha = 1f;
				Rect.localScale = new Vector3(1f, 1f, 1f);
				base.gameObject.SetActive(value: true);
				IsOpen = true;
			}
			else
			{
				Group.alpha = 0f;
				Rect.localScale = new Vector3(1.25f, 1.25f, 1.25f);
				base.gameObject.SetActive(value: false);
				IsOpen = false;
			}
			if (OpenOnStart)
			{
				Singleton<MusicPlayer>.Instance.SetTrackEnabled("Main Menu", enabled: true);
			}
		}

		private void OnDestroy()
		{
			if (Singleton<MusicPlayer>.Instance != null)
			{
				Singleton<MusicPlayer>.Instance.SetTrackEnabled("Main Menu", enabled: false);
			}
		}

		protected virtual void Exit(ExitAction action)
		{
			if (!action.used && action.exitType != 0 && !(PreviousScreen == null) && IsOpen)
			{
				Close(openPrevious: true);
				action.used = true;
			}
		}

		public virtual void Open(bool closePrevious)
		{
			IsOpen = true;
			Lerp(open: true);
			if (closePrevious && PreviousScreen != null)
			{
				PreviousScreen.Close(openPrevious: false);
			}
		}

		public virtual void Close(bool openPrevious)
		{
			IsOpen = false;
			Lerp(open: false);
			if (openPrevious && PreviousScreen != null)
			{
				PreviousScreen.Open(closePrevious: false);
			}
		}

		private void Lerp(bool open)
		{
			if (lerpRoutine != null)
			{
				StopCoroutine(lerpRoutine);
			}
			if (open)
			{
				base.gameObject.SetActive(value: true);
			}
			if (Rect == null)
			{
				Rect = GetComponent<RectTransform>();
			}
			lerpRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				float startAlpha = Group.alpha;
				float startScale = Rect.localScale.x;
				float endAlpha = (open ? 1f : 0f);
				float endScale = (open ? 1f : 1.25f);
				float lerpTime = Mathf.Abs(startScale - endScale) / Mathf.Abs(-0.25f) * 0.075f;
				for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
				{
					float num = Mathf.Lerp(startScale, endScale, i / lerpTime);
					Group.alpha = Mathf.Lerp(startAlpha, endAlpha, i / lerpTime);
					Rect.localScale = new Vector3(num, num, num);
					yield return new WaitForEndOfFrame();
				}
				Group.alpha = endAlpha;
				Rect.localScale = new Vector3(endScale, endScale, endScale);
				lerpRoutine = null;
				if (!open)
				{
					base.gameObject.SetActive(value: false);
				}
			}
		}
	}
}
