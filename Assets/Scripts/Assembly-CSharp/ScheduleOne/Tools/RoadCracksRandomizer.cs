using System.Collections.Generic;
using EasyButtons;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class RoadCracksRandomizer : MonoBehaviour
	{
		public Transform[] Cracks;

		public int MinCount;

		public int MaxCount = 4;

		[Button]
		private void Randomize()
		{
			List<Transform> list = new List<Transform>(Cracks);
			for (int i = 0; i < list.Count; i++)
			{
				int index = Random.Range(0, list.Count);
				Transform value = list[i];
				list[i] = list[index];
				list[index] = value;
			}
			int num = Random.Range(MinCount, MaxCount + 1);
			for (int j = 0; j < list.Count; j++)
			{
				list[j].gameObject.SetActive(j < num);
			}
		}
	}
}
