using UnityEngine;

namespace ScheduleOne.ObjectScripts.Cash
{
	public class Cash : MonoBehaviour
	{
		public static float stackSize = 250f;

		public static int[] amounts = new int[3]
		{
			5,
			50,
			(int)stackSize
		};

		public static int GetBillStacksToDisplay(float amount)
		{
			return Mathf.Clamp((int)(amount / 5f), 1, 50);
		}
	}
}
