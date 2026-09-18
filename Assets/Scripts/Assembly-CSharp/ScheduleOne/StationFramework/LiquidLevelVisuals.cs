using UnityEngine;

namespace ScheduleOne.StationFramework
{
	public class LiquidLevelVisuals : MonoBehaviour
	{
		public LiquidContainer Container;

		public Transform LiquidSurface;

		public Transform LiquidSurface_Min;

		public Transform LiquidSurface_Max;

		private void Update()
		{
			if (!(Container == null))
			{
				float num = Container.CurrentLiquidLevel / Container.MaxLevel;
				LiquidSurface.localPosition = Vector3.Lerp(LiquidSurface_Min.localPosition, LiquidSurface_Max.localPosition, num);
				LiquidSurface.localScale = new Vector3(LiquidSurface.localScale.x, num, LiquidSurface.localScale.z);
				LiquidSurface.gameObject.SetActive(Container.CurrentLiquidLevel > 0f);
			}
		}
	}
}
