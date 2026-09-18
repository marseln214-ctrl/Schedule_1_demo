using ScheduleOne.DevUtilities;
using ScheduleOne.Materials;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework.Animation
{
	public class AvatarFootstepDetector : MonoBehaviour
	{
		public const float MAX_DETECTION_RANGE = 20f;

		public const float GROUND_DETECTION_RANGE = 0.25f;

		public Avatar Avatar;

		public Transform ReferencePoint;

		public Transform LeftBone;

		public Transform RightBone;

		public float StepThreshold = 0.1f;

		public LayerMask GroundDetectionMask;

		private bool leftDown;

		private bool rightDown;

		public UnityEvent<EMaterialType, float> onStep = new UnityEvent<EMaterialType, float>();

		private void LateUpdate()
		{
			if (!Avatar.Anim.animator.enabled)
			{
				leftDown = false;
				rightDown = false;
			}
			else
			{
				if (!PlayerSingleton<PlayerCamera>.InstanceExists || !LeftBone.gameObject.activeInHierarchy)
				{
					return;
				}
				if (Vector3.Distance(ReferencePoint.position, PlayerSingleton<PlayerCamera>.Instance.transform.position) > 20f)
				{
					leftDown = false;
					rightDown = false;
					return;
				}
				if (LeftBone.position.y - ReferencePoint.position.y < StepThreshold)
				{
					if (!leftDown)
					{
						leftDown = true;
						TriggerStep();
					}
				}
				else
				{
					leftDown = false;
				}
				if (RightBone.position.y - ReferencePoint.position.y < StepThreshold)
				{
					if (!rightDown)
					{
						rightDown = true;
						TriggerStep();
					}
				}
				else
				{
					rightDown = false;
				}
			}
		}

		public void TriggerStep()
		{
			if (IsGrounded(out var surfaceType))
			{
				onStep.Invoke(surfaceType, 1f);
			}
		}

		public bool IsGrounded(out EMaterialType surfaceType)
		{
			surfaceType = EMaterialType.Generic;
			if (Physics.Raycast(ReferencePoint.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, 0.25f, GroundDetectionMask, QueryTriggerInteraction.Ignore))
			{
				MaterialTag componentInParent = hitInfo.collider.GetComponentInParent<MaterialTag>();
				if (componentInParent != null)
				{
					surfaceType = componentInParent.MaterialType;
				}
				return true;
			}
			return false;
		}
	}
}
