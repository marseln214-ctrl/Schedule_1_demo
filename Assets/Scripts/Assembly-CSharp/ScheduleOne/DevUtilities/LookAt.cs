using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class LookAt : MonoBehaviour
	{
		public Transform Target;

		private void LateUpdate()
		{
			if (Target != null)
			{
				base.transform.LookAt(Target);
			}
		}
	}
}
