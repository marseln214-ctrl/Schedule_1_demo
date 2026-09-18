using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Dialogue
{
	[Serializable]
	public class VocalReactionDatabase
	{
		[Serializable]
		public class Entry
		{
			public string Key;

			public string[] Reactions;

			public string name => Key;

			public string GetRandomReaction()
			{
				return Reactions[UnityEngine.Random.Range(0, Reactions.Length)];
			}
		}

		public List<Entry> Entries = new List<Entry>();

		public Entry GetEntry(string key)
		{
			foreach (Entry entry in Entries)
			{
				if (entry.Key == key)
				{
					return entry;
				}
			}
			return null;
		}
	}
}
