using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "Spicy", menuName = "Properties/Spicy Property")]
	public class Spicy : Property
	{
		[ColorUsage(true, true)]
		[SerializeField]
		public Color TintColor = Color.white;

		public override void ApplyToNPC(NPC npc)
		{
			npc.Avatar.Effects.SetFireActive(active: true);
		}

		public override void ApplyToPlayer(Player player)
		{
			player.Avatar.Effects.SetFireActive(active: true);
			if (player.Owner.IsLocalClient)
			{
				Singleton<PostProcessingManager>.Instance.ColorFilterController.AddOverride(TintColor, Tier, base.name);
			}
		}

		public override void ClearFromNPC(NPC npc)
		{
			npc.Avatar.Effects.SetFireActive(active: false);
		}

		public override void ClearFromPlayer(Player player)
		{
			player.Avatar.Effects.SetFireActive(active: false);
			if (player.Owner.IsLocalClient)
			{
				Singleton<PostProcessingManager>.Instance.ColorFilterController.RemoveOverride(base.name);
			}
		}
	}
}
