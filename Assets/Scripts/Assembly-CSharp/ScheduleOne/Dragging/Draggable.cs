using System;
using EasyButtons;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Dragging
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(InteractableObject))]
	public class Draggable : MonoBehaviour, IGUIDRegisterable
	{
		public enum EInitialReplicationMode
		{
			Off = 0,
			OnlyIfMoved = 1,
			Full = 2
		}

		public const float INITIAL_REPLICATION_DISTANCE = 1f;

		public const float MAX_DRAG_START_RANGE = 2.5f;

		public const float MAX_TARGET_OFFSET = 1.5f;

		[SerializeField]
		protected string BakedGUID = string.Empty;

		[Header("References")]
		public Rigidbody Rigidbody;

		public InteractableObject IntObj;

		public Transform DragOrigin;

		[Header("Settings")]
		public bool CreateCoM = true;

		[Range(0.5f, 2f)]
		public float HoldDistanceMultiplier = 1f;

		[Range(0f, 5f)]
		public float DragForceMultiplier = 1f;

		public EInitialReplicationMode InitialReplicationMode;

		private float timeSinceLastDrag = 100f;

		public UnityEvent onDragStart;

		public UnityEvent onDragEnd;

		public UnityEvent onHovered;

		public UnityEvent onInteracted;

		public bool IsBeingDragged => CurrentDragger != null;

		public Player CurrentDragger { get; protected set; }

		public Guid GUID { get; protected set; }

		public Vector3 initialPosition { get; private set; }

		[Button]
		public void RegenerateGUID()
		{
			BakedGUID = Guid.NewGuid().ToString();
		}

		protected virtual void Awake()
		{
			IntObj.MaxInteractionRange = 2.5f;
			IntObj.onHovered.AddListener(Hovered);
			IntObj.onInteractStart.AddListener(Interacted);
			IntObj.SetMessage("Pick up");
			initialPosition = base.transform.position;
			if (CreateCoM)
			{
				Transform transform = new GameObject("CenterOfMass").transform;
				transform.SetParent(base.transform);
				transform.localPosition = Rigidbody.centerOfMass;
				IntObj.displayLocationPoint = transform;
				DragOrigin = transform;
			}
			if (!string.IsNullOrEmpty(BakedGUID))
			{
				GUID = new Guid(BakedGUID);
				GUIDManager.RegisterObject(this);
			}
		}

		protected virtual void Start()
		{
			NetworkSingleton<DragManager>.Instance.RegisterDraggable(this);
		}

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
		}

		protected void OnValidate()
		{
			if (IntObj == null)
			{
				IntObj = GetComponent<InteractableObject>();
			}
			if (Rigidbody == null)
			{
				Rigidbody = GetComponent<Rigidbody>();
			}
		}

		protected void OnDestroy()
		{
			if (NetworkSingleton<DragManager>.InstanceExists)
			{
				if (IsBeingDragged && NetworkSingleton<DragManager>.Instance.CurrentDraggable == this)
				{
					NetworkSingleton<DragManager>.Instance.StopDragging(Vector3.zero);
				}
				NetworkSingleton<DragManager>.Instance.Deregister(this);
			}
		}

		protected virtual void Update()
		{
			if (IsBeingDragged)
			{
				timeSinceLastDrag = 0f;
			}
			else if (timeSinceLastDrag < 1f)
			{
				timeSinceLastDrag += Time.deltaTime;
			}
		}

		private void FixedUpdate()
		{
			if (IsBeingDragged && CurrentDragger != Player.Local)
			{
				Vector3 targetPosition = CurrentDragger.MimicCamera.position + CurrentDragger.MimicCamera.forward * 1.25f * HoldDistanceMultiplier;
				ApplyDragForces(targetPosition);
			}
		}

		public void ApplyDragForces(Vector3 targetPosition)
		{
			Vector3 vector = targetPosition - base.transform.position;
			if (DragOrigin != null)
			{
				vector = targetPosition - DragOrigin.position;
			}
			float magnitude = vector.magnitude;
			Vector3 vector2 = vector.normalized * NetworkSingleton<DragManager>.Instance.DragForce * magnitude;
			vector2 -= Rigidbody.velocity * NetworkSingleton<DragManager>.Instance.DampingFactor;
			Rigidbody.AddForce(vector2 * DragForceMultiplier, ForceMode.Acceleration);
			Vector3 vector3 = Vector3.Cross(base.transform.up, Vector3.up);
			vector3 -= Rigidbody.angularVelocity * NetworkSingleton<DragManager>.Instance.TorqueDampingFactor;
			Rigidbody.AddTorque(vector3 * NetworkSingleton<DragManager>.Instance.TorqueForce, ForceMode.Acceleration);
		}

		protected virtual void Hovered()
		{
			if (CanInteract() && base.enabled)
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				IntObj.SetMessage("Pick up");
			}
			else
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
			if (onHovered != null)
			{
				onHovered.Invoke();
			}
		}

		protected virtual void Interacted()
		{
			if (base.enabled)
			{
				if (onInteracted != null)
				{
					onInteracted.Invoke();
				}
				if (CanInteract())
				{
					NetworkSingleton<DragManager>.Instance.StartDragging(this);
				}
			}
		}

		private bool CanInteract()
		{
			if (IsBeingDragged)
			{
				return false;
			}
			if (timeSinceLastDrag < 0.1f)
			{
				return false;
			}
			if (NetworkSingleton<DragManager>.Instance.IsDragging)
			{
				return false;
			}
			if (!NetworkSingleton<DragManager>.Instance.IsDraggingAllowed())
			{
				return false;
			}
			return true;
		}

		public void StartDragging(Player dragger)
		{
			if (!IsBeingDragged)
			{
				CurrentDragger = dragger;
				Rigidbody.useGravity = false;
				if (onDragStart != null)
				{
					onDragStart.Invoke();
				}
			}
		}

		public void StopDragging()
		{
			if (IsBeingDragged)
			{
				CurrentDragger = null;
				Rigidbody.useGravity = true;
				if (onDragEnd != null)
				{
					onDragEnd.Invoke();
				}
			}
		}
	}
}
