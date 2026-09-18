using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.Compass
{
	public class CompassManager : Singleton<CompassManager>
	{
		public class Notch
		{
			public RectTransform Rect;

			public CanvasGroup Group;
		}

		public class Element
		{
			public bool Visible;

			public RectTransform Rect;

			public CanvasGroup Group;

			public TextMeshProUGUI DistanceLabel;

			public Transform Transform;
		}

		public const float DISTANCE_LABEL_THRESHOLD = 50f;

		[Header("References")]
		public RectTransform Container;

		public Transform NotchPointContainer;

		public RectTransform NotchUIContainer;

		public RectTransform ElementUIContainer;

		public Canvas Canvas;

		[Header("Prefabs")]
		public GameObject DirectionIndicatorPrefab;

		public GameObject NotchPrefab;

		public GameObject ElementPrefab;

		[Header("Settings")]
		public bool CompassEnabled = true;

		public Vector2 ElementContentSize = new Vector2(20f, 20f);

		public float CompassUIRange = 800f;

		public float FullAlphaRange = 40f;

		public float AngleDivisor = 60f;

		public float ClosedYPos = 30f;

		public float OpenYPos = -50f;

		private List<Transform> notchPositions = new List<Transform>();

		private List<Notch> notches = new List<Notch>();

		private List<Element> elements = new List<Element>();

		private Coroutine lerpContainerPositionCoroutine;

		private Transform cam => PlayerSingleton<PlayerCamera>.Instance.transform;

		protected override void Awake()
		{
			base.Awake();
			notchPositions = new List<Transform>(NotchPointContainer.GetComponentsInChildren<Transform>());
			notchPositions.Remove(NotchPointContainer);
			for (int i = 0; i < notchPositions.Count; i++)
			{
				GameObject original = NotchPrefab;
				int num = Mathf.RoundToInt((float)(i + 1) / (float)notchPositions.Count * 360f);
				if (num % 90 == 0)
				{
					original = DirectionIndicatorPrefab;
				}
				GameObject gameObject = Object.Instantiate(original, NotchUIContainer);
				Notch notch = new Notch();
				notch.Rect = gameObject.GetComponent<RectTransform>();
				notch.Group = gameObject.GetComponent<CanvasGroup>();
				notches.Add(notch);
				if (num % 90 == 0)
				{
					string text = "N";
					switch (num)
					{
					case 90:
						text = "E";
						break;
					case 180:
						text = "S";
						break;
					case 270:
						text = "W";
						break;
					}
					notch.Rect.GetComponentInChildren<TextMeshProUGUI>().text = text;
				}
			}
		}

		private void LateUpdate()
		{
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				if (Singleton<HUD>.Instance.canvas.enabled)
				{
					UpdateNotches();
					UpdateElements();
				}
				Canvas.enabled = Singleton<HUD>.Instance.canvas.enabled && CompassEnabled;
			}
		}

		public void SetCompassEnabled(bool enabled)
		{
			CompassEnabled = enabled;
		}

		public void SetVisible(bool visible)
		{
			if (lerpContainerPositionCoroutine != null)
			{
				StopCoroutine(lerpContainerPositionCoroutine);
			}
			lerpContainerPositionCoroutine = StartCoroutine(LerpContainerPosition(visible ? OpenYPos : ClosedYPos, visible));
			IEnumerator LerpContainerPosition(float yPos, bool visible)
			{
				if (visible)
				{
					Container.gameObject.SetActive(value: true);
				}
				float t = 0f;
				Vector2 startPos = Container.anchoredPosition;
				Vector2 endPos = new Vector2(startPos.x, yPos);
				while (t < 1f)
				{
					t += Time.deltaTime * 7f;
					Container.anchoredPosition = new Vector2(0f, Mathf.Lerp(startPos.y, endPos.y, t));
					yield return null;
				}
				Container.anchoredPosition = endPos;
				Container.gameObject.SetActive(visible);
			}
		}

		private void UpdateNotches()
		{
			for (int i = 0; i < notchPositions.Count; i++)
			{
				GetCompassData(notchPositions[i].position, out var xPos, out var alpha);
				notches[i].Rect.anchoredPosition = new Vector2(xPos, 0f);
				notches[i].Group.alpha = alpha;
				notches[i].Rect.gameObject.SetActive(alpha > 0f);
			}
		}

		private void UpdateElements()
		{
			for (int i = 0; i < elements.Count; i++)
			{
				UpdateElement(elements[i]);
			}
		}

		private void UpdateElement(Element element)
		{
			if (!element.Visible || element.Transform == null)
			{
				element.Group.alpha = 0f;
			}
			else
			{
				GetCompassData(element.Transform.position, out var xPos, out var alpha);
				element.Rect.anchoredPosition = new Vector2(xPos, 0f);
				element.Group.alpha = alpha;
				float num = Vector3.Distance(cam.position, element.Transform.position);
				if (num <= 50f)
				{
					element.DistanceLabel.text = Mathf.CeilToInt(num) + "m";
				}
				else
				{
					element.DistanceLabel.text = string.Empty;
				}
			}
			element.Rect.gameObject.SetActive(element.Group.alpha > 0f);
		}

		public void GetCompassData(Vector3 worldPosition, out float xPos, out float alpha)
		{
			Vector3 normalized = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
			Vector3 to = worldPosition - cam.position;
			to.y = 0f;
			float num = Vector3.SignedAngle(normalized, to, Vector3.up);
			xPos = Mathf.Clamp(num / AngleDivisor, -1f, 1f) * CompassUIRange * 0.5f;
			alpha = 1f;
			if (Mathf.Abs(num) > FullAlphaRange)
			{
				alpha = 1f - (Mathf.Abs(num) - FullAlphaRange) / (AngleDivisor - FullAlphaRange);
			}
		}

		public Element AddElement(Transform transform, RectTransform contentPrefab, bool visible = true)
		{
			Element element = new Element();
			element.Transform = transform;
			element.Rect = Object.Instantiate(ElementPrefab, ElementUIContainer).GetComponent<RectTransform>();
			element.Group = element.Rect.GetComponent<CanvasGroup>();
			element.DistanceLabel = element.Rect.Find("Text").GetComponent<TextMeshProUGUI>();
			RectTransform component = Object.Instantiate(contentPrefab, element.Rect).GetComponent<RectTransform>();
			component.anchoredPosition = Vector2.zero;
			component.sizeDelta = ElementContentSize;
			element.Visible = visible;
			elements.Add(element);
			UpdateElement(element);
			return element;
		}

		public void RemoveElement(Transform transform, bool alsoDestroyRect = true)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				if (elements[i].Transform == transform)
				{
					RemoveElement(elements[i], alsoDestroyRect);
					break;
				}
			}
		}

		public void RemoveElement(Element el, bool alsoDestroyRect = true)
		{
			if (alsoDestroyRect)
			{
				Object.Destroy(el.Rect.gameObject);
			}
			elements.Remove(el);
		}
	}
}
