using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class DraggableConstraint : MonoBehaviour
	{
		public Transform Container;

		public Rigidbody Anchor;

		public bool ProportionalZClamp;

		public bool AlignUpToContainerPlane;

		[Header("Up Direction Clamping")]
		public bool ClampUpDirection;

		public float UpDirectionMaxDifference = 45f;

		private Vector3 startLocalPos;

		private Draggable draggable;

		private ConfigurableJoint joint;

		private Vector3 RelativePos
		{
			get
			{
				if (!(Container != null))
				{
					return base.transform.localPosition;
				}
				return Container.InverseTransformPoint(base.transform.position);
			}
		}

		private void Start()
		{
			draggable = GetComponent<Draggable>();
			if (ClampUpDirection)
			{
				joint = draggable.Rb.gameObject.AddComponent<ConfigurableJoint>();
				if (Anchor == null && Container != null)
				{
					Container.gameObject.AddComponent<Rigidbody>();
					Anchor = Container.gameObject.GetComponent<Rigidbody>();
					Anchor.isKinematic = true;
					Anchor.useGravity = false;
				}
				joint.connectedBody = Anchor;
				joint.zMotion = ConfigurableJointMotion.Locked;
				joint.angularXMotion = ConfigurableJointMotion.Locked;
				joint.angularYMotion = ConfigurableJointMotion.Locked;
				joint.angularZMotion = ConfigurableJointMotion.Limited;
			}
		}

		public void SetContainer(Transform container)
		{
			Container = container;
			startLocalPos = RelativePos;
			if (joint != null && Anchor == null && Container != null)
			{
				Anchor = Container.gameObject.AddComponent<Rigidbody>();
				Anchor.isKinematic = true;
				Anchor.useGravity = false;
				joint.connectedBody = Anchor;
			}
		}

		protected virtual void FixedUpdate()
		{
			if (AlignUpToContainerPlane)
			{
				AlignToContainerPlane();
			}
		}

		protected virtual void LateUpdate()
		{
			if (ProportionalZClamp)
			{
				ProportionalClamp();
			}
			if (ClampUpDirection)
			{
				ClampUpRot();
			}
		}

		private void ProportionalClamp()
		{
			if (!(Container == null) && !(draggable == null))
			{
				float num = Mathf.Clamp(Mathf.Abs(RelativePos.x) / startLocalPos.x, 0f, 1f);
				float num2 = Mathf.Abs(startLocalPos.z) * num;
				Vector3 position = Container.InverseTransformPoint(draggable.originalHitPoint);
				position.z = Mathf.Clamp(position.z, 0f - num2, num2);
				Vector3 originalHitPoint = Container.TransformPoint(position);
				draggable.SetOriginalHitPoint(originalHitPoint);
			}
		}

		private void LockRotationX()
		{
			Vector3 eulerAngles = (base.transform.rotation * Quaternion.Inverse(Container.rotation)).eulerAngles;
			eulerAngles.x = 0f;
			base.transform.rotation = Container.rotation * Quaternion.Euler(eulerAngles);
		}

		private void LockRotationY()
		{
			Vector3 eulerAngles = (base.transform.rotation * Quaternion.Inverse(Container.rotation)).eulerAngles;
			eulerAngles.y = 0f;
			base.transform.rotation = Container.rotation * Quaternion.Euler(eulerAngles);
		}

		private void AlignToContainerPlane()
		{
			Vector3 forward = Container.forward;
			Quaternion quaternion = Quaternion.LookRotation(forward, base.transform.up);
			Vector3 normalized = Vector3.ProjectOnPlane(base.transform.forward, forward).normalized;
			_ = Quaternion.FromToRotation(base.transform.forward, normalized) * quaternion;
			base.transform.rotation = quaternion;
		}

		private void ClampUpRot()
		{
			if (joint == null)
			{
				Console.LogWarning("No joint found on DraggableConstraint, cannot clamp up rotation");
				return;
			}
			Vector3.Angle(draggable.transform.up, Vector3.up);
			SoftJointLimit angularZLimit = joint.angularZLimit;
			angularZLimit.limit = UpDirectionMaxDifference;
			joint.angularZLimit = angularZLimit;
		}
	}
}
