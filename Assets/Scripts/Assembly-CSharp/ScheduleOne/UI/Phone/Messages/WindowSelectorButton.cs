using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages
{
	public class WindowSelectorButton : MonoBehaviour
	{
		public const float SELECTION_INDICATOR_SCALE = 1.1f;

		public const float INDICATOR_LERP_TIME = 0.075f;

		public UnityEvent OnSelected;

		public EDealWindow WindowType;

		[Header("References")]
		public Button Button;

		public GameObject InactiveOverlay;

		public RectTransform HoverIndicator;

		private Coroutine hoverRoutine;

		private void Awake()
		{
			HoverIndicator.gameObject.SetActive(value: true);
			HoverIndicator.localScale = Vector3.one;
			Button.onClick.AddListener(Clicked);
		}

		public void SetInteractable(bool interactable)
		{
			Button.interactable = interactable;
			InactiveOverlay.SetActive(!interactable);
			if (!interactable)
			{
				SetHoverIndicator(shown: false);
			}
		}

		public void HoverStart()
		{
			if (Button.interactable)
			{
				SetHoverIndicator(shown: true);
			}
		}

		public void HoverEnd()
		{
			if (Button.interactable)
			{
				SetHoverIndicator(shown: false);
			}
		}

		public void Clicked()
		{
			if (OnSelected != null)
			{
				OnSelected.Invoke();
			}
		}

		public void SetHoverIndicator(bool shown)
		{
			if (hoverRoutine != null)
			{
				StopCoroutine(hoverRoutine);
			}
			hoverRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				float startScale = HoverIndicator.localScale.x;
				float targetScale = (shown ? 1.1f : 1f);
				if (shown)
				{
					HoverIndicator.gameObject.SetActive(value: true);
				}
				for (float i = 0f; i < 0.075f; i += Time.deltaTime)
				{
					HoverIndicator.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, i / 0.075f);
					yield return new WaitForEndOfFrame();
				}
				HoverIndicator.localScale = Vector3.one * targetScale;
				if (!shown)
				{
					HoverIndicator.gameObject.SetActive(value: false);
				}
				hoverRoutine = null;
			}
		}
	}
}
