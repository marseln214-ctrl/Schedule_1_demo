using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.UI.Input;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management
{
	public class ManagementMode : Singleton<ManagementMode>
	{
		[Header("References")]
		public InputPrompt ManagementModeInputPrompt;

		[Header("UI References")]
		public Canvas Canvas;

		public UnityEvent OnEnterManagementMode;

		public UnityEvent onExitManagementMode;

		public ScheduleOne.Property.Property CurrentProperty { get; private set; }

		public bool isActive => CurrentProperty != null;

		protected override void Start()
		{
			base.Start();
			GameInput.RegisterExitListener(Exit, 1);
			Canvas.enabled = false;
		}

		private void Update()
		{
			UpdateInput();
			if (isActive && Player.Local.CurrentProperty != CurrentProperty)
			{
				ExitManagementMode();
			}
		}

		private void UpdateInput()
		{
			if (!Singleton<GameInput>.InstanceExists)
			{
				return;
			}
			ManagementModeInputPrompt.enabled = (isActive ? CanExitManagementMode() : CanEnterManagementMode());
			ManagementModeInputPrompt.Label = (isActive ? "Exit Management Mode" : "Enter Management Mode");
			if (GameInput.GetButtonDown(GameInput.ButtonCode.ManagementMode))
			{
				if (CurrentProperty != null)
				{
					ExitManagementMode();
				}
				else if (Player.Local.CurrentProperty != null && Player.Local.CurrentProperty.IsOwned)
				{
					EnterManagementMode(Player.Local.CurrentProperty);
				}
			}
		}

		private void Exit(ExitAction exitAction)
		{
			if (isActive && !exitAction.used && exitAction.exitType == ExitType.Escape)
			{
				ExitManagementMode();
				exitAction.used = true;
			}
		}

		public void EnterManagementMode(ScheduleOne.Property.Property property)
		{
			CurrentProperty = property;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			Canvas.enabled = true;
			if (OnEnterManagementMode != null)
			{
				OnEnterManagementMode.Invoke();
			}
		}

		public void ExitManagementMode()
		{
			CurrentProperty = null;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			Canvas.enabled = false;
			if (onExitManagementMode != null)
			{
				onExitManagementMode.Invoke();
			}
		}

		public static bool CanEnterManagementMode()
		{
			if (Player.Local.CurrentProperty == null)
			{
				return false;
			}
			if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
			{
				return false;
			}
			return true;
		}

		public static bool CanExitManagementMode()
		{
			return true;
		}
	}
}
