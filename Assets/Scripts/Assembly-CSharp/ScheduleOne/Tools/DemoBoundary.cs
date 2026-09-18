using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class DemoBoundary : MonoBehaviour
	{
		public Collider Collider;

		private void OnValidate()
		{
			if (Collider == null)
			{
				Collider = GetComponent<Collider>();
			}
		}

		private void Start()
		{
			InvokeRepeating("UpdateBoundary", 0f, 0.25f);
		}

		private void UpdateBoundary()
		{
			if (!(Player.Local == null))
			{
				Vector3 vector = Collider.transform.InverseTransformPoint(Player.Local.transform.position);
				Collider.enabled = vector.x > 0f;
			}
		}
	}
}
