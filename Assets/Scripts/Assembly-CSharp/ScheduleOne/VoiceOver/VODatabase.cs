using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.VoiceOver
{
	[Serializable]
	[CreateAssetMenu(fileName = "VODatabase", menuName = "ScriptableObjects/VODatabase")]
	public class VODatabase : ScriptableObject
	{
		[Range(0f, 2f)]
		public float VolumeMultiplier = 1f;

		public List<VODatabaseEntry> Entries = new List<VODatabaseEntry>();

		public VODatabaseEntry GetEntry(EVOLineType lineType)
		{
			foreach (VODatabaseEntry entry in Entries)
			{
				if (entry.LineType == lineType)
				{
					return entry;
				}
			}
			return null;
		}

		public AudioClip GetRandomClip(EVOLineType lineType)
		{
			return GetEntry(lineType)?.GetRandomClip();
		}
	}
}
