using System;
using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class AnimationCurveExt
	{
		public static bool ValueIsOne(this AnimationCurve curve)
		{
			bool result = true;
			int num = curve.keys.Length;
			for (int i = 0; i < num; i++)
			{
				Keyframe keyframe = curve.keys[i];
				if (keyframe.inTangent != 0f || keyframe.outTangent != 0f || Math.Abs(keyframe.value - 1f) > 1.1E-44f)
				{
					result = false;
					break;
				}
			}
			return result;
		}
	}
}
