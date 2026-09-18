using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "ThoughtProvoking", menuName = "Properties/ThoughtProvoking Property")]
	public class ThoughtProvoking : Property
	{
		public override void ApplyToNPC(NPC npc)
		{
			npc.Avatar.Effects.SetBigHeadActive(active: true);
		}

		public override void ApplyToPlayer(Player player)
		{
			player.Avatar.Effects.SetBigHeadActive(active: true);
		}

		public override void ClearFromNPC(NPC npc)
		{
			npc.Avatar.Effects.SetBigHeadActive(active: false);
		}

		public override void ClearFromPlayer(Player player)
		{
			player.Avatar.Effects.SetBigHeadActive(active: false);
		}
	}
}
