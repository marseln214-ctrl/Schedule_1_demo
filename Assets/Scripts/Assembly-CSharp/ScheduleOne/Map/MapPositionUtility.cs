using EasyButtons;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class MapPositionUtility : Singleton<MapPositionUtility>
	{
		public Transform OriginPoint;

		public Transform EdgePoint;

		public float MapDimensions = 2048f;

		private float conversionFactor { get; set; }

		protected override void Awake()
		{
			base.Awake();
			Recalculate();
		}

		public Vector2 GetMapPosition(Vector3 worldPosition)
		{
			return new Vector2(worldPosition.x - OriginPoint.position.x, worldPosition.z - OriginPoint.position.z) * conversionFactor;
		}

		[Button]
		public void Recalculate()
		{
			conversionFactor = MapDimensions * 0.5f / Vector3.Distance(OriginPoint.position, EdgePoint.position);
		}
	}
}
