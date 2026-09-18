using ScheduleOne.AvatarFramework.Equipping;
using UnityEngine;

namespace ScheduleOne.NPCs.Other
{
	public class HoldItem : MonoBehaviour
	{
		public NPC Npc;

		public AvatarEquippable Equippable;

		public bool LookAtItem;

		public bool active { get; protected set; }

		public void Begin()
		{
			active = true;
			Npc.SetEquippable_Return(Equippable.AssetPath);
		}

		private void Update()
		{
			if (active && LookAtItem && Npc.Avatar.CurrentEquippable != null)
			{
				Npc.Avatar.LookController.OverrideLookTarget(Npc.Avatar.CurrentEquippable.transform.position, 0);
			}
		}

		public void End()
		{
			active = false;
			Npc.SetEquippable_Return(string.Empty);
		}
	}
}
