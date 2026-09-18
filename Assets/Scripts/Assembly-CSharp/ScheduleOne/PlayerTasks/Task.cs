using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.Compass;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class Task
	{
		public enum EOutcome
		{
			Cancelled = 0,
			Success = 1,
			Fail = 2
		}

		public const float ClickDetectionRange = 3f;

		public float ClickDetectionRadius;

		protected float MultiGrabRadius = 0.08f;

		public const float MultiGrabForceMultiplier = 1.25f;

		public bool ClickDetectionEnabled = true;

		public EOutcome Outcome;

		public Action onTaskSuccess;

		public Action onTaskFail;

		public Action onTaskStop;

		protected Clickable clickable;

		protected Draggable draggable;

		protected DraggableConstraint constraint;

		protected float hitDistance;

		protected Vector3 relativeHitOffset = Vector3.zero;

		private bool multiDraggingEnabled;

		private Transform multiGrabProjectionPlane;

		private List<Draggable> multiDragTargets = new List<Draggable>();

		private bool isMultiDragging;

		private List<Clickable> forcedClickables = new List<Clickable>();

		public virtual string TaskName { get; protected set; }

		public string CurrentInstruction { get; protected set; } = string.Empty;


		public bool TaskActive { get; private set; }

		public Task()
		{
			TaskActive = true;
			Singleton<TaskManager>.Instance.StartTask(this);
			Singleton<CompassManager>.Instance.SetVisible(visible: false);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(TaskName);
		}

		public virtual void StopTask()
		{
			Singleton<TaskManager>.Instance.currentTask = null;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(TaskName);
			Singleton<CompassManager>.Instance.SetVisible(visible: true);
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
			TaskActive = false;
			if (clickable != null)
			{
				clickable.EndClick();
			}
			if (onTaskStop != null)
			{
				onTaskStop();
			}
		}

		public virtual void Success()
		{
			Outcome = EOutcome.Success;
			StopTask();
			Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
			if (onTaskSuccess != null)
			{
				onTaskSuccess();
			}
		}

		public virtual void Fail()
		{
			Outcome = EOutcome.Fail;
			StopTask();
			if (onTaskFail != null)
			{
				onTaskFail();
			}
		}

		public virtual void Update()
		{
			if (ClickDetectionEnabled && !isMultiDragging)
			{
				if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
				{
					clickable = GetClickable(out var hit);
					if (clickable != null)
					{
						clickable.StartClick(hit);
					}
					if (clickable is Draggable)
					{
						draggable = clickable as Draggable;
						constraint = draggable.GetComponent<DraggableConstraint>();
					}
				}
				if (clickable != null && (!GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) || !clickable.ClickableEnabled) && !forcedClickables.Contains(clickable))
				{
					clickable.EndClick();
					clickable = null;
					draggable = null;
				}
			}
			else if (clickable != null)
			{
				clickable.EndClick();
				clickable = null;
			}
			UpdateCursor();
		}

		protected virtual void UpdateCursor()
		{
			if (draggable != null || isMultiDragging)
			{
				Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Grab);
				return;
			}
			RaycastHit hit;
			Clickable clickable = GetClickable(out hit);
			if (clickable != null)
			{
				Singleton<CursorManager>.Instance.SetCursorAppearance(clickable.HoveredCursor);
			}
			else
			{
				Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
			}
		}

		public virtual void LateUpdate()
		{
			if (isMultiDragging)
			{
				Singleton<TaskManagerUI>.Instance.multiGrabIndicator.position = Input.mousePosition;
				Vector3 multiDragOrigin = GetMultiDragOrigin();
				Vector3 a = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(multiDragOrigin);
				Vector3 position = multiDragOrigin + PlayerSingleton<PlayerCamera>.Instance.transform.right * MultiGrabRadius;
				Vector3 b = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(position);
				float num = Vector3.Distance(a, b) / Singleton<TaskManagerUI>.Instance.canvas.scaleFactor;
				Singleton<TaskManagerUI>.Instance.multiGrabIndicator.sizeDelta = new Vector2(num * 2f, num * 2f);
				Singleton<TaskManagerUI>.Instance.multiGrabIndicator.gameObject.SetActive(value: true);
			}
			else
			{
				Singleton<TaskManagerUI>.Instance.multiGrabIndicator.gameObject.SetActive(value: false);
			}
		}

		private Vector3 GetMultiDragOrigin()
		{
			Ray ray = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(Input.mousePosition);
			new Plane(multiGrabProjectionPlane.forward, multiGrabProjectionPlane.position).Raycast(ray, out var enter);
			LayerMask layerMask = (int)default(LayerMask) | (1 << LayerMask.NameToLayer("Default"));
			if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(enter, out var hit, layerMask, includeTriggers: false))
			{
				return hit.point;
			}
			return ray.GetPoint(enter);
		}

		public virtual void FixedUpdate()
		{
			UpdateDraggablePhysics();
			if (ClickDetectionEnabled && multiDraggingEnabled && multiGrabProjectionPlane != null && GameInput.GetButton(GameInput.ButtonCode.SecondaryClick) && this.draggable == null)
			{
				isMultiDragging = true;
				Vector3 multiDragOrigin = GetMultiDragOrigin();
				Collider[] array = Physics.OverlapSphere(multiDragOrigin, MultiGrabRadius, LayerMask.GetMask("Task"));
				List<Draggable> list = new List<Draggable>();
				Collider[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					Draggable componentInParent = array2[i].GetComponentInParent<Draggable>();
					if (componentInParent != null && componentInParent.ClickableEnabled && componentInParent.CanBeMultiDragged)
					{
						list.Add(componentInParent);
					}
				}
				foreach (Draggable item in list)
				{
					if (!multiDragTargets.Contains(item))
					{
						multiDragTargets.Add(item);
						item.StartClick(default(RaycastHit));
						item.Rb.useGravity = false;
					}
					Vector3 force = (multiDragOrigin - item.transform.position) * 10f * item.DragForceMultiplier * 1.25f;
					item.Rb.AddForce(force, ForceMode.Acceleration);
				}
				Draggable[] array3 = multiDragTargets.ToArray();
				foreach (Draggable draggable in array3)
				{
					if (!list.Contains(draggable))
					{
						multiDragTargets.Remove(draggable);
						draggable.EndClick();
						draggable.Rb.useGravity = true;
					}
				}
			}
			else
			{
				isMultiDragging = false;
				Draggable[] array3 = multiDragTargets.ToArray();
				foreach (Draggable draggable2 in array3)
				{
					multiDragTargets.Remove(draggable2);
					draggable2.EndClick();
					draggable2.Rb.useGravity = true;
				}
			}
		}

		public void ForceStartClick(Clickable _clickable)
		{
			if (!forcedClickables.Contains(_clickable))
			{
				forcedClickables.Add(_clickable);
			}
			_clickable.StartClick(default(RaycastHit));
		}

		public void ForceEndClick(Clickable _clickable)
		{
			if (_clickable != null)
			{
				_clickable.EndClick();
				forcedClickables.Remove(_clickable);
			}
		}

		private void UpdateDraggablePhysics()
		{
			if (draggable != null)
			{
				Vector3 normalized = Vector3.ProjectOnPlane(PlayerSingleton<PlayerCamera>.Instance.Camera.transform.forward, Vector3.up).normalized;
				Vector3 inNormal = ((draggable.DragProjectionMode == Draggable.EDragProjectionMode.CameraForward) ? PlayerSingleton<PlayerCamera>.Instance.transform.forward : normalized);
				if (constraint != null && constraint.ProportionalZClamp)
				{
					inNormal = constraint.Container.forward;
				}
				Plane plane = new Plane(inNormal, draggable.originalHitPoint);
				Ray ray = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(Input.mousePosition);
				plane.Raycast(ray, out var enter);
				Vector3 force = (ray.GetPoint(enter) - draggable.transform.TransformPoint(relativeHitOffset)) * 10f * draggable.DragForceMultiplier;
				if (draggable.DragForceOrigin != null)
				{
					draggable.Rb.AddForceAtPosition(force, draggable.DragForceOrigin.position, ForceMode.Acceleration);
				}
				else
				{
					draggable.Rb.AddForce(force, ForceMode.Acceleration);
				}
				if (draggable.RotationEnabled)
				{
					float x = GameInput.MotionAxis.x;
					Vector3 vector = normalized;
					draggable.Rb.AddTorque(vector * (0f - x) * draggable.TorqueMultiplier, ForceMode.Acceleration);
				}
				draggable.PostFixedUpdate();
			}
		}

		protected virtual Clickable GetClickable(out RaycastHit hit)
		{
			LayerMask layerMask = (int)default(LayerMask) | (1 << LayerMask.NameToLayer("Task"));
			layerMask = (int)layerMask | (1 << LayerMask.NameToLayer("Temporary"));
			if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(3f, out hit, layerMask, includeTriggers: true, ClickDetectionRadius))
			{
				Clickable componentInParent = hit.collider.GetComponentInParent<Clickable>();
				if (componentInParent != null)
				{
					if (!componentInParent.enabled)
					{
						return null;
					}
					if (!componentInParent.ClickableEnabled)
					{
						return null;
					}
					if (componentInParent.IsHeld)
					{
						return null;
					}
					hitDistance = Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, hit.point);
					componentInParent.SetOriginalHitPoint(hit.point);
					if (componentInParent.AutoCalculateOffset)
					{
						relativeHitOffset = componentInParent.transform.InverseTransformPoint(hit.point);
						if (componentInParent.FlattenZOffset)
						{
							relativeHitOffset.z = 0f;
						}
					}
					else
					{
						relativeHitOffset = Vector3.zero;
					}
				}
				return componentInParent;
			}
			return null;
		}

		protected void EnableMultiDragging(Transform projectionPlane, float radius = 0.08f)
		{
			multiDraggingEnabled = true;
			multiGrabProjectionPlane = projectionPlane;
			MultiGrabRadius = radius;
		}
	}
}
