using ScheduleOne.PlayerTasks;
using ScheduleOne.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts
{
	public class LabOvenHammer : MonoBehaviour
	{
		public Draggable Draggable;

		public DraggableConstraint Constraint;

		public RotateRigidbodyToTarget Rotator;

		public Transform CoM;

		public Transform ImpactPoint;

		public SmoothedVelocityCalculator VelocityCalculator;

		[Header("Settings")]
		public float MinHeight;

		public float MaxHeight = 0.3f;

		public float MinAngle = 100f;

		public float MaxAngle = 40f;

		public UnityEvent<Collision> onCollision;

		private void Start()
		{
			Draggable.Rb.centerOfMass = CoM.localPosition;
		}

		private void Update()
		{
			Rotator.enabled = Draggable.IsHeld;
			if (Draggable.IsHeld)
			{
				Rotator.TargetRotation.z = Mathf.Lerp(MinAngle, MaxAngle, Mathf.Clamp01(Mathf.InverseLerp(MinHeight, MaxHeight, base.transform.localPosition.y)));
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (onCollision != null)
			{
				onCollision.Invoke(collision);
			}
		}
	}
}
