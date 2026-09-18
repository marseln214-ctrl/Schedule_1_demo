using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class BetaEnabledGameObject : MonoBehaviour
	{
		private void Start()
		{
			if (!GameManager.IS_BETA)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
