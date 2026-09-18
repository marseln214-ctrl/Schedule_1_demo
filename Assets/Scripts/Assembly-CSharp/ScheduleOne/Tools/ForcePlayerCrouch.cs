using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class ForcePlayerCrouch : MonoBehaviour
	{
		private void OnTriggerStay(Collider other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
			{
				Player componentInParent = other.gameObject.GetComponentInParent<Player>();
				if (componentInParent != null && componentInParent.IsOwner && !PlayerSingleton<PlayerMovement>.Instance.isCrouched)
				{
					PlayerSingleton<PlayerMovement>.Instance.SetCrouched(c: true);
				}
			}
		}
	}
}
