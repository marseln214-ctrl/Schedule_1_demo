using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "Toxic", menuName = "Properties/Toxic Property")]
	public class Toxic : Property
	{
		[ColorUsage(true, true)]
		[SerializeField]
		public Color TintColor = Color.white;

		public override void ApplyToNPC(NPC npc)
		{
			npc.Avatar.Effects.TriggerSick();
			npc.Avatar.EmotionManager.AddEmotionOverride("Concerned", "toxic", 30f, 1);
		}

		public override void ApplyToPlayer(Player player)
		{
			player.Avatar.Effects.TriggerSick();
			player.Avatar.EmotionManager.AddEmotionOverride("Concerned", "toxic", 30f, 1);
			if (player.Owner.IsLocalClient)
			{
				Singleton<PostProcessingManager>.Instance.ColorFilterController.AddOverride(TintColor, Tier, "Toxic");
			}
		}

		public override void ClearFromNPC(NPC npc)
		{
		}

		public override void ClearFromPlayer(Player player)
		{
			if (player.Owner.IsLocalClient)
			{
				Singleton<PostProcessingManager>.Instance.ColorFilterController.RemoveOverride("Toxic");
			}
		}
	}
}
