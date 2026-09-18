using System.Collections.Generic;
using EasyButtons;
using ScheduleOne.Property.Utilities.Power;
using UnityEngine;

namespace ScheduleOne.Property.Utilities
{
	public class CosmeticPowerLine : MonoBehaviour
	{
		public Transform startPoint;

		public Transform endPoint;

		public List<Transform> segments = new List<Transform>();

		public float LengthFactor = 1.002f;

		[Button]
		public void Draw()
		{
			PowerLine.DrawPowerLine(startPoint.position, endPoint.position, segments, LengthFactor);
		}
	}
}
