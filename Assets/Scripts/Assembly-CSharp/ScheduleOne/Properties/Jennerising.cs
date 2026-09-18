using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "Jennerising", menuName = "Properties/Jennerising Property")]
	public class Jennerising : Property
	{
		public override void ApplyToNPC(NPC npc)
		{
			npc.Avatar.Effects.SetGenderInverted(inverted: true);
		}

		public override void ApplyToPlayer(Player player)
		{
			player.Avatar.Effects.SetGenderInverted(inverted: true);
		}

		public override void ClearFromNPC(NPC npc)
		{
			npc.Avatar.Effects.SetGenderInverted(inverted: false);
		}

		public override void ClearFromPlayer(Player player)
		{
			player.Avatar.Effects.SetGenderInverted(inverted: false);
		}
	}
}
