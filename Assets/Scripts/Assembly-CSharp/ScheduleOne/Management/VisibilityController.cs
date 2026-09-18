using UnityEngine;

namespace ScheduleOne.Management
{
	public class VisibilityController : MonoBehaviour
	{
		public bool visibleOnlyInFullscreen = true;

		private void Start()
		{
			_ = visibleOnlyInFullscreen;
		}

		private void OnEnterFullScreen()
		{
			if (visibleOnlyInFullscreen)
			{
				base.gameObject.SetActive(value: true);
			}
		}

		private void OnExitFullScreen()
		{
			if (visibleOnlyInFullscreen)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
