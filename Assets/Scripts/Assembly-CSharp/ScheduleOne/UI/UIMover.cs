using UnityEngine;

namespace ScheduleOne.UI
{
	public class UIMover : MonoBehaviour
	{
		public RectTransform Rect;

		public Vector2 MinSpeed = Vector2.one;

		public Vector2 MaxSpeed = Vector2.one;

		public float SpeedMultiplier = 1f;

		private Vector2 speed = Vector2.zero;

		private void Start()
		{
			speed = new Vector2(Random.Range(MinSpeed.x, MaxSpeed.x), Random.Range(MinSpeed.y, MaxSpeed.y));
		}

		public void Update()
		{
			Vector2 vector = speed * SpeedMultiplier * Time.deltaTime;
			Rect.anchoredPosition += vector;
		}
	}
}
