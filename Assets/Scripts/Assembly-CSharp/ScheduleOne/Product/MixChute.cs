using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Product
{
	public class MixChute : MonoBehaviour
	{
		[Header("References")]
		public InteractableObject IntObj;

		public Animation DoorAnim;

		private bool isDoorOpen;

		private void Update()
		{
			UpdateDoor();
			IntObj.gameObject.SetActive(!NetworkSingleton<ProductManager>.Instance.IsMixComplete);
		}

		private void UpdateDoor()
		{
			bool flag = false;
			if (NetworkSingleton<ProductManager>.Instance.IsMixComplete && NetworkSingleton<ProductManager>.Instance.CurrentMixOperation != null)
			{
				flag = true;
			}
			else if (Singleton<CreateMixInterface>.Instance.IsOpen)
			{
				flag = true;
			}
			if (flag != isDoorOpen)
			{
				SetDoorOpen(flag);
			}
		}

		public void Hovered()
		{
			if (!NetworkSingleton<ProductManager>.Instance.IsMixComplete)
			{
				if (NetworkSingleton<ProductManager>.Instance.IsMixingInProgress)
				{
					IntObj.SetMessage("Mix will be ready tomorrow");
					IntObj.SetInteractableState(InteractableObject.EInteractableState.Label);
				}
				else
				{
					IntObj.SetMessage("Create new mix");
					IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				}
			}
		}

		public void Interacted()
		{
			if (!NetworkSingleton<ProductManager>.Instance.IsMixComplete && !NetworkSingleton<ProductManager>.Instance.IsMixingInProgress)
			{
				Singleton<CreateMixInterface>.Instance.Open();
			}
		}

		public void SetDoorOpen(bool isOpen)
		{
			isDoorOpen = isOpen;
			DoorAnim.Play(isDoorOpen ? "Cabin flap open" : "Cabin flap close");
		}
	}
}
