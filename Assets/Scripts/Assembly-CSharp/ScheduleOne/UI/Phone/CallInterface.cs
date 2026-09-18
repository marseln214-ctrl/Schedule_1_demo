using System;
using System.Collections;
using System.Text.RegularExpressions;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.ScriptableObjects;
using ScheduleOne.UI.Input;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone
{
	public class CallInterface : Singleton<CallInterface>
	{
		public const float TIME_PER_CHAR = 0.015f;

		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public Image ProfilePicture;

		public TextMeshProUGUI NameLabel;

		public TextMeshProUGUI MainText;

		public RectTransform ContinuePrompt;

		public Animation OpenAnim;

		public AudioSourceController TypewriterEffectSound;

		public CanvasGroup CanvasGroup;

		[Header("Settings")]
		public Color Highlight1Color;

		private int currentCallStage = -1;

		private Coroutine slideRoutine;

		private bool skipRollout;

		private Coroutine rolloutRoutine;

		private string highlight1Hex;

		public Action<PhoneCallData> CallCompleted;

		public PhoneCallData ActiveCallData { get; private set; }

		public bool IsOpen { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			highlight1Hex = ColorUtility.ToHtmlStringRGB(Highlight1Color);
			ContinuePrompt.gameObject.SetActive(value: false);
			GameInput.RegisterExitListener(Exit, 1);
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
		}

		private void Update()
		{
			if (IsOpen && (GameInput.GetButtonDown(GameInput.ButtonCode.Submit) || GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) || GameInput.GetButtonDown(GameInput.ButtonCode.Jump)))
			{
				if (rolloutRoutine != null)
				{
					skipRollout = true;
				}
				else
				{
					Continue();
				}
			}
		}

		private void Exit(ExitAction exit)
		{
			if (!exit.used && IsOpen && exit.exitType == ExitType.Escape)
			{
				exit.used = true;
				Close();
			}
		}

		public void StartCall(PhoneCallData data, CallerID caller, int startStage = 0)
		{
			if (IsOpen)
			{
				Debug.LogWarning("CallInterface: There is already a call in progress; existing call will be forced complete");
				for (int i = currentCallStage; i < ActiveCallData.Stages.Length; i++)
				{
					if (i > currentCallStage)
					{
						ActiveCallData.Stages[i].OnStageStart();
					}
					ActiveCallData.Stages[i].OnStageEnd();
				}
				ActiveCallData.Completed();
			}
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0.2f);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			ActiveCallData = data;
			IsOpen = true;
			ProfilePicture.sprite = caller.ProfilePicture;
			MainText.text = string.Empty;
			NameLabel.text = caller.Name;
			currentCallStage = startStage;
			SetIsVisible(visible: true);
			ShowStage(0, 0.25f);
		}

		public void EndCall()
		{
			if (!IsOpen)
			{
				Debug.LogWarning("CallInterface: Attempted to end a call while no call was in progress.");
				return;
			}
			if (ActiveCallData != null)
			{
				ActiveCallData.Completed();
			}
			if (CallCompleted != null)
			{
				CallCompleted(ActiveCallData);
			}
			Close();
		}

		private void Close()
		{
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.2f);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.2f);
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			if (rolloutRoutine != null)
			{
				StopCoroutine(rolloutRoutine);
				rolloutRoutine = null;
			}
			ActiveCallData = null;
			IsOpen = false;
			SetIsVisible(visible: false);
		}

		public void Continue()
		{
			if (currentCallStage != -1)
			{
				ActiveCallData.Stages[currentCallStage].OnStageEnd();
			}
			if (currentCallStage == ActiveCallData.Stages.Length - 1)
			{
				EndCall();
			}
			else
			{
				ShowStage(currentCallStage + 1);
			}
		}

		private void ShowStage(int stageIndex, float initialDelay = 0f)
		{
			currentCallStage = stageIndex;
			ActiveCallData.Stages[stageIndex].OnStageStart();
			rolloutRoutine = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				ContinuePrompt.gameObject.SetActive(value: false);
				if (initialDelay > 0f)
				{
					yield return new WaitForSeconds(initialDelay);
				}
				string stageText = ProcessText(ActiveCallData.Stages[stageIndex].Text);
				MainText.text = stageText;
				MainText.ForceMeshUpdate();
				int parsedLength = MainText.GetParsedText().Length;
				for (int i = 0; i < parsedLength; i++)
				{
					if (skipRollout)
					{
						break;
					}
					MainText.maxVisibleCharacters = i;
					if (i % 2 == 0)
					{
						TypewriterEffectSound.Play();
					}
					yield return new WaitForSeconds(0.015f);
				}
				skipRollout = false;
				MainText.text = stageText;
				MainText.maxVisibleCharacters = stageText.Length;
				MainText.ForceMeshUpdate();
				ContinuePrompt.anchoredPosition = new Vector2(ContinuePrompt.anchoredPosition.x, -18f - MainText.preferredHeight);
				ContinuePrompt.gameObject.SetActive(value: true);
				rolloutRoutine = null;
			}
		}

		private string ProcessText(string text)
		{
			string pattern = "<Input_([a-zA-Z0-9]+)>";
			MatchEvaluator evaluator = delegate(Match match)
			{
				if (Enum.TryParse<GameInput.ButtonCode>(match.Groups[1].Value, out var result))
				{
					Singleton<GameInput>.Instance.GetAction(result).GetBindingDisplayString(0, out var _, out var controlPath);
					string displayNameForControlPath = Singleton<InputPromptsManager>.Instance.GetDisplayNameForControlPath(controlPath);
					return "<color=#" + highlight1Hex + ">" + displayNameForControlPath + "</color>";
				}
				return match.Value;
			};
			return Regex.Replace(text, pattern, evaluator).Replace("<h1>", "<color=#" + highlight1Hex + ">").Replace("</h>", "</color>");
		}

		private string GetVisibleText(int charactersShown, string fullText)
		{
			bool flag = false;
			string text = fullText.Substring(0, charactersShown);
			char[] array = text.ToCharArray();
			if (array[charactersShown - 1] == '<' || flag)
			{
				flag = true;
				if (array[charactersShown - 1] == '>')
				{
					flag = false;
				}
			}
			return text;
		}

		private void SetIsVisible(bool visible)
		{
			if (slideRoutine != null)
			{
				StopCoroutine(slideRoutine);
			}
			if (visible)
			{
				CanvasGroup.alpha = 0f;
				Canvas.enabled = true;
				Container.gameObject.SetActive(value: true);
				OpenAnim.Play();
			}
			else
			{
				Canvas.enabled = false;
				Container.gameObject.SetActive(value: false);
			}
		}
	}
}
