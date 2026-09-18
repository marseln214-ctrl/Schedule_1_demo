using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Quests;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone
{
	public class JournalApp : App<JournalApp>
	{
		[Header("References")]
		public RectTransform EntryContainer;

		public Text NoTasksLabel;

		public Text NoDetailsLabel;

		public RectTransform DetailsPanelContainer;

		[Header("Entry prefabs")]
		public GameObject GenericEntry;

		[Header("Details panel prefabs")]
		public GameObject GenericDetailsPanel;

		[Header("Quest Entry prefab")]
		public GameObject GenericQuestEntry;

		[Header("HUD entry prefabs")]
		public QuestHUDUI QuestHUDUIPrefab;

		public QuestEntryHUDUI QuestEntryHUDUIPrefab;

		protected Quest currentDetailsPanelQuest;

		protected RectTransform currentDetailsPanel;

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
			TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
			timeManager.onMinutePass = (Action)Delegate.Combine(timeManager.onMinutePass, new Action(MinPass));
		}

		public override void SetOpen(bool open)
		{
			base.SetOpen(open);
			if (!open && currentDetailsPanel != null)
			{
				currentDetailsPanelQuest.DestroyDetailDisplay();
				currentDetailsPanel = null;
				currentDetailsPanelQuest = null;
			}
		}

		protected override void Update()
		{
			base.Update();
			if (base.isOpen)
			{
				RefreshDetailsPanel();
				NoTasksLabel.enabled = Quest.ActiveQuests.Count == 0;
				NoDetailsLabel.enabled = currentDetailsPanel == null;
			}
		}

		private void RefreshDetailsPanel()
		{
			if (Quest.HoveredQuest != null)
			{
				if (currentDetailsPanelQuest != Quest.HoveredQuest)
				{
					if (currentDetailsPanel != null)
					{
						currentDetailsPanelQuest.DestroyDetailDisplay();
						currentDetailsPanel = null;
						currentDetailsPanelQuest = null;
					}
					currentDetailsPanel = Quest.HoveredQuest.CreateDetailDisplay(DetailsPanelContainer);
					currentDetailsPanelQuest = Quest.HoveredQuest;
				}
			}
			else if (currentDetailsPanel != null)
			{
				currentDetailsPanelQuest.DestroyDetailDisplay();
				currentDetailsPanel = null;
				currentDetailsPanelQuest = null;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (NetworkSingleton<TimeManager>.InstanceExists)
			{
				TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
				timeManager.onMinutePass = (Action)Delegate.Remove(timeManager.onMinutePass, new Action(MinPass));
			}
		}

		protected virtual void MinPass()
		{
			_ = base.isOpen;
		}
	}
}
