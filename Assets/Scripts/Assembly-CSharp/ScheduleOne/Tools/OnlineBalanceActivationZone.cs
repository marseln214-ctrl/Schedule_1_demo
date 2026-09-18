using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class OnlineBalanceActivationZone : MonoBehaviour
	{
		public const float ActivationDistance = 20f;

		private List<Collider> exclude = new List<Collider>();

		private Collider collider;

		private void Awake()
		{
			collider = GetComponent<Collider>();
			InvokeRepeating("UpdateCollider", 0f, 1f);
		}

		private void UpdateCollider()
		{
			Player.GetClosestPlayer(base.transform.position, out var distance);
			collider.enabled = distance < 20f;
		}

		private void OnTriggerStay(Collider other)
		{
			if (!exclude.Contains(other))
			{
				Player componentInParent = other.GetComponentInParent<Player>();
				if (componentInParent != null && componentInParent.IsOwner)
				{
					Singleton<HUD>.Instance.OnlineBalanceDisplay.Show();
				}
				else
				{
					exclude.Add(other);
				}
			}
		}
	}
}
