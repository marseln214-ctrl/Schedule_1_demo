using System;
using System.Collections;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.Misc;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.PlayerScripts;
using ScheduleOne.ScriptableObjects;
using ScheduleOne.UI;
using ScheduleOne.UI.Phone;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Quests
{
	public class Quest_TheDeepEnd : Quest
	{
		public const float MEETING_REMINDER_TIME = 36f;

		public const float KIDNAP_TIME = 82f;

		private bool kidnapQueued;

		private bool meetingSetup;

		public Thomas Thomas;

		public ManorGate Gate;

		public ModularSwitch Switch;

		public Transform MeetingTeleportPoint;

		public PhoneCallData PostMeetingCall;

		public SystemTriggerObject PostMeetingTrigger;

		protected override void Start()
		{
			base.Start();
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onHourPass = (Action)Delegate.Combine(instance.onHourPass, new Action(HourPass));
			TimeManager.onSleepStart = (Action)Delegate.Combine(TimeManager.onSleepStart, new Action(BeforeSleep));
			Singleton<SleepCanvas>.Instance.onSleepEndFade.AddListener(SleepFadeOut);
		}

		public override void Begin(bool network = true)
		{
			base.Begin(network);
			SetupFirstMeeting();
		}

		public void SetupFirstMeeting()
		{
			meetingSetup = true;
			Gate.ActivateIntercom();
			Switch.SwitchOn();
			Thomas.SetFirstMeetingEventActive(active: true);
			Thomas.dialogueHandler.onDialogueNodeDisplayed.AddListener(ThomasDialogueNodeDisplayed);
		}

		private void ThomasDialogueNodeDisplayed(string nodeLabel)
		{
			if (nodeLabel == "THOMAS_INTRO_DONE")
			{
				Debug.Log("Intro meeting done!");
				Gate.SetEnterable(enterable: false);
				Thomas.InitialMeetingComplete();
				Entries[0].SetState(EQuestState.Completed);
				Entries[1].SetState(EQuestState.Active);
				PostMeetingTrigger.Trigger();
				StartCoroutine(Wait());
			}
			IEnumerator Wait()
			{
				yield return new WaitUntil(() => Player.Local.CurrentProperty == null);
				Singleton<CallInterface>.Instance.StartCall(PostMeetingCall, PostMeetingCall.CallerID);
			}
		}

		private void HourPass()
		{
			if (!InstanceFinder.IsServer || Quest.GetQuest("Sink or Swim").QuestState != EQuestState.Completed)
			{
				return;
			}
			float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Hours_Since_LoanSharks_Arrived");
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Hours_Since_LoanSharks_Arrived", (value + 1f).ToString());
			if (Entries[0].State != EQuestState.Completed && value >= 36f && !Thomas.MeetingReminderSent)
			{
				Thomas.SendMeetingReminder();
				if (base.QuestState == EQuestState.Inactive)
				{
					Begin();
				}
			}
			if (Entries[0].State == EQuestState.Active && value >= 82f && !kidnapQueued)
			{
				kidnapQueued = true;
			}
		}

		private void BeforeSleep()
		{
			if (kidnapQueued)
			{
				Singleton<SleepCanvas>.Instance.QueueSleepMessage("In the middle of the night, the door is kicked in and you are dragged into a vehicle trunk...");
			}
		}

		private void SleepFadeOut()
		{
			if (kidnapQueued)
			{
				kidnapQueued = false;
				PlayerSingleton<PlayerMovement>.Instance.Teleport(MeetingTeleportPoint.position);
				Player.Local.transform.forward = MeetingTeleportPoint.forward;
			}
		}

		public override void SetQuestEntryState(int entryIndex, EQuestState state, bool network = true)
		{
			base.SetQuestEntryState(entryIndex, state, network);
			if (Entries[0].State == EQuestState.Active && !meetingSetup)
			{
				SetupFirstMeeting();
			}
		}
	}
}
