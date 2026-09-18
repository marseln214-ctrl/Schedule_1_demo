using System;
using EasyButtons;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class OptimizedColliderGroup : MonoBehaviour
	{
		public const int UPDATE_DISTANCE = 5;

		public Collider[] Colliders;

		public float ColliderEnableMaxDistance = 30f;

		private float sqrColliderEnableMaxDistance;

		private bool collidersEnabled = true;

		private void OnEnable()
		{
			sqrColliderEnableMaxDistance = ColliderEnableMaxDistance * ColliderEnableMaxDistance;
			if (PlayerSingleton<PlayerMovement>.InstanceExists)
			{
				RegisterEvent();
			}
			else
			{
				Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(RegisterEvent));
			}
		}

		private void OnDestroy()
		{
			if (PlayerSingleton<PlayerMovement>.InstanceExists)
			{
				PlayerSingleton<PlayerMovement>.Instance.DeregisterMovementEvent(Refresh);
			}
		}

		private void RegisterEvent()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(RegisterEvent));
			PlayerSingleton<PlayerMovement>.Instance.RegisterMovementEvent(5, Refresh);
		}

		[Button]
		public void GetColliders()
		{
			Colliders = GetComponentsInChildren<Collider>();
		}

		public void Start()
		{
		}

		private void Refresh()
		{
			if (!(Player.Local == null) && !(Player.Local.Avatar == null))
			{
				float sqrMagnitude = (Player.Local.Avatar.CenterPoint - base.transform.position).sqrMagnitude;
				SetCollidersEnabled(sqrMagnitude < sqrColliderEnableMaxDistance);
			}
		}

		private void SetCollidersEnabled(bool enabled)
		{
			if (collidersEnabled == enabled)
			{
				return;
			}
			collidersEnabled = enabled;
			Collider[] colliders = Colliders;
			foreach (Collider collider in colliders)
			{
				if (!(collider == null))
				{
					collider.enabled = enabled;
				}
			}
		}
	}
}
