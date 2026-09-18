using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E25_CameraLook : MonoBehaviour
	{
		[Range(0f, 10f)]
		[SerializeField]
		private float m_TurnSpeed = 1.5f;

		protected void Update()
		{
			if (!(Time.timeScale < float.Epsilon))
			{
				float axis = Input.GetAxis("Mouse X");
				float num = 0f - Input.GetAxis("Mouse Y");
				base.transform.Rotate(num * m_TurnSpeed, 0f, 0f, Space.Self);
				base.transform.Rotate(0f, axis * m_TurnSpeed, 0f, Space.World);
			}
		}
	}
}
