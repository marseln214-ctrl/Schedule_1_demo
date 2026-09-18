using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class MapHeightSampler
	{
		private static float SampleHeight = 100f;

		private static float SampleDistance = 200f;

		public static Vector3 ResetPosition = new Vector3(-166.5f, 3f, -60f);

		public static bool Sample(float x, out float y, float z)
		{
			y = 0f;
			Vector3 vector = new Vector3(x, SampleHeight, z);
			Debug.DrawRay(vector, Vector3.down * SampleDistance, Color.red, 100f);
			if (Physics.Raycast(vector, Vector3.down, out var hitInfo, SampleDistance, 1 << LayerMask.NameToLayer("Default"), QueryTriggerInteraction.Ignore))
			{
				y = hitInfo.point.y;
			}
			return false;
		}
	}
}
