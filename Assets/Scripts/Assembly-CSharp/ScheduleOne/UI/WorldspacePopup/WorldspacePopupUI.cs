using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.WorldspacePopup
{
	public class WorldspacePopupUI : MonoBehaviour
	{
		[HideInInspector]
		public WorldspacePopup Popup;

		[Header("References")]
		public RectTransform Rect;

		public Image FillImage;

		public UnityEvent onDestroyed;

		public void SetFill(float fill)
		{
			FillImage.fillAmount = fill;
		}

		public void Destroy()
		{
			if (onDestroyed != null)
			{
				onDestroyed.Invoke();
			}
			Object.Destroy(base.gameObject);
		}
	}
}
