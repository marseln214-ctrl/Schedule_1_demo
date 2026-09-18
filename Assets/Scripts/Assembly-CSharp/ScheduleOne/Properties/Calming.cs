using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "CalmingProperty", menuName = "Properties/Calming Property")]
	public class Calming : Property
	{
		public override void ApplyToNPC(NPC npc)
		{
			npc.Movement.SpeedController.SpeedMultiplier = 0.8f;
		}

		public override void ApplyToPlayer(Player player)
		{
			if (player.IsOwner)
			{
				Singleton<EyelidOverlay>.Instance.OpenMultiplier.AddOverride(0.9f, 6, "calming");
				PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.AddOverride(-4f, Tier, "calming");
			}
		}

		public override void ClearFromNPC(NPC npc)
		{
			npc.Movement.SpeedController.SpeedMultiplier = 1f;
		}

		public override void ClearFromPlayer(Player player)
		{
			if (player.IsOwner)
			{
				Singleton<EyelidOverlay>.Instance.OpenMultiplier.RemoveOverride("calming");
				PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.RemoveOverride("calming");
			}
		}
	}
}
