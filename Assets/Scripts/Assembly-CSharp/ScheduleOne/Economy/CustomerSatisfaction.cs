using UnityEngine;

namespace ScheduleOne.Economy
{
	public class CustomerSatisfaction
	{
		public static float GetRelationshipChange(float satisfaction)
		{
			return Mathf.Lerp(-0.5f, 0.5f, satisfaction);
		}
	}
}
