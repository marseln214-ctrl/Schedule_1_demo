using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Input;
using ScheduleOne.UI.Phone;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScheduleOne.UI
{
	public class HintDisplay : Singleton<HintDisplay>
	{
		private class Hint
		{
			public string Text;

			public float Duration;

			public Hint(string text, float duration)
			{
				Text = text;
				Duration = duration;
			}
		}

		public const float FadeTime = 0.3f;

		[Header("References")]
		public RectTransform Container;

		public TextMeshProUGUI Label;

		public CanvasGroup Group;

		public InputPrompt DismissPrompt;

		public Animation FlashAnim;

		[Header("Settings")]
		public Vector2 Padding;

		public Vector2 Offset;

		private Coroutine autoCloseRoutine;

		private Coroutine fadeRoutine;

		private List<Hint> hintQueue = new List<Hint>();

		private float timeSinceOpened;

		public bool IsOpen { get; protected set; }

		protected override void Start()
		{
			base.Start();
			Group.alpha = 0f;
			Container.gameObject.SetActive(value: false);
		}

		public void Update()
		{
			if (!IsOpen)
			{
				if (hintQueue.Count > 0 && Group.alpha == 0f)
				{
					ShowHint(hintQueue[0].Text, hintQueue[0].Duration);
					hintQueue.RemoveAt(0);
				}
				return;
			}
			timeSinceOpened += Time.deltaTime;
			if (Singleton<CallInterface>.Instance.IsOpen)
			{
				Hide();
			}
			DismissPrompt.SetLabel((hintQueue.Count > 0) ? "Next" : "Dismiss");
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Submit) && !GameInput.IsTyping && timeSinceOpened > 0.1f)
			{
				Hide();
			}
		}

		public void ShowHint_10s(string text)
		{
			ShowHint(text, 10f);
		}

		public void ShowHint_20s(string text)
		{
			ShowHint(text, 20f);
		}

		public void ShowHint(string text)
		{
			ShowHint(text, 0f);
		}

		public void ShowHint(string text, float autoCloseTime = 0f)
		{
			text = ProcessText(text);
			Console.Log("Showing hint: " + text);
			timeSinceOpened = 0f;
			SetAlpha(1f);
			FlashAnim.Play();
			Label.text = text;
			Label.ForceMeshUpdate();
			Container.sizeDelta = new Vector2(Label.renderedWidth + Padding.x, Label.renderedHeight + Padding.y);
			Container.anchoredPosition = new Vector2((0f - Container.sizeDelta.x) / 2f - Offset.x, (0f - Container.sizeDelta.y) / 2f + Offset.y);
			if (autoCloseRoutine != null)
			{
				StopCoroutine(autoCloseRoutine);
			}
			if (autoCloseTime > 0f)
			{
				autoCloseRoutine = StartCoroutine(AutoClose(autoCloseTime));
			}
			IsOpen = true;
			IEnumerator AutoClose(float time)
			{
				yield return new WaitForSeconds(time);
				Hide();
				autoCloseRoutine = null;
			}
		}

		public void Hide()
		{
			SetAlpha(0f);
			IsOpen = false;
		}

		private void SetAlpha(float alpha)
		{
			if (alpha > 0f)
			{
				Container.gameObject.SetActive(value: true);
			}
			if (fadeRoutine != null)
			{
				StopCoroutine(fadeRoutine);
			}
			fadeRoutine = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				float startAlpha = Group.alpha;
				for (float i = 0f; i < 0.3f; i += Time.deltaTime)
				{
					Group.alpha = Mathf.Lerp(startAlpha, alpha, i / 0.3f);
					yield return new WaitForEndOfFrame();
				}
				Group.alpha = alpha;
				if (alpha == 0f)
				{
					Container.gameObject.SetActive(value: false);
				}
				fadeRoutine = null;
			}
		}

		public void QueueHint_10s(string message)
		{
			hintQueue.Add(new Hint(message, 10f));
		}

		public void QueueHint_20s(string message)
		{
			hintQueue.Add(new Hint(message, 20f));
		}

		public void QueueHint(string message, float time)
		{
			hintQueue.Add(new Hint(message, time));
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
					return "<color=#88CBFF>" + displayNameForControlPath + "</color>";
				}
				return match.Value;
			};
			return Regex.Replace(text, pattern, evaluator).Replace("<h1>", "<color=#88CBFF>").Replace("<h2>", "<color=#F86266>")
				.Replace("<h3>", "<color=#46CB4F>")
				.Replace("</h>", "</color>");
		}
	}
}
