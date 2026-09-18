using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Interaction
{
	public class InteractableObject : MonoBehaviour
	{
		public enum EInteractionType
		{
			Key_Press = 0,
			LeftMouse_Click = 1
		}

		public enum EInteractableState
		{
			Default = 0,
			Invalid = 1,
			Disabled = 2,
			Label = 3
		}

		[Header("Settings")]
		[SerializeField]
		protected string message = "<Message>";

		[SerializeField]
		protected EInteractionType interactionType;

		[SerializeField]
		protected EInteractableState interactionState;

		public float MaxInteractionRange = 5f;

		public bool RequiresUniqueClick = true;

		public int Priority;

		[SerializeField]
		protected Collider displayLocationCollider;

		public Transform displayLocationPoint;

		[Header("Angle Limits")]
		public bool LimitInteractionAngle;

		public float AngleLimit = 90f;

		[Header("Events")]
		public UnityEvent onHovered = new UnityEvent();

		public UnityEvent onInteractStart = new UnityEvent();

		public UnityEvent onInteractEnd = new UnityEvent();

		public EInteractionType _interactionType => interactionType;

		public EInteractableState _interactionState => interactionState;

		public void SetInteractionType(EInteractionType type)
		{
			interactionType = type;
		}

		public void SetInteractableState(EInteractableState state)
		{
			interactionState = state;
		}

		public void SetMessage(string _message)
		{
			message = _message;
		}

		public virtual void Hovered()
		{
			if (onHovered != null)
			{
				onHovered.Invoke();
			}
			if (interactionState != EInteractableState.Disabled)
			{
				ShowMessage();
			}
		}

		public virtual void StartInteract()
		{
			if (interactionState != EInteractableState.Invalid)
			{
				if (onInteractStart != null)
				{
					onInteractStart.Invoke();
				}
				Singleton<InteractionManager>.Instance.LerpDisplayScale(0.9f);
			}
		}

		public virtual void EndInteract()
		{
			if (onInteractEnd != null)
			{
				onInteractEnd.Invoke();
			}
			Singleton<InteractionManager>.Instance.LerpDisplayScale(1f);
		}

		protected virtual void ShowMessage()
		{
			Vector3 pos = base.transform.position;
			if (displayLocationCollider != null)
			{
				pos = displayLocationCollider.ClosestPoint(PlayerSingleton<PlayerCamera>.Instance.transform.position);
			}
			else if (displayLocationPoint != null)
			{
				pos = displayLocationPoint.position;
			}
			Sprite icon = null;
			string spriteText = string.Empty;
			Color iconColor = Color.white;
			Color white = Color.white;
			switch (interactionState)
			{
			case EInteractableState.Default:
				white = Singleton<InteractionManager>.Instance.messageColor_Default;
				switch (interactionType)
				{
				case EInteractionType.Key_Press:
					icon = Singleton<InteractionManager>.Instance.icon_Key;
					spriteText = Singleton<InteractionManager>.Instance.InteractKey;
					iconColor = Singleton<InteractionManager>.Instance.iconColor_Default_Key;
					break;
				case EInteractionType.LeftMouse_Click:
					icon = Singleton<InteractionManager>.Instance.icon_LeftMouse;
					iconColor = Singleton<InteractionManager>.Instance.iconColor_Default;
					break;
				default:
					Console.LogWarning("EInteractionType not accounted for!");
					break;
				}
				break;
			case EInteractableState.Invalid:
				icon = Singleton<InteractionManager>.Instance.icon_Cross;
				iconColor = Singleton<InteractionManager>.Instance.iconColor_Invalid;
				white = Singleton<InteractionManager>.Instance.messageColor_Invalid;
				break;
			case EInteractableState.Disabled:
				return;
			case EInteractableState.Label:
				icon = null;
				white = Singleton<InteractionManager>.Instance.messageColor_Default;
				break;
			default:
				Console.LogWarning("EInteractableState not accounted for!");
				return;
			}
			Singleton<InteractionManager>.Instance.EnableInteractionDisplay(pos, icon, spriteText, message, white, iconColor);
		}

		public bool CheckAngleLimit(Vector3 interactionSource)
		{
			if (!LimitInteractionAngle)
			{
				return true;
			}
			Vector3 normalized = (interactionSource - base.transform.position).normalized;
			return Mathf.Abs(Vector3.SignedAngle(base.transform.forward, normalized, Vector3.up)) < AngleLimit;
		}
	}
}
