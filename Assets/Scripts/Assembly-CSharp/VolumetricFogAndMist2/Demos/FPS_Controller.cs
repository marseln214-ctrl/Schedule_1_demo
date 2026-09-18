using UnityEngine;

namespace VolumetricFogAndMist2.Demos
{
	public class FPS_Controller : MonoBehaviour
	{
		private CharacterController characterController;

		private Transform mainCamera;

		private float inputHor;

		private float inputVert;

		private float mouseHor;

		private float mouseVert;

		private float mouseInvertX = 1f;

		private float mouseInvertY = -1f;

		private float camVertAngle;

		private bool isGrounded;

		private Vector3 jumpDirection = Vector3.zero;

		private float sprint = 1f;

		public float sprintMax = 2f;

		public float airControl = 1.5f;

		public float jumpHeight = 10f;

		public float gravity = 20f;

		public float characterHeight = 1.8f;

		public float cameraHeight = 1.7f;

		public float speed = 15f;

		public float rotationSpeed = 2f;

		public float mouseSensitivity = 1f;

		private void Start()
		{
			characterController = base.gameObject.AddComponent<CharacterController>();
			mainCamera = Camera.main.transform;
			characterController.height = characterHeight;
			characterController.center = Vector3.up * characterHeight / 2f;
			mainCamera.position = base.transform.position + Vector3.up * characterHeight;
			mainCamera.rotation = Quaternion.identity;
			mainCamera.parent = base.transform;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		private void Update()
		{
			Vector3 mousePosition = Input.mousePosition;
			if (mousePosition.x < 0f || mousePosition.x >= (float)Screen.width || mousePosition.y < 0f || mousePosition.y >= (float)Screen.height)
			{
				return;
			}
			isGrounded = characterController.isGrounded;
			inputHor = Input.GetAxis("Horizontal");
			inputVert = Input.GetAxis("Vertical");
			mouseHor = Input.GetAxis("Mouse X");
			mouseVert = Input.GetAxis("Mouse Y");
			base.transform.Rotate(0f, mouseHor * rotationSpeed * mouseSensitivity * mouseInvertX, 0f);
			Vector3 vector = base.transform.forward * inputVert + base.transform.right * inputHor;
			vector *= speed;
			if (isGrounded)
			{
				if (Input.GetKey(KeyCode.LeftShift))
				{
					if (sprint < sprintMax)
					{
						sprint += 10f * Time.deltaTime;
					}
				}
				else if (sprint > 1f)
				{
					sprint -= 10f * Time.deltaTime;
				}
				if (Input.GetKeyDown(KeyCode.Space))
				{
					jumpDirection.y = jumpHeight;
				}
				else
				{
					jumpDirection.y = -1f;
				}
			}
			else
			{
				vector *= airControl;
			}
			jumpDirection.y -= gravity * Time.deltaTime;
			characterController.Move(vector * sprint * Time.deltaTime);
			characterController.Move(jumpDirection * Time.deltaTime);
			camVertAngle += mouseVert * rotationSpeed * mouseSensitivity * mouseInvertY;
			camVertAngle = Mathf.Clamp(camVertAngle, -85f, 85f);
			mainCamera.localEulerAngles = new Vector3(camVertAngle, 0f, 0f);
		}
	}
}
