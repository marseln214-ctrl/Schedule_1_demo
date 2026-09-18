using EasyButtons;
using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.Storage
{
	public class StorageDoorAnimation : MonoBehaviour
	{
		public Transform ItemContainer;

		[Header("Animations")]
		public Animation[] Anims;

		public AnimationClip OpenAnim;

		public AnimationClip CloseAnim;

		public AudioSourceController OpenSound;

		public AudioSourceController CloseSound;

		public bool IsOpen { get; protected set; }

		private void Start()
		{
			if (ItemContainer != null)
			{
				ItemContainer.gameObject.SetActive(value: false);
			}
		}

		[Button]
		public void Open()
		{
			SetIsOpen(open: true);
		}

		[Button]
		public void Close()
		{
			SetIsOpen(open: false);
		}

		public void SetIsOpen(bool open)
		{
			if (IsOpen == open)
			{
				return;
			}
			if (open && ItemContainer != null)
			{
				ItemContainer.gameObject.SetActive(value: true);
			}
			IsOpen = open;
			for (int i = 0; i < Anims.Length; i++)
			{
				Anims[i].Play(IsOpen ? OpenAnim.name : CloseAnim.name);
			}
			if (IsOpen)
			{
				if (OpenSound != null)
				{
					OpenSound.Play();
				}
			}
			else if (CloseSound != null)
			{
				CloseSound.Play();
			}
			if (!open)
			{
				Invoke("DisableItems", CloseAnim.length);
			}
		}

		private void DisableItems()
		{
			if (!IsOpen && ItemContainer != null)
			{
				ItemContainer.gameObject.SetActive(value: false);
			}
		}
	}
}
