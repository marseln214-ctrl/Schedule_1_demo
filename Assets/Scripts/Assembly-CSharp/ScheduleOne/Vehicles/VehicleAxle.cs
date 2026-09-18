using UnityEngine;

namespace ScheduleOne.Vehicles
{
	public class VehicleAxle : MonoBehaviour
	{
		[Header("References")]
		[SerializeField]
		protected Wheel wheel;

		private Transform model;

		protected virtual void Awake()
		{
			model = base.transform.Find("Model");
		}

		protected virtual void LateUpdate()
		{
			Vector3 position = base.transform.position;
			Vector3 position2 = wheel.axleConnectionPoint.position;
			model.transform.position = (position + position2) / 2f;
			base.transform.LookAt(position2);
			model.transform.localScale = new Vector3(model.transform.localScale.x, 0.5f * Vector3.Distance(position, position2), model.transform.localScale.z);
		}
	}
}
