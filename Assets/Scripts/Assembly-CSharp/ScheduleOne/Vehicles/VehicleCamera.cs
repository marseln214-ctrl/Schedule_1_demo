using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles
{
	public class VehicleCamera : MonoBehaviour
	{
		private const float followDelta = 7.5f;

		private const float yMinLimit = -20f;

		private const float manualOverrideTime = 0.01f;

		private const float manualOverrideReturnTime = 0.6f;

		private const float xSpeed = 60f;

		private const float ySpeed = 40f;

		private const float yMaxLimit = 89f;

		[Header("References")]
		public LandVehicle vehicle;

		[Header("Camera Settings")]
		[SerializeField]
		protected Transform cameraOrigin;

		[SerializeField]
		protected float lateralOffset = 4f;

		[SerializeField]
		protected float verticalOffset = 1.5f;

		protected bool cameraReversed;

		protected float timeSinceCameraManuallyAdjusted = float.MaxValue;

		protected float orbitDistance;

		protected Vector3 lastFrameCameraOffset = Vector3.zero;

		protected Vector3 lastManualOffset = Vector3.zero;

		private Transform targetTransform;

		private Transform cameraDolly;

		private float x;

		private float y;

		private Transform cam => PlayerSingleton<PlayerCamera>.Instance.transform;

		protected virtual void Start()
		{
			targetTransform = new GameObject("VehicleCameraTargetTransform").transform;
			targetTransform.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
			cameraDolly = new GameObject("VehicleCameraDolly").transform;
			cameraDolly.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
			if (Player.Local != null)
			{
				Subscribe();
			}
			else
			{
				Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Subscribe));
			}
		}

		private void Subscribe()
		{
			Player local = Player.Local;
			local.onEnterVehicle = (Player.VehicleEvent)Delegate.Combine(local.onEnterVehicle, new Player.VehicleEvent(PlayerEnteredVehicle));
		}

		protected virtual void Update()
		{
			timeSinceCameraManuallyAdjusted += Time.deltaTime;
			CheckForClick();
		}

		private void PlayerEnteredVehicle(LandVehicle veh)
		{
			if (!(veh != vehicle))
			{
				timeSinceCameraManuallyAdjusted = 100f;
				LateUpdate();
				PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(targetTransform.position, targetTransform.rotation, 0f);
			}
		}

		private void CheckForClick()
		{
			if (vehicle.localPlayerIsInVehicle && GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
			{
				if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick) && timeSinceCameraManuallyAdjusted > 0.01f)
				{
					Vector3 eulerAngles = cam.rotation.eulerAngles;
					x = eulerAngles.y;
					y = eulerAngles.x;
					orbitDistance = Mathf.Sqrt(Mathf.Pow(lateralOffset, 2f) + Mathf.Pow(verticalOffset, 2f));
				}
				timeSinceCameraManuallyAdjusted = 0f;
			}
		}

		protected virtual void LateUpdate()
		{
			if (!vehicle.localPlayerIsInVehicle)
			{
				return;
			}
			if (vehicle.speed_Kmh > 2f)
			{
				cameraReversed = false;
			}
			else if (vehicle.speed_Kmh < -15f)
			{
				cameraReversed = true;
			}
			targetTransform.position = LimitCameraPosition(GetTargetCameraPosition());
			targetTransform.LookAt(cameraOrigin);
			cameraDolly.position = Vector3.Lerp(cameraDolly.position, targetTransform.position, Time.deltaTime * 7.5f);
			cameraDolly.rotation = Quaternion.Lerp(cameraDolly.rotation, targetTransform.rotation, Time.deltaTime * 7.5f);
			orbitDistance = Mathf.Clamp(Vector3.Distance(cameraOrigin.position, cameraDolly.position), Mathf.Sqrt(Mathf.Pow(lateralOffset, 2f) + Mathf.Pow(verticalOffset, 2f)), 100f);
			if (timeSinceCameraManuallyAdjusted <= 0.01f)
			{
				if (GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
				{
					x += GameInput.MouseDelta.x * 60f * 0.02f * Singleton<Settings>.Instance.LookSensitivity;
					y -= GameInput.MouseDelta.y * 40f * 0.02f * Singleton<Settings>.Instance.LookSensitivity;
					y = ClampAngle(y, -20f, 89f);
					Quaternion quaternion = Quaternion.Euler(y, x, 0f);
					Vector3 targetPosition = quaternion * new Vector3(0f, 0f, 0f - orbitDistance) + cameraOrigin.position;
					cam.rotation = quaternion;
					cam.position = LimitCameraPosition(targetPosition);
				}
				else
				{
					Vector3 normalized = (cameraOrigin.TransformPoint(lastFrameCameraOffset) - cameraOrigin.position).normalized;
					Vector3 targetPosition2 = cameraOrigin.position + normalized * orbitDistance;
					cam.position = LimitCameraPosition(targetPosition2);
					cam.LookAt(cameraOrigin);
					Vector3 eulerAngles = cam.rotation.eulerAngles;
					x = eulerAngles.y;
					y = eulerAngles.x;
				}
				lastManualOffset = cameraOrigin.InverseTransformPoint(cam.position);
			}
			else if (timeSinceCameraManuallyAdjusted < 0.61f)
			{
				targetTransform.position = Vector3.Lerp(cameraOrigin.TransformPoint(lastManualOffset), targetTransform.position, (timeSinceCameraManuallyAdjusted - 0.01f) / 0.6f);
				targetTransform.LookAt(cameraOrigin);
				cam.position = Vector3.Lerp(cam.position, targetTransform.position, Time.deltaTime * 7.5f);
				cam.rotation = Quaternion.Lerp(cam.rotation, targetTransform.rotation, Time.deltaTime * 7.5f);
			}
			else
			{
				cam.position = Vector3.Lerp(cam.position, targetTransform.position, Time.deltaTime * 7.5f);
				cam.rotation = Quaternion.Lerp(cam.rotation, targetTransform.rotation, Time.deltaTime * 7.5f);
			}
			lastFrameCameraOffset = cameraOrigin.InverseTransformPoint(cam.position);
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

		private Vector3 GetTargetCameraPosition()
		{
			Vector3 vector = -base.transform.forward;
			vector.y = 0f;
			vector.Normalize();
			if (cameraReversed)
			{
				vector *= -1f;
			}
			return base.transform.position + vector * lateralOffset + Vector3.up * verticalOffset;
		}

		private Vector3 LimitCameraPosition(Vector3 targetPosition)
		{
			Vector3 vector = targetPosition;
			_ = (LayerMask)((int)(LayerMask)((int)default(LayerMask) | (1 << LayerMask.NameToLayer("Default"))) | (1 << LayerMask.NameToLayer("Terrain")));
			float num = 0.45f;
			Vector3 vector2 = Vector3.Normalize(vector - cameraOrigin.position);
			if (Physics.Raycast(cameraOrigin.position, vector2, out var hitInfo, Vector3.Distance(base.transform.position, vector) + num, 1 << LayerMask.NameToLayer("Default")))
			{
				vector = hitInfo.point - vector2 * num;
			}
			return vector;
		}
	}
}
