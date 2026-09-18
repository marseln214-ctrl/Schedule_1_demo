using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Interaction
{
	public class InteractableToggleable : MonoBehaviour
	{
		public string ActivateMessage = "Activate";

		public string DeactivateMessage = "Deactivate";

		public float CoolDown;

		[Header("References")]
		public InteractableObject IntObj;

		public UnityEvent onToggle = new UnityEvent();

		public UnityEvent onActivate = new UnityEvent();

		public UnityEvent onDeactivate = new UnityEvent();

		private float lastActivated;

		public bool IsActivated { get; private set; }

		public void Start()
		{
			IntObj.onHovered.AddListener(Hovered);
			IntObj.onInteractStart.AddListener(Interacted);
		}

		public void Hovered()
		{
			if (Time.time - lastActivated < CoolDown)
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
				return;
			}
			IntObj.SetMessage(IsActivated ? DeactivateMessage : ActivateMessage);
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}

		public void Interacted()
		{
			Toggle();
		}

		public void Toggle()
		{
			lastActivated = Time.time;
			IsActivated = !IsActivated;
			if (onToggle != null)
			{
				onToggle.Invoke();
			}
			if (IsActivated)
			{
				onActivate.Invoke();
			}
			else
			{
				onDeactivate.Invoke();
			}
		}

		public void SetState(bool activated)
		{
			if (IsActivated != activated)
			{
				lastActivated = Time.time;
				IsActivated = !IsActivated;
				if (IsActivated)
				{
					onActivate.Invoke();
				}
				else
				{
					onDeactivate.Invoke();
				}
			}
		}

		public void PoliceDetected()
		{
			if (!IsActivated)
			{
				Toggle();
			}
		}
	}
}
