using UnityEngine;

namespace ScheduleOne.TV
{
	public class PongPaddle : MonoBehaviour
	{
		public const float BOUND_Y = 160f;

		public const float MOVE_SPEED = 20f;

		public float SpeedMultiplier = 1f;

		public RectTransform Rect;

		public float TargetY { get; set; }

		public void SetTargetY(float y)
		{
			TargetY = y;
		}

		private void Update()
		{
			UpdateMove();
		}

		private void UpdateMove()
		{
			float y = Rect.anchoredPosition.y;
			y = Mathf.Lerp(y, TargetY, 20f * Time.deltaTime * SpeedMultiplier);
			y = Mathf.Clamp(y, -160f, 160f);
			Rect.anchoredPosition = new Vector3(Rect.anchoredPosition.x, y);
		}
	}
}
