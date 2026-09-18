using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Building
{
	public class BuildStop_Base : MonoBehaviour
	{
		public virtual void Stop_Building()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount == 0)
			{
				Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
			}
			GetComponent<BuildUpdate_Base>().Stop();
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			Object.Destroy(base.gameObject);
		}
	}
}
