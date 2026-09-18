using ScheduleOne.AvatarFramework.Animation;
using UnityEngine;

namespace ScheduleOne.NPCs.Other
{
	public class SmokeCigarette : MonoBehaviour
	{
		public NPC Npc;

		public GameObject CigarettePrefab;

		public AvatarAnimation Anim;

		private GameObject cigarette;

		private void Awake()
		{
			if (Npc == null)
			{
				Npc = GetComponentInParent<NPC>();
			}
		}

		public void Begin()
		{
			Anim.SetBool("Smoking", value: true);
			cigarette = Object.Instantiate(CigarettePrefab, Anim.RightHandContainer);
			Npc.Avatar.LookController.OverrideIKWeight(0.3f);
		}

		public void End()
		{
			Anim.SetBool("Smoking", value: false);
			if (cigarette != null)
			{
				Object.Destroy(cigarette.gameObject);
				cigarette = null;
			}
			Npc.Avatar.LookController.OverrideIKWeight(0.2f);
		}
	}
}
