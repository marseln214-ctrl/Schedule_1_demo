using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class CopyPosition : MonoBehaviour
	{
		public Transform ToCopy;

		private void LateUpdate()
		{
			base.transform.position = ToCopy.position;
		}
	}
}
