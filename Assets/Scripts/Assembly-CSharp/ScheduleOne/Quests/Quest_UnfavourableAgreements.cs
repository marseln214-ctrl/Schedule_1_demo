using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.Misc;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Quests
{
	public class Quest_UnfavourableAgreements : Quest
	{
		public const float WEEKLY_DELIVERY_HOURS = 168f;

		public const float REMINDER_THRESHOLD = 144f;

		public Thomas Thomas;

		public ManorGate Gate;

		public ModularSwitch Switch;

		public RV RV;

		public string QuestEntryTitle;

		private bool handoverSetup;

		protected override void Start()
		{
			base.Start();
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onHourPass = (Action)Delegate.Combine(instance.onHourPass, new Action(HourPass));
			Thomas.onCartelContractReceived.AddListener(HandoverCompleted);
			Singleton<SleepCanvas>.Instance.onSleepEndFade.AddListener(CheckHandoverExpiry);
			UpdateName();
		}

		public override void Begin(bool network = true)
		{
			base.Begin(network);
			ResetTimer(allowBuildup: false);
		}

		private void HourPass()
		{
			float num = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Hours_Since_Cartel_Handover");
			float num2 = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Hours_Until_CartelContract_Due");
			if (Entries[0].State == EQuestState.Active)
			{
				num += 1f;
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Hours_Since_Cartel_Handover", num.ToString());
				num2 -= 1f;
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Hours_Until_CartelContract_Due", num2.ToString());
				UpdateName();
			}
			if (!handoverSetup && num >= 12f)
			{
				SetupHandover();
			}
			if (!Thomas.HandoverReminderSent && num2 <= 24f)
			{
				Thomas.SendHandoverReminder();
			}
		}

		private void SetupHandover()
		{
			handoverSetup = true;
			Debug.Log("Setting up handover");
			Gate.ActivateIntercom();
			Switch.SwitchOn();
			Thomas.SetHandoverEventActive(active: true);
		}

		private void CheckHandoverExpiry()
		{
			if (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Hours_Until_CartelContract_Due") <= 0f)
			{
				Singleton<SleepCanvas>.Instance.QueueSleepMessage("You have failed to make the weekly delivery. Benzies family goons break in during the night, taking your stock and leaving you nearly dead.", 5f);
				RV.Ransack();
				ResetTimer(allowBuildup: false);
				Player.Local.Health.SetHealth(65f);
			}
		}

		private void UpdateName()
		{
			int num = Mathf.FloorToInt(NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Hours_Until_CartelContract_Due") / 24f);
			string text = num switch
			{
				-1 => string.Empty, 
				0 => "(due today)", 
				1 => "(" + num + " day)", 
				_ => "(" + num + " days)", 
			};
			Entries[0].SetEntryTitle(QuestEntryTitle + " " + text);
		}

		private void HandoverCompleted()
		{
			ResetTimer(allowBuildup: true);
		}

		public void ResetTimer(bool allowBuildup)
		{
			float num = Mathf.Floor((float)TimeManager.GetMinSumFrom24HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime) / 60f);
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Hours_Since_Cartel_Handover", num.ToString());
			float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Hours_Until_CartelContract_Due");
			float num2 = 168f;
			if (allowBuildup)
			{
				num2 += value;
			}
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Hours_Until_CartelContract_Due", num2.ToString());
			UpdateName();
		}
	}
}
