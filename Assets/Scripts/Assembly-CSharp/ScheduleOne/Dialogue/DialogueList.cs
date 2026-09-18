using System;
using UnityEngine;

namespace ScheduleOne.Dialogue
{
	[Serializable]
	public class DialogueList
	{
		public string[] Lines;

		public string GetRandomLine()
		{
			if (Lines.Length == 0)
			{
				return string.Empty;
			}
			int num = UnityEngine.Random.Range(0, Lines.Length);
			return Lines[num];
		}
	}
}
