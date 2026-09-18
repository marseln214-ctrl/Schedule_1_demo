using System.Collections;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class BirdsEyeView : Singleton<BirdsEyeView>
	{
		[Header("Settings")]
		public Vector3 bounds_Min;

		public Vector3 bounds_Max;

		[Header("Camera settings")]
		public float lateralMovementSpeed = 1f;

		public float scrollMovementSpeed = 1f;

		public float targetFollowSpeed = 1f;

		[Header("Camera orbit settings")]
		public float xSpeed = 250f;

		public float ySpeed = 120f;

		public float yMinLimit = -20f;

		public float yMaxLimit = 80f;

		private Vector3 rotationOriginPoint = Vector3.zero;

		private float distance = 10f;

		private float prevDistance;

		private float x;

		private float y;

		private Transform targetTransform;

		private Coroutine originSlideRoutine;

		private Transform playerCam => PlayerSingleton<PlayerCamera>.Instance.transform;

		public bool isEnabled { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			targetTransform = new GameObject("_TargetCameraTransform").transform;
			targetTransform.SetParent(GameObject.Find("_Temp").transform);
		}

		protected virtual void Update()
		{
			if (isEnabled)
			{
				UpdateLateralMovement();
				UpdateRotation();
				UpdateScrollMovement();
			}
		}

		protected virtual void LateUpdate()
		{
			if (isEnabled)
			{
				FinalizeCameraMovement();
			}
		}

		public void Enable(Vector3 startPosition, Quaternion startRotation)
		{
			isEnabled = true;
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(startPosition, startRotation, 0f);
			Vector3 eulerAngles = startRotation.eulerAngles;
			x = eulerAngles.y;
			y = eulerAngles.x;
			targetTransform.position = startPosition;
			targetTransform.rotation = startRotation;
		}

		public void Disable(bool reenableCameraLook = true)
		{
			isEnabled = false;
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook);
		}

		protected void UpdateLateralMovement()
		{
			float num = GameInput.MotionAxis.y;
			float num2 = GameInput.MotionAxis.x;
			int num3 = 0;
			if (Input.GetKey(KeyCode.Space))
			{
				num3++;
			}
			if (Input.GetKey(KeyCode.LeftControl))
			{
				num3--;
			}
			if (num != 0f || num3 != 0)
			{
				CancelOriginSlide();
			}
			Vector3 forward = playerCam.forward;
			forward.y = 0f;
			forward.Normalize();
			Vector3 right = playerCam.right;
			right.y = 0f;
			right.Normalize();
			Vector3 vector = forward * num * lateralMovementSpeed * Time.deltaTime;
			Vector3 vector2 = right * num2 * lateralMovementSpeed * Time.deltaTime;
			Vector3 vector3 = Vector3.up * num3 * lateralMovementSpeed * Time.deltaTime * 0.5f;
			targetTransform.position += vector;
			targetTransform.position += vector2;
			targetTransform.position += vector3;
			rotationOriginPoint += vector;
			rotationOriginPoint += vector2;
			rotationOriginPoint += vector3;
		}

		protected void UpdateScrollMovement()
		{
			float num = Input.mouseScrollDelta.y;
			Vector3 normalized = playerCam.forward.normalized;
			if (GameInput.GetButton(GameInput.ButtonCode.TertiaryClick) || GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
			{
				distance += num * scrollMovementSpeed * Time.deltaTime;
			}
			else
			{
				targetTransform.position += normalized * num * scrollMovementSpeed * Time.deltaTime;
			}
		}

		protected void UpdateRotation()
		{
			if (GameInput.GetButtonDown(GameInput.ButtonCode.TertiaryClick) || GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
			{
				Plane plane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));
				Ray ray = new Ray(targetTransform.position, targetTransform.forward);
				float enter = 0f;
				plane.Raycast(ray, out enter);
				distance = enter;
				rotationOriginPoint = ray.GetPoint(enter);
			}
			if (GameInput.GetButton(GameInput.ButtonCode.TertiaryClick) || GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
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

		private void FinalizeCameraMovement()
		{
			playerCam.position = Vector3.Lerp(playerCam.position, targetTransform.position, Time.deltaTime * targetFollowSpeed);
			playerCam.rotation = Quaternion.Lerp(playerCam.rotation, targetTransform.rotation, Time.deltaTime * targetFollowSpeed);
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

		private void CancelOriginSlide()
		{
			if (originSlideRoutine != null)
			{
				StopCoroutine(originSlideRoutine);
				originSlideRoutine = null;
			}
		}

		public void SlideCameraOrigin(Vector3 position, float offsetDistance, float time = 0f)
		{
			if (originSlideRoutine != null)
			{
				StopCoroutine(originSlideRoutine);
			}
			Plane plane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));
			Ray ray = new Ray(targetTransform.position, targetTransform.forward);
			float enter = 0f;
			plane.Raycast(ray, out enter);
			Vector3 point = ray.GetPoint(enter);
			position += (targetTransform.position - point).normalized * offsetDistance;
			originSlideRoutine = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				Vector3 startPosition = targetTransform.transform.position;
				for (float i = 0f; i < time; i += Time.deltaTime)
				{
					targetTransform.position = Vector3.Lerp(startPosition, position, i / time);
					yield return new WaitForEndOfFrame();
				}
				targetTransform.position = position;
				originSlideRoutine = null;
			}
		}
	}
}
