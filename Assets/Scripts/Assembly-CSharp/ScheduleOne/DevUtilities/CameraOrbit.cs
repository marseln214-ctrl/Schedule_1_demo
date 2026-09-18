using System.Collections.Generic;
using ScheduleOne.AvatarFramework.Animation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.DevUtilities
{
	public class CameraOrbit : MonoBehaviour
	{
		[Header("Required")]
		public Transform target;

		public Transform cam;

		public GraphicRaycaster raycaster;

		public AvatarLookController LookAt;

		[Header("Config")]
		public float targetdistance = 5f;

		public float xSpeed = 120f;

		public float ySpeed = 120f;

		public float sideOffset = 1f;

		public float yMinLimit = -20f;

		public float yMaxLimit = 80f;

		public float distanceMin = 0.5f;

		public float distanceMax = 15f;

		public float ScrollSensativity = 4f;

		private Rigidbody rb;

		private float x;

		private float y;

		private float targetx;

		private float targety;

		private float distance = 5f;

		private bool hoveringUI;

		private void Start()
		{
			Vector3 eulerAngles = base.transform.eulerAngles;
			x = eulerAngles.y;
			y = eulerAngles.x;
			rb = GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.freezeRotation = true;
			}
		}

		private void Update()
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = Input.mousePosition;
			List<RaycastResult> list = new List<RaycastResult>();
			raycaster.Raycast(pointerEventData, list);
			hoveringUI = list.Count > 0;
			LookAt.OverrideLookTarget(cam.transform.position, 100);
		}

		private void LateUpdate()
		{
			if ((bool)target)
			{
				if (Input.GetMouseButton(0) && !hoveringUI)
				{
					targetx += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f * (5f / (distance + 2f));
					targety -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
				}
				targety = ClampAngle(targety, yMinLimit, yMaxLimit);
				x = Mathf.LerpAngle(x, targetx, 0.1f);
				y = Mathf.LerpAngle(y, targety, 1f);
				Quaternion quaternion = Quaternion.Euler(y, x, 0f);
				if (!hoveringUI)
				{
					targetdistance = Mathf.Clamp(targetdistance - Input.GetAxis("Mouse ScrollWheel") * ScrollSensativity, distanceMin, distanceMax);
				}
				distance = Mathf.Lerp(distance, targetdistance, 0.1f);
				if (Physics.Linecast(target.position, base.transform.position, out var hitInfo))
				{
					targetdistance -= hitInfo.distance;
				}
				Vector3 vector = new Vector3(0f, 0f, 0f - distance);
				Vector3 position = quaternion * vector + target.position;
				base.transform.rotation = quaternion;
				base.transform.position = position;
			}
			cam.position = base.transform.position;
			cam.rotation = base.transform.rotation;
			cam.position -= base.transform.right * sideOffset * Vector3.Distance(cam.position, target.position);
			if (Input.GetKey(KeyCode.KeypadPlus))
			{
				GetComponent<Camera>().fieldOfView += 0.3f;
			}
			if (Input.GetKey(KeyCode.KeypadMinus))
			{
				GetComponent<Camera>().fieldOfView -= 0.3f;
			}
		}

		public static float ClampAngle(float angle, float min, float max)
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
	}
}
