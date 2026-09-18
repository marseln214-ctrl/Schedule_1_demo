using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerTasks
{
	public class Draggable : Clickable
	{
		public enum EDragProjectionMode
		{
			CameraForward = 0,
			FlatCameraForward = 1
		}

		[Header("Drag Force")]
		public float DragForceMultiplier = 30f;

		public Transform DragForceOrigin;

		[Header("Rotation")]
		public bool RotationEnabled = true;

		public float TorqueMultiplier = 20f;

		[Header("Settings")]
		public EDragProjectionMode DragProjectionMode;

		public bool DisableGravityWhenDragged;

		public float NormalRBDrag = 3f;

		public float HeldRBDrag = 15f;

		public bool CanBeMultiDragged = true;

		[Header("Additional force")]
		public float idleUpForce;

		[HideInInspector]
		public bool LocationRestrictionEnabled;

		[HideInInspector]
		public Vector3 Origin = Vector3.zero;

		[HideInInspector]
		public float MaxDistanceFromOrigin = 0.5f;

		public UnityEvent<Collider> onTriggerExit;

		protected DraggableConstraint constraint;

		public Rigidbody Rb { get; protected set; }

		public override CursorManager.ECursorType HoveredCursor { get; protected set; } = CursorManager.ECursorType.OpenHand;


		protected virtual void Awake()
		{
			Rb = GetComponent<Rigidbody>();
			constraint = GetComponent<DraggableConstraint>();
			if (base.gameObject.isStatic)
			{
				Console.LogWarning("Draggable object is static, this will cause issues with dragging.");
			}
		}

		protected virtual void FixedUpdate()
		{
			if (!(Rb == null))
			{
				Rb.drag = (base.IsHeld ? HeldRBDrag : NormalRBDrag);
				if (!base.IsHeld && !Rb.isKinematic)
				{
					Rb.angularVelocity = Vector3.ClampMagnitude(Rb.angularVelocity, Rb.angularVelocity.magnitude * 0.9f);
					Rb.velocity = Vector3.ClampMagnitude(Rb.velocity, Rb.velocity.magnitude * 0.95f);
					Rb.AddForce(Vector3.up * idleUpForce, ForceMode.Acceleration);
				}
			}
		}

		protected virtual void Update()
		{
		}

		public virtual void PostFixedUpdate()
		{
		}

		protected virtual void LateUpdate()
		{
			if (LocationRestrictionEnabled && Vector3.Distance(base.transform.position, Origin) > MaxDistanceFromOrigin)
			{
				base.transform.position = Origin + (base.transform.position - Origin).normalized * MaxDistanceFromOrigin;
			}
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			if (onTriggerExit != null)
			{
				onTriggerExit.Invoke(other);
			}
		}

		public override void StartClick(RaycastHit hit)
		{
			base.StartClick(hit);
			if (DisableGravityWhenDragged)
			{
				Rb.useGravity = false;
			}
		}

		public override void EndClick()
		{
			base.EndClick();
			if (DisableGravityWhenDragged && Rb != null)
			{
				Rb.useGravity = true;
			}
		}
	}
}
