using ScheduleOne.NPCs;
using UnityEngine.UI;

namespace ScheduleOne.Map
{
	public class NPCPoI : POI
	{
		public NPC NPC { get; private set; }

		public override void InitializeUI()
		{
			base.InitializeUI();
			if (base.IconContainer != null && NPC != null)
			{
				base.IconContainer.Find("Outline/Icon").GetComponent<Image>().sprite = NPC.MugshotSprite;
			}
		}

		public void SetNPC(NPC npc)
		{
			NPC = npc;
			if (base.IconContainer != null && NPC != null)
			{
				base.IconContainer.Find("Outline/Icon").GetComponent<Image>().sprite = NPC.MugshotSprite;
			}
		}
	}
}
