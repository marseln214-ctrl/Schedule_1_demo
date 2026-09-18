using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Levelling;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Phone;
using ScheduleOne.UI.Phone.Map;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.Quests
{
	[Serializable]
	public class Quest : MonoBehaviour, IGUIDRegisterable, ISaveable
	{
		public const int MAX_HUD_ENTRY_LABELS = 10;

		public const int CriticalExpiryThreshold = 120;

		public static List<Quest> Quests = new List<Quest>();

		public static Quest HoveredQuest = null;

		public static List<Quest> ActiveQuests = new List<Quest>();

		[Header("Basic Settings")]
		[SerializeField]
		protected string title = string.Empty;

		public string Subtitle = string.Empty;

		public Action onSubtitleChanged;

		[TextArea(3, 10)]
		public string Description = string.Empty;

		public string StaticGUID = string.Empty;

		public bool TrackOnBegin;

		public EExpiryVisibility ExpiryVisibility;

		public bool AutoCompleteOnAllEntriesComplete;

		public bool PlayQuestCompleteSound = true;

		public int CompletionXP;

		[Header("Entries")]
		public bool AutoStartFirstEntry = true;

		public List<QuestEntry> Entries = new List<QuestEntry>();

		[Header("UI")]
		public RectTransform IconPrefab;

		[Header("PoI Settings")]
		public GameObject PoIPrefab;

		[Header("Events")]
		public UnityEvent onQuestBegin;

		public UnityEvent<EQuestState> onQuestEnd;

		public UnityEvent onActiveState;

		public UnityEvent<bool> onTrackChange;

		public UnityEvent onComplete;

		[Header("Reminders")]
		public bool ShouldSendExpiryReminder = true;

		public bool ShouldSendExpiredNotification = true;

		protected RectTransform journalEntry;

		protected RectTransform entryTitleRect;

		protected RectTransform trackedRect;

		protected Text entryTimeLabel;

		protected Image criticalTimeBackground;

		protected RectTransform detailPanel;

		public Action onHudUICreated;

		private bool expiryReminderSent;

		private CompassManager.Element compassElement;

		protected bool autoInitialize = true;

		public EQuestState QuestState { get; protected set; }

		public Guid GUID { get; protected set; }

		public bool IsTracked { get; protected set; }

		public int ActiveEntryCount => Entries.Count((QuestEntry x) => x.State == EQuestState.Active);

		public string Title => GetQuestTitle();

		public bool Expires { get; protected set; }

		public GameDateTime Expiry { get; protected set; }

		public bool hudUIExists => hudUI != null;

		public QuestHUDUI hudUI { get; private set; }

		public string SaveFolderName => "Quest_" + GUID.ToString().Substring(0, 6);

		public string SaveFileName => "Quest_" + GUID.ToString().Substring(0, 6);

		public Loader Loader => null;

		public bool ShouldSaveUnderFolder => false;

		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		protected virtual void Awake()
		{
		}

		protected virtual void Start()
		{
			if (autoInitialize)
			{
				if (Player.Local != null)
				{
					Initialize();
				}
				else
				{
					Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Initialize));
				}
			}
			if (AutoCompleteOnAllEntriesComplete)
			{
				for (int i = 0; i < Entries.Count; i++)
				{
					Entries[i].onComplete.AddListener(CheckAutoComplete);
				}
			}
			void Initialize()
			{
				Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(Initialize));
				if (!GUIDManager.IsGUIDValid(StaticGUID))
				{
					Console.LogWarning("Invalid GUID for quest: " + title + " Generating random GUID");
					StaticGUID = GUIDManager.GenerateUniqueGUID().ToString();
				}
				QuestEntryData[] entries = new QuestEntryData[0];
				InitializeQuest(title, Description, entries, StaticGUID);
			}
		}

		public virtual void InitializeQuest(string title, string description, QuestEntryData[] entries, string guid)
		{
			if (guid == string.Empty)
			{
				guid = Guid.NewGuid().ToString();
			}
			if (entries.Length == 0 && Entries.Count == 0)
			{
				Console.LogWarning(title + " quest has no entries!");
			}
			base.gameObject.name = title;
			for (int i = 0; i < entries.Length; i++)
			{
				GameObject obj = new GameObject(entries[i].Name);
				obj.transform.SetParent(base.transform);
				QuestEntry questEntry = obj.AddComponent<QuestEntry>();
				Entries.Add(questEntry);
				questEntry.SetData(entries[i]);
			}
			GUID = new Guid(guid);
			GUIDManager.RegisterObject(this);
			this.title = title;
			Description = description;
			HasChanged = true;
			Quests.Add(this);
			InitializeSaveable();
			SetupJournalEntry();
			SetupHudUI();
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public void ConfigureExpiry(bool expires, GameDateTime expiry)
		{
			Debug.Log("Configuring expiry for quest: " + Title + " to: " + expiry);
			Expires = expires;
			Expiry = expiry;
		}

		public virtual void Begin(bool network = true)
		{
			if (QuestState != EQuestState.Active)
			{
				SetQuestState(EQuestState.Active, network: false);
				if (AutoStartFirstEntry && Entries.Count > 0)
				{
					Entries[0].SetState(EQuestState.Active, network);
				}
				if (TrackOnBegin)
				{
					SetIsTracked(tracked: true);
				}
				UpdateHUDUI();
				if (network)
				{
					NetworkSingleton<QuestManager>.Instance.SendQuestAction(GUID.ToString(), QuestManager.EQuestAction.Begin);
				}
				if (onQuestBegin != null)
				{
					onQuestBegin.Invoke();
				}
			}
		}

		public virtual void Complete(bool network = true)
		{
			if (QuestState != EQuestState.Completed)
			{
				if (CompletionXP > 0 && InstanceFinder.IsServer && !Singleton<LoadManager>.Instance.IsLoading)
				{
					Console.Log("Adding XP for quest: " + Title);
					NetworkSingleton<LevelManager>.Instance.AddXP(CompletionXP);
				}
				SetQuestState(EQuestState.Completed, network: false);
				if (PlayQuestCompleteSound)
				{
					NetworkSingleton<QuestManager>.Instance.PlayCompleteQuestSound();
				}
				End();
				if (network)
				{
					NetworkSingleton<QuestManager>.Instance.SendQuestAction(GUID.ToString(), QuestManager.EQuestAction.Success);
				}
				if (onComplete != null)
				{
					onComplete.Invoke();
				}
			}
		}

		public virtual void Fail(bool network = true)
		{
			SetQuestState(EQuestState.Failed, network: false);
			End();
			if (network)
			{
				NetworkSingleton<QuestManager>.Instance.SendQuestAction(GUID.ToString(), QuestManager.EQuestAction.Fail);
			}
		}

		public virtual void Expire(bool network = true)
		{
			if (QuestState != EQuestState.Expired)
			{
				SetQuestState(EQuestState.Expired, network: false);
				if (ShouldSendExpiredNotification)
				{
					SendExpiredNotification();
				}
				End();
				if (network)
				{
					NetworkSingleton<QuestManager>.Instance.SendQuestAction(GUID.ToString(), QuestManager.EQuestAction.Expire);
				}
			}
		}

		public virtual void Cancel(bool network = true)
		{
			SetQuestState(EQuestState.Cancelled, network: false);
			End();
			if (network)
			{
				NetworkSingleton<QuestManager>.Instance.SendQuestAction(GUID.ToString(), QuestManager.EQuestAction.Cancel);
			}
		}

		public virtual void End()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			ActiveQuests.Remove(this);
			DestroyDetailDisplay();
			DestroyJournalEntry();
			if (onQuestEnd != null)
			{
				onQuestEnd.Invoke(QuestState);
			}
		}

		public virtual void SetQuestState(EQuestState state, bool network = true)
		{
			QuestState = state;
			HasChanged = true;
			StateMachine.ChangeState();
			if (hudUI != null)
			{
				hudUI.gameObject.SetActive(IsTracked && (QuestState == EQuestState.Active || QuestState == EQuestState.Completed));
			}
			if (journalEntry != null)
			{
				journalEntry.gameObject.SetActive(ShouldShowJournalEntry());
			}
			for (int i = 0; i < Entries.Count; i++)
			{
				Entries[i].UpdateCompassElement();
			}
			if (state == EQuestState.Active && onActiveState != null)
			{
				onActiveState.Invoke();
			}
			if (network)
			{
				NetworkSingleton<QuestManager>.Instance.SendQuestState(GUID.ToString(), state);
			}
		}

		protected virtual bool ShouldShowJournalEntry()
		{
			return QuestState == EQuestState.Active;
		}

		public virtual void SetQuestEntryState(int entryIndex, EQuestState state, bool network = true)
		{
			if (entryIndex < 0 || entryIndex >= Entries.Count)
			{
				Console.LogWarning("Invalid entry index: " + entryIndex);
				return;
			}
			HasChanged = true;
			Entries[entryIndex].SetState(state, network);
			if (state == EQuestState.Completed)
			{
				BopHUDUI();
			}
		}

		protected virtual void MinPass()
		{
			if (Expires)
			{
				bool flag = GetMinsUntilExpiry() <= 120;
				if (entryTimeLabel != null)
				{
					entryTimeLabel.text = GetExpiryText();
				}
				if (criticalTimeBackground != null)
				{
					criticalTimeBackground.enabled = flag;
				}
				UpdateHUDUI();
				CheckExpiry();
				if (ShouldSendExpiryReminder && flag && !expiryReminderSent)
				{
					SendExpiryReminder();
					expiryReminderSent = true;
				}
			}
		}

		protected virtual void CheckExpiry()
		{
			if (InstanceFinder.IsServer && Expires && GetMinsUntilExpiry() <= 0 && CanExpire())
			{
				Expire();
			}
		}

		private void CheckAutoComplete()
		{
			bool flag = true;
			for (int i = 0; i < Entries.Count; i++)
			{
				if (Entries[i].State != EQuestState.Completed)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				Complete();
			}
		}

		protected virtual bool CanExpire()
		{
			return true;
		}

		protected virtual void SendExpiryReminder()
		{
			Singleton<NotificationsManager>.Instance.SendNotification("<color=#FFB43C>Quest Expiring Soon</color>", title, PlayerSingleton<JournalApp>.Instance.AppIcon);
		}

		protected virtual void SendExpiredNotification()
		{
			Singleton<NotificationsManager>.Instance.SendNotification("<color=#FF6455>Quest Expired</color>", title, PlayerSingleton<JournalApp>.Instance.AppIcon);
		}

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
		}

		public void SetSubtitle(string subtitle)
		{
			Subtitle = subtitle;
		}

		public virtual void SetIsTracked(bool tracked)
		{
			IsTracked = tracked;
			if (hudUI != null)
			{
				hudUI.gameObject.SetActive(tracked && QuestState == EQuestState.Active);
			}
			if (journalEntry != null)
			{
				trackedRect.gameObject.SetActive(tracked);
				journalEntry.GetComponent<Image>().color = (IsTracked ? new Color32(75, 75, 75, byte.MaxValue) : new Color32(150, 150, 150, byte.MaxValue));
			}
			HasChanged = true;
			for (int i = 0; i < Entries.Count; i++)
			{
				Entries[i].UpdateCompassElement();
			}
			if (onTrackChange != null)
			{
				onTrackChange.Invoke(tracked);
			}
		}

		public virtual void SetupJournalEntry()
		{
			journalEntry = UnityEngine.Object.Instantiate(PlayerSingleton<JournalApp>.Instance.GenericEntry, PlayerSingleton<JournalApp>.Instance.EntryContainer).GetComponent<RectTransform>();
			journalEntry.Find("Title").GetComponent<Text>().text = title;
			entryTitleRect = journalEntry.Find("Title").GetComponent<RectTransform>();
			trackedRect = journalEntry.Find("Tracked").GetComponent<RectTransform>();
			SetIsTracked(IsTracked);
			journalEntry.Find("Expiry").gameObject.SetActive(Expires);
			entryTimeLabel = journalEntry.Find("Expiry/Time").GetComponent<Text>();
			criticalTimeBackground = journalEntry.Find("Expiry/Critical").GetComponent<Image>();
			journalEntry.GetComponent<Button>().onClick.AddListener(JournalEntryClicked);
			EventTrigger component = journalEntry.GetComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener(delegate
			{
				JournalEntryHoverStart();
			});
			component.triggers.Add(entry);
			UnityEngine.Object.Instantiate(IconPrefab, journalEntry.Find("IconContainer")).GetComponent<RectTransform>().sizeDelta = new Vector2(25f, 25f);
			journalEntry.gameObject.SetActive(value: false);
			if (Expires)
			{
				entryTimeLabel.text = GetExpiryText();
			}
		}

		private void DestroyJournalEntry()
		{
			if (!(journalEntry == null))
			{
				UnityEngine.Object.Destroy(journalEntry.gameObject);
				journalEntry = null;
			}
		}

		private void JournalEntryClicked()
		{
			SetIsTracked(!IsTracked);
		}

		private void JournalEntryHoverStart()
		{
			HoveredQuest = this;
		}

		public int GetMinsUntilExpiry()
		{
			int totalMinSum = NetworkSingleton<TimeManager>.Instance.GetTotalMinSum();
			int num = Expiry.GetMinSum() - totalMinSum;
			if (num > 0)
			{
				return num;
			}
			return 0;
		}

		public string GetExpiryText()
		{
			int minsUntilExpiry = GetMinsUntilExpiry();
			if (minsUntilExpiry >= 60)
			{
				return Mathf.RoundToInt((float)minsUntilExpiry / 60f) + " hrs";
			}
			return minsUntilExpiry + " min";
		}

		public virtual QuestHUDUI SetupHudUI()
		{
			if (hudUI != null)
			{
				return hudUI;
			}
			hudUI = UnityEngine.Object.Instantiate(PlayerSingleton<JournalApp>.Instance.QuestHUDUIPrefab, Singleton<HUD>.Instance.QuestEntryContainer).GetComponent<QuestHUDUI>();
			hudUI.Initialize(this);
			if (onHudUICreated != null)
			{
				onHudUICreated();
			}
			hudUI.gameObject.SetActive(IsTracked && QuestState == EQuestState.Active);
			return hudUI;
		}

		public void UpdateHUDUI()
		{
			hudUI?.UpdateUI();
		}

		public void BopHUDUI()
		{
			if (!(hudUI == null))
			{
				hudUI.BopIcon();
			}
		}

		public virtual string GetQuestTitle()
		{
			return title;
		}

		public QuestEntry GetFirstActiveEntry()
		{
			for (int i = 0; i < Entries.Count; i++)
			{
				if (Entries[i].State == EQuestState.Active)
				{
					return Entries[i];
				}
			}
			return null;
		}

		private void DestroyHudUI()
		{
			if (hudUI != null)
			{
				UnityEngine.Object.Destroy(hudUI.gameObject);
			}
		}

		public virtual RectTransform CreateDetailDisplay(RectTransform parent)
		{
			if (detailPanel != null)
			{
				Console.LogWarning("Detail panel already exists!");
				return null;
			}
			if (!PlayerSingleton<JournalApp>.InstanceExists)
			{
				Console.LogWarning("Journal app does not exist!");
				return null;
			}
			detailPanel = UnityEngine.Object.Instantiate(PlayerSingleton<JournalApp>.Instance.GenericDetailsPanel, parent).GetComponent<RectTransform>();
			detailPanel.Find("Title").GetComponent<Text>().text = title;
			detailPanel.Find("Description").GetComponent<Text>().text = Description;
			float preferredHeight = detailPanel.Find("Description").GetComponent<Text>().preferredHeight;
			detailPanel.Find("OuterContainer").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -45f - preferredHeight);
			RectTransform component = detailPanel.Find("OuterContainer/Entries").GetComponent<RectTransform>();
			int num = 0;
			for (int i = 0; i < Entries.Count; i++)
			{
				GameObject obj = UnityEngine.Object.Instantiate(PlayerSingleton<JournalApp>.Instance.GenericQuestEntry, component).gameObject;
				obj.transform.Find("Title").GetComponent<Text>().text = Entries[i].Title;
				obj.transform.Find("State").GetComponent<Text>().text = Entries[i].State.ToString();
				obj.transform.Find("State").GetComponent<Text>().color = ((Entries[i].State == EQuestState.Active) ? new Color32(50, 50, 50, byte.MaxValue) : new Color32(150, 150, 150, byte.MaxValue));
				obj.gameObject.SetActive(Entries[i].State != EQuestState.Inactive);
				if (obj.gameObject.activeSelf)
				{
					num++;
				}
			}
			detailPanel.Find("OuterContainer/Contents").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f - (float)num * 35f);
			POI pOI = null;
			QuestEntry firstActiveEntry = GetFirstActiveEntry();
			if (firstActiveEntry != null)
			{
				pOI = firstActiveEntry.PoI;
			}
			GameObject obj2 = detailPanel.Find("OuterContainer/Contents/ShowOnMap").gameObject;
			obj2.SetActive(pOI != null);
			obj2.GetComponent<Button>().onClick.AddListener(ShowOnMap);
			return detailPanel;
			void ShowOnMap()
			{
				POI pOI2 = null;
				QuestEntry firstActiveEntry2 = GetFirstActiveEntry();
				if (firstActiveEntry2 != null)
				{
					pOI2 = firstActiveEntry2.PoI;
				}
				if (pOI2 != null && pOI2.UI != null && PlayerSingleton<MapApp>.InstanceExists && PlayerSingleton<JournalApp>.InstanceExists)
				{
					PlayerSingleton<MapApp>.Instance.FocusPosition(pOI2.UI.anchoredPosition);
					PlayerSingleton<JournalApp>.Instance.SetOpen(open: false);
					PlayerSingleton<MapApp>.Instance.SkipFocusPlayer = true;
					PlayerSingleton<MapApp>.Instance.SetOpen(open: true);
				}
			}
		}

		public void DestroyDetailDisplay()
		{
			if (detailPanel != null)
			{
				UnityEngine.Object.Destroy(detailPanel.gameObject);
			}
			detailPanel = null;
		}

		public virtual string GetSaveString()
		{
			List<QuestEntryData> list = new List<QuestEntryData>();
			for (int i = 0; i < Entries.Count; i++)
			{
				list.Add(Entries[i].GetSaveData());
			}
			return new QuestData(GUID.ToString(), QuestState, IsTracked, title, Description, Expires, new GameDateTimeData(Expiry), list.ToArray()).GetJson();
		}

		public virtual void Load(QuestData data)
		{
			SetQuestState(data.State);
			if (data.IsTracked)
			{
				SetIsTracked(tracked: true);
			}
			for (int i = 0; i < data.Entries.Length; i++)
			{
				int num = i;
				float versionNumber = SaveManager.GetVersionNumber(data.GameVersion);
				if (SaveManager.GetVersionNumber(Application.version) > versionNumber)
				{
					int num2 = i;
					for (int j = 0; j < num2 && j < Entries.Count; j++)
					{
						if (SaveManager.GetVersionNumber(Entries[j].EntryAddedIn) > versionNumber)
						{
							Console.Log("Increasing index for quest entry: " + Entries[j].Title);
							num++;
							num2++;
						}
					}
				}
				SetQuestEntryState(num, data.Entries[i].State);
			}
		}

		public static Quest GetQuest(string questName)
		{
			return Quests.FirstOrDefault((Quest x) => x.title.ToLower() == questName.ToLower());
		}
	}
}
