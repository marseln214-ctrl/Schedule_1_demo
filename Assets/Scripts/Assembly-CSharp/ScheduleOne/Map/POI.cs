using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Phone.Map;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.Map
{
	public class POI : MonoBehaviour
	{
		public enum TextShowMode
		{
			Off = 0,
			Always = 1,
			OnHover = 2
		}

		public TextShowMode MainTextVisibility = TextShowMode.Always;

		public string DefaultMainText = "PoI Main Text";

		public bool AutoUpdatePosition = true;

		public bool Rotate;

		[SerializeField]
		protected GameObject UIPrefab;

		protected Text mainLabel;

		protected Button button;

		protected EventTrigger eventTrigger;

		private bool mainTextSet;

		public UnityEvent onUICreated;

		public bool UISetup { get; protected set; }

		public string MainText { get; protected set; } = string.Empty;


		public RectTransform UI { get; protected set; }

		public RectTransform IconContainer { get; protected set; }

		private void OnEnable()
		{
			if (UI == null)
			{
				if (PlayerSingleton<MapApp>.Instance == null)
				{
					StartCoroutine(Wait());
				}
				else if (UI == null)
				{
					UI = Object.Instantiate(UIPrefab, PlayerSingleton<MapApp>.Instance.PoIContainer).GetComponent<RectTransform>();
					InitializeUI();
				}
			}
			IEnumerator Wait()
			{
				yield return new WaitUntil(() => PlayerSingleton<MapApp>.Instance != null);
				if (base.enabled && UI == null)
				{
					UI = Object.Instantiate(UIPrefab, PlayerSingleton<MapApp>.Instance.PoIContainer).GetComponent<RectTransform>();
					InitializeUI();
				}
			}
		}

		private void OnDisable()
		{
			if (UI != null)
			{
				Object.Destroy(UI.gameObject);
				UI = null;
			}
		}

		private void Update()
		{
			if (AutoUpdatePosition && PlayerSingleton<MapApp>.InstanceExists && PlayerSingleton<MapApp>.Instance.isOpen)
			{
				UpdatePosition();
			}
		}

		public void SetMainText(string text)
		{
			mainTextSet = true;
			MainText = text;
			if (mainLabel != null)
			{
				mainLabel.text = text;
			}
		}

		public virtual void UpdatePosition()
		{
			if (!(UI == null) && Singleton<MapPositionUtility>.InstanceExists)
			{
				UI.anchoredPosition = Singleton<MapPositionUtility>.Instance.GetMapPosition(base.transform.position);
				if (Rotate)
				{
					IconContainer.localEulerAngles = new Vector3(0f, 0f, Vector3.SignedAngle(base.transform.forward, Vector3.forward, Vector3.up));
				}
			}
		}

		public virtual void InitializeUI()
		{
			mainLabel = UI.Find("MainLabel").GetComponent<Text>();
			if (mainLabel == null)
			{
				Console.LogError("Failed to find main label");
			}
			if (MainTextVisibility == TextShowMode.Off || MainTextVisibility == TextShowMode.OnHover)
			{
				mainLabel.enabled = false;
			}
			else
			{
				mainLabel.enabled = true;
			}
			eventTrigger = UI.GetComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener(delegate
			{
				HoverStart();
			});
			eventTrigger.triggers.Add(entry);
			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener(delegate
			{
				HoverEnd();
			});
			eventTrigger.triggers.Add(entry);
			button = UI.GetComponent<Button>();
			button.onClick.AddListener(delegate
			{
				Clicked();
			});
			IconContainer = UI.Find("IconContainer").GetComponent<RectTransform>();
			if (IconContainer == null)
			{
				Console.LogError("Failed to find icon container");
			}
			if (!mainTextSet)
			{
				SetMainText(DefaultMainText);
			}
			else
			{
				SetMainText(MainText);
			}
			if (onUICreated != null)
			{
				onUICreated.Invoke();
			}
			UISetup = true;
			UpdatePosition();
		}

		protected virtual void HoverStart()
		{
			if (MainTextVisibility == TextShowMode.OnHover)
			{
				mainLabel.enabled = true;
			}
		}

		protected virtual void HoverEnd()
		{
			if (MainTextVisibility == TextShowMode.OnHover)
			{
				mainLabel.enabled = false;
			}
		}

		protected virtual void Clicked()
		{
			PlayerSingleton<MapApp>.Instance.FocusPosition(UI.anchoredPosition);
		}
	}
}
