using System;
using UnityEngine;

namespace ScheduleOne.Dialogue
{
	[Serializable]
	public struct Entry
	{
		public string Key;

		public DialogueChain[] Chains;

		public DialogueChain GetRandomChain()
		{
			if (Chains.Length == 0)
			{
				return null;
			}
			int num = UnityEngine.Random.Range(0, Chains.Length);
			return Chains[num];
		}

		public string GetRandomLine()
		{
			return GetRandomChain().Lines[0];
		}
	}
}
