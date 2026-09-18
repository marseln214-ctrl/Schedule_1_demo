using System;
using UnityEngine;

namespace ScheduleOne.Properties.MixMaps
{
	[Serializable]
	public class MixerMapEffect
	{
		public Vector2 Position;

		public float Radius;

		public Property Property;

		public bool IsPointInEffect(Vector2 point)
		{
			return Vector2.Distance(point, Position) < Radius;
		}
	}
}
