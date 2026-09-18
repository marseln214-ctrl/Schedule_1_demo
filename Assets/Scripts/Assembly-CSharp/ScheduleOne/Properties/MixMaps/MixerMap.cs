using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Properties.MixMaps
{
	[Serializable]
	public class MixerMap : ScriptableObject
	{
		public float MapRadius;

		public List<MixerMapEffect> Effects;

		public MixerMapEffect GetEffectAtPoint(Vector2 point)
		{
			if (point.magnitude > MapRadius)
			{
				return null;
			}
			for (int i = 0; i < Effects.Count; i++)
			{
				if (Effects[i].IsPointInEffect(point))
				{
					return Effects[i];
				}
			}
			return null;
		}

		public MixerMapEffect GetEffect(Property property)
		{
			for (int i = 0; i < Effects.Count; i++)
			{
				if (Effects[i].Property == property)
				{
					return Effects[i];
				}
			}
			return null;
		}
	}
}
