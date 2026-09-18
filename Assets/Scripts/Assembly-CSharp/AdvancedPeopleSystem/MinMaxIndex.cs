using System;
using UnityEngine;

namespace AdvancedPeopleSystem
{
	[Serializable]
	public class MinMaxIndex
	{
		public int Min;

		public int Max;

		public int GetRandom(int max)
		{
			return Mathf.Clamp(UnityEngine.Random.Range(Min, Max), -1, max);
		}
	}
}
