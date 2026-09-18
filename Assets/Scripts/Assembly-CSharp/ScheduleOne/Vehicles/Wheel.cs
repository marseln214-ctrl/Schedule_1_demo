using System;
using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.Vehicles
{
	public class Wheel : MonoBehaviour
	{
		public const float SIDEWAY_SLIP_THRESHOLD = 0.2f;

		public const float FORWARD_SLIP_THRESHOLD = 0.8f;

		public const float DRIFT_AUDIO_THRESHOLD = 0.2f;

		public const float MIN_SPEED_FOR_DRIFT = 5f;

		public bool DEBUG_MODE;

		[Header("References")]
		public Transform wheelModel;

		public Transform modelContainer;

		public WheelCollider wheelCollider;

		public Transform axleConnectionPoint;

		public Collider staticCollider;

		public ParticleSystem DriftParticles;

		[Header("Settings")]
		public bool DriftParticlesEnabled = true;

		public float ForwardStiffnessMultiplier_Handbrake = 0.5f;

		public float SidewayStiffnessMultiplier_Handbrake = 0.5f;

		[Header("Drift Audio")]
		public bool DriftAudioEnabled;

		public AudioSourceController DriftAudioSource;

		private float defaultForwardStiffness = 1f;

		private float defaultSidewaysStiffness = 1f;

		private LandVehicle vehicle;

		private Vector3 lastFramePosition = Vector3.zero;

		private WheelHit wheelData;

		private WheelFrictionCurve forwardCurve;

		private WheelFrictionCurve sidewaysCurve;

		private Transform wheelTransform;

		public bool isStatic { get; protected set; }

		public bool IsDrifting { get; protected set; }

		public bool IsDrifting_Smoothed => DriftTime > 0.2f;

		public float DriftTime { get; protected set; }

		public float DriftIntensity { get; protected set; }

		protected virtual void Start()
		{
			vehicle = GetComponentInParent<LandVehicle>();
			wheelCollider.ConfigureVehicleSubsteps(5f, 12, 15);
			defaultForwardStiffness = wheelCollider.forwardFriction.stiffness;
			defaultSidewaysStiffness = wheelCollider.sidewaysFriction.stiffness;
			wheelTransform = base.transform;
		}

		protected virtual void LateUpdate()
		{
			if (wheelCollider.enabled)
			{
				wheelCollider.GetWorldPose(out var pos, out var quat);
				wheelModel.transform.position = pos;
				if (vehicle.localPlayerIsDriver)
				{
					modelContainer.transform.localRotation = Quaternion.identity;
					wheelModel.transform.rotation = quat;
				}
				else
				{
					Vector3 vector = wheelTransform.position - lastFramePosition;
					float xAngle = wheelTransform.InverseTransformVector(vector).z / (MathF.PI * 2f * wheelCollider.radius) * 360f;
					wheelModel.transform.Rotate(xAngle, 0f, 0f, Space.Self);
					modelContainer.transform.localEulerAngles = new Vector3(0f, wheelCollider.steerAngle, 0f);
				}
			}
			if (DriftParticlesEnabled)
			{
				DriftParticles.transform.position = wheelTransform.position - Vector3.up * wheelCollider.radius;
			}
			lastFramePosition = wheelTransform.position;
		}

		private void FixedUpdate()
		{
			if (!vehicle.localPlayerIsDriver)
			{
				DriftParticles.Stop();
				DriftAudioSource.Stop();
				return;
			}
			ApplyFriction();
			CheckDrifting();
			UpdateDriftEffects();
			UpdateDriftAudio();
		}

		private void CheckDrifting()
		{
			if (!wheelCollider.enabled)
			{
				IsDrifting = false;
				DriftTime = 0f;
				DriftIntensity = 0f;
				return;
			}
			if (Mathf.Abs(vehicle.speed_Kmh) < 5f)
			{
				IsDrifting = false;
				DriftTime = 0f;
				DriftIntensity = 0f;
				return;
			}
			wheelCollider.GetGroundHit(out wheelData);
			IsDrifting = (Mathf.Abs(wheelData.sidewaysSlip) > 0.2f || Mathf.Abs(wheelData.forwardSlip) > 0.8f) && Mathf.Abs(vehicle.speed_Kmh) > 2f;
			float a = Mathf.Clamp01(Mathf.Abs(wheelData.sidewaysSlip));
			float b = Mathf.Clamp01(Mathf.Abs(wheelData.forwardSlip));
			DriftIntensity = Mathf.Max(a, b);
			if (IsDrifting)
			{
				DriftTime += Time.fixedDeltaTime;
			}
			else
			{
				DriftTime = 0f;
			}
			if (DEBUG_MODE)
			{
				Debug.Log("Sideways slip: " + wheelData.sidewaysSlip + "\nForward slip: " + wheelData.forwardSlip);
				Debug.Log("Drifting: " + IsDrifting);
			}
		}

		private void UpdateDriftEffects()
		{
			if (IsDrifting_Smoothed && DriftParticlesEnabled)
			{
				if (!DriftParticles.isPlaying)
				{
					DriftParticles.Play();
				}
			}
			else if (DriftParticles.isPlaying)
			{
				DriftParticles.Stop();
			}
		}

		private void UpdateDriftAudio()
		{
			if (DriftAudioEnabled)
			{
				if (IsDrifting_Smoothed && DriftIntensity > 0.2f && !DriftAudioSource.isPlaying)
				{
					DriftAudioSource.Play();
				}
				if (DriftAudioSource.isPlaying)
				{
					float volumeMultiplier = Mathf.Clamp01(Mathf.InverseLerp(0.2f, 1f, DriftIntensity));
					DriftAudioSource.VolumeMultiplier = volumeMultiplier;
				}
			}
		}

		private void ApplyFriction()
		{
			forwardCurve = wheelCollider.forwardFriction;
			forwardCurve.stiffness = defaultForwardStiffness * ((vehicle.handbrakeApplied && vehicle.isOccupied) ? ForwardStiffnessMultiplier_Handbrake : 1f);
			wheelCollider.forwardFriction = forwardCurve;
			sidewaysCurve = wheelCollider.sidewaysFriction;
			sidewaysCurve.stiffness = defaultSidewaysStiffness * ((vehicle.handbrakeApplied && vehicle.isOccupied) ? SidewayStiffnessMultiplier_Handbrake : 1f);
			wheelCollider.sidewaysFriction = sidewaysCurve;
		}

		public virtual void SetIsStatic(bool s)
		{
			isStatic = s;
			if (isStatic)
			{
				wheelCollider.enabled = false;
				wheelModel.transform.localPosition = new Vector3(wheelModel.transform.localPosition.x, (0f - wheelCollider.suspensionDistance) * wheelCollider.suspensionSpring.targetPosition, wheelModel.transform.localPosition.z);
				staticCollider.enabled = true;
				GroundWheelModel();
			}
			else
			{
				wheelCollider.enabled = true;
				staticCollider.enabled = false;
			}
		}

		private void GroundWheelModel()
		{
			wheelModel.localPosition = Vector3.zero;
		}

		public bool IsWheelGrounded()
		{
			WheelHit hit;
			return wheelCollider.GetGroundHit(out hit);
		}
	}
}
