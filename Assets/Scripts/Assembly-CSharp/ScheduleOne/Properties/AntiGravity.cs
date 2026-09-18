using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "AntiGravity", menuName = "Properties/AntiGravity Property")]
	public class AntiGravity : Property
	{
		public const float GravityMultiplier = 0.3f;

		public override void ApplyToNPC(NPC npc)
		{
			npc.Movement.SetGravityMultiplier(0.3f);
			npc.Avatar.Effects.SetAntiGrav(active: true);
		}

		public override void ApplyToPlayer(Player player)
		{
			player.SetGravityMultiplier(0.3f);
			player.Avatar.Effects.SetAntiGrav(active: true);
		}

		public override void ClearFromNPC(NPC npc)
		{
			npc.Movement.SetGravityMultiplier(1f);
			npc.Avatar.Effects.SetAntiGrav(active: false);
		}

		public override void ClearFromPlayer(Player player)
		{
			player.SetGravityMultiplier(1f);
			player.Avatar.Effects.SetAntiGrav(active: false);
		}
	}
}
