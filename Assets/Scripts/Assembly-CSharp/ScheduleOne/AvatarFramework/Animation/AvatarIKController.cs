using RootMotion.FinalIK;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Animation
{
	public class AvatarIKController : MonoBehaviour
	{
		[Header("References")]
		public BipedIK BodyIK;

		private Transform defaultLeftLegBendTarget;

		private Transform defaultRightLegBendTarget;

		private void Awake()
		{
			BodyIK.InitiateBipedIK();
			defaultLeftLegBendTarget = BodyIK.solvers.leftFoot.bendGoal;
			defaultRightLegBendTarget = BodyIK.solvers.rightFoot.bendGoal;
		}

		private void Start()
		{
			SetIKActive(active: false);
		}

		public void SetIKActive(bool active)
		{
			BodyIK.enabled = active;
		}

		public void OverrideLegBendTargets(Transform leftLegTarget, Transform rightLegTarget)
		{
			BodyIK.solvers.leftFoot.bendGoal = leftLegTarget;
			BodyIK.solvers.rightFoot.bendGoal = rightLegTarget;
		}

		public void ResetLegBendTargets()
		{
			BodyIK.solvers.leftFoot.bendGoal = defaultLeftLegBendTarget;
			BodyIK.solvers.rightFoot.bendGoal = defaultRightLegBendTarget;
		}
	}
}
