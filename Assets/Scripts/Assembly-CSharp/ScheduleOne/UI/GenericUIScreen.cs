using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI
{
	public class GenericUIScreen : MonoBehaviour
	{
		[Header("Settings")]
		public string Name;

		public bool UseExitActions = true;

		public int ExitActionPriority;

		public bool CanExitWithRightClick = true;

		public bool ReenableControlsOnClose = true;

		public bool ReenableInventoryOnClose = true;

		public bool ReenableEquippingOnClose = true;

		public UnityEvent onOpen;

		public UnityEvent onClose;

		public bool IsOpen { get; private set; }

		private void Awake()
		{
			if (UseExitActions)
			{
				GameInput.RegisterExitListener(Exit, ExitActionPriority);
			}
		}

		public void Open()
		{
			IsOpen = true;
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(Name);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			if (onOpen != null)
			{
				onOpen.Invoke();
			}
		}

		public void Close()
		{
			IsOpen = false;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(Name);
			if (ReenableControlsOnClose)
			{
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			}
			if (ReenableInventoryOnClose)
			{
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			}
			if (!ReenableEquippingOnClose)
			{
				PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			}
			if (onClose != null)
			{
				onClose.Invoke();
			}
		}

		private void Exit(ExitAction action)
		{
			if (IsOpen && !action.used && (CanExitWithRightClick || action.exitType == ExitType.Escape))
			{
				action.used = true;
				Close();
			}
		}
	}
}
