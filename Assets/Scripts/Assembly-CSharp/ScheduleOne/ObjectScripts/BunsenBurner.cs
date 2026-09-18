using ScheduleOne.Audio;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class BunsenBurner : MonoBehaviour
	{
		public bool LockDial;

		[Header("Settings")]
		public Gradient FlameColor;

		public AnimationCurve LightIntensity;

		public float HandleRotationSpeed = 1f;

		public AnimationCurve FlamePitch;

		[Header("References")]
		public ParticleSystem Flame;

		public Light Light;

		public Transform Handle;

		public Clickable HandleClickable;

		public Transform Handle_Min;

		public Transform Handle_Max;

		public Transform Highlight;

		public Animation Anim;

		public AudioSourceController FlameSound;

		public bool Interactable { get; private set; }

		public bool IsDialHeld { get; private set; }

		public float CurrentDialValue { get; private set; }

		public float CurrentHeat { get; private set; }

		private void Start()
		{
			SetInteractable(e: false);
			HandleClickable.onClickStart.AddListener(ClickStart);
			HandleClickable.onClickEnd.AddListener(ClickEnd);
		}

		private void Update()
		{
			if (!LockDial)
			{
				if (IsDialHeld)
				{
					CurrentDialValue = Mathf.Clamp01(CurrentDialValue + HandleRotationSpeed * Time.deltaTime);
				}
				else
				{
					CurrentDialValue = Mathf.Clamp01(CurrentDialValue - HandleRotationSpeed * Time.deltaTime);
				}
				Handle.localRotation = Quaternion.Lerp(Handle_Min.localRotation, Handle_Max.localRotation, CurrentDialValue);
			}
			CurrentHeat = CurrentDialValue;
			Highlight.gameObject.SetActive(Interactable && !IsDialHeld);
			if (CurrentHeat > 0f)
			{
				FlameSound.VolumeMultiplier = CurrentHeat;
				FlameSound.AudioSource.pitch = FlamePitch.Evaluate(CurrentHeat);
				if (!FlameSound.AudioSource.isPlaying)
				{
					FlameSound.Play();
				}
			}
			else if (FlameSound.AudioSource.isPlaying)
			{
				FlameSound.Stop();
			}
			UpdateEffects();
		}

		private void UpdateEffects()
		{
			if (CurrentHeat > 0f)
			{
				if (!Flame.isPlaying)
				{
					Flame.Play();
				}
				Light.gameObject.SetActive(value: true);
				Flame.startColor = FlameColor.Evaluate(CurrentHeat);
				Light.color = Flame.startColor;
				Light.intensity = LightIntensity.Evaluate(CurrentHeat);
			}
			else
			{
				if (Flame.isPlaying)
				{
					Flame.Stop();
				}
				Light.gameObject.SetActive(value: false);
			}
		}

		public void SetDialPosition(float pos)
		{
			CurrentDialValue = Mathf.Clamp01(pos);
			Handle.localRotation = Quaternion.Lerp(Handle_Min.localRotation, Handle_Max.localRotation, CurrentDialValue);
		}

		public void SetInteractable(bool e)
		{
			Interactable = e;
			HandleClickable.ClickableEnabled = e;
			if (!Interactable)
			{
				IsDialHeld = false;
			}
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
			IsDialHeld = true;
		}

		public void ClickEnd()
		{
			IsDialHeld = false;
		}
	}
}
