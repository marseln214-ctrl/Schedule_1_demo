using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.Doors
{
	public class Peephole : MonoBehaviour
	{
		public Animation DoorAnim;

		public AudioSourceController OpenSound;

		public AudioSourceController CloseSound;

		public void Open()
		{
			DoorAnim.Play("Peephole open");
			OpenSound.Play();
		}

		public void Close()
		{
			DoorAnim.Play("Peephole close");
			CloseSound.Play();
		}
	}
}
