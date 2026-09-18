using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI
{
	[RequireComponent(typeof(BoxCollider))]
	public class FunnelZone : MonoBehaviour
	{
		public static List<FunnelZone> funnelZones = new List<FunnelZone>();

		public BoxCollider col;

		public Transform entryPoint;

		protected virtual void Awake()
		{
			funnelZones.Add(this);
		}

		public static FunnelZone GetFunnelZone(Vector3 point)
		{
			for (int i = 0; i < funnelZones.Count; i++)
			{
				if (funnelZones[i].col.bounds.Contains(point))
				{
					return funnelZones[i];
				}
			}
			return null;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f);
			Gizmos.DrawCube(base.transform.TransformPoint(col.center), new Vector3(col.size.x, col.size.y, col.size.z));
			Gizmos.DrawLine(base.transform.position, entryPoint.position);
		}
	}
}
