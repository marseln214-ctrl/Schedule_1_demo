using System;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Relation
{
	public class NPCUnlockTracker : MonoBehaviour
	{
		public NPC Npc;

		public UnityEvent onUnlocked;

		private void Awake()
		{
			if (Npc.RelationData.Unlocked)
			{
				Invoke(Npc.RelationData.UnlockType, t: false);
			}
			NPCRelationData relationData = Npc.RelationData;
			relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(Invoke));
		}

		private void Invoke(NPCRelationData.EUnlockType type, bool t)
		{
			if (onUnlocked != null)
			{
				onUnlocked.Invoke();
			}
		}
	}
}
