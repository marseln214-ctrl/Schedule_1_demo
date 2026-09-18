using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules
{
	public class ConversationLocation : MonoBehaviour
	{
		public Transform[] StandPoints;

		[HideInInspector]
		public List<NPC> NPCs = new List<NPC>();

		private Dictionary<NPC, bool> npcReady = new Dictionary<NPC, bool>();

		public bool NPCsReady => npcReady.Where((KeyValuePair<NPC, bool> npcReady) => npcReady.Value).Count() >= 2;

		public void Awake()
		{
			if (StandPoints.Length < NPCs.Count)
			{
				Console.LogError("ConversationLocation has less StandPoints than NPCs");
			}
			foreach (NPC nPC in NPCs)
			{
				npcReady.Add(nPC, value: false);
			}
		}

		public Transform GetStandPoint(NPC npc)
		{
			if (!NPCs.Contains(npc))
			{
				Console.LogWarning("NPC is not part of this conversation");
				return StandPoints[0];
			}
			return StandPoints[NPCs.IndexOf(npc)];
		}

		public void SetNPCReady(NPC npc, bool ready)
		{
			if (!NPCs.Contains(npc))
			{
				Console.LogWarning("NPC is not part of this conversation");
			}
			else
			{
				npcReady[npc] = ready;
			}
		}

		public NPC GetOtherNPC(NPC npc)
		{
			if (!NPCs.Contains(npc))
			{
				Console.LogWarning("NPC is not part of this conversation");
				return null;
			}
			return NPCs.Where((NPC otherNPC) => otherNPC != npc).FirstOrDefault();
		}
	}
}
