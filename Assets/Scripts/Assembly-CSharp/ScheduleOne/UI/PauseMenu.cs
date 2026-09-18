using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.MainMenu;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI
{
	public class PauseMenu : Singleton<PauseMenu>
	{
		public Canvas Canvas;

		public RectTransform Container;

		public MainMenuScreen Screen;

		public FeedbackForm FeedbackForm;

		private bool noActiveUIElements = true;

		private bool justPaused;

		private bool justResumed;

		private bool couldLook;

		private bool lockedMouse;

		private bool crosshairVisible;

		private bool hudVisible;

		public UnityEvent onPause;

		public UnityEvent onResume;

		public bool IsPaused { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			GameInput.RegisterExitListener(Exit, -100);
		}

		protected override void Start()
		{
			base.Start();
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && action.exitType != 0 && !justResumed && !GameInput.IsTyping)
			{
				if (IsPaused)
				{
					Resume();
				}
				else
				{
					Pause();
				}
			}
		}

		private void Update()
		{
			_ = PlayerSingleton<PlayerCamera>.InstanceExists;
		}

		private void LateUpdate()
		{
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				noActiveUIElements = PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount == 0;
				justPaused = false;
				justResumed = false;
			}
		}

		public void Pause()
		{
			Console.Log("Game paused");
			IsPaused = true;
			justPaused = true;
			if (FeedbackForm != null)
			{
				FeedbackForm.PrepScreenshot();
			}
			if (Singleton<ScheduleOne.DevUtilities.Settings>.InstanceExists && Singleton<ScheduleOne.DevUtilities.Settings>.Instance.PausingFreezesTime)
			{
				Time.timeScale = 0f;
			}
			Canvas.enabled = true;
			Container.gameObject.SetActive(value: true);
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				couldLook = PlayerSingleton<PlayerCamera>.Instance.canLook;
				lockedMouse = Cursor.lockState == CursorLockMode.Locked;
				crosshairVisible = Singleton<HUD>.Instance.crosshair.gameObject.activeSelf;
				hudVisible = Singleton<HUD>.Instance.canvas.enabled;
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
				PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
				PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0.075f);
				Singleton<HUD>.Instance.canvas.enabled = false;
			}
			Screen.Open(closePrevious: false);
		}

		public void Resume()
		{
			Console.Log("Game resumed");
			IsPaused = false;
			justResumed = true;
			if (Singleton<ScheduleOne.DevUtilities.Settings>.InstanceExists && Singleton<ScheduleOne.DevUtilities.Settings>.Instance.PausingFreezesTime)
			{
				if (NetworkSingleton<TimeManager>.Instance.SleepInProgress)
				{
					Time.timeScale = 1f;
				}
				else
				{
					Time.timeScale = 1f;
				}
			}
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				if (couldLook)
				{
					PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
				}
				if (lockedMouse)
				{
					PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				}
				PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0.075f);
			}
			if (Singleton<HUD>.InstanceExists)
			{
				Singleton<HUD>.Instance.SetCrosshairVisible(crosshairVisible);
				Singleton<HUD>.Instance.canvas.enabled = hudVisible;
			}
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			Screen.Close(openPrevious: false);
		}

		public void StuckButtonClicked()
		{
			Resume();
			PlayerSingleton<PlayerMovement>.Instance.WarpToNavMesh();
		}
	}
}
