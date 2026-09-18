using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.StationFramework
{
	public class PourableAngleLimit : MonoBehaviour
	{
		public PourableModule Pourable;

		public DraggableConstraint Constraint;

		[Header("Settings")]
		public float AngleAtMaxFill = 15f;

		public float AngleAtMinFill = 90f;

		public float PourAngleMaxFill = 15f;

		public float PourAngleMinFill = 90f;

		private void Awake()
		{
			Constraint.ClampUpDirection = true;
		}

		public void FixedUpdate()
		{
			float upDirectionMaxDifference = Mathf.Lerp(AngleAtMinFill, AngleAtMaxFill, Pourable.NormalizedLiquidLevel);
			Constraint.UpDirectionMaxDifference = upDirectionMaxDifference;
			float angleFromUpToPour = Mathf.Lerp(PourAngleMinFill, PourAngleMaxFill, Pourable.NormalizedLiquidLevel);
			Pourable.AngleFromUpToPour = angleFromUpToPour;
		}
	}
}
