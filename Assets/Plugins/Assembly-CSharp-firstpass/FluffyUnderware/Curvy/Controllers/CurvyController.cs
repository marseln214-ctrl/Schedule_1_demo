using System;
using System.Globalization;
using System.Reflection;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace FluffyUnderware.Curvy.Controllers
{
	[ExecuteAlways]
	public abstract class CurvyController : DTVersionedMonoBehaviour, ISerializationCallbackReceiver
	{
		public enum CurvyControllerState
		{
			Stopped = 0,
			Playing = 1,
			Paused = 2
		}

		public enum MoveModeEnum
		{
			Relative = 0,
			AbsolutePrecise = 1
		}

		protected class OrientationDamper
		{
			[NotNull]
			private readonly CurvyController controller;

			[UsedImplicitly]
			[Obsolete]
			public Vector3 DirectionDampingVelocity;

			[UsedImplicitly]
			[Obsolete]
			public Vector3 UpDampingVelocity;

			public OrientationDamper([NotNull] CurvyController controller)
			{
				this.controller = controller;
			}

			public Quaternion Damp(Quaternion sourceOrientation, Vector3 targetForward, Vector3 targetUp, float deltaTime)
			{
				Vector3 forward = DampenVector(sourceOrientation * Vector3.forward, targetForward, deltaTime, controller.DirectionDampingTime, ref DirectionDampingVelocity);
				Vector3 upwards = DampenVector(sourceOrientation * Vector3.up, targetUp, deltaTime, controller.UpDampingTime, ref UpDampingVelocity);
				return Quaternion.LookRotation(forward, upwards);
			}

			private Vector3 DampenVector(Vector3 current, Vector3 target, float deltaTime, float dampingTime, ref Vector3 velocity)
			{
				if (dampingTime > 0f && controller.State == CurvyControllerState.Playing)
				{
					return (deltaTime > 0f) ? Vector3.SmoothDamp(current, target, ref velocity, dampingTime, float.PositiveInfinity, deltaTime) : current;
				}
				return target;
			}

			public void Reset()
			{
				DirectionDampingVelocity = (UpDampingVelocity = Vector3.zero);
			}
		}

		[Section("General", true, false, 100, Sort = 0, HelpURL = "https://curvyeditor.com/doclink/curvycontroller_general")]
		[Label(Tooltip = "Determines when to update")]
		public CurvyUpdateMethod UpdateIn;

		[SerializeField]
		[FieldCondition("IsNeededRigidbodyMissing", true, false, ActionAttribute.ActionEnum.ShowError, "Missing Rigidbody component. Its 'Is Kinematic' setting should be set to true", ActionAttribute.ActionPositionEnum.Below)]
		[FieldCondition("IsNeeded2DRigidbodyMissing", true, false, ActionAttribute.ActionEnum.ShowError, "Missing Rigidbody 2D component. Its 'Body Type' setting should be set to 'Kinematic'", ActionAttribute.ActionPositionEnum.Below)]
		[FieldCondition("IsNeededRigidbodyNotKinematic", true, false, ActionAttribute.ActionEnum.ShowError, "Rigidbody's 'Is Kinematic' setting should be set to true", ActionAttribute.ActionPositionEnum.Below)]
		[FieldCondition("IsNeeded2DRigidbodyNotKinematic", true, false, ActionAttribute.ActionEnum.ShowError, "Rigidbody 2Ds 'Body Type' setting should be set to 'Kinematic'", ActionAttribute.ActionPositionEnum.Below)]
		[FieldCondition("targetComponent", TargetComponent.Transform, false, ActionAttribute.ActionEnum.ShowInfo, "The transform's position and rotation are updated at the selected 'Update In' method.", ActionAttribute.ActionPositionEnum.Below)]
		[FieldCondition("targetComponent", TargetComponent.Transform, true, ActionAttribute.ActionEnum.ShowInfo, "The rigidbody's position and rotation are updated at the physics simulation, and not at the selected 'Update In' method. Please consider this if getting the position or rotation via script.", ActionAttribute.ActionPositionEnum.Below)]
		[Tooltip("The component controlled by the controller")]
		private TargetComponent targetComponent;

		[Section("Position", true, false, 100, Sort = 100, HelpURL = "https://curvyeditor.com/doclink/curvycontroller_position")]
		[SerializeField]
		private CurvyPositionMode m_PositionMode = CurvyPositionMode.WorldUnits;

		[RangeEx(0f, "maxPosition", "", "")]
		[SerializeField]
		[FormerlySerializedAs("m_InitialPosition")]
		[FieldCondition("ShouldDisablePositionSlider", true, false, ActionAttribute.ActionEnum.Disable, null, ActionAttribute.ActionPositionEnum.Below)]
		protected float m_Position;

		[Section("Motion", true, false, 100, Sort = 200, HelpURL = "https://curvyeditor.com/doclink/curvycontroller_move")]
		[SerializeField]
		private MoveModeEnum m_MoveMode = MoveModeEnum.AbsolutePrecise;

		[Positive]
		[SerializeField]
		private float m_Speed;

		[SerializeField]
		private MovementDirection m_Direction;

		[SerializeField]
		private CurvyClamping m_Clamping = CurvyClamping.Loop;

		[Label("Constraints", "")]
		[Tooltip("Defines what motions are to be frozen")]
		[FieldCondition("AreConstraintsConflicting", true, false, ActionAttribute.ActionEnum.ShowWarning, "The controller targets a Rididbody that has constraints on it. This can creates conflicts with the controller's constraints", ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private MotionConstraints motionConstraints;

		[SerializeField]
		[Tooltip("Start playing automatically when entering play mode")]
		private bool m_PlayAutomatically = true;

		[Section("Orientation", true, false, 100, Sort = 300, HelpURL = "https://curvyeditor.com/doclink/curvycontroller_orientation")]
		[Label("Source", "Source Vector")]
		[FieldCondition("ShowOrientationSection", false, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Hide)]
		[SerializeField]
		private OrientationModeEnum m_OrientationMode = OrientationModeEnum.Orientation;

		[Label("Lock Rotation", "When set, the controller will enforce the rotation to not change")]
		[SerializeField]
		private bool m_LockRotation = true;

		[Label("Target", "Target Vector3")]
		[FieldCondition("m_OrientationMode", OrientationModeEnum.None, false, ConditionalAttribute.OperatorEnum.OR, "ShowOrientationSection", false, false, Action = ActionAttribute.ActionEnum.Hide)]
		[SerializeField]
		private OrientationAxisEnum m_OrientationAxis;

		[Tooltip("Should the orientation ignore the movement direction?")]
		[FieldCondition("m_OrientationMode", OrientationModeEnum.None, false, ConditionalAttribute.OperatorEnum.OR, "ShowOrientationSection", false, false, Action = ActionAttribute.ActionEnum.Hide)]
		[SerializeField]
		private bool m_IgnoreDirection;

		[FluffyUnderware.DevTools.Min(0f, "Direction Damping Time", "If non zero, the direction vector (forward) of the controlled object will not be updated instantly, but using a damping effect that will last the specified amount of time.")]
		[FieldCondition("ShowOrientationSection", false, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Hide)]
		[SerializeField]
		private float m_DampingDirection;

		[FluffyUnderware.DevTools.Min(0f, "Up Damping Time", "If non zero, the up vector of the controlled object will not be updated instantly, but using a damping effect that will last the specified amount of time.")]
		[FieldCondition("ShowOrientationSection", false, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Hide)]
		[SerializeField]
		private float m_DampingUp;

		[Section("Offset", true, false, 100, Sort = 400, HelpURL = "https://curvyeditor.com/doclink/curvycontroller_orientation")]
		[FieldCondition("ShowOffsetSection", false, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Hide)]
		[RangeEx(-180f, 180f, "", "")]
		[SerializeField]
		private float m_OffsetAngle;

		[FieldCondition("ShowOffsetSection", false, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Hide)]
		[SerializeField]
		private float m_OffsetRadius;

		[FieldCondition("ShowOffsetSection", false, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Hide)]
		[Label("Compensate Offset", "Adjusts speed to match the change of travel distance due to offset")]
		[SerializeField]
		private bool m_OffsetCompensation = true;

		[Section("Events", true, false, 100, Sort = 500)]
		[SerializeField]
		protected ControllerEvent onInitialized = new ControllerEvent();

		protected const string ControllerNotReadyMessage = "The controller is not yet ready";

		protected CurvyControllerState State;

		protected float PrePlayPosition;

		protected MovementDirection PrePlayDirection;

		protected Quaternion LockedRotation;

		public ControllerEvent OnInitialized => onInitialized;

		public TargetComponent TargetComponent
		{
			get
			{
				return targetComponent;
			}
			set
			{
				targetComponent = value;
			}
		}

		public CurvyPositionMode PositionMode
		{
			get
			{
				return m_PositionMode;
			}
			set
			{
				m_PositionMode = value;
			}
		}

		public MoveModeEnum MoveMode
		{
			get
			{
				return m_MoveMode;
			}
			set
			{
				m_MoveMode = value;
			}
		}

		public bool PlayAutomatically
		{
			get
			{
				return m_PlayAutomatically;
			}
			set
			{
				m_PlayAutomatically = value;
			}
		}

		public CurvyClamping Clamping
		{
			get
			{
				return m_Clamping;
			}
			set
			{
				m_Clamping = value;
			}
		}

		public MotionConstraints MotionConstraints
		{
			get
			{
				return motionConstraints;
			}
			set
			{
				motionConstraints = value;
			}
		}

		public OrientationModeEnum OrientationMode
		{
			get
			{
				return m_OrientationMode;
			}
			set
			{
				m_OrientationMode = value;
			}
		}

		public bool LockRotation
		{
			get
			{
				return m_LockRotation;
			}
			set
			{
				m_LockRotation = value;
				if (m_LockRotation)
				{
					GetPositionAndRotation(out var _, out var rotation);
					LockedRotation = rotation;
				}
			}
		}

		public OrientationAxisEnum OrientationAxis
		{
			get
			{
				return m_OrientationAxis;
			}
			set
			{
				m_OrientationAxis = value;
			}
		}

		public float DirectionDampingTime
		{
			get
			{
				return m_DampingDirection;
			}
			set
			{
				float dampingDirection = Mathf.Max(0f, value);
				m_DampingDirection = dampingDirection;
			}
		}

		public float UpDampingTime
		{
			get
			{
				return m_DampingUp;
			}
			set
			{
				float dampingUp = Mathf.Max(0f, value);
				m_DampingUp = dampingUp;
			}
		}

		public bool IgnoreDirection
		{
			get
			{
				return m_IgnoreDirection;
			}
			set
			{
				m_IgnoreDirection = value;
			}
		}

		public float OffsetAngle
		{
			get
			{
				return m_OffsetAngle;
			}
			set
			{
				m_OffsetAngle = value;
			}
		}

		public float OffsetRadius
		{
			get
			{
				return m_OffsetRadius;
			}
			set
			{
				m_OffsetRadius = value;
			}
		}

		public bool OffsetCompensation
		{
			get
			{
				return m_OffsetCompensation;
			}
			set
			{
				m_OffsetCompensation = value;
			}
		}

		public float Speed
		{
			get
			{
				return m_Speed;
			}
			set
			{
				if (value < 0f)
				{
					value = 0f - value;
				}
				m_Speed = value;
			}
		}

		public float RelativePosition
		{
			get
			{
				return PositionMode switch
				{
					CurvyPositionMode.Relative => GetClampedPosition(m_Position, CurvyPositionMode.Relative, Clamping, Length), 
					CurvyPositionMode.WorldUnits => AbsoluteToRelative(GetClampedPosition(m_Position, CurvyPositionMode.WorldUnits, Clamping, Length)), 
					_ => throw new NotSupportedException(), 
				};
			}
			set
			{
				float clampedPosition = GetClampedPosition(value, CurvyPositionMode.Relative, Clamping, Length);
				switch (PositionMode)
				{
				case CurvyPositionMode.Relative:
					m_Position = clampedPosition;
					break;
				case CurvyPositionMode.WorldUnits:
					m_Position = RelativeToAbsolute(clampedPosition);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}

		public float AbsolutePosition
		{
			get
			{
				return PositionMode switch
				{
					CurvyPositionMode.Relative => RelativeToAbsolute(GetClampedPosition(m_Position, CurvyPositionMode.Relative, Clamping, Length)), 
					CurvyPositionMode.WorldUnits => GetClampedPosition(m_Position, CurvyPositionMode.WorldUnits, Clamping, Length), 
					_ => throw new NotSupportedException(), 
				};
			}
			set
			{
				float clampedPosition = GetClampedPosition(value, CurvyPositionMode.WorldUnits, Clamping, Length);
				switch (PositionMode)
				{
				case CurvyPositionMode.Relative:
					m_Position = AbsoluteToRelative(clampedPosition);
					break;
				case CurvyPositionMode.WorldUnits:
					m_Position = clampedPosition;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}

		public float Position
		{
			get
			{
				return PositionMode switch
				{
					CurvyPositionMode.Relative => RelativePosition, 
					CurvyPositionMode.WorldUnits => AbsolutePosition, 
					_ => throw new NotSupportedException(), 
				};
			}
			set
			{
				switch (PositionMode)
				{
				case CurvyPositionMode.Relative:
					RelativePosition = value;
					break;
				case CurvyPositionMode.WorldUnits:
					AbsolutePosition = value;
					break;
				default:
					throw new NotSupportedException();
				}
			}
		}

		public MovementDirection MovementDirection
		{
			get
			{
				return m_Direction;
			}
			set
			{
				m_Direction = value;
			}
		}

		public CurvyControllerState PlayState => State;

		public abstract bool IsReady { get; }

		protected virtual bool ShouldDisablePositionSlider
		{
			get
			{
				if (PositionMode == CurvyPositionMode.WorldUnits)
				{
					return !IsReady;
				}
				return false;
			}
		}

		[NotNull]
		protected OrientationDamper Damper { get; }

		public virtual Transform Transform => base.transform;

		public virtual Rigidbody Rigidbody
		{
			[CanBeNull]
			get
			{
				return base.transform.GetComponent<Rigidbody>();
			}
		}

		public virtual Rigidbody2D Rigidbody2D
		{
			[CanBeNull]
			get
			{
				return base.transform.GetComponent<Rigidbody2D>();
			}
		}

		protected virtual bool ShowOrientationSection => true;

		protected virtual bool ShowOffsetSection => OrientationMode != OrientationModeEnum.None;

		public abstract float Length { get; }

		protected bool isInitialized { get; private set; }

		protected float TimeSinceLastUpdate => Time.deltaTime;

		protected bool UseOffset => ShowOffsetSection;

		private float maxPosition => GetMaxPosition(PositionMode);

		private bool IsNeededRigidbodyMissing
		{
			get
			{
				if (targetComponent == TargetComponent.KinematicRigidbody)
				{
					return Rigidbody == null;
				}
				return false;
			}
		}

		private bool IsNeeded2DRigidbodyMissing
		{
			get
			{
				if (targetComponent == TargetComponent.KinematicRigidbody2D)
				{
					return Rigidbody2D == null;
				}
				return false;
			}
		}

		private bool IsNeededRigidbodyNotKinematic
		{
			get
			{
				Rigidbody rigidbody = Rigidbody;
				if (targetComponent == TargetComponent.KinematicRigidbody && rigidbody != null)
				{
					return !rigidbody.isKinematic;
				}
				return false;
			}
		}

		private bool IsNeeded2DRigidbodyNotKinematic
		{
			get
			{
				Rigidbody2D rigidbody2D = Rigidbody2D;
				if (targetComponent == TargetComponent.KinematicRigidbody2D && rigidbody2D != null)
				{
					return !rigidbody2D.isKinematic;
				}
				return false;
			}
		}

		private bool AreConstraintsConflicting
		{
			get
			{
				switch (TargetComponent)
				{
				case TargetComponent.KinematicRigidbody:
				{
					Rigidbody rigidbody;
					if ((rigidbody = Rigidbody) != null)
					{
						return rigidbody.constraints != RigidbodyConstraints.None;
					}
					break;
				}
				case TargetComponent.KinematicRigidbody2D:
				{
					Rigidbody2D rigidbody2D;
					if ((rigidbody2D = Rigidbody2D) != null)
					{
						return rigidbody2D.constraints != RigidbodyConstraints2D.None;
					}
					break;
				}
				}
				return false;
			}
		}

		[UsedImplicitly]
		[Obsolete]
		protected Vector3 DirectionDampingVelocity
		{
			get
			{
				return Damper.DirectionDampingVelocity;
			}
			set
			{
				Damper.DirectionDampingVelocity = value;
			}
		}

		[UsedImplicitly]
		[Obsolete]
		protected Vector3 UpDampingVelocity
		{
			get
			{
				return Damper.UpDampingVelocity;
			}
			set
			{
				Damper.UpDampingVelocity = value;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (!isInitialized && IsReady)
			{
				Initialize();
				InitializedApplyDeltaTime(0f);
			}
		}

		[UsedImplicitly]
		protected virtual void Start()
		{
			if (!isInitialized && IsReady)
			{
				Initialize();
				InitializedApplyDeltaTime(0f);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (isInitialized)
			{
				Deinitialize();
			}
		}

		[UsedImplicitly]
		protected virtual void Update()
		{
			if (UpdateIn == CurvyUpdateMethod.Update)
			{
				ApplyDeltaTime(TimeSinceLastUpdate);
			}
		}

		[UsedImplicitly]
		protected virtual void LateUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.LateUpdate || (!Application.isPlaying && UpdateIn == CurvyUpdateMethod.FixedUpdate))
			{
				ApplyDeltaTime(TimeSinceLastUpdate);
			}
		}

		[UsedImplicitly]
		protected virtual void FixedUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.FixedUpdate)
			{
				ApplyDeltaTime(TimeSinceLastUpdate);
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			Speed = m_Speed;
			LockRotation = m_LockRotation;
		}

		protected virtual void InitializedApplyDeltaTime(float deltaTime)
		{
			if (State == CurvyControllerState.Playing && Speed * deltaTime != 0f)
			{
				float num = ((UseOffset && OffsetCompensation && OffsetRadius != 0f) ? ComputeOffsetCompensatedSpeed(deltaTime) : Speed);
				if (num * deltaTime != 0f)
				{
					Advance(num, deltaTime);
				}
			}
			GetPositionAndRotation(out var position, out var rotation);
			ComputeTargetPositionAndRotation(out var targetPosition, out var targetUp, out var targetForward);
			Quaternion quaternion = Damper.Damp(rotation, targetForward, targetUp, deltaTime);
			SetPositionAndRotation(targetPosition, quaternion);
			if (position.NotApproximately(targetPosition) || rotation.DifferentOrientation(quaternion))
			{
				UserAfterUpdate();
			}
		}

		protected virtual void ComputeTargetPositionAndRotation(out Vector3 targetPosition, out Vector3 targetUp, out Vector3 targetForward)
		{
			GetInterpolatedSourcePosition(RelativePosition, out var interpolatedPosition, out var tangent, out var up);
			if (tangent == Vector3.zero || up == Vector3.zero)
			{
				GetOrientationNoneUpAndForward(out targetUp, out targetForward);
			}
			else
			{
				switch (OrientationMode)
				{
				case OrientationModeEnum.None:
					GetOrientationNoneUpAndForward(out targetUp, out targetForward);
					break;
				case OrientationModeEnum.Orientation:
				{
					Vector3 vector2 = ((m_Direction == MovementDirection.Backward && !IgnoreDirection) ? (-tangent) : tangent);
					switch (OrientationAxis)
					{
					case OrientationAxisEnum.Up:
						targetUp = up;
						targetForward = vector2;
						break;
					case OrientationAxisEnum.Down:
						targetUp = -up;
						targetForward = vector2;
						break;
					case OrientationAxisEnum.Forward:
						targetUp = -vector2;
						targetForward = up;
						break;
					case OrientationAxisEnum.Backward:
						targetUp = vector2;
						targetForward = -up;
						break;
					case OrientationAxisEnum.Left:
						targetUp = Vector3.Cross(up, vector2);
						targetForward = vector2;
						break;
					case OrientationAxisEnum.Right:
						targetUp = Vector3.Cross(vector2, up);
						targetForward = vector2;
						break;
					default:
						throw new NotSupportedException();
					}
					break;
				}
				case OrientationModeEnum.Tangent:
				{
					Vector3 vector = ((m_Direction == MovementDirection.Backward && !IgnoreDirection) ? (-tangent) : tangent);
					switch (OrientationAxis)
					{
					case OrientationAxisEnum.Up:
						targetUp = vector;
						targetForward = -up;
						break;
					case OrientationAxisEnum.Down:
						targetUp = -vector;
						targetForward = up;
						break;
					case OrientationAxisEnum.Forward:
						targetUp = up;
						targetForward = vector;
						break;
					case OrientationAxisEnum.Backward:
						targetUp = up;
						targetForward = -vector;
						break;
					case OrientationAxisEnum.Left:
						targetUp = up;
						targetForward = Vector3.Cross(up, vector);
						break;
					case OrientationAxisEnum.Right:
						targetUp = up;
						targetForward = Vector3.Cross(vector, up);
						break;
					default:
						throw new NotSupportedException();
					}
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			targetPosition = ((UseOffset && OffsetRadius != 0f) ? ApplyOffset(interpolatedPosition, tangent, up, OffsetAngle, OffsetRadius) : interpolatedPosition);
		}

		protected virtual void Initialize()
		{
			isInitialized = true;
			GetPositionAndRotation(out var _, out var rotation);
			LockedRotation = rotation;
			Damper.Reset();
			State = CurvyControllerState.Stopped;
			ResetPrePlayState();
			if (PlayAutomatically && Application.isPlaying)
			{
				Play();
			}
			BindEvents();
			UserAfterInit();
			onInitialized.Invoke(this);
		}

		protected virtual void Deinitialize()
		{
			UnbindEvents();
			isInitialized = false;
		}

		protected virtual void BindEvents()
		{
		}

		protected virtual void UnbindEvents()
		{
		}

		protected virtual void SavePrePlayState()
		{
			PrePlayPosition = m_Position;
			PrePlayDirection = m_Direction;
		}

		protected virtual void RestorePrePlayState()
		{
			m_Position = PrePlayPosition;
			m_Direction = PrePlayDirection;
		}

		protected virtual void ResetPrePlayState()
		{
			PrePlayPosition = 0f;
			PrePlayDirection = MovementDirection.Forward;
		}

		protected virtual void GetPositionAndRotation(out Vector3 position, out Quaternion rotation)
		{
			switch (TargetComponent)
			{
			case TargetComponent.Transform:
			{
				Transform transform3 = Transform;
				position = transform3.position;
				rotation = transform3.rotation;
				break;
			}
			case TargetComponent.KinematicRigidbody:
			{
				Rigidbody rigidbody = Rigidbody;
				if (rigidbody == null || !Application.isPlaying)
				{
					Transform transform2 = Transform;
					position = transform2.position;
					rotation = transform2.rotation;
				}
				else
				{
					position = rigidbody.position;
					rotation = rigidbody.rotation;
				}
				break;
			}
			case TargetComponent.KinematicRigidbody2D:
			{
				Rigidbody2D rigidbody2D = Rigidbody2D;
				if (rigidbody2D == null || !Application.isPlaying)
				{
					Transform transform = Transform;
					position = transform.position;
					rotation = transform.rotation;
				}
				else
				{
					position = rigidbody2D.position;
					rotation = Quaternion.AngleAxis(Rigidbody2D.rotation, rigidbody2D.transform.rotation * new Vector3(0f, 0f, 1f));
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		protected virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
		{
			Vector3 vector = default(Vector3);
			Quaternion quaternion;
			if (MotionConstraints == MotionConstraints.None)
			{
				vector = position;
				quaternion = rotation;
			}
			else
			{
				GetPositionAndRotation(out var position2, out var rotation2);
				vector.x = (((MotionConstraints & MotionConstraints.FreezePositionX) == 0) ? position.x : position2.x);
				vector.y = (((MotionConstraints & MotionConstraints.FreezePositionY) == 0) ? position.y : position2.y);
				vector.z = (((MotionConstraints & MotionConstraints.FreezePositionZ) == 0) ? position.z : position2.z);
				Vector3 eulerAngles = rotation.eulerAngles;
				Vector3 eulerAngles2 = rotation2.eulerAngles;
				Vector3 euler = default(Vector3);
				euler.x = (((MotionConstraints & MotionConstraints.FreezeRotationX) == 0) ? eulerAngles.x : eulerAngles2.x);
				euler.y = (((MotionConstraints & MotionConstraints.FreezeRotationY) == 0) ? eulerAngles.y : eulerAngles2.y);
				euler.z = (((MotionConstraints & MotionConstraints.FreezeRotationZ) == 0) ? eulerAngles.z : eulerAngles2.z);
				quaternion = Quaternion.Euler(euler);
			}
			switch (TargetComponent)
			{
			case TargetComponent.Transform:
				Transform.SetPositionAndRotation(vector, quaternion);
				break;
			case TargetComponent.KinematicRigidbody:
			{
				Rigidbody rigidbody = Rigidbody;
				if (rigidbody == null || !Application.isPlaying)
				{
					Transform.SetPositionAndRotation(vector, quaternion);
					break;
				}
				rigidbody.MovePosition(vector);
				rigidbody.MoveRotation(quaternion);
				break;
			}
			case TargetComponent.KinematicRigidbody2D:
			{
				Rigidbody2D rigidbody2D = Rigidbody2D;
				if (rigidbody2D == null || !Application.isPlaying)
				{
					Transform.SetPositionAndRotation(vector, quaternion);
					break;
				}
				rigidbody2D.MovePosition(vector);
				rigidbody2D.MoveRotation(quaternion);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		protected virtual void UserAfterInit()
		{
		}

		protected virtual void UserAfterUpdate()
		{
		}

		protected abstract void Advance(float speed, float deltaTime);

		protected abstract void SimulateAdvance(ref float tf, ref MovementDirection direction, float speed, float deltaTime);

		protected abstract float AbsoluteToRelative(float worldUnitDistance);

		protected abstract float RelativeToAbsolute(float relativeDistance);

		protected abstract Vector3 GetInterpolatedSourcePosition(float tf);

		protected abstract void GetInterpolatedSourcePosition(float tf, out Vector3 interpolatedPosition, out Vector3 tangent, out Vector3 up);

		protected abstract Vector3 GetOrientation(float tf);

		protected abstract Vector3 GetTangent(float tf);

		public CurvyController()
		{
			Damper = new OrientationDamper(this);
		}

		public void Play()
		{
			if (PlayState == CurvyControllerState.Stopped)
			{
				SavePrePlayState();
			}
			State = CurvyControllerState.Playing;
		}

		public void Stop()
		{
			if (PlayState != 0)
			{
				RestorePrePlayState();
			}
			State = CurvyControllerState.Stopped;
		}

		public void Pause()
		{
			if (PlayState == CurvyControllerState.Playing)
			{
				State = CurvyControllerState.Paused;
			}
		}

		public void Refresh()
		{
			ApplyDeltaTime(0f);
		}

		public void ApplyDeltaTime(float deltaTime)
		{
			if (!isInitialized && IsReady)
			{
				Initialize();
			}
			else if (isInitialized && !IsReady)
			{
				Deinitialize();
			}
			if (isInitialized)
			{
				InitializedApplyDeltaTime(deltaTime);
			}
		}

		public void TeleportTo(float newPosition)
		{
			float distance = Mathf.Abs(Position - newPosition);
			MovementDirection direction = ((!(Position < newPosition)) ? MovementDirection.Backward : MovementDirection.Forward);
			TeleportBy(distance, direction);
		}

		public void TeleportBy(float distance, MovementDirection direction)
		{
			if (PlayState != CurvyControllerState.Playing)
			{
				DTLog.LogError("[Curvy] Calling TeleportBy on a controller that is stopped. Please make the controller play first", this);
			}
			float speed = Speed;
			MovementDirection movementDirection = MovementDirection;
			Speed = Mathf.Abs(distance) * 1000f;
			MovementDirection = direction;
			ApplyDeltaTime(0.001f);
			Speed = speed;
			MovementDirection = movementDirection;
		}

		public void SetFromString(string fieldAndValue)
		{
			string[] array = fieldAndValue.Split('=');
			if (array.Length != 2)
			{
				return;
			}
			FieldInfo fieldInfo = GetType().FieldByName(array[0], includeInherited: true);
			if (fieldInfo != null)
			{
				try
				{
					if (fieldInfo.FieldType.IsEnum)
					{
						fieldInfo.SetValue(this, Enum.Parse(fieldInfo.FieldType, array[1]));
					}
					else
					{
						fieldInfo.SetValue(this, Convert.ChangeType(array[1], fieldInfo.FieldType, CultureInfo.InvariantCulture));
					}
					return;
				}
				catch (Exception ex)
				{
					Debug.LogWarning(base.name + ".SetFromString(): " + ex);
					return;
				}
			}
			PropertyInfo propertyInfo = GetType().PropertyByName(array[0], includeInherited: true);
			if (!(propertyInfo != null))
			{
				return;
			}
			try
			{
				if (propertyInfo.PropertyType.IsEnum)
				{
					propertyInfo.SetValue(this, Enum.Parse(propertyInfo.PropertyType, array[1]), null);
				}
				else
				{
					propertyInfo.SetValue(this, Convert.ChangeType(array[1], propertyInfo.PropertyType, CultureInfo.InvariantCulture), null);
				}
			}
			catch (Exception ex2)
			{
				Debug.LogWarning(base.name + ".SetFromString(): " + ex2);
			}
		}

		protected static Vector3 ApplyOffset(Vector3 position, Vector3 tangent, Vector3 up, float offsetAngle, float offsetRadius)
		{
			Quaternion quaternion = Quaternion.AngleAxis(offsetAngle, tangent);
			return position.Addition((quaternion * up).Multiply(offsetRadius));
		}

		protected static float GetClampedPosition(float position, CurvyPositionMode positionMode, CurvyClamping clampingMode, float length)
		{
			switch (positionMode)
			{
			case CurvyPositionMode.Relative:
				if (position == 1f)
				{
					return 1f;
				}
				return CurvyUtility.ClampTF(position, clampingMode);
			case CurvyPositionMode.WorldUnits:
				if (position == length)
				{
					return length;
				}
				return CurvyUtility.ClampDistance(position, clampingMode, length);
			default:
				throw new NotSupportedException();
			}
		}

		protected float GetMaxPosition(CurvyPositionMode positionMode)
		{
			return positionMode switch
			{
				CurvyPositionMode.Relative => 1f, 
				CurvyPositionMode.WorldUnits => IsReady ? Length : 0f, 
				_ => throw new NotSupportedException(), 
			};
		}

		protected float ComputeOffsetCompensatedSpeed(float deltaTime)
		{
			if (OffsetRadius == 0f)
			{
				return Speed;
			}
			GetInterpolatedSourcePosition(RelativePosition, out var interpolatedPosition, out var tangent, out var up);
			Vector3 vector = ApplyOffset(interpolatedPosition, tangent, up, OffsetAngle, OffsetRadius);
			float tf = RelativePosition;
			MovementDirection direction = m_Direction;
			SimulateAdvance(ref tf, ref direction, Speed, deltaTime);
			GetInterpolatedSourcePosition(tf, out var interpolatedPosition2, out var tangent2, out var up2);
			Vector3 vector2 = ApplyOffset(interpolatedPosition2, tangent2, up2, OffsetAngle, OffsetRadius);
			float magnitude = (interpolatedPosition2 - interpolatedPosition).magnitude;
			float magnitude2 = (vector - vector2).magnitude;
			float num = magnitude / magnitude2;
			return Speed * (float.IsNaN(num) ? 1f : num);
		}

		private void GetOrientationNoneUpAndForward(out Vector3 targetUp, out Vector3 targetForward)
		{
			if (LockRotation)
			{
				targetUp = LockedRotation * Vector3.up;
				targetForward = LockedRotation * Vector3.forward;
			}
			else
			{
				GetPositionAndRotation(out var _, out var rotation);
				targetUp = rotation * Vector3.up;
				targetForward = rotation * Vector3.forward;
			}
		}

		public void OnBeforeSerialize()
		{
		}

		public virtual void OnAfterDeserialize()
		{
			if (m_Speed < 0f)
			{
				m_Speed = Mathf.Abs(m_Speed);
				m_Direction = MovementDirection.Backward;
			}
			if ((short)MoveMode == 2)
			{
				MoveMode = MoveModeEnum.AbsolutePrecise;
			}
		}
	}
}
