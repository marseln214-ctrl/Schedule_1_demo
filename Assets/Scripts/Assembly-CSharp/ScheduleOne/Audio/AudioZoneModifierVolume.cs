using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Audio
{
	public class AudioZoneModifierVolume : MonoBehaviour
	{
		public List<AudioZone> Zones = new List<AudioZone>();

		public float VolumeMultiplier = 0.5f;

		private BoxCollider[] colliders;

		private void Start()
		{
			InvokeRepeating("Refresh", 0f, 0.25f);
			colliders = GetComponentsInChildren<BoxCollider>();
			LayerUtility.SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Invisible"));
		}

		private void Refresh()
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				return;
			}
			BoxCollider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].bounds.Contains(PlayerSingleton<PlayerCamera>.Instance.transform.position))
				{
					continue;
				}
				{
					foreach (AudioZone zone in Zones)
					{
						zone.AddModifier(this, VolumeMultiplier);
					}
					return;
				}
			}
			foreach (AudioZone zone2 in Zones)
			{
				zone2.RemoveModifier(this);
			}
		}
	}
}
