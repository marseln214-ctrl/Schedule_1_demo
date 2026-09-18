using UnityEngine;

namespace ScheduleOne.AvatarFramework
{
	public class Hair : Accessory
	{
		[SerializeField]
		private GameObject[] hairToHide;

		public bool BlockedByHat { get; protected set; }

		public void SetBlockedByHat(bool blocked)
		{
			BlockedByHat = blocked;
			if (blocked)
			{
				BlockHair();
			}
			else
			{
				UnBlockHair();
			}
		}

		protected virtual void BlockHair()
		{
			GameObject[] array = hairToHide;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
		}

		protected virtual void UnBlockHair()
		{
			GameObject[] array = hairToHide;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
	}
}
