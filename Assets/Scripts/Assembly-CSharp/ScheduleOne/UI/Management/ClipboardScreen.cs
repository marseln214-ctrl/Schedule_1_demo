using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class ClipboardScreen : MonoBehaviour
	{
		[Header("References")]
		public RectTransform Container;

		[Header("Settings")]
		public float ClosedOffset = 420f;

		public bool OpenOnStart;

		public bool UseExitListener = true;

		public int ExitActionPriority = 10;

		private Coroutine lerpRoutine;

		public bool IsOpen { get; protected set; }

		protected virtual void Start()
		{
			if (OpenOnStart)
			{
				IsOpen = true;
				Container.anchoredPosition = new Vector2(0f, Container.anchoredPosition.y);
			}
			else
			{
				IsOpen = false;
				Container.anchoredPosition = new Vector2(ClosedOffset, Container.anchoredPosition.y);
				Container.gameObject.SetActive(value: false);
			}
			if (UseExitListener)
			{
				GameInput.RegisterExitListener(Exit, ExitActionPriority);
			}
		}

		private void Exit(ExitAction exitAction)
		{
			if (IsOpen && !exitAction.used)
			{
				exitAction.used = true;
				Close();
			}
		}

		public virtual void Open()
		{
			Container.gameObject.SetActive(value: true);
			IsOpen = true;
			Lerp(open: true, null);
		}

		public virtual void Close()
		{
			IsOpen = false;
			Lerp(open: false, delegate
			{
				Container.gameObject.SetActive(value: false);
			});
		}

		private void Lerp(bool open, Action callback)
		{
			if (lerpRoutine != null)
			{
				Singleton<CoroutineService>.Instance.StopCoroutine(lerpRoutine);
			}
			lerpRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				float startX = Container.anchoredPosition.x;
				float endX = (open ? 0f : ClosedOffset);
				for (float i = 0f; i < 0.1f; i += 1f)
				{
					Container.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, i / 0.1f), Container.anchoredPosition.y);
					yield return new WaitForEndOfFrame();
				}
				Container.anchoredPosition = new Vector2(endX, Container.anchoredPosition.y);
				if (callback != null)
				{
					callback();
				}
				lerpRoutine = null;
			}
		}
	}
}
