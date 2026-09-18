using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class FoliageRustleSound : MonoBehaviour
	{
		public const float ACTIVATION_RANGE_SQUARED = 900f;

		public const float COOLDOWN = 1f;

		public AudioSourceController Sound;

		public GameObject Container;

		private float timeOnLastHit;

		private void Awake()
		{
			InvokeRepeating("UpdateActive", Random.Range(0f, 3f), 3f);
			Container.SetActive(value: false);
		}

		public void OnTriggerEnter(Collider other)
		{
			if (!(Time.timeSinceLevelLoad - timeOnLastHit > 1f))
			{
				return;
			}
			Player componentInParent = other.gameObject.GetComponentInParent<Player>();
			if (componentInParent != null)
			{
				if (componentInParent.IsOwner)
				{
					Sound.VolumeMultiplier = Mathf.Clamp01(PlayerSingleton<PlayerMovement>.Instance.Controller.velocity.magnitude / (PlayerMovement.WalkSpeed * PlayerMovement.SprintMultiplier));
				}
				else
				{
					Sound.VolumeMultiplier = 1f;
				}
				Sound.Play();
				timeOnLastHit = Time.timeSinceLevelLoad;
			}
		}

		private void UpdateActive()
		{
			if (!(Player.Local == null))
			{
				float num = Vector3.SqrMagnitude(Player.Local.Avatar.CenterPoint - base.transform.position);
				Container.SetActive(num < 900f);
			}
		}
	}
}
