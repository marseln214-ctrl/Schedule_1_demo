using FluffyUnderware.Curvy.Controllers;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E21_VolumeControllerInput : MonoBehaviour
	{
		public float AngularVelocity = 0.2f;

		public ParticleSystem explosionEmitter;

		public VolumeController volumeController;

		public Transform rotatedTransform;

		public float maxSpeed = 40f;

		public float accelerationForward = 20f;

		public float accelerationBackward = 40f;

		private bool mGameOver;

		[UsedImplicitly]
		private void Awake()
		{
			if (!volumeController)
			{
				volumeController = GetComponent<VolumeController>();
			}
		}

		[UsedImplicitly]
		private void Start()
		{
			if (volumeController.IsReady)
			{
				ResetController();
				return;
			}
			volumeController.OnInitialized.AddListener(delegate
			{
				ResetController();
			});
		}

		[UsedImplicitly]
		private void ResetController()
		{
			volumeController.Speed = 0f;
			volumeController.RelativePosition = 0f;
			volumeController.CrossRelativePosition = 0f;
		}

		[UsedImplicitly]
		private void Update()
		{
			if ((bool)volumeController && !mGameOver)
			{
				if (volumeController.PlayState != CurvyController.CurvyControllerState.Playing)
				{
					volumeController.Play();
				}
				Vector2 normalized = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
				float value = volumeController.Speed + normalized.y * Time.deltaTime * Mathf.Lerp(accelerationBackward, accelerationForward, (normalized.y + 1f) / 2f);
				volumeController.Speed = Mathf.Clamp(value, 0f, maxSpeed);
				volumeController.CrossRelativePosition += AngularVelocity * Mathf.Clamp(volumeController.Speed / 10f, 0.2f, 1f) * normalized.x * Time.deltaTime;
				if ((bool)rotatedTransform)
				{
					float y = Mathf.Lerp(-90f, 90f, (normalized.x + 1f) / 2f);
					rotatedTransform.localRotation = Quaternion.Euler(0f, y, 0f);
				}
			}
		}

		public void OnCollisionEnter(Collision collision)
		{
		}

		public void OnTriggerEnter(Collider other)
		{
			if (!mGameOver)
			{
				explosionEmitter.Emit(200);
				volumeController.Pause();
				mGameOver = true;
				Invoke("StartOver", 1f);
			}
		}

		[UsedImplicitly]
		private void StartOver()
		{
			ResetController();
			mGameOver = false;
		}
	}
}
