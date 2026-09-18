using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Law;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class HUD : Singleton<HUD>
	{
		[Header("References")]
		public Canvas canvas;

		public RectTransform canvasRect;

		public Image crosshair;

		[SerializeField]
		protected Image blackOverlay;

		[SerializeField]
		protected Image radialIndicator;

		[SerializeField]
		protected GraphicRaycaster raycaster;

		[SerializeField]
		protected TextMeshProUGUI topScreenText;

		[SerializeField]
		protected RectTransform topScreenText_Background;

		[SerializeField]
		protected Text fpsLabel;

		public RectTransform cashSlotContainer;

		public RectTransform cashSlotUI;

		public RectTransform onlineBalanceContainer;

		public RectTransform onlineBalanceSlotUI;

		public RectTransform managementSlotContainer;

		public ItemSlotUI managementSlotUI;

		public RectTransform HotbarContainer;

		public RectTransform SlotContainer;

		public ItemSlotUI discardSlot;

		public Image discardSlotFill;

		public TextMeshProUGUI selectedItemLabel;

		public RectTransform QuestEntryContainer;

		public TextMeshProUGUI QuestEntryTitle;

		public CrimeStatusUI CrimeStatusUI;

		public BalanceDisplay OnlineBalanceDisplay;

		public BalanceDisplay SafeBalanceDisplay;

		public CrosshairText CrosshairText;

		public RectTransform UnreadMessagesPrompt;

		public TextMeshProUGUI SleepPrompt;

		public TextMeshProUGUI CurfewPrompt;

		[Header("Settings")]
		public Gradient RedGreenGradient;

		private int SampleSize = 60;

		private List<float> _previousFPS = new List<float>();

		private EventSystem eventSystem;

		private Coroutine blackOverlayFade;

		private bool radialIndicatorSetThisFrame;

		public ItemSlotUI hoveredItemSlotUI { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			eventSystem = EventSystem.current;
			managementSlotContainer.gameObject.SetActive(value: false);
			HideTopScreenText();
		}

		public void SetCrosshairVisible(bool vis)
		{
			crosshair.gameObject.SetActive(vis);
		}

		public void SetBlackOverlayVisible(bool vis, float fadeTime)
		{
			if (blackOverlayFade != null)
			{
				StopCoroutine(blackOverlayFade);
			}
			blackOverlayFade = StartCoroutine(FadeBlackOverlay(vis, fadeTime));
		}

		protected virtual void Update()
		{
			if (!Singleton<GameInput>.InstanceExists)
			{
				return;
			}
			hoveredItemSlotUI = null;
			PointerEventData pointerEventData = new PointerEventData(eventSystem);
			pointerEventData.position = UnityEngine.Input.mousePosition;
			List<RaycastResult> list = new List<RaycastResult>();
			raycaster.Raycast(pointerEventData, list);
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i].gameObject.GetComponentInParent<ItemSlotUI>())
				{
					hoveredItemSlotUI = list[i].gameObject.GetComponentInParent<ItemSlotUI>();
				}
			}
			SleepPrompt.gameObject.SetActive(NetworkSingleton<TimeManager>.Instance.CurrentTime == 400);
			if (NetworkSingleton<CurfewManager>.InstanceExists)
			{
				if (NetworkSingleton<CurfewManager>.Instance.IsCurrentlyActive)
				{
					CurfewPrompt.text = "Police curfew in effect until 5AM";
					CurfewPrompt.color = new Color32(byte.MaxValue, 108, 88, 60);
					CurfewPrompt.gameObject.SetActive(value: true);
				}
				else if (NetworkSingleton<CurfewManager>.Instance.IsEnabled && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(1930, 500))
				{
					CurfewPrompt.text = "Police curfew starting soon";
					CurfewPrompt.color = new Color32(byte.MaxValue, 182, 88, 60);
					CurfewPrompt.gameObject.SetActive(value: true);
				}
				else
				{
					CurfewPrompt.gameObject.SetActive(value: false);
				}
			}
			UpdateQuestEntryTitle();
			RefreshFPS();
		}

		private void UpdateQuestEntryTitle()
		{
			int num = 0;
			for (int i = 0; i < QuestEntryContainer.childCount; i++)
			{
				if (QuestEntryContainer.GetChild(i).gameObject.activeSelf)
				{
					num++;
				}
			}
			QuestEntryTitle.enabled = num > 1;
		}

		private void RefreshFPS()
		{
			_previousFPS.Add(1f / Time.unscaledDeltaTime);
			if (_previousFPS.Count > SampleSize)
			{
				_previousFPS.RemoveAt(0);
			}
			fpsLabel.text = Mathf.Floor(GetAverageFPS()) + " FPS";
		}

		private float GetAverageFPS()
		{
			float num = 0f;
			for (int i = 0; i < _previousFPS.Count; i++)
			{
				num += _previousFPS[i];
			}
			return num / (float)_previousFPS.Count;
		}

		protected virtual void LateUpdate()
		{
			if (!radialIndicatorSetThisFrame)
			{
				radialIndicator.enabled = false;
			}
			radialIndicatorSetThisFrame = false;
		}

		protected IEnumerator FadeBlackOverlay(bool visible, float fadeTime)
		{
			if (visible)
			{
				blackOverlay.enabled = true;
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement("Blackout");
			}
			float startAlpha = blackOverlay.color.a;
			float endAlpha = 1f;
			if (!visible)
			{
				endAlpha = 0f;
			}
			for (float i = 0f; i < fadeTime; i += Time.unscaledDeltaTime)
			{
				blackOverlay.color = new Color(blackOverlay.color.r, blackOverlay.color.g, blackOverlay.color.b, Mathf.Lerp(startAlpha, endAlpha, i / fadeTime));
				yield return new WaitForEndOfFrame();
			}
			blackOverlay.color = new Color(blackOverlay.color.r, blackOverlay.color.g, blackOverlay.color.b, endAlpha);
			blackOverlayFade = null;
			if (!visible)
			{
				blackOverlay.enabled = false;
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement("Blackout");
			}
		}

		public void ShowRadialIndicator(float fill)
		{
			radialIndicatorSetThisFrame = true;
			radialIndicator.fillAmount = fill;
			radialIndicator.enabled = true;
		}

		public void ShowTopScreenText(string t)
		{
			topScreenText.text = t;
			topScreenText_Background.sizeDelta = new Vector2(topScreenText.preferredWidth + 30f, topScreenText_Background.sizeDelta.y);
			topScreenText_Background.gameObject.SetActive(value: true);
		}

		public void HideTopScreenText()
		{
			topScreenText_Background.gameObject.SetActive(value: false);
		}
	}
}
