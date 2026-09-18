using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class NotificationsManager : Singleton<NotificationsManager>
	{
		public const int MAX_NOTIFICATIONS = 6;

		[Header("References")]
		public RectTransform EntryContainer;

		public AudioSourceController Sound;

		[Header("Prefab")]
		public GameObject NotificationPrefab;

		private Dictionary<RectTransform, Coroutine> coroutines = new Dictionary<RectTransform, Coroutine>();

		private List<RectTransform> entries = new List<RectTransform>();

		private void Update()
		{
		}

		public void SendNotification(string title, string subtitle, Sprite icon, float duration = 5f, bool playSound = true)
		{
			RectTransform newEntry = Object.Instantiate(NotificationPrefab, EntryContainer).GetComponent<RectTransform>();
			newEntry.SetAsLastSibling();
			RectTransform container = newEntry.Find("Container").GetComponent<RectTransform>();
			container.Find("Title").GetComponent<TextMeshProUGUI>().text = title;
			container.Find("Subtitle").GetComponent<TextMeshProUGUI>().text = subtitle;
			container.Find("AppIcon/Mask/Image").GetComponent<Image>().sprite = icon;
			float startX = -200f;
			float endX = 0f;
			float lerpTime = 0.15f;
			container.anchoredPosition = new Vector2(startX, container.anchoredPosition.y);
			if (playSound)
			{
				Sound.Play();
			}
			if (entries.Count >= 6)
			{
				RectTransform rectTransform = entries[0];
				if (rectTransform != null)
				{
					StopCoroutine(coroutines[rectTransform]);
					coroutines.Remove(rectTransform);
					Object.Destroy(rectTransform.gameObject);
				}
				entries.RemoveAt(0);
			}
			coroutines.Add(container, StartCoroutine(Routine()));
			entries.Add(container);
			IEnumerator Routine()
			{
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					container.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, i / lerpTime), container.anchoredPosition.y);
					yield return new WaitForEndOfFrame();
				}
				container.anchoredPosition = new Vector2(endX, container.anchoredPosition.y);
				yield return new WaitForSeconds(duration);
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					container.anchoredPosition = new Vector2(Mathf.Lerp(endX, startX, i / lerpTime), container.anchoredPosition.y);
					yield return new WaitForEndOfFrame();
				}
				if (container != null && coroutines.ContainsKey(container))
				{
					coroutines.Remove(container);
				}
				Object.Destroy(newEntry.gameObject);
			}
		}
	}
}
