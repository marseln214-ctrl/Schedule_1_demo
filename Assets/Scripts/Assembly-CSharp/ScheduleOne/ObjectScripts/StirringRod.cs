using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class StirringRod : MonoBehaviour
	{
		public const float MAX_STIR_RATE = 20f;

		public const float MAX_PIVOT_ANGLE = 7f;

		public float LerpSpeed = 10f;

		[Header("References")]
		public Clickable Clickable;

		public Transform PlaneNormal;

		public Transform Container;

		public Transform RodPivot;

		public AudioSourceController StirSound;

		private Vector3 clickOffset = Vector3.zero;

		private bool isMoving;

		public bool Interactable { get; private set; }

		public float CurrentStirringSpeed { get; private set; }

		private void Start()
		{
			SetInteractable(e: true);
			Clickable.onClickStart.AddListener(ClickStart);
			Clickable.onClickEnd.AddListener(ClickEnd);
		}

		private void Update()
		{
			float volumeMultiplier = Mathf.MoveTowards(StirSound.VolumeMultiplier, CurrentStirringSpeed, Time.deltaTime * 4f);
			StirSound.VolumeMultiplier = volumeMultiplier;
			if (StirSound.VolumeMultiplier > 0f && !StirSound.AudioSource.isPlaying)
			{
				StirSound.AudioSource.Play();
			}
			else if (StirSound.VolumeMultiplier == 0f)
			{
				StirSound.AudioSource.Stop();
			}
		}

		private void LateUpdate()
		{
			if (isMoving)
			{
				Vector3 forward = Container.forward;
				float num = Vector3.SignedAngle(to: GetPlaneHit() - PlaneNormal.position, from: PlaneNormal.forward, axis: PlaneNormal.up);
				Quaternion b = PlaneNormal.rotation * Quaternion.Euler(Vector3.up * num);
				Container.rotation = Quaternion.Lerp(Container.rotation, b, Time.deltaTime * LerpSpeed);
				float f = Vector3.SignedAngle(forward, Container.forward, PlaneNormal.up);
				CurrentStirringSpeed = Mathf.Clamp01(Mathf.Abs(f) / 20f);
				RodPivot.localEulerAngles = new Vector3(7f * (1f - CurrentStirringSpeed), 0f, 0f);
			}
			else
			{
				CurrentStirringSpeed = 0f;
			}
		}

		public void SetInteractable(bool e)
		{
			Interactable = e;
			Clickable.ClickableEnabled = e;
		}

		public void ClickStart(RaycastHit hit)
		{
			isMoving = true;
			clickOffset = Clickable.transform.position - GetPlaneHit();
		}

		private Vector3 GetPlaneHit()
		{
			Plane plane = new Plane(PlaneNormal.up, PlaneNormal.position);
			Ray ray = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(Input.mousePosition);
			plane.Raycast(ray, out var enter);
			return ray.GetPoint(enter);
		}

		public void ClickEnd()
		{
			isMoving = false;
		}

		public void Destroy()
		{
			Object.Destroy(base.gameObject);
		}
	}
}
