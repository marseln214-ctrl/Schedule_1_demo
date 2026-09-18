using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone
{
	public class CallNotification : Singleton<CallNotification>
	{
		public const float TIME_PER_CHAR = 0.015f;

		[Header("References")]
		public RectTransform Container;

		public Image ProfilePicture;

		public CanvasGroup Group;

		private Coroutine slideRoutine;

		public PhoneCallData ActiveCallData { get; private set; }

		public bool IsOpen { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			Group.alpha = 0f;
			Container.anchoredPosition = new Vector2(-600f, 0f);
			Container.gameObject.SetActive(value: false);
		}

		public void SetIsOpen(bool visible, CallerID caller)
		{
			IsOpen = visible;
			if (slideRoutine != null)
			{
				StopCoroutine(slideRoutine);
			}
			slideRoutine = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				if (visible)
				{
					Container.gameObject.SetActive(value: true);
				}
				if (caller != null)
				{
					ProfilePicture.sprite = caller.ProfilePicture;
				}
				float startX = Container.anchoredPosition.x;
				float endX = (visible ? 0f : (-600f));
				float startAlpha = Group.alpha;
				float endAlpha = (visible ? 1f : 0f);
				float lerpTime = 0.25f;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					Container.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, i / lerpTime), 0f);
					Group.alpha = Mathf.Lerp(startAlpha, endAlpha, i / lerpTime);
					yield return new WaitForEndOfFrame();
				}
				Container.anchoredPosition = new Vector2(endX, 0f);
				Group.alpha = endAlpha;
				if (!visible)
				{
					Container.gameObject.SetActive(value: false);
				}
				slideRoutine = null;
			}
		}
	}
}
