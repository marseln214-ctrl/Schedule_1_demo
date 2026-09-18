using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	[RequireComponent(typeof(Button))]
	[RequireComponent(typeof(EventTrigger))]
	public class ButtonScaler : MonoBehaviour
	{
		public RectTransform ScaleTarget;

		public float HoverScale = 1.1f;

		public float ScaleTime = 0.1f;

		private Coroutine scaleCoroutine;

		private Button button;

		private void Awake()
		{
			button = GetComponent<Button>();
			EventTrigger component = GetComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener(delegate
			{
				Hovered();
			});
			component.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry();
			entry2.eventID = EventTriggerType.PointerExit;
			entry2.callback.AddListener(delegate
			{
				HoverEnd();
			});
			component.triggers.Add(entry2);
		}

		private void Hovered()
		{
			if (button.interactable)
			{
				SetScale(HoverScale);
			}
		}

		private void HoverEnd()
		{
			if (button.interactable)
			{
				SetScale(1f);
			}
		}

		private void SetScale(float endScale)
		{
			if (scaleCoroutine != null)
			{
				StopCoroutine(scaleCoroutine);
			}
			scaleCoroutine = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				float startScale = ScaleTarget.localScale.x;
				float lerpTime = Mathf.Abs(startScale - endScale) / Mathf.Abs(1f - HoverScale) * ScaleTime;
				for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
				{
					float num = Mathf.Lerp(startScale, endScale, i / lerpTime);
					ScaleTarget.localScale = new Vector3(num, num, num);
					yield return new WaitForEndOfFrame();
				}
				ScaleTarget.localScale = new Vector3(endScale, endScale, endScale);
				scaleCoroutine = null;
			}
		}
	}
}
