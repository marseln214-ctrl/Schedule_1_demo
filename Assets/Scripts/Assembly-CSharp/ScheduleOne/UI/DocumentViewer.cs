using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI
{
	public class DocumentViewer : Singleton<DocumentViewer>
	{
		[Header("References")]
		public Canvas Canvas;

		public RectTransform[] Documents;

		public UnityEvent onOpen;

		public bool IsOpen { get; protected set; }

		protected override void Start()
		{
			base.Start();
			IsOpen = false;
			Canvas.enabled = false;
			GameInput.RegisterExitListener(Exit, 15);
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && IsOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				Close();
			}
		}

		public void Open(string documentName)
		{
			IsOpen = true;
			for (int i = 0; i < Documents.Length; i++)
			{
				Documents[i].gameObject.SetActive(Documents[i].name == documentName);
			}
			Canvas.enabled = true;
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0f);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			Singleton<HUD>.Instance.canvas.enabled = false;
			if (onOpen != null)
			{
				onOpen.Invoke();
			}
		}

		public void Close()
		{
			IsOpen = false;
			Canvas.enabled = false;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0f);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			Singleton<HUD>.Instance.canvas.enabled = true;
		}
	}
}
