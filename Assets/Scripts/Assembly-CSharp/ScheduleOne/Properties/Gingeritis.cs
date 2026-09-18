using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "Gingeritis", menuName = "Properties/Gingeritis Property")]
	public class Gingeritis : Property
	{
		public static Color32 Color = new Color32(198, 113, 34, byte.MaxValue);

		public override void ApplyToNPC(NPC npc)
		{
			npc.Avatar.Effects.OverrideHairColor(Color);
		}

		public override void ApplyToPlayer(Player player)
		{
			player.Avatar.Effects.OverrideHairColor(Color);
		}

		public override void ClearFromNPC(NPC npc)
		{
			npc.Avatar.Effects.ResetHairColor();
		}

		public override void ClearFromPlayer(Player player)
		{
			player.Avatar.Effects.ResetHairColor();
		}
	}
}
