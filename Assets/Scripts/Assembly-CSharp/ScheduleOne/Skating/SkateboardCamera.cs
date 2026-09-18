using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Skating
{
	[RequireComponent(typeof(Skateboard))]
	public class SkateboardCamera : NetworkBehaviour
	{
		private const float followDelta = 7.5f;

		private const float yMinLimit = -20f;

		private const float manualOverrideTime = 0.01f;

		private const float manualOverrideReturnTime = 0.6f;

		private const float xSpeed = 60f;

		private const float ySpeed = 40f;

		private const float yMaxLimit = 89f;

		[Header("References")]
		public Transform cameraOrigin;

		[Header("Settings")]
		public float CameraFollowSpeed = 10f;

		public float HorizontalOffset = -2.5f;

		public float VerticalOffset = 2f;

		public float CameraDownAngle = 18f;

		[Header("Settings")]
		public float FOVMultiplier_MinSpeed = 1f;

		public float FOVMultiplier_MaxSpeed = 1.3f;

		public float FOVMultiplierChangeRate = 3f;

		private Skateboard board;

		private float currentFovMultiplier = 1f;

		private bool cameraReversed;

		private bool cameraAdjusted;

		private float timeSinceCameraManuallyAdjusted = float.MaxValue;

		private float orbitDistance;

		private Vector3 lastFrameCameraOffset = Vector3.zero;

		private Vector3 lastManualOffset = Vector3.zero;

		private Transform targetTransform;

		private Transform cameraDolly;

		private float x;

		private float y;

		private bool NetworkInitialize___EarlyScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted;

		private Transform cam => PlayerSingleton<PlayerCamera>.Instance.transform;

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ESkating_002ESkateboardCamera_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void OnStartClient()
		{
			base.OnStartClient();
			if (!board.IsOwner)
			{
				base.enabled = false;
			}
		}

		private void OnDestroy()
		{
			Object.Destroy(targetTransform.gameObject);
			Object.Destroy(cameraDolly.gameObject);
		}

		private void Update()
		{
			if (base.IsSpawned)
			{
				timeSinceCameraManuallyAdjusted += Time.deltaTime;
				CheckForClick();
			}
		}

		private void CheckForClick()
		{
			if (GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
			{
				if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick) && timeSinceCameraManuallyAdjusted > 0.01f)
				{
					cameraAdjusted = true;
					Vector3 eulerAngles = cam.rotation.eulerAngles;
					x = eulerAngles.y;
					y = eulerAngles.x;
					orbitDistance = Mathf.Sqrt(Mathf.Pow(HorizontalOffset, 2f) + Mathf.Pow(VerticalOffset, 2f));
				}
				if (cameraAdjusted)
				{
					timeSinceCameraManuallyAdjusted = 0f;
				}
			}
			else
			{
				cameraAdjusted = false;
			}
		}

		private void LateUpdate()
		{
			if (base.IsSpawned && PlayerSingleton<PlayerCamera>.InstanceExists && board.Owner.IsLocalClient)
			{
				UpdateCamera();
				UpdateFOV();
			}
		}

		private void UpdateCamera()
		{
			targetTransform.position = LimitCameraPosition(GetTargetCameraPosition());
			targetTransform.LookAt(cameraOrigin);
			cameraDolly.position = Vector3.Lerp(cameraDolly.position, targetTransform.position, Time.deltaTime * 7.5f);
			cameraDolly.rotation = Quaternion.Lerp(cameraDolly.rotation, targetTransform.rotation, Time.deltaTime * 7.5f);
			orbitDistance = Mathf.Clamp(Vector3.Distance(cameraOrigin.position, cameraDolly.position), Mathf.Sqrt(Mathf.Pow(HorizontalOffset, 2f) + Mathf.Pow(VerticalOffset, 2f)), 100f);
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

		private void UpdateFOV()
		{
			float b = Mathf.Lerp(FOVMultiplier_MinSpeed, FOVMultiplier_MaxSpeed, Mathf.Clamp01(board.Rb.velocity.magnitude / board.TopSpeed_Ms));
			currentFovMultiplier = Mathf.Lerp(currentFovMultiplier, b, Time.deltaTime * FOVMultiplierChangeRate);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(currentFovMultiplier * Singleton<Settings>.Instance.CameraFOV, 0f);
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
			Vector3 vector = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up);
			Vector3 up = Vector3.up;
			return base.transform.position + vector * HorizontalOffset + up * VerticalOffset;
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

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void Awake_UserLogic_ScheduleOne_002ESkating_002ESkateboardCamera_Assembly_002DCSharp_002Edll()
		{
			board = GetComponent<Skateboard>();
			targetTransform = new GameObject("VehicleCameraTargetTransform").transform;
			targetTransform.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
			cameraDolly = new GameObject("VehicleCameraDolly").transform;
			cameraDolly.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
		}
	}
}
