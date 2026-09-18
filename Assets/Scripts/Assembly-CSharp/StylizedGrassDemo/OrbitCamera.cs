using ScheduleOne;
using UnityEngine;

namespace StylizedGrassDemo
{
	public class OrbitCamera : MonoBehaviour
	{
		[Space]
		public Transform pivot;

		[Space]
		public bool enableMouse = true;

		public float idleRotationSpeed = 0.05f;

		public float lookSmoothSpeed = 5f;

		public float moveSmoothSpeed = 5f;

		public float scrollSmoothSpeed = 5f;

		private Transform cam;

		private float cameraRotSide;

		private float cameraRotUp;

		private float cameraRotSideCur;

		private float cameraRotUpCur;

		private float distance;

		private void Start()
		{
			cam = Camera.main.transform;
			cameraRotSide = base.transform.eulerAngles.y;
			cameraRotSideCur = base.transform.eulerAngles.y;
			cameraRotUp = base.transform.eulerAngles.x;
			cameraRotUpCur = base.transform.eulerAngles.x;
			distance = 0f - cam.localPosition.z;
		}

		private void LateUpdate()
		{
			Cursor.visible = false;
			if ((bool)pivot)
			{
				if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && enableMouse)
				{
					cameraRotSide += GameInput.MouseDelta.x * 5f;
					cameraRotUp -= GameInput.MouseDelta.y * 5f;
				}
				else
				{
					cameraRotSide += idleRotationSpeed;
				}
				cameraRotSideCur = Mathf.LerpAngle(cameraRotSideCur, cameraRotSide, Time.deltaTime * lookSmoothSpeed);
				cameraRotUpCur = Mathf.Lerp(cameraRotUpCur, cameraRotUp, Time.deltaTime * lookSmoothSpeed);
				if (GameInput.GetButton(GameInput.ButtonCode.SecondaryClick) && enableMouse)
				{
					distance *= 1f - 0.1f * GameInput.MouseDelta.y;
				}
				if (enableMouse)
				{
					distance *= 1f - 1f * GameInput.MouseScrollDelta;
				}
				Vector3 position = pivot.position;
				base.transform.position = Vector3.Lerp(base.transform.position, position, Time.deltaTime * moveSmoothSpeed);
				base.transform.rotation = Quaternion.Euler(cameraRotUpCur, cameraRotSideCur, 0f);
				float num = Mathf.Lerp(0f - cam.transform.localPosition.z, distance, Time.deltaTime * scrollSmoothSpeed);
				cam.localPosition = -Vector3.forward * num;
			}
		}
	}
}
