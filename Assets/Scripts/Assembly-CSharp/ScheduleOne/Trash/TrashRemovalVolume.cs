using System.Collections.Generic;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Trash
{
	[RequireComponent(typeof(BoxCollider))]
	public class TrashRemovalVolume : MonoBehaviour
	{
		public BoxCollider Collider;

		public float RemovalChance = 1f;

		public void Awake()
		{
			NetworkSingleton<TimeManager>.Instance._onSleepStart.AddListener(SleepStart);
		}

		private void OnDestroy()
		{
			if (NetworkSingleton<TimeManager>.InstanceExists)
			{
				NetworkSingleton<TimeManager>.Instance._onSleepStart.RemoveListener(SleepStart);
			}
		}

		private void SleepStart()
		{
			if (InstanceFinder.IsServer && !(Random.value > RemovalChance))
			{
				TrashItem[] trash = GetTrash();
				for (int i = 0; i < trash.Length; i++)
				{
					trash[i].DestroyTrash();
				}
			}
		}

		private TrashItem[] GetTrash()
		{
			List<TrashItem> list = new List<TrashItem>();
			Vector3 center = Collider.transform.TransformPoint(Collider.center);
			Vector3 halfExtents = Vector3.Scale(Collider.size, Collider.transform.lossyScale) * 0.5f;
			Collider[] array = Physics.OverlapBox(center, halfExtents, Collider.transform.rotation, 1 << LayerMask.NameToLayer("Trash"), QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				TrashItem componentInParent = array[i].GetComponentInParent<TrashItem>();
				if (componentInParent != null)
				{
					list.Add(componentInParent);
				}
			}
			return list.ToArray();
		}
	}
}
