using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using UnityEngine;

namespace ScheduleOne.Market
{
	public class Merchant : MonoBehaviour
	{
		[Header("Settings")]
		[SerializeField]
		protected string shopName = "Store";

		[SerializeField]
		protected int openTime = 600;

		[SerializeField]
		protected int closeTime = 1800;

		[Header("References")]
		[SerializeField]
		protected InteractableObject intObj;

		protected virtual void Start()
		{
		}

		public void Hovered()
		{
			if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(openTime, closeTime))
			{
				intObj.SetMessage("Browse " + shopName);
				intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
				intObj.SetMessage("Closed");
			}
		}

		public virtual void Interacted()
		{
		}
	}
}
