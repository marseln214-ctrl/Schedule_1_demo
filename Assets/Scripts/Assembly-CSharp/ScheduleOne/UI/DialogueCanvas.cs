using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class DialogueCanvas : Singleton<DialogueCanvas>
	{
		public const float TIME_PER_CHAR = 0.015f;

		public bool SkipNextRollout;

		[Header("References")]
		[SerializeField]
		protected Canvas canvas;

		public RectTransform Container;

		[SerializeField]
		protected TextMeshProUGUI dialogueText;

		[SerializeField]
		protected GameObject continuePopup;

		[SerializeField]
		protected List<DialogueChoiceEntry> dialogueChoices = new List<DialogueChoiceEntry>();

		private DialogueHandler currentHandler;

		private DialogueNodeData currentNode;

		private bool spaceDownThisFrame;

		private bool leftClickThisFrame;

		private string overrideText = string.Empty;

		private Coroutine dialogueRollout;

		private Coroutine choiceSelectionResidualCoroutine;

		private bool hasChoiceBeenSelected;

		public bool isActive => currentHandler != null;

		protected override void Awake()
		{
			base.Awake();
			canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			GameInput.RegisterExitListener(Exit);
		}

		public void DisplayDialogueNode(DialogueHandler diag, DialogueNodeData node, string dialogueText, List<string> choices)
		{
			if (diag != currentHandler)
			{
				StartDialogue(diag);
			}
			if (dialogueRollout != null)
			{
				StopCoroutine(dialogueRollout);
			}
			currentNode = node;
			dialogueRollout = StartCoroutine(RolloutDialogue(dialogueText, choices));
		}

		public void OverrideText(string text)
		{
			overrideText = text;
			if (dialogueRollout != null)
			{
				StopCoroutine(dialogueRollout);
			}
			dialogueText.text = overrideText;
			canvas.enabled = true;
			Container.gameObject.SetActive(value: true);
		}

		public void StopTextOverride()
		{
			overrideText = string.Empty;
		}

		private void Update()
		{
			if (isActive)
			{
				if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
				{
					spaceDownThisFrame = true;
				}
				else
				{
					spaceDownThisFrame = false;
				}
				if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
				{
					leftClickThisFrame = true;
				}
				else
				{
					leftClickThisFrame = false;
				}
			}
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && isActive && action.exitType == ExitType.Escape && DialogueHandler.activeDialogue.AllowExit)
			{
				action.used = true;
				currentHandler.EndDialogue();
			}
		}

		protected IEnumerator RolloutDialogue(string text, List<string> choices)
		{
			List<int> activeDialogueChoices = new List<int>();
			dialogueText.maxVisibleCharacters = 0;
			dialogueText.text = text;
			canvas.enabled = true;
			Container.gameObject.SetActive(value: true);
			float rolloutTime = (float)text.Length * 0.015f;
			if (SkipNextRollout)
			{
				SkipNextRollout = false;
				rolloutTime = 0f;
			}
			for (float i = 0f; i < rolloutTime; i += Time.deltaTime)
			{
				if (spaceDownThisFrame)
				{
					break;
				}
				if (leftClickThisFrame)
				{
					break;
				}
				int maxVisibleCharacters = (int)(i / 0.015f);
				dialogueText.maxVisibleCharacters = maxVisibleCharacters;
				yield return new WaitForEndOfFrame();
			}
			dialogueText.maxVisibleCharacters = text.Length;
			spaceDownThisFrame = false;
			leftClickThisFrame = false;
			hasChoiceBeenSelected = false;
			if (choiceSelectionResidualCoroutine != null)
			{
				StopCoroutine(choiceSelectionResidualCoroutine);
			}
			continuePopup.gameObject.SetActive(value: false);
			for (int j = 0; j < dialogueChoices.Count; j++)
			{
				dialogueChoices[j].gameObject.SetActive(value: false);
				dialogueChoices[j].canvasGroup.alpha = 1f;
				if (choices.Count > j)
				{
					dialogueChoices[j].text.text = choices[j];
					dialogueChoices[j].button.interactable = true;
					string reason = string.Empty;
					if (IsChoiceValid(j, out reason))
					{
						dialogueChoices[j].notPossibleGameObject.SetActive(value: false);
						dialogueChoices[j].button.interactable = true;
						ColorBlock colors = dialogueChoices[j].button.colors;
						colors.disabledColor = colors.pressedColor;
						dialogueChoices[j].button.colors = colors;
					}
					else
					{
						dialogueChoices[j].notPossibleText.text = reason.ToUpper();
						dialogueChoices[j].notPossibleGameObject.SetActive(value: true);
						ColorBlock colors2 = dialogueChoices[j].button.colors;
						colors2.disabledColor = colors2.normalColor;
						dialogueChoices[j].button.colors = colors2;
						dialogueChoices[j].button.interactable = false;
					}
					activeDialogueChoices.Add(j);
				}
			}
			if (activeDialogueChoices.Count == 0 || (activeDialogueChoices.Count == 1 && choices[0] == ""))
			{
				continuePopup.gameObject.SetActive(value: true);
				yield return new WaitUntil(() => spaceDownThisFrame || leftClickThisFrame);
				continuePopup.gameObject.SetActive(value: false);
				spaceDownThisFrame = false;
				leftClickThisFrame = false;
				currentHandler.ContinueSubmitted();
				yield break;
			}
			for (int k = 0; k < activeDialogueChoices.Count; k++)
			{
				dialogueChoices[activeDialogueChoices[k]].gameObject.SetActive(value: true);
			}
			while (!hasChoiceBeenSelected)
			{
				string reason2 = string.Empty;
				if (UnityEngine.Input.GetKey(KeyCode.Alpha1) && IsChoiceValid(0, out reason2))
				{
					ChoiceSelected(0);
				}
				else if (UnityEngine.Input.GetKey(KeyCode.Alpha2) && IsChoiceValid(1, out reason2))
				{
					ChoiceSelected(1);
				}
				else if (UnityEngine.Input.GetKey(KeyCode.Alpha3) && IsChoiceValid(2, out reason2))
				{
					ChoiceSelected(2);
				}
				else if (UnityEngine.Input.GetKey(KeyCode.Alpha4) && IsChoiceValid(3, out reason2))
				{
					ChoiceSelected(3);
				}
				else if (UnityEngine.Input.GetKey(KeyCode.Alpha5) && IsChoiceValid(4, out reason2))
				{
					ChoiceSelected(4);
				}
				else if (UnityEngine.Input.GetKey(KeyCode.Alpha6) && IsChoiceValid(5, out reason2))
				{
					ChoiceSelected(5);
				}
				yield return new WaitForEndOfFrame();
			}
		}

		private IEnumerator ChoiceSelectionResidual(DialogueChoiceEntry choice, float fadeTime)
		{
			yield return new WaitForSeconds(0.25f);
			float realFadeTime = fadeTime - 0.25f;
			for (float i = 0f; i < realFadeTime; i += Time.deltaTime)
			{
				choice.canvasGroup.alpha = Mathf.Sqrt(Mathf.Lerp(1f, 0f, i / realFadeTime));
				yield return new WaitForEndOfFrame();
			}
			choice.gameObject.SetActive(value: false);
			choiceSelectionResidualCoroutine = null;
		}

		private void StartDialogue(DialogueHandler handler)
		{
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			currentHandler = handler;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			Vector3 normalized = (currentHandler.LookPosition.transform.position - PlayerSingleton<PlayerCamera>.Instance.transform.position).normalized;
			Quaternion quaternion = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z), Vector3.up);
			PlayerSingleton<PlayerMovement>.Instance.LerpPlayerRotation(quaternion, 0.3f);
			Vector3 vector = new Vector3(Mathf.Sqrt(Mathf.Pow(normalized.x, 2f) + Mathf.Pow(normalized.z, 2f)), normalized.y, 0f);
			float x = (0f - Mathf.Atan2(vector.y, vector.x)) * (180f / MathF.PI);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(PlayerSingleton<PlayerCamera>.Instance.transform.position, quaternion * Quaternion.Euler(x, 0f, 0f), 0.3f, keepParented: true);
		}

		public void EndDialogue()
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			continuePopup.gameObject.SetActive(value: false);
			for (int i = 0; i < dialogueChoices.Count; i++)
			{
				dialogueChoices[i].gameObject.SetActive(value: false);
			}
			if (dialogueRollout != null)
			{
				StopCoroutine(dialogueRollout);
			}
			if (choiceSelectionResidualCoroutine != null)
			{
				StopCoroutine(choiceSelectionResidualCoroutine);
			}
			canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			currentHandler = null;
			currentNode = null;
			if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount == 0)
			{
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				PlayerSingleton<PlayerMovement>.Instance.canMove = true;
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: true, returnToOriginalRotation: false);
			}
			else
			{
				PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: false, returnToOriginalRotation: false);
			}
		}

		public void ChoiceSelected(int choiceIndex)
		{
			string reason = string.Empty;
			if (!IsChoiceValid(choiceIndex, out reason))
			{
				return;
			}
			hasChoiceBeenSelected = true;
			for (int i = 0; i < dialogueChoices.Count; i++)
			{
				if (i == choiceIndex)
				{
					dialogueChoices[i].button.interactable = false;
					if (choiceSelectionResidualCoroutine != null)
					{
						StopCoroutine(choiceSelectionResidualCoroutine);
					}
					choiceSelectionResidualCoroutine = StartCoroutine(ChoiceSelectionResidual(dialogueChoices[i], 0.75f));
				}
				else
				{
					dialogueChoices[i].gameObject.SetActive(value: false);
				}
			}
			currentHandler.ChoiceSelected(choiceIndex);
		}

		private bool IsChoiceValid(int choiceIndex, out string reason)
		{
			if (currentNode != null && currentHandler.CurrentChoices.Count > choiceIndex)
			{
				return currentHandler.CheckChoice(currentHandler.CurrentChoices[choiceIndex].ChoiceLabel, out reason);
			}
			reason = string.Empty;
			return false;
		}
	}
}
