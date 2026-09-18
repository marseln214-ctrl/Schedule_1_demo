using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedPeopleSystem
{
	[Serializable]
	public class MinMaxColor
	{
		public List<Color> minColors = new List<Color>();

		public List<Color> maxColors = new List<Color>();

		public Color GetRandom()
		{
			int index = UnityEngine.Random.Range(0, minColors.Count);
			return Color.Lerp(minColors[index], maxColors[index], UnityEngine.Random.Range(0f, 1f));
		}
	}
}
