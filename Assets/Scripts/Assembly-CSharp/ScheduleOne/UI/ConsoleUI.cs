using System.Collections;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScheduleOne.UI
{
	public class ConsoleUI : MonoBehaviour
	{
		[Header("References")]
		public Canvas canvas;

		public TMP_InputField InputField;

		public GameObject Container;

		public bool IS_CONSOLE_ENABLED
		{
			get
			{
				if (!Application.isEditor)
				{
					return Debug.isDebugBuild;
				}
				return true;
			}
		}

		private void Awake()
		{
			InputField.onSubmit.AddListener(Submit);
			Container.gameObject.SetActive(value: false);
			GameInput.RegisterExitListener(Exit, 5);
		}

		private void Update()
		{
			if (UnityEngine.Input.GetKeyDown(KeyCode.BackQuote) && !Singleton<PauseMenu>.Instance.IsPaused && IS_CONSOLE_ENABLED)
			{
				SetIsOpen(!canvas.enabled);
			}
			if (canvas.enabled && !Player.Local.Health.IsAlive)
			{
				SetIsOpen(open: false);
			}
		}

		private void Exit(ExitAction exitAction)
		{
			if (!(canvas == null) && canvas.enabled && !exitAction.used && exitAction.exitType == ExitType.Escape)
			{
				exitAction.used = true;
				SetIsOpen(open: false);
			}
		}

		public void SetIsOpen(bool open)
		{
			if (InstanceFinder.IsHost || !(InstanceFinder.NetworkManager != null) || Application.isEditor || Debug.isDebugBuild)
			{
				canvas.enabled = open;
				Container.gameObject.SetActive(open);
				InputField.SetTextWithoutNotify("");
				if (open)
				{
					PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
					GameInput.IsTyping = true;
					StartCoroutine(Routine());
				}
				else
				{
					PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
					GameInput.IsTyping = false;
				}
			}
			IEnumerator Routine()
			{
				yield return null;
				EventSystem.current.SetSelectedGameObject(null);
				EventSystem.current.SetSelectedGameObject(InputField.gameObject);
			}
		}

		public void Submit(string val)
		{
			if (canvas.enabled)
			{
				Console.SubmitCommand(val);
				SetIsOpen(open: false);
			}
		}
	}
}
