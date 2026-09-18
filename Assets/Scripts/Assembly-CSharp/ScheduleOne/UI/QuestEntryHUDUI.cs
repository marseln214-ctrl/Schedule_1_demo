using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Quests;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class QuestEntryHUDUI : MonoBehaviour
	{
		[Header("References")]
		public TextMeshProUGUI MainLabel;

		public Animation Animation;

		public QuestEntry QuestEntry { get; private set; }

		public void Initialize(QuestEntry entry)
		{
			QuestEntry = entry;
			MainLabel.text = entry.Title;
			QuestHUDUI hudUI = QuestEntry.ParentQuest.hudUI;
			hudUI.onUpdateUI = (Action)Delegate.Combine(hudUI.onUpdateUI, new Action(UpdateUI));
			if (QuestEntry.State == EQuestState.Active)
			{
				FadeIn();
			}
			else
			{
				QuestEntry.onStart.AddListener(FadeIn);
			}
			QuestEntry.onEnd.AddListener(EntryEnded);
		}

		public void Destroy()
		{
			QuestHUDUI hudUI = QuestEntry.ParentQuest.hudUI;
			hudUI.onUpdateUI = (Action)Delegate.Remove(hudUI.onUpdateUI, new Action(UpdateUI));
			QuestEntry.onStart.RemoveListener(FadeIn);
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public virtual void UpdateUI()
		{
			if (QuestEntry.State != EQuestState.Active)
			{
				if (!Animation.isPlaying)
				{
					base.gameObject.SetActive(value: false);
				}
				return;
			}
			if (QuestEntry.ParentQuest.ActiveEntryCount > 1)
			{
				MainLabel.text = "• " + QuestEntry.Title;
			}
			else
			{
				MainLabel.text = QuestEntry.Title;
			}
			base.gameObject.SetActive(value: true);
			MainLabel.ForceMeshUpdate();
		}

		private void FadeIn()
		{
			QuestEntry.UpdateEntryUI();
			base.transform.SetAsLastSibling();
			Animation.Play("Quest entry enter");
		}

		private void EntryEnded()
		{
			if (QuestEntry.State == EQuestState.Completed)
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
			Animation.Play("Quest entry exit");
			Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(Animation.GetClip("Quest entry exit").length);
				base.gameObject.SetActive(value: false);
				QuestEntry.UpdateEntryUI();
			}
		}

		private void Complete()
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			Animation.Play("Quest entry complete");
			Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(3f);
				FadeOut();
			}
		}
	}
}
