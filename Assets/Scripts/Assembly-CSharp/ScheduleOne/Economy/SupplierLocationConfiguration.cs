using UnityEngine;

namespace ScheduleOne.Economy
{
	public class SupplierLocationConfiguration : MonoBehaviour
	{
		public string SupplierID;

		public void Activate()
		{
			base.gameObject.SetActive(value: true);
		}

		public void Deactivate()
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
