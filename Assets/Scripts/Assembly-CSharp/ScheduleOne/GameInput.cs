using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScheduleOne
{
	public class GameInput : PersistentSingleton<GameInput>
	{
		public enum ButtonCode
		{
			PrimaryClick = 0,
			SecondaryClick = 1,
			TertiaryClick = 2,
			Forward = 3,
			Backward = 4,
			Left = 5,
			Right = 6,
			Jump = 7,
			Crouch = 8,
			Sprint = 9,
			Escape = 10,
			Back = 11,
			Interact = 12,
			Submit = 13,
			TogglePhone = 14,
			ToggleLights = 15,
			Handbrake = 16,
			RotateLeft = 17,
			RotateRight = 18,
			ManagementMode = 19,
			OpenMap = 20,
			OpenJournal = 21,
			OpenTexts = 22,
			QuickMove = 23,
			ToggleFlashlight = 24,
			ViewAvatar = 25,
			Reload = 26
		}

		public class ExitListener
		{
			public ExitDelegate listenerFunction;

			public int priority;
		}

		public delegate void ExitDelegate(ExitAction exitAction);

		public static List<ExitListener> exitListeners = new List<ExitListener>();

		public PlayerInput PlayerInput;

		public static bool IsTyping = false;

		public static Vector2 MotionAxis = Vector2.zero;

		private List<ButtonCode> buttonsDownThisFrame = new List<ButtonCode>();

		private List<ButtonCode> buttonsDown = new List<ButtonCode>();

		private List<ButtonCode> buttonsUpThisFrame = new List<ButtonCode>();

		public static Vector2 MouseDelta => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

		public static Vector3 MousePosition => Input.mousePosition;

		public static float MouseScrollDelta => Input.GetAxis("Mouse ScrollWheel");

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
			Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(delegate
			{
				exitListeners.Clear();
			});
		}

		private void OnApplicationFocus(bool focus)
		{
			if (focus)
			{
				return;
			}
			foreach (ButtonCode item in buttonsDown)
			{
				buttonsUpThisFrame.Add(item);
			}
			buttonsDown.Clear();
		}

		public static bool GetButton(ButtonCode buttonCode)
		{
			return Singleton<GameInput>.Instance.buttonsDown.Contains(buttonCode);
		}

		public static bool GetButtonDown(ButtonCode buttonCode)
		{
			return Singleton<GameInput>.Instance.buttonsDownThisFrame.Contains(buttonCode);
		}

		public static bool GetButtonUp(ButtonCode buttonCode)
		{
			return Singleton<GameInput>.Instance.buttonsUpThisFrame.Contains(buttonCode);
		}

		protected virtual void Update()
		{
			if (Singleton<GameInput>.InstanceExists)
			{
				if (GetButtonDown(ButtonCode.Escape) || GetButtonDown(ButtonCode.Back))
				{
					Exit(GetButtonDown(ButtonCode.Escape) ? ExitType.Escape : ExitType.RightClick);
				}
				if (GetButton(ButtonCode.PrimaryClick) && !Input.GetMouseButton(0))
				{
					Console.LogWarning("Mouse button (0) sticking detected!");
					OnPrimaryClick();
				}
				if (GetButton(ButtonCode.SecondaryClick) && !Input.GetMouseButton(1))
				{
					Console.LogWarning("Mouse button (1) sticking detected!");
					OnSecondaryClick();
				}
			}
		}

		private void Exit(ExitType type)
		{
			ExitAction exitAction = new ExitAction();
			exitAction.exitType = type;
			for (int i = 0; i < exitListeners.Count; i++)
			{
				exitListeners[exitListeners.Count - (1 + i)].listenerFunction(exitAction);
			}
		}

		private void LateUpdate()
		{
			buttonsDownThisFrame.Clear();
			buttonsUpThisFrame.Clear();
		}

		public void ExitAll()
		{
			int num = 20;
			while (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
			{
				num--;
				if (num <= 0)
				{
					Console.LogError("Failed to exit from all active UI elements.");
					for (int i = 0; i < PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount; i++)
					{
						Debug.LogError(PlayerSingleton<PlayerCamera>.Instance.activeUIElements[i]);
					}
					break;
				}
				Exit(ExitType.Escape);
			}
		}

		private void OnMotion(InputValue value)
		{
			MotionAxis = value.Get<Vector2>();
			if (MotionAxis.x > 0f)
			{
				if (!buttonsDown.Contains(ButtonCode.Right))
				{
					buttonsDownThisFrame.Add(ButtonCode.Right);
					buttonsDown.Add(ButtonCode.Right);
				}
			}
			else if (buttonsDown.Contains(ButtonCode.Right))
			{
				buttonsUpThisFrame.Add(ButtonCode.Right);
				buttonsDown.Remove(ButtonCode.Right);
			}
			if (MotionAxis.x < 0f)
			{
				if (!buttonsDown.Contains(ButtonCode.Left))
				{
					buttonsDownThisFrame.Add(ButtonCode.Left);
					buttonsDown.Add(ButtonCode.Left);
				}
			}
			else if (buttonsDown.Contains(ButtonCode.Left))
			{
				buttonsUpThisFrame.Add(ButtonCode.Left);
				buttonsDown.Remove(ButtonCode.Left);
			}
			if (MotionAxis.y > 0f)
			{
				if (!buttonsDown.Contains(ButtonCode.Forward))
				{
					buttonsDownThisFrame.Add(ButtonCode.Forward);
					buttonsDown.Add(ButtonCode.Forward);
				}
			}
			else if (buttonsDown.Contains(ButtonCode.Forward))
			{
				buttonsUpThisFrame.Add(ButtonCode.Forward);
				buttonsDown.Remove(ButtonCode.Forward);
			}
			if (MotionAxis.y < 0f)
			{
				if (!buttonsDown.Contains(ButtonCode.Backward))
				{
					buttonsDownThisFrame.Add(ButtonCode.Backward);
					buttonsDown.Add(ButtonCode.Backward);
				}
			}
			else if (buttonsDown.Contains(ButtonCode.Backward))
			{
				buttonsUpThisFrame.Add(ButtonCode.Backward);
				buttonsDown.Remove(ButtonCode.Backward);
			}
		}

		private void OnPrimaryClick()
		{
			if (buttonsDown.Contains(ButtonCode.PrimaryClick))
			{
				buttonsDown.Remove(ButtonCode.PrimaryClick);
				buttonsUpThisFrame.Add(ButtonCode.PrimaryClick);
			}
			else
			{
				buttonsDown.Add(ButtonCode.PrimaryClick);
				buttonsDownThisFrame.Add(ButtonCode.PrimaryClick);
			}
		}

		private void OnSecondaryClick()
		{
			if (buttonsDown.Contains(ButtonCode.SecondaryClick))
			{
				buttonsDown.Remove(ButtonCode.SecondaryClick);
				buttonsUpThisFrame.Add(ButtonCode.SecondaryClick);
			}
			else
			{
				buttonsDown.Add(ButtonCode.SecondaryClick);
				buttonsDownThisFrame.Add(ButtonCode.SecondaryClick);
			}
		}

		private void OnTertiaryClick()
		{
			if (buttonsDown.Contains(ButtonCode.TertiaryClick))
			{
				buttonsDown.Remove(ButtonCode.TertiaryClick);
				buttonsUpThisFrame.Add(ButtonCode.TertiaryClick);
			}
			else
			{
				buttonsDown.Add(ButtonCode.TertiaryClick);
				buttonsDownThisFrame.Add(ButtonCode.TertiaryClick);
			}
		}

		private void OnJump()
		{
			if (buttonsDown.Contains(ButtonCode.Jump))
			{
				buttonsDown.Remove(ButtonCode.Jump);
				buttonsUpThisFrame.Add(ButtonCode.Jump);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Jump);
				buttonsDownThisFrame.Add(ButtonCode.Jump);
			}
		}

		private void OnCrouch()
		{
			if (buttonsDown.Contains(ButtonCode.Crouch))
			{
				buttonsDown.Remove(ButtonCode.Crouch);
				buttonsUpThisFrame.Add(ButtonCode.Crouch);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Crouch);
				buttonsDownThisFrame.Add(ButtonCode.Crouch);
			}
		}

		private void OnSprint()
		{
			if (buttonsDown.Contains(ButtonCode.Sprint))
			{
				buttonsDown.Remove(ButtonCode.Sprint);
				buttonsUpThisFrame.Add(ButtonCode.Sprint);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Sprint);
				buttonsDownThisFrame.Add(ButtonCode.Sprint);
			}
		}

		private void OnEscape()
		{
			if (buttonsDown.Contains(ButtonCode.Escape))
			{
				buttonsDown.Remove(ButtonCode.Escape);
				buttonsUpThisFrame.Add(ButtonCode.Escape);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Escape);
				buttonsDownThisFrame.Add(ButtonCode.Escape);
			}
		}

		private void OnBack()
		{
			if (buttonsDown.Contains(ButtonCode.Back))
			{
				buttonsDown.Remove(ButtonCode.Back);
				buttonsUpThisFrame.Add(ButtonCode.Back);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Back);
				buttonsDownThisFrame.Add(ButtonCode.Back);
			}
		}

		private void OnInteract()
		{
			if (buttonsDown.Contains(ButtonCode.Interact))
			{
				buttonsDown.Remove(ButtonCode.Interact);
				buttonsUpThisFrame.Add(ButtonCode.Interact);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Interact);
				buttonsDownThisFrame.Add(ButtonCode.Interact);
			}
		}

		private void OnSubmit()
		{
			if (buttonsDown.Contains(ButtonCode.Submit))
			{
				buttonsDown.Remove(ButtonCode.Submit);
				buttonsUpThisFrame.Add(ButtonCode.Submit);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Submit);
				buttonsDownThisFrame.Add(ButtonCode.Submit);
			}
		}

		private void OnTogglePhone()
		{
			if (buttonsDown.Contains(ButtonCode.TogglePhone))
			{
				buttonsDown.Remove(ButtonCode.TogglePhone);
				buttonsUpThisFrame.Add(ButtonCode.TogglePhone);
			}
			else
			{
				buttonsDown.Add(ButtonCode.TogglePhone);
				buttonsDownThisFrame.Add(ButtonCode.TogglePhone);
			}
		}

		private void OnToggleLights()
		{
			if (buttonsDown.Contains(ButtonCode.ToggleLights))
			{
				buttonsDown.Remove(ButtonCode.ToggleLights);
				buttonsUpThisFrame.Add(ButtonCode.ToggleLights);
			}
			else
			{
				buttonsDown.Add(ButtonCode.ToggleLights);
				buttonsDownThisFrame.Add(ButtonCode.ToggleLights);
			}
		}

		private void OnHandbrake()
		{
			if (buttonsDown.Contains(ButtonCode.Handbrake))
			{
				buttonsDown.Remove(ButtonCode.Handbrake);
				buttonsUpThisFrame.Add(ButtonCode.Handbrake);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Handbrake);
				buttonsDownThisFrame.Add(ButtonCode.Handbrake);
			}
		}

		private void OnRotateLeft()
		{
			if (buttonsDown.Contains(ButtonCode.RotateLeft))
			{
				buttonsDown.Remove(ButtonCode.RotateLeft);
				buttonsUpThisFrame.Add(ButtonCode.RotateLeft);
			}
			else
			{
				buttonsDown.Add(ButtonCode.RotateLeft);
				buttonsDownThisFrame.Add(ButtonCode.RotateLeft);
			}
		}

		private void OnRotateRight()
		{
			if (buttonsDown.Contains(ButtonCode.RotateRight))
			{
				buttonsDown.Remove(ButtonCode.RotateRight);
				buttonsUpThisFrame.Add(ButtonCode.RotateRight);
			}
			else
			{
				buttonsDown.Add(ButtonCode.RotateRight);
				buttonsDownThisFrame.Add(ButtonCode.RotateRight);
			}
		}

		private void OnManagementMode()
		{
			if (buttonsDown.Contains(ButtonCode.ManagementMode))
			{
				buttonsDown.Remove(ButtonCode.ManagementMode);
				buttonsUpThisFrame.Add(ButtonCode.ManagementMode);
			}
			else
			{
				buttonsDown.Add(ButtonCode.ManagementMode);
				buttonsDownThisFrame.Add(ButtonCode.ManagementMode);
			}
		}

		private void OnOpenMap()
		{
			if (buttonsDown.Contains(ButtonCode.OpenMap))
			{
				buttonsDown.Remove(ButtonCode.OpenMap);
				buttonsUpThisFrame.Add(ButtonCode.OpenMap);
			}
			else
			{
				buttonsDown.Add(ButtonCode.OpenMap);
				buttonsDownThisFrame.Add(ButtonCode.OpenMap);
			}
		}

		private void OnOpenJournal()
		{
			if (buttonsDown.Contains(ButtonCode.OpenJournal))
			{
				buttonsDown.Remove(ButtonCode.OpenJournal);
				buttonsUpThisFrame.Add(ButtonCode.OpenJournal);
			}
			else
			{
				buttonsDown.Add(ButtonCode.OpenJournal);
				buttonsDownThisFrame.Add(ButtonCode.OpenJournal);
			}
		}

		private void OnOpenTexts()
		{
			if (buttonsDown.Contains(ButtonCode.OpenTexts))
			{
				buttonsDown.Remove(ButtonCode.OpenTexts);
				buttonsUpThisFrame.Add(ButtonCode.OpenTexts);
			}
			else
			{
				buttonsDown.Add(ButtonCode.OpenTexts);
				buttonsDownThisFrame.Add(ButtonCode.OpenTexts);
			}
		}

		private void OnQuickMove()
		{
			if (buttonsDown.Contains(ButtonCode.QuickMove))
			{
				buttonsDown.Remove(ButtonCode.QuickMove);
				buttonsUpThisFrame.Add(ButtonCode.QuickMove);
			}
			else
			{
				buttonsDown.Add(ButtonCode.QuickMove);
				buttonsDownThisFrame.Add(ButtonCode.QuickMove);
			}
		}

		private void OnToggleFlashlight()
		{
			if (buttonsDown.Contains(ButtonCode.ToggleFlashlight))
			{
				buttonsDown.Remove(ButtonCode.ToggleFlashlight);
				buttonsUpThisFrame.Add(ButtonCode.ToggleFlashlight);
			}
			else
			{
				buttonsDown.Add(ButtonCode.ToggleFlashlight);
				buttonsDownThisFrame.Add(ButtonCode.ToggleFlashlight);
			}
		}

		private void OnViewAvatar()
		{
			if (buttonsDown.Contains(ButtonCode.ViewAvatar))
			{
				buttonsDown.Remove(ButtonCode.ViewAvatar);
				buttonsUpThisFrame.Add(ButtonCode.ViewAvatar);
			}
			else
			{
				buttonsDown.Add(ButtonCode.ViewAvatar);
				buttonsDownThisFrame.Add(ButtonCode.ViewAvatar);
			}
		}

		private void OnReload()
		{
			if (buttonsDown.Contains(ButtonCode.Reload))
			{
				buttonsDown.Remove(ButtonCode.Reload);
				buttonsUpThisFrame.Add(ButtonCode.Reload);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Reload);
				buttonsDownThisFrame.Add(ButtonCode.Reload);
			}
		}

		public static void RegisterExitListener(ExitDelegate listener, int priority = 0)
		{
			ExitListener exitListener = new ExitListener();
			exitListener.listenerFunction = listener;
			exitListener.priority = priority;
			for (int i = 0; i < exitListeners.Count; i++)
			{
				if (priority <= exitListeners[i].priority)
				{
					exitListeners.Insert(i, exitListener);
					return;
				}
			}
			exitListeners.Add(exitListener);
		}

		public static void DeregisterExitListener(ExitDelegate listener)
		{
			for (int i = 0; i < exitListeners.Count; i++)
			{
				if (exitListeners[i].listenerFunction == listener)
				{
					exitListeners.RemoveAt(i);
					i--;
				}
			}
		}

		public InputAction GetAction(ButtonCode code)
		{
			return PlayerInput.currentActionMap.FindAction(code.ToString());
		}
	}
}
