using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class LabStand : MonoBehaviour
	{
		[Header("Settings")]
		public float MoveSpeed = 1f;

		public bool FunnelEnabled;

		public float FunnelThreshold = 0.05f;

		[Header("References")]
		public Animation Anim;

		public Transform GripTransform;

		public Transform SpinnyThingy;

		public Transform RaisedTransform;

		public Transform LoweredTransform;

		public Transform PlaneNormal;

		public Clickable HandleClickable;

		public Transform Funnel;

		public GameObject Highlight;

		public AudioSourceController LowerSound;

		public AudioSourceController RaiseSound;

		private Vector3 clickOffset = Vector3.zero;

		private bool isMoving;

		public bool Interactable { get; private set; }

		public float CurrentPosition { get; private set; } = 1f;


		private void Start()
		{
			SetPosition(1f);
			SetInteractable(e: false);
			HandleClickable.onClickStart.AddListener(ClickStart);
			HandleClickable.onClickEnd.AddListener(ClickEnd);
		}

		private void LateUpdate()
		{
			if (isMoving)
			{
				Vector3 vector = GetPlaneHit() + clickOffset;
				float position = Mathf.Clamp01(Mathf.InverseLerp(Mathf.Min(LoweredTransform.position.y, RaisedTransform.position.y), Mathf.Max(LoweredTransform.position.y, RaisedTransform.position.y), vector.y));
				SetPosition(position);
			}
			Highlight.gameObject.SetActive(Interactable && !isMoving);
			Move();
			Funnel.gameObject.SetActive(FunnelEnabled && CurrentPosition < FunnelThreshold);
		}

		private void Move()
		{
			float y = GripTransform.localPosition.y;
			Vector3 b = Vector3.Lerp(LoweredTransform.localPosition, RaisedTransform.localPosition, CurrentPosition);
			Quaternion b2 = Quaternion.Lerp(LoweredTransform.localRotation, RaisedTransform.localRotation, CurrentPosition);
			GripTransform.localPosition = Vector3.Lerp(GripTransform.localPosition, b, Time.deltaTime * MoveSpeed);
			GripTransform.localRotation = Quaternion.Lerp(GripTransform.localRotation, b2, Time.deltaTime * MoveSpeed);
			float num = GripTransform.localPosition.y - y;
			SpinnyThingy.Rotate(Vector3.up, num * 1800f, Space.Self);
			UpdateSound(num);
		}

		private void UpdateSound(float difference)
		{
			difference /= 0.05f;
			float num = 0f;
			if (difference < 0f)
			{
				num = Mathf.Abs(difference);
			}
			float num2 = 0f;
			if (difference > 0f)
			{
				num2 = Mathf.Abs(difference);
			}
			LowerSound.VolumeMultiplier = num;
			RaiseSound.VolumeMultiplier = num2;
			if (num > 0f && !LowerSound.AudioSource.isPlaying)
			{
				LowerSound.Play();
			}
			else if (num == 0f)
			{
				LowerSound.Stop();
			}
			if (num2 > 0f && !RaiseSound.AudioSource.isPlaying)
			{
				RaiseSound.Play();
			}
			else if (num2 == 0f)
			{
				RaiseSound.Stop();
			}
		}

		public void SetPosition(float position)
		{
			CurrentPosition = position;
		}

		public void SetInteractable(bool e)
		{
			Interactable = e;
			HandleClickable.ClickableEnabled = e;
			if (Interactable)
			{
				Anim.Play();
			}
			else
			{
				Anim.Stop();
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
