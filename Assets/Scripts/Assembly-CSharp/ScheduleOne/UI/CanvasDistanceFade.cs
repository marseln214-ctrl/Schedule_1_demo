using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class CanvasDistanceFade : MonoBehaviour
	{
		public CanvasGroup CanvasGroup;

		public float MinDistance = 5f;

		public float MaxDistance = 10f;

		public void LateUpdate()
		{
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				float num = Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, base.transform.position);
				if (num < MinDistance)
				{
					CanvasGroup.alpha = 1f;
				}
				else if (num > MaxDistance)
				{
					CanvasGroup.alpha = 0f;
				}
				else
				{
					CanvasGroup.alpha = 1f - (num - MinDistance) / (MaxDistance - MinDistance);
				}
			}
		}
	}
}
