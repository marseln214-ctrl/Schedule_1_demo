using System;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Law;
using ScheduleOne.NPCs.Schedules;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.NPCs
{
	public class NPCScheduleManager : MonoBehaviour
	{
		public bool DEBUG_MODE;

		[Header("References")]
		public GameObject[] EnabledDuringCurfew;

		public GameObject[] EnabledDuringNoCurfew;

		public List<NPCAction> ActionList = new List<NPCAction>();

		protected int lastProcessedTime;

		public bool ScheduleEnabled { get; protected set; }

		public bool CurfewModeEnabled { get; protected set; }

		public NPCAction ActiveAction { get; set; }

		public List<NPCAction> PendingActions { get; set; } = new List<NPCAction>();


		public NPC Npc { get; protected set; }

		protected List<NPCAction> ActionsAwaitingStart { get; set; } = new List<NPCAction>();


		protected TimeManager Time => NetworkSingleton<TimeManager>.Instance;

		protected virtual void Awake()
		{
			Npc = GetComponentInParent<NPC>();
			SetCurfewModeEnabled(enabled: false);
		}

		protected virtual void Start()
		{
			InitializeActions();
			TimeManager time = Time;
			time.onTimeChanged = (Action)Delegate.Remove(time.onTimeChanged, new Action(EnforceState));
			TimeManager time2 = Time;
			time2.onTimeChanged = (Action)Delegate.Combine(time2.onTimeChanged, new Action(EnforceState));
			TimeManager time3 = Time;
			time3.onMinutePass = (Action)Delegate.Remove(time3.onMinutePass, new Action(MinPass));
			TimeManager time4 = Time;
			time4.onMinutePass = (Action)Delegate.Combine(time4.onMinutePass, new Action(MinPass));
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(LocalPlayerSpawned));
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(LocalPlayerSpawned));
			NetworkSingleton<CurfewManager>.Instance.onCurfewEnabled.AddListener(CurfewEnabled);
			NetworkSingleton<CurfewManager>.Instance.onCurfewDisabled.AddListener(CurfewDisabled);
			if (DEBUG_MODE)
			{
				int min = 1250;
				int max = 930;
				GetActionsTotallyOccurringWithinRange(min, max, checkShouldStart: true);
			}
		}

		private void LocalPlayerSpawned()
		{
			if (InstanceFinder.IsServer)
			{
				EnforceState(initial: true);
			}
		}

		private void OnValidate()
		{
			_ = Application.isPlaying;
		}

		protected virtual void Update()
		{
			if (ActiveAction != null)
			{
				ActiveAction.ActiveUpdate();
			}
		}

		public void EnableSchedule()
		{
			ScheduleEnabled = true;
			MinPass();
		}

		public void DisableSchedule()
		{
			ScheduleEnabled = false;
			MinPass();
			if (Npc.Movement.IsMoving)
			{
				Npc.Movement.Stop();
			}
		}

		[Button]
		public void InitializeActions()
		{
			List<NPCAction> list = base.gameObject.GetComponentsInChildren<NPCAction>(includeInactive: true).ToList();
			list.Sort(delegate(NPCAction a, NPCAction b)
			{
				float num = a.StartTime;
				float value = b.StartTime;
				int num2 = num.CompareTo(value);
				return (num2 == 0) ? ((!a.IsSignal) ? 1 : (-1)) : num2;
			});
			foreach (NPCAction item in list)
			{
				item.transform.name = item.GetName() + " (" + item.GetTimeDescription() + ")";
				item.transform.SetAsLastSibling();
			}
			ActionList = list;
		}

		protected virtual void MinPass()
		{
			if (!Npc.IsSpawned)
			{
				return;
			}
			if (!ScheduleEnabled)
			{
				if (ActiveAction != null)
				{
					ActiveAction.Interrupt();
				}
				return;
			}
			if (ActiveAction != null)
			{
				ActiveAction.ActiveMinPassed();
			}
			if (ActiveAction != null && !ActiveAction.gameObject.activeInHierarchy)
			{
				ActiveAction.End();
			}
			List<NPCAction> actionsOccurringAt = GetActionsOccurringAt(NetworkSingleton<TimeManager>.Instance.CurrentTime);
			_ = DEBUG_MODE;
			if (actionsOccurringAt.Count > 0)
			{
				NPCAction nPCAction = actionsOccurringAt[0];
				if (ActiveAction != nPCAction)
				{
					if (ActiveAction != null && nPCAction.Priority > ActiveAction.Priority)
					{
						if (DEBUG_MODE)
						{
							Debug.Log("New active action: " + nPCAction.GetName());
						}
						ActiveAction.Interrupt();
					}
					if (ActiveAction == null)
					{
						StartAction(nPCAction);
					}
				}
			}
			foreach (NPCAction item in actionsOccurringAt)
			{
				if (!item.HasStarted && !ActionsAwaitingStart.Contains(item))
				{
					ActionsAwaitingStart.Add(item);
				}
			}
			foreach (NPCAction item2 in ActionsAwaitingStart.ToList())
			{
				if (!NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(item2.StartTime, item2.GetEndTime()))
				{
					item2.Skipped();
					ActionsAwaitingStart.Remove(item2);
				}
			}
			lastProcessedTime = Time.CurrentTime;
			if (DEBUG_MODE)
			{
				Console.Log("Active action: " + ((ActiveAction != null) ? ActiveAction.GetName() : "None"));
			}
		}

		private List<NPCAction> GetActionsOccurringAt(int time)
		{
			List<NPCAction> list = new List<NPCAction>();
			foreach (NPCAction action in ActionList)
			{
				if (!(action == null) && action.ShouldStart() && TimeManager.IsGivenTimeWithinRange(time, action.StartTime, TimeManager.AddMinutesTo24HourTime(action.GetEndTime(), -1)))
				{
					list.Add(action);
				}
			}
			return list.OrderByDescending((NPCAction x) => x.Priority).ToList();
		}

		private List<NPCAction> GetActionsTotallyOccurringWithinRange(int min, int max, bool checkShouldStart)
		{
			List<NPCAction> list = new List<NPCAction>();
			foreach (NPCAction action in ActionList)
			{
				if ((!checkShouldStart || action.ShouldStart()) && TimeManager.IsGivenTimeWithinRange(action.StartTime, min, max) && TimeManager.IsGivenTimeWithinRange(action.GetEndTime(), min, max))
				{
					list.Add(action);
				}
			}
			list = list.OrderByDescending((NPCAction x) => x.Priority).ToList();
			_ = DEBUG_MODE;
			return list;
		}

		private void StartAction(NPCAction action)
		{
			if (ActiveAction != null)
			{
				Console.LogWarning("JumpToAction called but there is already an active action! Existing action should first be ended or interrupted!");
			}
			if (ActionsAwaitingStart.Contains(action))
			{
				ActionsAwaitingStart.Remove(action);
			}
			if (NetworkSingleton<TimeManager>.Instance.CurrentTime == action.StartTime)
			{
				action.Started();
			}
			else if (action.HasStarted)
			{
				action.Resume();
			}
			else
			{
				action.LateStarted();
			}
		}

		private void EnforceState()
		{
			EnforceState(Singleton<LoadManager>.Instance.IsLoading);
		}

		public void EnforceState(bool initial = false)
		{
			ActionsAwaitingStart.Clear();
			int currentTime = NetworkSingleton<TimeManager>.Instance.CurrentTime;
			int minSumFrom24HourTime = TimeManager.GetMinSumFrom24HourTime(currentTime);
			if (DEBUG_MODE)
			{
				Debug.Log("Enforcing state. Last processed time: " + lastProcessedTime + ", Current time: " + NetworkSingleton<TimeManager>.Instance.CurrentTime);
			}
			List<NPCAction> actionsTotallyOccurringWithinRange = GetActionsTotallyOccurringWithinRange(lastProcessedTime, NetworkSingleton<TimeManager>.Instance.CurrentTime, checkShouldStart: true);
			List<NPCAction> actionsOccurringThisFrame = GetActionsOccurringAt(NetworkSingleton<TimeManager>.Instance.CurrentTime);
			actionsTotallyOccurringWithinRange.RemoveAll((NPCAction x) => x.IsActive || actionsOccurringThisFrame.Contains(x));
			NPCAction nPCAction = null;
			if (actionsOccurringThisFrame.Count > 0)
			{
				nPCAction = actionsOccurringThisFrame[0];
			}
			if (ActiveAction != null && ActiveAction != nPCAction)
			{
				ActiveAction.Interrupt();
			}
			Dictionary<NPCAction, float> skippedActionOrder = new Dictionary<NPCAction, float>();
			for (int i = 0; i < actionsTotallyOccurringWithinRange.Count; i++)
			{
				float num = 0f;
				num = ((actionsTotallyOccurringWithinRange[i].StartTime < currentTime) ? (1440f - (float)minSumFrom24HourTime + (float)TimeManager.GetMinSumFrom24HourTime(actionsTotallyOccurringWithinRange[i].StartTime)) : ((float)(TimeManager.GetMinSumFrom24HourTime(actionsTotallyOccurringWithinRange[i].StartTime) - minSumFrom24HourTime)));
				num -= 0.01f * (float)actionsTotallyOccurringWithinRange[i].Priority;
				skippedActionOrder.Add(actionsTotallyOccurringWithinRange[i], num);
			}
			actionsTotallyOccurringWithinRange = actionsTotallyOccurringWithinRange.OrderBy((NPCAction x) => skippedActionOrder[x]).ToList();
			if (DEBUG_MODE)
			{
				Debug.Log("Ordered skipped actions: " + actionsTotallyOccurringWithinRange.Count);
			}
			if (!initial)
			{
				for (int j = 0; j < actionsTotallyOccurringWithinRange.Count; j++)
				{
					actionsTotallyOccurringWithinRange[j].Skipped();
				}
			}
			if (nPCAction != null)
			{
				nPCAction.JumpTo();
			}
		}

		protected virtual void CurfewEnabled()
		{
			SetCurfewModeEnabled(enabled: true);
		}

		protected virtual void CurfewDisabled()
		{
			SetCurfewModeEnabled(enabled: false);
		}

		public void SetCurfewModeEnabled(bool enabled)
		{
			for (int i = 0; i < EnabledDuringCurfew.Length; i++)
			{
				EnabledDuringCurfew[i].gameObject.SetActive(enabled);
			}
			for (int j = 0; j < EnabledDuringNoCurfew.Length; j++)
			{
				EnabledDuringNoCurfew[j].gameObject.SetActive(!enabled);
			}
		}
	}
}
