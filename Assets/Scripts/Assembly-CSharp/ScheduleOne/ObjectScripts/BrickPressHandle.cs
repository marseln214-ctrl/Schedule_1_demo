using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class BrickPressHandle : MonoBehaviour
	{
		private float lastClickPosition;

		[Header("Settings")]
		public float MoveSpeed = 1f;

		public bool Locked;

		[Header("References")]
		public Transform PlaneNormal;

		public Transform RaisedTransform;

		public Transform LoweredTransform;

		public Clickable HandleClickable;

		public AudioSourceController ClickSound;

		private Vector3 clickOffset = Vector3.zero;

		private bool isMoving;

		public bool Interactable { get; private set; }

		public float CurrentPosition { get; private set; }

		public float TargetPosition { get; private set; }

		private void Start()
		{
			SetPosition(0f);
			SetInteractable(e: false);
			HandleClickable.onClickStart.AddListener(ClickStart);
			HandleClickable.onClickEnd.AddListener(ClickEnd);
		}

		private void LateUpdate()
		{
			if (!Locked)
			{
				if (isMoving)
				{
					Vector3 vector = GetPlaneHit() + clickOffset;
					float position = 1f - Mathf.Clamp01(Mathf.InverseLerp(Mathf.Min(LoweredTransform.position.y, RaisedTransform.position.y), Mathf.Max(LoweredTransform.position.y, RaisedTransform.position.y), vector.y));
					SetPosition(position);
				}
				else
				{
					SetPosition(Mathf.MoveTowards(TargetPosition, 0f, Time.deltaTime));
				}
			}
			Move();
		}

		private void Move()
		{
			CurrentPosition = Mathf.MoveTowards(CurrentPosition, TargetPosition, MoveSpeed * Time.deltaTime);
			base.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 360f, CurrentPosition));
			if (Mathf.Abs(CurrentPosition - lastClickPosition) > 0.1666f)
			{
				lastClickPosition = CurrentPosition;
				ClickSound.AudioSource.pitch = Mathf.Lerp(0.7f, 1.1f, CurrentPosition);
				ClickSound.Play();
			}
		}

		private void UpdateSound(float difference)
		{
			difference /= 0.05f;
			if (difference < 0f)
			{
				Mathf.Abs(difference);
			}
			if (difference > 0f)
			{
				Mathf.Abs(difference);
			}
		}

		public void SetPosition(float position)
		{
			TargetPosition = position;
		}

		public void SetInteractable(bool e)
		{
			Interactable = e;
			HandleClickable.ClickableEnabled = e;
		}

		public void ClickStart(RaycastHit hit)
		{
			isMoving = true;
			clickOffset = HandleClickable.transform.position - GetPlaneHit();
		}

		public void ClickEnd()
		{
			isMoving = false;
		}

		private Vector3 GetPlaneHit()
		{
			Plane plane = new Plane(PlaneNormal.forward, PlaneNormal.position);
			Ray ray = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(Input.mousePosition);
			plane.Raycast(ray, out var enter);
			return ray.GetPoint(enter);
		}
	}
}
