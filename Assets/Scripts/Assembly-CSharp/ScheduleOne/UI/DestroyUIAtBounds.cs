using UnityEngine;

namespace ScheduleOne.UI
{
	public class DestroyUIAtBounds : MonoBehaviour
	{
		public RectTransform Rect;

		public Vector2 MinBounds = new Vector2(-1000f, -1000f);

		public Vector2 MaxBounds = new Vector2(1000f, 1000f);

		public void Update()
		{
			if (Rect.anchoredPosition.x < MinBounds.x || Rect.anchoredPosition.x > MaxBounds.x || Rect.anchoredPosition.y < MinBounds.y || Rect.anchoredPosition.y > MaxBounds.y)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
