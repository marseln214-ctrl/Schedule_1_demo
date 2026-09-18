using System;
using System.Linq;
using FishNet.Serializing.Helping;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.UI;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Phone;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Quests
{
	[Serializable]
	public class QuestEntry : MonoBehaviour
	{
		[Header("Naming")]
		[SerializeField]
		protected string EntryTitle = string.Empty;

		[SerializeField]
		protected EQuestState state;

		[Header("Settings")]
		public bool AutoComplete;

		public Conditions AutoCompleteConditions;

		public bool CompleteParentQuest;

		public string EntryAddedIn = "0.0.1";

		[Header("PoI Settings")]
		public bool AutoCreatePoI = true;

		public Transform PoILocation;

		public bool AutoUpdatePoILocation;

		public POI PoI;

		public UnityEvent onStart = new UnityEvent();

		public UnityEvent onEnd = new UnityEvent();

		public UnityEvent onComplete = new UnityEvent();

		public UnityEvent onInitialComplete = new UnityEvent();

		private CompassManager.Element compassElement;

		private QuestEntryHUDUI entryUI;

		[CodegenExclude]
		[field: NonSerialized]
		public Quest ParentQuest { get; private set; }

		[CodegenExclude]
		public string Title => EntryTitle;

		[CodegenExclude]
		public EQuestState State => state;

		protected virtual void Awake()
		{
			ParentQuest = GetComponentInParent<Quest>();
			ParentQuest.onQuestEnd.AddListener(delegate
			{
				DestroyPoI();
			});
			ParentQuest.onTrackChange.AddListener(delegate
			{
				UpdatePoI();
			});
			if (AutoComplete)
			{
				StateMachine.OnStateChange = (Action)Delegate.Combine(StateMachine.OnStateChange, new Action(EvaluateConditions));
			}
		}

		protected virtual void Start()
		{
			if (AutoCreatePoI && PoI == null)
			{
				CreatePoI();
			}
			if (!ParentQuest.Entries.Contains(this))
			{
				Console.LogError("Parent quest '" + ParentQuest.GetQuestTitle() + "' does not contain entry '" + EntryTitle + "'.");
			}
			if (ParentQuest.hudUIExists)
			{
				CreateEntryUI();
			}
			else
			{
				Quest parentQuest = ParentQuest;
				parentQuest.onHudUICreated = (Action)Delegate.Combine(parentQuest.onHudUICreated, new Action(CreateEntryUI));
			}
			CreateCompassElement();
		}

		private void OnValidate()
		{
			UpdateName();
			if (EntryAddedIn == null || EntryAddedIn == string.Empty)
			{
				EntryAddedIn = Application.version;
			}
		}

		public virtual void MinPass()
		{
			if (AutoUpdatePoILocation && PoI != null)
			{
				PoI.transform.position = PoILocation.position;
				PoI.UpdatePosition();
			}
		}

		public void SetData(QuestEntryData data)
		{
			EntryTitle = data.Name;
			SetState(data.State, network: false);
		}

		public void Begin()
		{
			SetState(EQuestState.Active);
		}

		public void Complete()
		{
			SetState(EQuestState.Completed);
		}

		public void SetActive(bool network = true)
		{
			SetState(EQuestState.Active, network);
		}

		public virtual void SetState(EQuestState newState, bool network = true)
		{
			EQuestState eQuestState = state;
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			state = newState;
			if (newState == EQuestState.Active && eQuestState != EQuestState.Active)
			{
				if (onStart != null)
				{
					onStart.Invoke();
				}
				TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
				instance2.onMinutePass = (Action)Delegate.Combine(instance2.onMinutePass, new Action(MinPass));
			}
			if (newState != EQuestState.Active && eQuestState == EQuestState.Active && onEnd != null)
			{
				onEnd.Invoke();
			}
			if (newState == EQuestState.Completed && eQuestState != EQuestState.Completed)
			{
				if (onComplete != null)
				{
					onComplete.Invoke();
				}
				if (eQuestState == EQuestState.Active)
				{
					if (onInitialComplete != null)
					{
						onInitialComplete.Invoke();
					}
					NetworkSingleton<QuestManager>.Instance.PlayCompleteQuestEntrySound();
				}
				if (CompleteParentQuest)
				{
					ParentQuest.Complete(network);
				}
			}
			if (PoI != null)
			{
				PoI.gameObject.SetActive(ShouldShowPoI());
			}
			ParentQuest.UpdateHUDUI();
			UpdateCompassElement();
			if (network)
			{
				int entryIndex = ParentQuest.Entries.ToList().IndexOf(this);
				NetworkSingleton<QuestManager>.Instance.SendQuestEntryState(ParentQuest.GUID.ToString(), entryIndex, newState);
			}
			UpdateName();
			StateMachine.ChangeState();
		}

		protected virtual bool ShouldShowPoI()
		{
			if (State == EQuestState.Active)
			{
				return ParentQuest.IsTracked;
			}
			return false;
		}

		protected virtual void UpdatePoI()
		{
			if (PoI != null)
			{
				PoI.gameObject.SetActive(ShouldShowPoI());
			}
		}

		public void SetPoILocation(Vector3 location)
		{
			PoILocation.position = location;
			if (PoI != null)
			{
				PoI.transform.position = location;
				PoI.UpdatePosition();
			}
		}

		public void CreatePoI()
		{
			if (PoI != null)
			{
				Console.LogWarning("PoI already exists for quest entry " + EntryTitle);
				return;
			}
			PoI = UnityEngine.Object.Instantiate(ParentQuest.PoIPrefab, base.transform).GetComponent<POI>();
			PoI.transform.position = PoILocation.position;
			PoI.SetMainText(Title);
			PoI.UpdatePosition();
			PoI.gameObject.SetActive(ShouldShowPoI());
			if (PoI.IconContainer != null)
			{
				CreateUI();
			}
			else
			{
				PoI.onUICreated.AddListener(CreateUI);
			}
			void CreateUI()
			{
				UnityEngine.Object.Instantiate(ParentQuest.IconPrefab.gameObject, PoI.IconContainer).GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 20f);
			}
		}

		public void DestroyPoI()
		{
			if (PoI != null)
			{
				UnityEngine.Object.Destroy(PoI.gameObject);
				PoI = null;
			}
		}

		public void CreateCompassElement()
		{
			if (compassElement != null)
			{
				Console.LogWarning("Compass element already exists for quest: " + Title);
				return;
			}
			compassElement = Singleton<CompassManager>.Instance.AddElement(PoILocation, ParentQuest.IconPrefab, state == EQuestState.Active);
			UpdateCompassElement();
		}

		public void UpdateCompassElement()
		{
			if (compassElement != null)
			{
				compassElement.Transform = PoILocation;
				compassElement.Visible = ParentQuest.QuestState == EQuestState.Active && ParentQuest.IsTracked && state == EQuestState.Active && PoILocation != null;
			}
		}

		public QuestEntryData GetSaveData()
		{
			return new QuestEntryData(EntryTitle, state);
		}

		private void UpdateName()
		{
			base.name = EntryTitle + " (" + state.ToString() + ")";
		}

		private void EvaluateConditions()
		{
			if (State == EQuestState.Active && AutoCompleteConditions.Evaluate())
			{
				SetState(EQuestState.Completed);
			}
		}

		public void SetEntryTitle(string newTitle)
		{
			EntryTitle = newTitle;
			ParentQuest.UpdateHUDUI();
		}

		protected virtual void CreateEntryUI()
		{
			if (!ParentQuest.hudUIExists)
			{
				Console.LogWarning("Quest HUD UI does not exist for quest " + ParentQuest.GetQuestTitle());
				return;
			}
			entryUI = UnityEngine.Object.Instantiate(PlayerSingleton<JournalApp>.Instance.QuestEntryHUDUIPrefab, ParentQuest.hudUI.EntryContainer).GetComponent<QuestEntryHUDUI>();
			entryUI.Initialize(this);
			UpdateEntryUI();
		}

		public virtual void UpdateEntryUI()
		{
			entryUI.UpdateUI();
		}
	}
}
