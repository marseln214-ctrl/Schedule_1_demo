using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "Laxative", menuName = "Properties/Laxative Property")]
	public class Laxative : Property
	{
		public override void ApplyToNPC(NPC npc)
		{
			npc.Avatar.Effects.EnableLaxative();
		}

		public override void ApplyToPlayer(Player player)
		{
			player.Avatar.Effects.EnableLaxative();
		}

		public override void ClearFromNPC(NPC npc)
		{
			npc.Avatar.Effects.DisableLaxative();
		}

		public override void ClearFromPlayer(Player player)
		{
			player.Avatar.Effects.DisableLaxative();
		}
	}
}
