using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Tooltips
{
	public class TooltipManager : Singleton<TooltipManager>
	{
		[Header("References")]
		[SerializeField]
		private RectTransform anchor;

		[SerializeField]
		private TextMeshProUGUI tooltipLabel;

		[Header("Canvas")]
		public List<Canvas> canvases = new List<Canvas>();

		private List<Canvas> sortedCanvases = new List<Canvas>();

		private List<GraphicRaycaster> raycasters = new List<GraphicRaycaster>();

		private EventSystem eventSystem;

		private bool tooltipShownThisFrame;

		protected override void Awake()
		{
			base.Awake();
			eventSystem = EventSystem.current;
			sortedCanvases = (from canvas in canvases
				where canvas.GetComponent<GraphicRaycaster>() != null
				orderby canvas.sortingOrder, canvas.transform.GetSiblingIndex()
				select canvas).ToList();
			for (int i = 0; i < sortedCanvases.Count; i++)
			{
				raycasters.Add(sortedCanvases[i].GetComponent<GraphicRaycaster>());
			}
		}

		protected virtual void Update()
		{
			CheckForTooltipHover();
		}

		protected virtual void LateUpdate()
		{
			if (!tooltipShownThisFrame)
			{
				anchor.gameObject.SetActive(value: false);
			}
			tooltipShownThisFrame = false;
		}

		public void AddCanvas(Canvas canvas)
		{
			canvases.Add(canvas);
			sortedCanvases = (from c in canvases
				where c != null && c.GetComponent<GraphicRaycaster>() != null
				orderby c.sortingOrder, c.transform.GetSiblingIndex()
				select c).ToList();
			raycasters.Clear();
			for (int i = 0; i < sortedCanvases.Count; i++)
			{
				raycasters.Add(sortedCanvases[i].GetComponent<GraphicRaycaster>());
			}
		}

		private void CheckForTooltipHover()
		{
			PointerEventData pointerEventData = new PointerEventData(eventSystem);
			pointerEventData.position = UnityEngine.Input.mousePosition;
			for (int i = 0; i < sortedCanvases.Count; i++)
			{
				if (sortedCanvases[i] == null || !sortedCanvases[i].enabled || !sortedCanvases[i].gameObject.activeSelf)
				{
					continue;
				}
				List<RaycastResult> list = new List<RaycastResult>();
				raycasters[i].Raycast(pointerEventData, list);
				if (list.Count > 0)
				{
					Tooltip componentInParent = list[0].gameObject.GetComponentInParent<Tooltip>();
					if (componentInParent != null && componentInParent.enabled)
					{
						ShowTooltip(componentInParent.text, componentInParent.labelPosition, componentInParent.isWorldspace);
					}
					break;
				}
			}
		}

		public void ShowTooltip(string text, Vector2 position, bool worldspace)
		{
			if (text == string.Empty || string.IsNullOrWhiteSpace(text))
			{
				Console.LogWarning("ShowTooltip: text is empty");
				return;
			}
			tooltipShownThisFrame = true;
			string text2 = tooltipLabel.text;
			tooltipLabel.text = text;
			if (text2 != text)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(anchor);
				tooltipLabel.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
			}
			anchor.sizeDelta = new Vector2(tooltipLabel.renderedWidth + 4f, tooltipLabel.renderedHeight + 1f);
			anchor.position = position + new Vector2(anchor.sizeDelta.x / 2f, (0f - anchor.sizeDelta.y) / 2f);
			Vector2 anchoredPosition = anchor.anchoredPosition;
			float min = Singleton<HUD>.Instance.canvasRect.sizeDelta.x * -0.5f - anchor.sizeDelta.x * anchor.pivot.x * -1f;
			float max = Singleton<HUD>.Instance.canvasRect.sizeDelta.x * 0.5f - anchor.sizeDelta.x * (1f - anchor.pivot.x);
			float min2 = Singleton<HUD>.Instance.canvasRect.sizeDelta.y * -0.5f - anchor.sizeDelta.y * anchor.pivot.y * -1f;
			float max2 = Singleton<HUD>.Instance.canvasRect.sizeDelta.y * 0.5f - anchor.sizeDelta.y * (1f - anchor.pivot.y);
			anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, min, max);
			anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, min2, max2);
			anchor.anchoredPosition = anchoredPosition;
			anchor.gameObject.SetActive(value: true);
		}
	}
}
