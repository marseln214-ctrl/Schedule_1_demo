using System;
using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class QuaternionExt
	{
		public static bool SameOrientation(this Quaternion q1, Quaternion q2)
		{
			return Math.Abs((double)Quaternion.Dot(q1, q2)) > 0.999998986721039;
		}

		public static bool DifferentOrientation(this Quaternion q1, Quaternion q2)
		{
			return Math.Abs((double)Quaternion.Dot(q1, q2)) <= 0.999998986721039;
		}
	}
}
