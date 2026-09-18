using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class CameraOverrider : MonoBehaviour
	{
		public float FOV = 70f;

		public void LateUpdate()
		{
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(base.transform.position, base.transform.rotation, 0f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(FOV, 0f);
		}
	}
}
