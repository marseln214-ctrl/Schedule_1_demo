using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;

namespace ScheduleOne.DevUtilities
{
	public static class PlayerUtilities
	{
		public static void OpenMenu()
		{
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		}

		public static void CloseMenu(bool reenableLookInstantly = false, bool reenableInventory = true)
		{
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			if (reenableLookInstantly)
			{
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			}
			if (reenableInventory)
			{
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			}
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		}
	}
}
