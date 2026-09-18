using ScheduleOne.Interaction;
using UnityEngine;

namespace ScheduleOne.TV
{
	public class TVInteractable : MonoBehaviour
	{
		public InteractableObject IntObj;

		public TVInterface Interface;

		private void Start()
		{
			IntObj.onHovered.AddListener(Hovered);
			IntObj.onInteractStart.AddListener(Interacted);
		}

		private void Hovered()
		{
			if (Interface.CanOpen())
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				IntObj.SetMessage("Use TV");
			}
			else
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}

		private void Interacted()
		{
			if (Interface.CanOpen())
			{
				Interface.Open();
			}
		}
	}
}
