using ScheduleOne.NPCs;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class AssignedWorkerDisplay : MonoBehaviour
	{
		public Image Icon;

		public void Set(NPC npc)
		{
			if (npc != null)
			{
				Icon.sprite = npc.MugshotSprite;
			}
			base.gameObject.SetActive(npc != null);
		}
	}
}
