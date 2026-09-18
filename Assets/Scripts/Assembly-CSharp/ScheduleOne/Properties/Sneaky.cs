using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vision;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "Sneaky", menuName = "Properties/Sneaky Property")]
	public class Sneaky : Property
	{
		private VisibilityAttribute visibilityAttribute;

		public override void ApplyToNPC(NPC npc)
		{
		}

		public override void ApplyToPlayer(Player player)
		{
			player.Sneaky = true;
			visibilityAttribute = new VisibilityAttribute("sneaky", 0f, 0.6f);
		}

		public override void ClearFromNPC(NPC npc)
		{
		}

		public override void ClearFromPlayer(Player player)
		{
			player.Sneaky = true;
			visibilityAttribute.Delete();
		}
	}
}
