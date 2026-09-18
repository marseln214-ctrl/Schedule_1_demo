using ScheduleOne.Construction;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.Property;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Market
{
	public class BuilderMerchant : MonoBehaviour
	{
		[Header("Settings")]
		[SerializeField]
		protected int openTime = 600;

		[SerializeField]
		protected int closeTime = 1800;

		[Header("References")]
		[SerializeField]
		protected InteractableObject intObj;

		[SerializeField]
		private PropertySelector selector;

		public void Hovered()
		{
			if (Singleton<ConstructionManager>.Instance.constructionModeEnabled || selector.isOpen)
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
			else if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(openTime, closeTime))
			{
				intObj.SetMessage("View construction menu");
				intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
				intObj.SetMessage("Closed");
			}
		}

		public void Interacted()
		{
			selector.OpenSelector(PropertySelected);
		}

		private void PropertySelected(ScheduleOne.Property.Property p)
		{
			Singleton<ConstructionManager>.Instance.EnterConstructionMode(p);
		}
	}
}
