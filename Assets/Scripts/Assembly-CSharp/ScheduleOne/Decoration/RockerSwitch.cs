using UnityEngine;

namespace ScheduleOne.Decoration
{
	public class RockerSwitch : MonoBehaviour
	{
		public MeshRenderer ButtonMesh;

		public Transform ButtonTransform;

		public Light Light;

		public bool isOn;

		private void Awake()
		{
			SetIsOn(isOn);
		}

		public void SetIsOn(bool on)
		{
			isOn = on;
			Light.enabled = on;
			ButtonTransform.localEulerAngles = new Vector3(on ? 10f : (-10f), 0f, 0f);
		}
	}
}
