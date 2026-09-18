using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Controllers
{
	[AddComponentMenu("Curvy/Controllers/Spline Controller")]
	[HelpURL("https://curvyeditor.com/doclink/splinecontroller")]
	public class SplineController : CurvyController
	{
		protected class SplineSwitcher
		{
			public float StartTime { get; set; }

			public float Duration { get; set; }

			public CurvySpline Spline { get; set; }

			public float Tf { get; set; }

			public MovementDirection Direction { get; set; }

			public bool IsSwitching { get; set; }

			public float Progress
			{
				get
				{
					if (!IsSwitching)
					{
						return 0f;
					}
					return Mathf.Clamp01((Time.time - StartTime) / Duration);
				}
			}

			public void Start([NotNull] CurvySpline spline, float tf, float duration, MovementDirection direction)
			{
				if (duration <= 0f)
				{
					throw new ArgumentOutOfRangeException("duration", "Duration must be greater than 0");
				}
				if (tf < 0f || tf > 1f)
				{
					throw new ArgumentOutOfRangeException("tf", "Destination TF must be between 0 and 1");
				}
				StartTime = Time.time;
				Duration = duration;
				Spline = spline;
				Tf = tf;
				Direction = direction;
				IsSwitching = true;
			}

			public void Advance(CurvySpline spline, MoveModeEnum moveMode, float distance, CurvyClamping clamping)
			{
				float tf = Tf;
				MovementDirection direction = Direction;
				SimulateAdvanceOnSpline(spline, ref tf, ref direction, distance, moveMode, clamping);
				Tf = tf;
				Direction = direction;
			}

			public void Stop()
			{
				StartTime = 0f;
				Duration = 0f;
				Spline = null;
				Tf = 0f;
				Direction = MovementDirection.Forward;
				IsSwitching = false;
			}
		}

		[Section("General", true, false, 100, Sort = 0)]
		[FieldCondition("m_Spline", null, false, ActionAttribute.ActionEnum.ShowError, "Missing source Spline", ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		protected CurvySpline m_Spline;

		[SerializeField]
		[Tooltip("Whether spline's cache data should be used. Set this to true to gain performance if precision is not required.")]
		private bool m_UseCache;

		[Section("Connections Handling", true, false, 100, Sort = 250, HelpURL = "https://curvyeditor.com/doclink/curvycontroller_connectionshandling")]
		[SerializeField]
		[Label("At connection, use", "What spline should the controller use when reaching a Connection")]
		private SplineControllerConnectionBehavior connectionBehavior;

		[SerializeField]
		[Label("Allow direction change", "When true, the controller will modify its direction to best fit the connected spline")]
		private bool allowDirectionChange = true;

		[SerializeField]
		[Label("Reject current spline", "Whether the current spline should be excluded from the randomly selected splines")]
		[FieldCondition("ShowRandomConnectionOptions", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		private bool rejectCurrentSpline = true;

		[SerializeField]
		[Label("Reject divergent splines", "Whether splines that diverge from the current spline with more than a specific angle should be excluded from the randomly selected splines")]
		[FieldCondition("ShowRandomConnectionOptions", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		private bool rejectTooDivergentSplines;

		[SerializeField]
		[Label("Max allowed angle", "Maximum allowed divergence angle in degrees")]
		[Range(0f, 180f)]
		private float maxAllowedDivergenceAngle = 90f;

		[SerializeField]
		[Label("Custom Selector", "A custom logic to select which connected spline to follow. Select a Script inheriting from SplineControllerConnectionBehavior")]
		[FieldCondition("connectionBehavior", SplineControllerConnectionBehavior.Custom, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[FieldCondition("connectionCustomSelector", null, false, ActionAttribute.ActionEnum.ShowWarning, "Missing custom selector", ActionAttribute.ActionPositionEnum.Below)]
		private ConnectedControlPointsSelector connectionCustomSelector;

		[Section("Events", false, false, 1000, HelpURL = "https://curvyeditor.com/doclink/splinecontroller_events")]
		[SerializeField]
		[ArrayEx]
		protected List<OnPositionReachedSettings> onPositionReachedList = new List<OnPositionReachedSettings>();

		[SerializeField]
		protected CurvySplineMoveEvent m_OnControlPointReached = new CurvySplineMoveEvent();

		[SerializeField]
		protected CurvySplineMoveEvent m_OnEndReached = new CurvySplineMoveEvent();

		[SerializeField]
		protected CurvySplineMoveEvent m_OnSwitch = new CurvySplineMoveEvent();

		protected readonly SplineSwitcher Switcher;

		private CurvySpline prePlaySpline;

		private readonly CurvySplineMoveEventArgs preAllocatedEventArgs;

		private const string InvalidSegmentErrorMessage = "[Curvy] Controller {0} reached segment {1} which is invalid segment because it has a length of 0. Please fix the invalid segment to avoid issues with the controller";

		public virtual CurvySpline Spline
		{
			get
			{
				return m_Spline;
			}
			set
			{
				m_Spline = value;
			}
		}

		public bool UseCache
		{
			get
			{
				return m_UseCache;
			}
			set
			{
				m_UseCache = value;
			}
		}

		public SplineControllerConnectionBehavior ConnectionBehavior
		{
			get
			{
				return connectionBehavior;
			}
			set
			{
				connectionBehavior = value;
			}
		}

		public ConnectedControlPointsSelector ConnectionCustomSelector
		{
			get
			{
				return connectionCustomSelector;
			}
			set
			{
				connectionCustomSelector = value;
			}
		}

		public bool AllowDirectionChange
		{
			get
			{
				return allowDirectionChange;
			}
			set
			{
				allowDirectionChange = value;
			}
		}

		public bool RejectCurrentSpline
		{
			get
			{
				return rejectCurrentSpline;
			}
			set
			{
				rejectCurrentSpline = value;
			}
		}

		public bool RejectTooDivergentSplines
		{
			get
			{
				return rejectTooDivergentSplines;
			}
			set
			{
				rejectTooDivergentSplines = value;
			}
		}

		public float MaxAllowedDivergenceAngle
		{
			get
			{
				return maxAllowedDivergenceAngle;
			}
			set
			{
				maxAllowedDivergenceAngle = value;
			}
		}

		public List<OnPositionReachedSettings> OnPositionReachedList
		{
			get
			{
				return onPositionReachedList;
			}
			set
			{
				onPositionReachedList = value;
			}
		}

		public CurvySplineMoveEvent OnControlPointReached
		{
			get
			{
				return m_OnControlPointReached;
			}
			set
			{
				m_OnControlPointReached = value;
			}
		}

		public CurvySplineMoveEvent OnEndReached
		{
			get
			{
				return m_OnEndReached;
			}
			set
			{
				m_OnEndReached = value;
			}
		}

		public override float Length
		{
			get
			{
				if ((object)Spline == null)
				{
					return 0f;
				}
				return Spline.Length;
			}
		}

		public bool IsSwitching => Switcher.IsSwitching;

		public float SwitchProgress => Switcher.Progress;

		public CurvySplineMoveEvent OnSwitch
		{
			get
			{
				return m_OnSwitch;
			}
			set
			{
				m_OnSwitch = value;
			}
		}

		public override bool IsReady
		{
			get
			{
				if ((object)Spline != null)
				{
					return Spline.IsInitialized;
				}
				return false;
			}
		}

		private bool ShowRandomConnectionOptions
		{
			get
			{
				if (ConnectionBehavior != SplineControllerConnectionBehavior.FollowUpOtherwiseRandom)
				{
					return ConnectionBehavior == SplineControllerConnectionBehavior.RandomSpline;
				}
				return true;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use Switcher instead")]
		protected float SwitchStartTime => Switcher.StartTime;

		[UsedImplicitly]
		[Obsolete("Use Switcher instead")]
		protected float SwitchDuration => Switcher.Duration;

		[UsedImplicitly]
		[Obsolete("Use Switcher instead")]
		protected CurvySpline SwitchTarget => Switcher.Spline;

		[UsedImplicitly]
		[Obsolete("Use Switcher instead")]
		protected float TfOnSwitchTarget => Switcher.Tf;

		[UsedImplicitly]
		[Obsolete("Use Switcher instead")]
		protected MovementDirection DirectionOnSwitchTarget => Switcher.Direction;

		public SplineController()
		{
			preAllocatedEventArgs = new CurvySplineMoveEventArgs(this, Spline, null, float.NaN, usingWorldUnits: false, float.NaN, MovementDirection.Forward);
			Switcher = new SplineSwitcher();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (!IsReady)
			{
				return;
			}
			foreach (OnPositionReachedSettings onPositionReached in OnPositionReachedList)
			{
				onPositionReached.Position = Mathf.Min(Mathf.Max(onPositionReached.Position, 0f), GetMaxPosition(onPositionReached.PositionMode));
			}
		}

		public virtual void SwitchTo(CurvySpline destinationSpline, float destinationTf, float duration)
		{
			if (base.PlayState == CurvyControllerState.Stopped)
			{
				DTLog.LogError("[Curvy] Controller can not switch when stopped. The switch call will be ignored", this);
			}
			else if (duration <= 0f)
			{
				DTLog.LogWarning($"[Curvy] Controller switch has a duration set to {duration}. Duration should be a strictly positive value", this);
				Spline = destinationSpline;
				base.RelativePosition = destinationTf;
			}
			else
			{
				Switcher.Start(destinationSpline, destinationTf, duration, base.MovementDirection);
			}
		}

		public void FinishCurrentSwitch()
		{
			if (Switcher.IsSwitching)
			{
				Spline = Switcher.Spline;
				base.RelativePosition = Switcher.Tf;
				Switcher.Stop();
			}
		}

		public void CancelCurrentSwitch()
		{
			if (Switcher.IsSwitching)
			{
				Switcher.Stop();
			}
		}

		public static float GetAngleBetweenConnectedSplines(CurvySplineSegment before, MovementDirection movementMode, CurvySplineSegment after, bool allowMovementModeChange)
		{
			Vector3 from = before.GetTangentFast(0f) * movementMode.ToInt();
			Vector3 to = after.GetTangentFast(0f) * GetPostConnectionDirection(after, movementMode, allowMovementModeChange).ToInt();
			return Vector3.Angle(from, to);
		}

		protected override void SavePrePlayState()
		{
			prePlaySpline = Spline;
			base.SavePrePlayState();
		}

		protected override void RestorePrePlayState()
		{
			Spline = prePlaySpline;
			base.RestorePrePlayState();
		}

		protected override void ResetPrePlayState()
		{
			prePlaySpline = null;
			base.ResetPrePlayState();
		}

		protected override float RelativeToAbsolute(float relativeDistance)
		{
			return Spline.TFToDistance(relativeDistance, base.Clamping);
		}

		protected override float AbsoluteToRelative(float worldUnitDistance)
		{
			return Spline.DistanceToTF(worldUnitDistance, base.Clamping);
		}

		protected override Vector3 GetInterpolatedSourcePosition(float tf)
		{
			Vector3 position = (UseCache ? Spline.InterpolateFast(tf) : Spline.Interpolate(tf));
			return Spline.transform.TransformPoint(position);
		}

		protected override void GetInterpolatedSourcePosition(float tf, out Vector3 interpolatedPosition, out Vector3 tangent, out Vector3 up)
		{
			CurvySpline spline = Spline;
			Transform transform = spline.transform;
			float localF;
			CurvySplineSegment curvySplineSegment = spline.TFToSegment(tf, out localF);
			if ((object)curvySplineSegment != null)
			{
				if (UseCache)
				{
					curvySplineSegment.InterpolateAndGetTangentFast(localF, out interpolatedPosition, out tangent);
				}
				else
				{
					curvySplineSegment.InterpolateAndGetTangent(localF, out interpolatedPosition, out tangent);
				}
				up = curvySplineSegment.GetOrientationUpFast(localF);
			}
			else
			{
				interpolatedPosition = Vector3.zero;
				tangent = Vector3.zero;
				up = Vector3.zero;
			}
			interpolatedPosition = transform.TransformPoint(interpolatedPosition);
			tangent = transform.TransformDirection(tangent);
			up = transform.TransformDirection(up);
		}

		protected override Vector3 GetTangent(float tf)
		{
			Vector3 direction = (UseCache ? Spline.GetTangentFast(tf) : Spline.GetTangent(tf));
			return Spline.transform.TransformDirection(direction);
		}

		protected override Vector3 GetOrientation(float tf)
		{
			return Spline.transform.TransformDirection(Spline.GetOrientationUpFast(tf));
		}

		protected override void Advance(float speed, float deltaTime)
		{
			float distance = speed * deltaTime;
			if (Spline.Count != 0)
			{
				EventAwareMove(distance);
			}
			if (Switcher.IsSwitching && Switcher.Spline.Count > 0)
			{
				AdvanceSwitching(distance);
			}
		}

		protected override void SimulateAdvance(ref float tf, ref MovementDirection direction, float speed, float deltaTime)
		{
			float distance = speed * deltaTime;
			SimulateAdvanceOnSpline(Spline, ref tf, ref direction, distance, base.MoveMode, base.Clamping);
		}

		private static void SimulateAdvanceOnSpline(CurvySpline spline, ref float tf, ref MovementDirection direction, float distance, MoveModeEnum moveModeEnum, CurvyClamping curvyClamping)
		{
			if (spline.Count > 0)
			{
				int dir = direction.ToInt();
				switch (moveModeEnum)
				{
				case MoveModeEnum.AbsolutePrecise:
					tf = spline.DistanceToTF(spline.ClampDistance(spline.TFToDistance(tf) + distance * (float)dir, ref dir, curvyClamping));
					break;
				case MoveModeEnum.Relative:
					tf = CurvyUtility.ClampTF(tf + distance * (float)dir, ref dir, curvyClamping);
					break;
				default:
					throw new NotSupportedException();
				}
				direction = MovementDirectionMethods.FromInt(dir);
			}
		}

		protected override void InitializedApplyDeltaTime(float deltaTime)
		{
			if (Spline.Dirty)
			{
				Spline.Refresh();
			}
			base.InitializedApplyDeltaTime(deltaTime);
			if (Switcher.IsSwitching && Switcher.Progress >= 1f)
			{
				FinishCurrentSwitch();
			}
		}

		protected override void ComputeTargetPositionAndRotation(out Vector3 targetPosition, out Vector3 targetUp, out Vector3 targetForward)
		{
			base.ComputeTargetPositionAndRotation(out var targetPosition2, out var targetUp2, out var targetForward2);
			if (Switcher.IsSwitching)
			{
				GetSwitchingPositionAndRotation(targetForward2, targetUp2, targetPosition2, out var interpolatedPosition, out var interpolatedRotation);
				targetPosition = interpolatedPosition;
				targetUp = interpolatedRotation * Vector3.up;
				targetForward = interpolatedRotation * Vector3.forward;
			}
			else
			{
				targetPosition = targetPosition2;
				targetUp = targetUp2;
				targetForward = targetForward2;
			}
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			preAllocatedEventArgs.Set_INTERNAL(this, Spline, null, float.NaN, float.NaN, MovementDirection.Forward, usingWorldUnits: false);
			Switcher.Stop();
		}

		private void AdvanceSwitching(float distance)
		{
			Switcher.Advance(Switcher.Spline, base.MoveMode, distance, base.Clamping);
			preAllocatedEventArgs.Set_INTERNAL(this, Switcher.Spline, null, Switcher.Tf, Switcher.Progress, Switcher.Direction, usingWorldUnits: false);
			OnSwitch.Invoke(preAllocatedEventArgs);
			if (preAllocatedEventArgs.Cancel)
			{
				CancelCurrentSwitch();
			}
		}

		private void GetSwitchingPositionAndRotation(Vector3 forwardOnCurrentSpline, Vector3 upOnCurrentSpline, Vector3 positionOnCurrentSpline, out Vector3 interpolatedPosition, out Quaternion interpolatedRotation)
		{
			Quaternion a = Quaternion.LookRotation(forwardOnCurrentSpline, upOnCurrentSpline);
			ComputePositionAndRotationOnSwitchTarget(out var positionOnSwitchToSpline, out var rotationOnSwitchToSpline);
			interpolatedPosition = positionOnCurrentSpline.LerpUnclamped(positionOnSwitchToSpline, Switcher.Progress);
			interpolatedRotation = Quaternion.LerpUnclamped(a, rotationOnSwitchToSpline, Switcher.Progress);
		}

		private void ComputePositionAndRotationOnSwitchTarget(out Vector3 positionOnSwitchToSpline, out Quaternion rotationOnSwitchToSpline)
		{
			CurvySpline spline = Spline;
			float relativePosition = base.RelativePosition;
			m_Spline = Switcher.Spline;
			base.RelativePosition = Switcher.Tf;
			base.ComputeTargetPositionAndRotation(out positionOnSwitchToSpline, out var targetUp, out var targetForward);
			rotationOnSwitchToSpline = Quaternion.LookRotation(targetForward, targetUp);
			m_Spline = spline;
			base.RelativePosition = relativePosition;
		}

		private static float MovementCompatibleGetPosition(SplineController controller, float clampedPosition, CurvyPositionMode positionMode, out CurvySplineSegment controlPoint, out bool isOnControlPoint)
		{
			CurvySpline spline = controller.Spline;
			float localF;
			bool isOnSegmentStart;
			bool isOnSegmentEnd;
			switch (controller.PositionMode)
			{
			case CurvyPositionMode.Relative:
				controlPoint = spline.TFToSegment(clampedPosition, out localF, out isOnSegmentStart, out isOnSegmentEnd, CurvyClamping.Clamp);
				break;
			case CurvyPositionMode.WorldUnits:
				controlPoint = spline.DistanceToSegment(clampedPosition, out localF, out isOnSegmentStart, out isOnSegmentEnd);
				break;
			default:
				throw new NotSupportedException();
			}
			float result = ((positionMode == controller.PositionMode) ? clampedPosition : (positionMode switch
			{
				CurvyPositionMode.Relative => spline.SegmentToTF(controlPoint, controlPoint.DistanceToLocalF(localF)), 
				CurvyPositionMode.WorldUnits => controlPoint.Distance + controlPoint.LocalFToDistance(localF), 
				_ => throw new ArgumentOutOfRangeException(), 
			}));
			if (isOnSegmentEnd)
			{
				controlPoint = spline.GetNextControlPoint(controlPoint);
			}
			isOnControlPoint = isOnSegmentStart || isOnSegmentEnd;
			return result;
		}

		private static void MovementCompatibleSetPosition(SplineController controller, CurvyPositionMode positionMode, float specialClampedPosition)
		{
			if (positionMode == controller.PositionMode)
			{
				controller.m_Position = specialClampedPosition;
				return;
			}
			switch (positionMode)
			{
			case CurvyPositionMode.Relative:
				controller.m_Position = controller.Spline.TFToDistance(specialClampedPosition, controller.Clamping);
				break;
			case CurvyPositionMode.WorldUnits:
				controller.m_Position = controller.Spline.DistanceToTF(specialClampedPosition, controller.Clamping);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private void EventAwareMove(float distance)
		{
			CurvyPositionMode positionMode = base.MoveMode switch
			{
				MoveModeEnum.AbsolutePrecise => CurvyPositionMode.WorldUnits, 
				MoveModeEnum.Relative => CurvyPositionMode.Relative, 
				_ => throw new NotSupportedException(), 
			};
			float num = distance;
			bool cancelMovement = false;
			switch (base.MovementDirection)
			{
			case MovementDirection.Backward:
				if (m_Position == 0f)
				{
					if (base.Clamping == CurvyClamping.PingPong)
					{
						base.MovementDirection = base.MovementDirection.GetOpposite();
					}
					else if (base.Clamping == CurvyClamping.Clamp)
					{
						return;
					}
				}
				break;
			case MovementDirection.Forward:
			{
				float num2 = base.PositionMode switch
				{
					CurvyPositionMode.Relative => 1f, 
					CurvyPositionMode.WorldUnits => m_Spline.Length, 
					_ => throw new ArgumentOutOfRangeException(), 
				};
				if (m_Position == num2)
				{
					if (base.Clamping == CurvyClamping.PingPong)
					{
						base.MovementDirection = base.MovementDirection.GetOpposite();
					}
					else if (base.Clamping == CurvyClamping.Clamp)
					{
						return;
					}
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			CurvySplineSegment controlPoint;
			bool isOnControlPoint;
			float postEventsControlPointPosition = MovementCompatibleGetPosition(this, m_Position, positionMode, out controlPoint, out isOnControlPoint);
			if (controlPoint.Length == 0f && Spline.IsControlPointASegment(controlPoint))
			{
				DTLog.LogWarning($"[Curvy] Controller {base.name} reached segment {controlPoint} which is invalid segment because it has a length of 0. Please fix the invalid segment to avoid issues with the controller", this);
			}
			int num3 = 10000;
			while (!cancelMovement && num > 0f && num3-- > 0)
			{
				CurvySplineSegment curvySplineSegment = ((base.MovementDirection != 0) ? (isOnControlPoint ? Spline.GetPreviousControlPoint(controlPoint) : controlPoint) : Spline.GetNextControlPoint(controlPoint));
				if ((object)curvySplineSegment != null && Spline.IsControlPointVisible(curvySplineSegment))
				{
					float controlPointPosition = GetControlPointPosition(curvySplineSegment, positionMode);
					if (base.MovementDirection == MovementDirection.Forward && m_Spline.Closed && controlPointPosition == 0f)
					{
						controlPointPosition = GetMaxPosition(positionMode);
					}
					float num4 = Mathf.Abs(controlPointPosition - postEventsControlPointPosition);
					float postEventsEndPosition;
					if (num4 > num)
					{
						float num5 = postEventsControlPointPosition + num * (float)base.MovementDirection.ToInt();
						float clampedPosition = CurvyController.GetClampedPosition(num5, positionMode, base.Clamping, m_Spline.Length);
						HandleOnPositionReachedEvents(positionMode, postEventsControlPointPosition, clampedPosition, num5, out postEventsEndPosition, num, controlPoint, ref cancelMovement);
						MovementCompatibleSetPosition(this, positionMode, postEventsEndPosition);
						break;
					}
					HandleOnPositionReachedEvents(positionMode, postEventsControlPointPosition, controlPointPosition, controlPointPosition, out postEventsEndPosition, num, controlPoint, ref cancelMovement);
					if (!postEventsEndPosition.Approximately(controlPointPosition))
					{
						DTLog.LogWarning("[Curvy] Spline Controller " + base.name + ": Position was modified in an OnPositionReachedList event handler. That modification will be ignored to prioritize the controller reaching a new control point. You can use the OnControlPointReached event or OnEndReached instead. If this behavior is problematic, please contact the developers.", this);
					}
					num -= num4;
					HandleReachingNewControlPoint(curvySplineSegment, controlPointPosition, positionMode, num, ref cancelMovement, out controlPoint, out isOnControlPoint, out postEventsControlPointPosition);
				}
				if (isOnControlPoint && (bool)controlPoint.Connection && controlPoint.Connection.ControlPointsList.Count > 1)
				{
					CurvySplineSegment curvySplineSegment2;
					MovementDirection newDirection;
					switch (ConnectionBehavior)
					{
					case SplineControllerConnectionBehavior.CurrentSpline:
						curvySplineSegment2 = controlPoint;
						newDirection = base.MovementDirection;
						break;
					case SplineControllerConnectionBehavior.FollowUpSpline:
						curvySplineSegment2 = HandleFollowUpConnectionBehavior(controlPoint, base.MovementDirection, out newDirection);
						break;
					case SplineControllerConnectionBehavior.FollowUpOtherwiseRandom:
						curvySplineSegment2 = (controlPoint.FollowUp ? HandleFollowUpConnectionBehavior(controlPoint, base.MovementDirection, out newDirection) : HandleRandomConnectionBehavior(controlPoint, base.MovementDirection, out newDirection, controlPoint.Connection.ControlPointsList));
						break;
					case SplineControllerConnectionBehavior.RandomSpline:
						curvySplineSegment2 = HandleRandomConnectionBehavior(controlPoint, base.MovementDirection, out newDirection, controlPoint.Connection.ControlPointsList);
						break;
					case SplineControllerConnectionBehavior.Custom:
						if (ConnectionCustomSelector == null)
						{
							DTLog.LogError("[Curvy] You need to set a non null ConnectionCustomSelector when using SplineControllerConnectionBehavior.Custom", this);
							curvySplineSegment2 = controlPoint;
						}
						else
						{
							curvySplineSegment2 = ConnectionCustomSelector.SelectConnectedControlPoint(this, controlPoint.Connection, controlPoint);
						}
						newDirection = base.MovementDirection;
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
					if ((object)curvySplineSegment2 != controlPoint)
					{
						base.MovementDirection = newDirection;
						float controlPointPosition2 = GetControlPointPosition(curvySplineSegment2, positionMode);
						HandleReachingNewControlPoint(curvySplineSegment2, controlPointPosition2, positionMode, num, ref cancelMovement, out controlPoint, out isOnControlPoint, out postEventsControlPointPosition);
					}
				}
				if (!isOnControlPoint)
				{
					continue;
				}
				switch (base.Clamping)
				{
				case CurvyClamping.Loop:
					if (!Spline.Closed)
					{
						CurvySplineSegment curvySplineSegment3 = ((base.MovementDirection == MovementDirection.Backward && (object)controlPoint == Spline.FirstVisibleControlPoint) ? Spline.LastVisibleControlPoint : ((base.MovementDirection != 0 || (object)controlPoint != Spline.LastVisibleControlPoint) ? null : Spline.FirstVisibleControlPoint));
						if ((object)curvySplineSegment3 != null)
						{
							float controlPointPosition3 = GetControlPointPosition(curvySplineSegment3, positionMode);
							HandleReachingNewControlPoint(curvySplineSegment3, controlPointPosition3, positionMode, num, ref cancelMovement, out controlPoint, out isOnControlPoint, out postEventsControlPointPosition);
						}
					}
					break;
				case CurvyClamping.Clamp:
					if ((base.MovementDirection == MovementDirection.Backward && (object)controlPoint == Spline.FirstVisibleControlPoint) || (base.MovementDirection == MovementDirection.Forward && (object)controlPoint == Spline.LastVisibleControlPoint))
					{
						num = 0f;
					}
					break;
				case CurvyClamping.PingPong:
					if ((base.MovementDirection == MovementDirection.Backward && (object)controlPoint == Spline.FirstVisibleControlPoint) || (base.MovementDirection == MovementDirection.Forward && (object)controlPoint == Spline.LastVisibleControlPoint))
					{
						base.MovementDirection = base.MovementDirection.GetOpposite();
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			if (num3 <= 0)
			{
				DTLog.LogError($"[Curvy] Unexpected behavior in Spline Controller '{base.name}'. Please raise a Bug Report.", this);
			}
		}

		private void HandleOnPositionReachedEvents(CurvyPositionMode positionMode, float startPosition, float endPosition, float endPositionUnclamped, out float postEventsEndPosition, float currentDelta, CurvySplineSegment currentCp, ref bool cancelMovement)
		{
			float? num = null;
			foreach (OnPositionReachedSettings onPositionReached in OnPositionReachedList)
			{
				num = HandleOnPositionReachedEvent(positionMode, startPosition, endPositionUnclamped, currentDelta, currentCp, ref cancelMovement, onPositionReached, num);
				if (Spline.Closed)
				{
					OnPositionReachedSettings onPositionReachedSettings;
					if (base.MovementDirection == MovementDirection.Forward && onPositionReached.Position == 0f)
					{
						onPositionReachedSettings = onPositionReached.Clone();
						onPositionReachedSettings.Position = GetMaxPosition(onPositionReached.PositionMode);
					}
					else if (base.MovementDirection == MovementDirection.Backward && Mathf.Approximately(onPositionReached.Position, GetMaxPosition(onPositionReached.PositionMode)))
					{
						onPositionReachedSettings = onPositionReached.Clone();
						onPositionReachedSettings.Position = 0f;
					}
					else
					{
						onPositionReachedSettings = null;
					}
					if (onPositionReachedSettings != null)
					{
						num = HandleOnPositionReachedEvent(positionMode, startPosition, endPositionUnclamped, currentDelta, currentCp, ref cancelMovement, onPositionReachedSettings, num);
					}
				}
			}
			postEventsEndPosition = num ?? endPosition;
		}

		private float? HandleOnPositionReachedEvent(CurvyPositionMode positionMode, float startPosition, float endPositionUnclamped, float currentDelta, CurvySplineSegment currentCp, ref bool cancelMovement, OnPositionReachedSettings settings, float? postEventEndPosition)
		{
			float num = ((positionMode == settings.PositionMode) ? settings.Position : (positionMode switch
			{
				CurvyPositionMode.Relative => Spline.DistanceToTF(settings.Position), 
				CurvyPositionMode.WorldUnits => Spline.TFToDistance(settings.Position), 
				_ => throw new ArgumentOutOfRangeException("positionMode", positionMode, null), 
			}));
			TriggeringDirections triggeringDirections = settings.TriggeringDirections;
			bool num2 = (triggeringDirections == TriggeringDirections.All || triggeringDirections == TriggeringDirections.Forward) && startPosition < num && num <= endPositionUnclamped;
			bool flag = (triggeringDirections == TriggeringDirections.All || triggeringDirections == TriggeringDirections.Backward) && endPositionUnclamped <= num && num < startPosition;
			if (num2 || flag)
			{
				float num3 = Math.Abs(num - startPosition);
				MovementCompatibleSetPosition(this, settings.PositionMode, num);
				preAllocatedEventArgs.Set_INTERNAL(this, Spline, currentCp, num, currentDelta - num3, base.MovementDirection, settings.PositionMode == CurvyPositionMode.WorldUnits);
				InvokeEventHandler(settings.Event, preAllocatedEventArgs, positionMode, out var _, out var _, out postEventEndPosition);
				cancelMovement |= preAllocatedEventArgs.Cancel;
			}
			return postEventEndPosition;
		}

		private void HandleReachingNewControlPoint(CurvySplineSegment controlPoint, float controlPointPosition, CurvyPositionMode positionMode, float currentDelta, ref bool cancelMovement, out CurvySplineSegment postEventsControlPoint, out bool postEventsIsControllerOnControlPoint, out float postEventsControlPointPosition)
		{
			MovementCompatibleSetPosition(this, positionMode, controlPointPosition);
			Spline = controlPoint.Spline;
			postEventsControlPoint = controlPoint;
			postEventsIsControllerOnControlPoint = true;
			postEventsControlPointPosition = controlPointPosition;
			if (controlPoint.Length == 0f && Spline.IsControlPointASegment(controlPoint))
			{
				DTLog.LogWarning($"[Curvy] Controller {base.name} reached segment {controlPoint} which is invalid segment because it has a length of 0. Please fix the invalid segment to avoid issues with the controller", this);
			}
			preAllocatedEventArgs.Set_INTERNAL(this, Spline, controlPoint, controlPointPosition, currentDelta, base.MovementDirection, positionMode == CurvyPositionMode.WorldUnits);
			InvokeEventHandler(OnControlPointReached, preAllocatedEventArgs, positionMode, ref postEventsControlPoint, ref postEventsIsControllerOnControlPoint, ref postEventsControlPointPosition);
			if ((object)preAllocatedEventArgs.Spline.FirstVisibleControlPoint == preAllocatedEventArgs.ControlPoint || (object)preAllocatedEventArgs.Spline.LastVisibleControlPoint == preAllocatedEventArgs.ControlPoint)
			{
				InvokeEventHandler(OnEndReached, preAllocatedEventArgs, positionMode, ref postEventsControlPoint, ref postEventsIsControllerOnControlPoint, ref postEventsControlPointPosition);
			}
			cancelMovement |= preAllocatedEventArgs.Cancel;
		}

		private void InvokeEventHandler(CurvySplineMoveEvent @event, CurvySplineMoveEventArgs eventArgument, CurvyPositionMode positionMode, ref CurvySplineSegment postEventsControlPoint, ref bool postEventsIsControllerOnControlPoint, ref float postEventPosition)
		{
			InvokeEventHandler(@event, eventArgument, positionMode, out var postEventsControlPoint2, out var postEventsIsControllerOnControlPoint2, out var postEventPosition2);
			if (postEventPosition2.HasValue)
			{
				postEventPosition = postEventPosition2.Value;
			}
			if (postEventsIsControllerOnControlPoint2.HasValue)
			{
				postEventsIsControllerOnControlPoint = postEventsIsControllerOnControlPoint2.Value;
			}
			if (postEventsControlPoint2 != null)
			{
				postEventsControlPoint = postEventsControlPoint2;
			}
		}

		private void InvokeEventHandler(CurvySplineMoveEvent @event, CurvySplineMoveEventArgs eventArgument, CurvyPositionMode positionMode, out CurvySplineSegment postEventsControlPoint, out bool? postEventsIsControllerOnControlPoint, out float? postEventPosition)
		{
			float position = m_Position;
			CurvyPositionMode positionMode2 = base.PositionMode;
			CurvySpline spline = m_Spline;
			@event.Invoke(eventArgument);
			if (m_Position != position || base.PositionMode != positionMode2 || (object)m_Spline != spline)
			{
				postEventPosition = MovementCompatibleGetPosition(this, m_Position, positionMode, out postEventsControlPoint, out var isOnControlPoint);
				postEventsIsControllerOnControlPoint = isOnControlPoint;
			}
			else
			{
				postEventsControlPoint = null;
				postEventsIsControllerOnControlPoint = null;
				postEventPosition = null;
			}
		}

		private CurvySplineSegment HandleRandomConnectionBehavior(CurvySplineSegment currentControlPoint, MovementDirection currentDirection, out MovementDirection newDirection, ReadOnlyCollection<CurvySplineSegment> connectedControlPoints)
		{
			List<CurvySplineSegment> list = new List<CurvySplineSegment>(connectedControlPoints.Count);
			for (int i = 0; i < connectedControlPoints.Count; i++)
			{
				CurvySplineSegment curvySplineSegment = connectedControlPoints[i];
				if ((!RejectCurrentSpline || !(curvySplineSegment == currentControlPoint)) && (!RejectTooDivergentSplines || !(GetAngleBetweenConnectedSplines(currentControlPoint, currentDirection, curvySplineSegment, AllowDirectionChange) > MaxAllowedDivergenceAngle)))
				{
					list.Add(curvySplineSegment);
				}
			}
			CurvySplineSegment curvySplineSegment2 = ((list.Count == 0) ? currentControlPoint : list[UnityEngine.Random.Range(0, list.Count)]);
			newDirection = GetPostConnectionDirection(curvySplineSegment2, currentDirection, AllowDirectionChange);
			return curvySplineSegment2;
		}

		private static MovementDirection GetPostConnectionDirection(CurvySplineSegment connectedControlPoint, MovementDirection currentDirection, bool directionChangeAllowed)
		{
			if (!directionChangeAllowed || connectedControlPoint.Spline.Closed)
			{
				return currentDirection;
			}
			return HeadingToDirection(ConnectionHeadingEnum.Auto, connectedControlPoint, currentDirection);
		}

		private CurvySplineSegment HandleFollowUpConnectionBehavior(CurvySplineSegment currentControlPoint, MovementDirection currentDirection, out MovementDirection newDirection)
		{
			CurvySplineSegment result = (currentControlPoint.FollowUp ? currentControlPoint.FollowUp : currentControlPoint);
			newDirection = ((AllowDirectionChange && (bool)currentControlPoint.FollowUp) ? HeadingToDirection(currentControlPoint.FollowUpHeading, currentControlPoint.FollowUp, currentDirection) : currentDirection);
			return result;
		}

		private static MovementDirection HeadingToDirection(ConnectionHeadingEnum heading, CurvySplineSegment controlPoint, MovementDirection currentDirection)
		{
			return heading.ResolveAuto(controlPoint) switch
			{
				ConnectionHeadingEnum.Minus => MovementDirection.Backward, 
				ConnectionHeadingEnum.Sharp => currentDirection, 
				ConnectionHeadingEnum.Plus => MovementDirection.Forward, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		private static float GetControlPointPosition(CurvySplineSegment controlPoint, CurvyPositionMode positionMode)
		{
			return positionMode switch
			{
				CurvyPositionMode.Relative => controlPoint.TF, 
				CurvyPositionMode.WorldUnits => controlPoint.Distance, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}
}
