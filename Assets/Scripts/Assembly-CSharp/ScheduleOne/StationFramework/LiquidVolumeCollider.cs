using UnityEngine;

namespace ScheduleOne.StationFramework
{
	public class LiquidVolumeCollider : MonoBehaviour
	{
		public LiquidContainer LiquidContainer;

		private void Awake()
		{
			if (LiquidContainer == null)
			{
				LiquidContainer = GetComponentInParent<LiquidContainer>();
			}
		}
	}
}
