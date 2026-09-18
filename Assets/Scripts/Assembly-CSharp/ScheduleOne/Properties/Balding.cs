using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "Balding", menuName = "Properties/Balding Property")]
	public class Balding : Property
	{
		public override void ApplyToNPC(NPC npc)
		{
			npc.Avatar.Effects.VanishHair();
		}

		public override void ApplyToPlayer(Player player)
		{
			player.Avatar.Effects.VanishHair();
		}

		public override void ClearFromNPC(NPC npc)
		{
			npc.Avatar.Effects.ReturnHair();
		}

		public override void ClearFromPlayer(Player player)
		{
			player.Avatar.Effects.ReturnHair();
		}
	}
}
