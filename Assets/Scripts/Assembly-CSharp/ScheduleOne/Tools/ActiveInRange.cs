using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class ActiveInRange : MonoBehaviour
	{
		public float Distance = 10f;

		public bool ScaleByLODBias = true;

		public GameObject[] ObjectsToActivate;

		public bool Reverse;

		private bool isVisible = true;

		private void LateUpdate()
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				return;
			}
			bool flag = Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, base.transform.position) < Distance * (ScaleByLODBias ? QualitySettings.lodBias : 1f);
			if (flag && !isVisible)
			{
				isVisible = true;
				GameObject[] objectsToActivate = ObjectsToActivate;
				for (int i = 0; i < objectsToActivate.Length; i++)
				{
					objectsToActivate[i].SetActive(!Reverse);
				}
			}
			else if (!flag && isVisible)
			{
				isVisible = false;
				GameObject[] objectsToActivate = ObjectsToActivate;
				for (int i = 0; i < objectsToActivate.Length; i++)
				{
					objectsToActivate[i].SetActive(Reverse ? true : false);
				}
			}
		}
	}
}
