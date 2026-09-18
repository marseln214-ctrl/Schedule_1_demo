using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Phone;
using ScheduleOne.UI.Phone.Map;
using ScheduleOne.UI.Phone.Messages;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class GameplayMenu : Singleton<GameplayMenu>
	{
		public enum EGameplayScreen
		{
			Phone = 0,
			Character = 1
		}

		public const float OpenVerticalOffset = 0.002f;

		public const float ClosedVerticalOffset = -0.2f;

		public const float OpenTime = 0.06f;

		public const float SlideTime = 0.12f;

		[Header("References")]
		public Camera OverlayCamera;

		public Light OverlayLight;

		[Header("Settings")]
		public float ContainerOffset_PhoneScreen = -0.1f;

		private Coroutine openCloseRoutine;

		private Coroutine screenChangeRoutine;

		public bool IsOpen { get; protected set; }

		public bool CharacterScreenEnabled => false;

		public EGameplayScreen CurrentScreen { get; protected set; }

		protected override void Start()
		{
			base.Start();
			OverlayCamera.enabled = false;
			OverlayLight.enabled = false;
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, -0.2f, base.transform.localPosition.z);
			GameInput.RegisterExitListener(Exit);
		}

		public void Exit(ExitAction exit)
		{
			if (!exit.used && (exit.exitType != 0 || !Singleton<ItemUIManager>.InstanceExists || !Singleton<ItemUIManager>.Instance.CanDragFromSlot(Singleton<ItemUIManager>.Instance.HoveredSlot)) && IsOpen)
			{
				exit.used = true;
				SetIsOpen(open: false);
			}
		}

		protected virtual void Update()
		{
			if (GameInput.IsTyping || Singleton<PauseMenu>.Instance.IsPaused || (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount != 0 && !IsOpen))
			{
				return;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.TogglePhone))
			{
				SetIsOpen(!IsOpen);
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.OpenMap))
			{
				if (PlayerSingleton<MapApp>.Instance.isOpen && IsOpen && CurrentScreen == EGameplayScreen.Phone)
				{
					SetIsOpen(open: false);
				}
				else
				{
					PrepAppOpen();
					PlayerSingleton<MapApp>.Instance.SetOpen(open: true);
				}
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.OpenJournal))
			{
				if (PlayerSingleton<JournalApp>.Instance.isOpen && IsOpen && CurrentScreen == EGameplayScreen.Phone)
				{
					SetIsOpen(open: false);
				}
				else
				{
					PrepAppOpen();
					PlayerSingleton<JournalApp>.Instance.SetOpen(open: true);
				}
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.OpenTexts))
			{
				if (PlayerSingleton<MessagesApp>.Instance.isOpen && IsOpen && CurrentScreen == EGameplayScreen.Phone)
				{
					SetIsOpen(open: false);
				}
				else
				{
					PrepAppOpen();
					PlayerSingleton<MessagesApp>.Instance.SetOpen(open: true);
				}
			}
			if (IsOpen)
			{
				_ = CharacterScreenEnabled;
			}
			void PrepAppOpen()
			{
				if (!IsOpen)
				{
					SetIsOpen(open: true);
				}
				if (CurrentScreen != 0)
				{
					SetScreen(EGameplayScreen.Phone);
				}
				if (ScheduleOne.UI.Phone.Phone.ActiveApp != null)
				{
					PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.RequestCloseApp();
				}
			}
		}

		public void SetScreen(EGameplayScreen screen)
		{
			EGameplayScreen previousScreen;
			if (CurrentScreen != screen)
			{
				previousScreen = CurrentScreen;
				CurrentScreen = screen;
				if (screen == EGameplayScreen.Phone)
				{
					PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.SetIsOpen(o: true);
				}
				else if (screen == EGameplayScreen.Character)
				{
					Singleton<CharacterDisplay>.Instance.SetOpen(open: true);
				}
				if (screenChangeRoutine != null)
				{
					Singleton<CoroutineService>.Instance.StopCoroutine(screenChangeRoutine);
				}
				Singleton<GameplayMenuInterface>.Instance.SetSelected(screen);
				screenChangeRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(ScreenChange());
			}
			IEnumerator ScreenChange()
			{
				float endXPos = 0f;
				if (screen == EGameplayScreen.Character)
				{
					endXPos = ContainerOffset_PhoneScreen;
				}
				float startXPos = base.transform.localPosition.x;
				for (float t = 0f; t < 0.12f; t += Time.deltaTime)
				{
					base.transform.localPosition = new Vector3(Mathf.Lerp(startXPos, endXPos, t / 0.12f), base.transform.localPosition.y, base.transform.localPosition.z);
					yield return new WaitForEndOfFrame();
				}
				base.transform.localPosition = new Vector3(endXPos, base.transform.localPosition.y, base.transform.localPosition.z);
				if (previousScreen == EGameplayScreen.Phone)
				{
					PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.SetIsOpen(o: false);
				}
				else if (previousScreen == EGameplayScreen.Character)
				{
					Singleton<CharacterDisplay>.Instance.SetOpen(open: false);
				}
				screenChangeRoutine = null;
			}
		}

		public void SetIsOpen(bool open)
		{
			IsOpen = open;
			if (open)
			{
				OverlayLight.enabled = true;
			}
			if (CurrentScreen == EGameplayScreen.Phone)
			{
				if (open)
				{
					PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.SetIsOpen(o: true);
				}
				else
				{
					PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.SetIsOpen(o: false);
				}
			}
			else if (CurrentScreen == EGameplayScreen.Character)
			{
				if (open)
				{
					Singleton<CharacterDisplay>.Instance.SetOpen(open: true);
				}
				else
				{
					Singleton<CharacterDisplay>.Instance.SetOpen(open: false);
				}
			}
			if (IsOpen)
			{
				PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
				PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
				PlayerSingleton<PlayerMovement>.Instance.canMove = false;
				Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true, modifierPromptsVisible: false);
				Singleton<CompassManager>.Instance.SetVisible(visible: false);
				Player.Local.SendEquippable_Networked("Avatar/Equippables/Phone_Lowered");
				Singleton<InputPromptsCanvas>.Instance.LoadModule("phone");
			}
			else
			{
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				if (Player.Local.CurrentVehicle == null)
				{
					PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
					PlayerSingleton<PlayerMovement>.Instance.canMove = true;
					PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
				}
				else
				{
					Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
				}
				Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
				Singleton<CompassManager>.Instance.SetVisible(visible: true);
				Player.Local.SendEquippable_Networked(string.Empty);
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			}
			if (openCloseRoutine != null)
			{
				StopCoroutine(openCloseRoutine);
			}
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			if (IsOpen)
			{
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			}
			openCloseRoutine = StartCoroutine(SetIsOpenRoutine(open));
			IEnumerator SetIsOpenRoutine(bool open)
			{
				if (open)
				{
					OverlayCamera.enabled = true;
				}
				float num = 1f - base.transform.localPosition.y / -0.2f;
				float adjustedLerpTime2 = 0.06f;
				float startVert = base.transform.localPosition.y;
				float endVert;
				if (open)
				{
					adjustedLerpTime2 *= 1f - num;
					endVert = 0.002f;
				}
				else
				{
					adjustedLerpTime2 *= num;
					endVert = -0.2f;
				}
				PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(open, adjustedLerpTime2);
				for (float i = 0f; i < adjustedLerpTime2; i += Time.deltaTime)
				{
					base.transform.localPosition = new Vector3(base.transform.localPosition.x, Mathf.Lerp(startVert, endVert, i / adjustedLerpTime2), base.transform.localPosition.z);
					yield return new WaitForEndOfFrame();
				}
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, endVert, base.transform.localPosition.z);
				if (!open)
				{
					OverlayCamera.enabled = false;
					OverlayLight.enabled = false;
				}
				openCloseRoutine = null;
			}
		}
	}
}
