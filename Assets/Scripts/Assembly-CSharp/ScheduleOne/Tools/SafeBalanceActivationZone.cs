using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class SafeBalanceActivationZone : MonoBehaviour
	{
		public const float ActivationDistance = 30f;

		public Safe Safe;

		private List<Collider> exclude = new List<Collider>();

		private Collider[] colliders;

		private bool active;

		private void Awake()
		{
			colliders = GetComponentsInChildren<Collider>();
			InvokeRepeating("UpdateCollider", 0f, 1f);
			InvokeRepeating("Activate", 0f, 0.25f);
		}

		private void UpdateCollider()
		{
			Player.GetClosestPlayer(base.transform.position, out var distance);
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = distance < 30f;
			}
		}

		private void Activate()
		{
			active = true;
		}

		private void OnTriggerStay(Collider other)
		{
			if (!active)
			{
				return;
			}
			active = true;
			if (!exclude.Contains(other))
			{
				Player componentInParent = other.GetComponentInParent<Player>();
				if (componentInParent != null && componentInParent.IsOwner)
				{
					Singleton<HUD>.Instance.SafeBalanceDisplay.SetBalance(Safe.GetCash());
					Singleton<HUD>.Instance.SafeBalanceDisplay.Show();
				}
				else
				{
					exclude.Add(other);
				}
			}
		}
	}
}
