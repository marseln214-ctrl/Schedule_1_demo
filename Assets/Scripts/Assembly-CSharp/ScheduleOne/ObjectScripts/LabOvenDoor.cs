using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class LabOvenDoor : MonoBehaviour
	{
		public const float HIT_OFFSET_MAX = 0.24f;

		public const float HIT_OFFSET_MIN = -0.25f;

		public const float DOOR_ANGLE_CLOSED = 90f;

		public const float DOOR_ANGLE_OPEN = 10f;

		[Header("References")]
		public Clickable HandleClickable;

		public Transform Door;

		public Transform PlaneNormal;

		public AnimationCurve HitMapCurve;

		[Header("Sounds")]
		public AudioSourceController OpenSound;

		public AudioSourceController CloseSound;

		public AudioSourceController ShutSound;

		[Header("Settings")]
		public float DoorMoveSpeed = 2f;

		private Vector3 clickOffset = Vector3.zero;

		private bool isMoving;

		public bool Interactable { get; private set; }

		public float TargetPosition { get; private set; }

		public float ActualPosition { get; private set; }

		private void Start()
		{
			SetPosition(0f);
			SetInteractable(interactable: false);
			HandleClickable.onClickStart.AddListener(ClickStart);
			HandleClickable.onClickEnd.AddListener(ClickEnd);
		}

		private void LateUpdate()
		{
			if (isMoving)
			{
				Vector3 position = GetPlaneHit() + clickOffset;
				float y = PlaneNormal.InverseTransformPoint(position).y;
				y = Mathf.Clamp01(Mathf.InverseLerp(-0.25f, 0.24f, y));
				SetPosition(HitMapCurve.Evaluate(y));
			}
			Move();
		}

		private void Move()
		{
			float y = Mathf.Lerp(90f, 10f, TargetPosition);
			Quaternion b = Quaternion.Euler(0f, y, 0f);
			Door.localRotation = Quaternion.Lerp(Door.localRotation, b, Time.deltaTime * DoorMoveSpeed);
			ActualPosition = Mathf.Lerp(ActualPosition, TargetPosition, Time.deltaTime * DoorMoveSpeed);
		}

		public void SetInteractable(bool interactable)
		{
			Interactable = interactable;
			HandleClickable.ClickableEnabled = interactable;
		}

		public void SetPosition(float newPosition)
		{
			float targetPosition = TargetPosition;
			TargetPosition = newPosition;
			if (targetPosition == 0f && newPosition > 0.02f)
			{
				OpenSound.Play();
			}
			else if (targetPosition >= 0.98f && newPosition < 0.98f)
			{
				CloseSound.Play();
			}
			else if (targetPosition > 0.01f && newPosition <= 0.001f)
			{
				ShutSound.Play();
			}
		}

		public void ClickStart(RaycastHit hit)
		{
			isMoving = true;
			clickOffset = HandleClickable.transform.position - GetPlaneHit();
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
