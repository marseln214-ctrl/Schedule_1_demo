using UnityEngine;

namespace ScheduleOne.Building.Doors
{
	public class DoorKnocker : MonoBehaviour
	{
		[Header("References")]
		public Animation Anim;

		public string KnockingSoundClipName;

		public AudioSource KnockingSound;

		public void Knock()
		{
			if (Anim.isPlaying)
			{
				Anim.Stop();
			}
			Anim.Play(KnockingSoundClipName);
		}

		public void PlayKnockingSound()
		{
			KnockingSound.Play();
		}
	}
}
