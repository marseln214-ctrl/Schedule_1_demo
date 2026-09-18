using UnityEngine;

namespace ScheduleOne.Vehicles
{
	public class LoanSharkCarVisuals : MonoBehaviour
	{
		public GameObject Note;

		public GameObject BulletHoleDecals;

		private void Awake()
		{
			Note.gameObject.SetActive(value: false);
			BulletHoleDecals.gameObject.SetActive(value: false);
		}

		public void Configure(bool enabled, bool noteVisible)
		{
			Note.SetActive(noteVisible);
			BulletHoleDecals.SetActive(enabled);
		}
	}
}
