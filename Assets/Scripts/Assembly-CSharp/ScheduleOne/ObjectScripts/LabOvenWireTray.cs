using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class LabOvenWireTray : MonoBehaviour
	{
		public const float HIT_OFFSET_MAX = 0.24f;

		public const float HIT_OFFSET_MIN = -0.25f;

		[Header("References")]
		public Transform Tray;

		public Transform PlaneNormal;

		public Transform ClosedPosition;

		public Transform OpenPosition;

		public LabOvenDoor OvenDoor;

		[Header("Settings")]
		public float MoveSpeed = 2f;

		public AnimationCurve DoorClampCurve;

		private Vector3 clickOffset = Vector3.zero;

		private bool isMoving;

		public bool Interactable { get; private set; }

		public float TargetPosition { get; private set; }

		public float ActualPosition { get; private set; }

		private void Start()
		{
			SetPosition(0f);
			SetInteractable(interactable: false);
		}

		private void LateUpdate()
		{
			if (isMoving)
			{
				Vector3 position = GetPlaneHit() + clickOffset;
				float y = PlaneNormal.InverseTransformPoint(position).y;
				Debug.Log("Hit offset: " + y);
				y = Mathf.Clamp01(Mathf.InverseLerp(-0.25f, 0.24f, y));
				TargetPosition = y;
			}
			Move();
			ClampAngle();
		}

		private void Move()
		{
			Vector3 b = Vector3.Lerp(ClosedPosition.localPosition, OpenPosition.localPosition, TargetPosition);
			Tray.localPosition = Vector3.Lerp(Tray.localPosition, b, Time.deltaTime * MoveSpeed);
			ActualPosition = Mathf.Lerp(ActualPosition, TargetPosition, Time.deltaTime * MoveSpeed);
		}

		private void ClampAngle()
		{
			float max = DoorClampCurve.Evaluate(OvenDoor.ActualPosition);
			ActualPosition = Mathf.Clamp(ActualPosition, 0f, max);
			Vector3 localPosition = Vector3.Lerp(ClosedPosition.localPosition, OpenPosition.localPosition, ActualPosition);
			Tray.localPosition = localPosition;
		}

		public void SetInteractable(bool interactable)
		{
			Interactable = interactable;
		}

		public void SetPosition(float position)
		{
			TargetPosition = position;
		}

		public void ClickStart(RaycastHit hit)
		{
			isMoving = true;
		}

		private Vector3 GetPlaneHit()
		{
			Plane plane = new Plane(PlaneNormal.forward, PlaneNormal.position);
			Ray ray = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(Input.mousePosition);
			plane.Raycast(ray, out var enter);
			return ray.GetPoint(enter);
		}

		public void ClickEnd()
		{
			isMoving = false;
		}
	}
}
