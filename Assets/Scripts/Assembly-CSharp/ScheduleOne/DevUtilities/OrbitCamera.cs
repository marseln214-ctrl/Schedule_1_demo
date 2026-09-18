using EasyButtons;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class OrbitCamera : MonoBehaviour
	{
		[Header("References")]
		[SerializeField]
		protected Transform cameraStartPoint;

		[SerializeField]
		protected Transform centrePoint;

		[Header("Settings")]
		public float targetFollowSpeed = 1f;

		public float yMinLimit = -20f;

		public float yMaxLimit = 80f;

		public static float xSpeed = 200f;

		public static float ySpeed = 100f;

		private Vector3 rotationOriginPoint = Vector3.zero;

		private float distance = 10f;

		private float prevDistance;

		private float x;

		private float y;

		private Transform targetTransform;

		public bool isEnabled { get; protected set; }

		protected Transform cam => PlayerSingleton<PlayerCamera>.Instance.transform;

		protected virtual void Awake()
		{
			targetTransform = new GameObject("_OrbitCamTarget").transform;
			targetTransform.SetParent(GameObject.Find("_Temp").transform);
		}

		protected virtual void Start()
		{
		}

		protected virtual void Update()
		{
			if (isEnabled)
			{
				UpdateRotation();
			}
		}

		protected virtual void LateUpdate()
		{
			if (isEnabled)
			{
				FinalizeCameraMovement();
			}
		}

		[Button]
		public void Enable()
		{
			isEnabled = true;
			cameraStartPoint.LookAt(centrePoint);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(80f, 0.25f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(cam.position, cam.rotation, 0f);
			PlayerSingleton<PlayerCamera>.Instance.blockNextStopTransformOverride = true;
			Vector3 eulerAngles = cameraStartPoint.eulerAngles;
			x = eulerAngles.y;
			y = eulerAngles.x;
			targetTransform.position = cameraStartPoint.position;
			targetTransform.rotation = cameraStartPoint.rotation;
		}

		public void Disable()
		{
			isEnabled = false;
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.25f);
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.25f, reenableCameraLook: false);
		}

		protected void UpdateRotation()
		{
			if (GameInput.GetButtonDown(GameInput.ButtonCode.TertiaryClick))
			{
				distance = Vector3.Distance(centrePoint.position, targetTransform.position);
				rotationOriginPoint = centrePoint.position;
			}
			if (GameInput.GetButton(GameInput.ButtonCode.TertiaryClick))
			{
				x += GameInput.MouseDelta.x * xSpeed * 0.02f;
				y -= GameInput.MouseDelta.y * ySpeed * 0.02f;
				y = ClampAngle(y, yMinLimit, yMaxLimit);
				Quaternion quaternion = Quaternion.Euler(y, x, 0f);
				Vector3 position = quaternion * new Vector3(0f, 0f, 0f - distance) + rotationOriginPoint;
				targetTransform.rotation = quaternion;
				targetTransform.position = position;
			}
		}

		private static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
			return Mathf.Clamp(angle, min, max);
		}

		private void FinalizeCameraMovement()
		{
			cam.position = Vector3.Lerp(cam.position, targetTransform.position, Time.deltaTime * targetFollowSpeed);
			cam.rotation = Quaternion.Lerp(cam.rotation, targetTransform.rotation, Time.deltaTime * targetFollowSpeed);
		}
	}
}
