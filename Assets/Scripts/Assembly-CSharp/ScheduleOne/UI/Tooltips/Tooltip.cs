using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Tooltips
{
	public class Tooltip : MonoBehaviour
	{
		[Header("Settings")]
		public string text;

		public Vector2 labelOffset;

		private RectTransform rect;

		private Canvas canvas;

		public Vector3 labelPosition
		{
			get
			{
				if (isWorldspace)
				{
					return RectTransformUtility.WorldToScreenPoint(Singleton<GameplayMenu>.Instance.OverlayCamera, rect.position);
				}
				return rect.position + new Vector3(labelOffset.x, labelOffset.y, 0f);
			}
		}

		public bool isWorldspace { get; private set; }

		protected virtual void Awake()
		{
			rect = GetComponent<RectTransform>();
			if (GetComponentInParent<GraphicRaycaster>() == null)
			{
				Console.LogWarning("Tooltip has not parent GraphicRaycaster! Tooltip won't ever be activated");
			}
			canvas = GetComponentInParent<Canvas>();
			if (canvas != null)
			{
				isWorldspace = canvas.renderMode == RenderMode.WorldSpace;
			}
		}
	}
}
