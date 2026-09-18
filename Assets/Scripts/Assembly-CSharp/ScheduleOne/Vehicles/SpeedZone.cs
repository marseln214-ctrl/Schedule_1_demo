using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Vehicles
{
	[RequireComponent(typeof(BoxCollider))]
	public class SpeedZone : MonoBehaviour
	{
		public static List<SpeedZone> speedZones = new List<SpeedZone>();

		public BoxCollider col;

		public float speed = 20f;

		public virtual void Awake()
		{
			speedZones.Add(this);
		}

		public static List<SpeedZone> GetSpeedZones(Vector3 point)
		{
			List<SpeedZone> list = new List<SpeedZone>();
			for (int i = 0; i < speedZones.Count; i++)
			{
				if (speedZones[i].col.bounds.Contains(point))
				{
					list.Add(speedZones[i]);
				}
			}
			return list;
		}

		private void OnDrawGizmos()
		{
		}

		private void OnDrawGizmosSelected()
		{
		}
	}
}
