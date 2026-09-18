using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class QuestHUDUI : MonoBehaviour
	{
		public string CriticalTimeColor = "FF7A7A";

		[Header("References")]
		public RectTransform EntryContainer;

		public TextMeshProUGUI MainLabel;

		public VerticalLayoutGroup hudUILayout;

		public Animation Animation;

		public RectTransform Shade;

		public Action onUpdateUI;

		public Quest Quest { get; private set; }

		public void Initialize(Quest quest)
		{
			Quest = quest;
			Quest quest2 = Quest;
			quest2.onSubtitleChanged = (Action)Delegate.Combine(quest2.onSubtitleChanged, new Action(UpdateMainLabel));
			UnityEngine.Object.Instantiate(Quest.IconPrefab, base.transform.Find("Title/IconContainer")).GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 20f);
			UpdateUI();
			if (Quest.QuestState == EQuestState.Active)
			{
				FadeIn();
			}
			else
			{
				Quest.onQuestBegin.AddListener(FadeIn);
				base.gameObject.SetActive(value: false);
			}
			Quest.onQuestEnd.AddListener(EntryEnded);
		}

		public void Destroy()
		{
			Quest quest = Quest;
			quest.onSubtitleChanged = (Action)Delegate.Remove(quest.onSubtitleChanged, new Action(UpdateMainLabel));
			QuestEntryHUDUI[] componentsInChildren = GetComponentsInChildren<QuestEntryHUDUI>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Destroy();
			}
		}

		public void UpdateUI()
		{
			UpdateMainLabel();
			UpdateExpiry();
			if (onUpdateUI != null)
			{
				onUpdateUI();
			}
			hudUILayout.CalculateLayoutInputVertical();
			hudUILayout.SetLayoutVertical();
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)hudUILayout.transform);
			hudUILayout.enabled = false;
			hudUILayout.enabled = true;
			UpdateShade();
			Singleton<CoroutineService>.Instance.StartCoroutine(DelayFix());
			IEnumerator DelayFix()
			{
				yield return new WaitForEndOfFrame();
				hudUILayout.CalculateLayoutInputVertical();
				hudUILayout.SetLayoutVertical();
				LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)hudUILayout.transform);
				hudUILayout.enabled = false;
				hudUILayout.enabled = true;
				UpdateShade();
			}
		}

		public void UpdateMainLabel()
		{
			MainLabel.text = Quest.GetQuestTitle() + Quest.Subtitle;
			MainLabel.ForceMeshUpdate();
		}

		public void UpdateExpiry()
		{
		}

		public void UpdateShade()
		{
			Shade.sizeDelta = new Vector2(550f, hudUILayout.preferredHeight + 120f);
		}

		public void BopIcon()
		{
			base.transform.Find("Title/IconContainer").GetComponent<Animation>().Play();
		}

		private void FadeIn()
		{
			if (Quest.IsTracked)
			{
				base.gameObject.SetActive(value: true);
			}
			Animation.Play("Quest enter");
		}

		private void EntryEnded(EQuestState endState)
		{
			if (endState == EQuestState.Completed)
			{
				Complete();
			}
			else
			{
				FadeOut();
			}
		}

		private void FadeOut()
		{
			Animation.Play("Quest exit");
			Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(0.5f);
				base.gameObject.SetActive(value: false);
			}
		}

		private void Complete()
		{
			Animation.Play("Quest complete");
			Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(3f);
				FadeOut();
			}
		}
	}
}
