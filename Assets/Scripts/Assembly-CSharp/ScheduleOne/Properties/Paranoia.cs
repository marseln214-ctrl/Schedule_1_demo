using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "Paranoia", menuName = "Properties/Paranoia Property")]
	public class Paranoia : Property
	{
		public override void ApplyToNPC(NPC npc)
		{
			npc.Avatar.EmotionManager.AddEmotionOverride("Concerned", "paranoia");
		}

		public override void ApplyToPlayer(Player player)
		{
			player.Paranoid = true;
			player.Avatar.EmotionManager.AddEmotionOverride("Concerned", "paranoia");
		}

		public override void ClearFromNPC(NPC npc)
		{
			npc.Avatar.EmotionManager.RemoveEmotionOverride("paranoia");
		}

		public override void ClearFromPlayer(Player player)
		{
			player.Paranoid = false;
			player.Avatar.EmotionManager.RemoveEmotionOverride("paranoia");
		}
	}
}
