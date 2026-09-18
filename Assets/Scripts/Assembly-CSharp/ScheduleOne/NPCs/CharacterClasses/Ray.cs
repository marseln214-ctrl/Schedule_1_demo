using System;
using System.Collections.Generic;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Levelling;
using ScheduleOne.Money;
using ScheduleOne.Property;
using ScheduleOne.UI.Phone.Messages;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.NPCs.CharacterClasses
{
	public class Ray : NPC
	{
		public string IntroductionMessage;

		public string IntroSentVariable = "RayIntroSent";

		[Header("Intro message conditions")]
		public FullRank IntroRank;

		public int IntroDaysPlayed = 21;

		public float IntroNetworth = 15000f;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted;

		protected override void Start()
		{
			base.Start();
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onHourPass = (Action)Delegate.Combine(instance.onHourPass, new Action(HourPass));
		}

		public void SendIntroductionMessage()
		{
			if (InstanceFinder.IsServer)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(IntroSentVariable, true.ToString());
				base.MSGConversation.SendMessageChain(new MessageChain
				{
					Messages = new List<string> { IntroductionMessage }
				});
			}
		}

		private void HourPass()
		{
			if (InstanceFinder.IsServer)
			{
				CheckSendMessage();
			}
		}

		private void CheckSendMessage()
		{
			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>(IntroSentVariable) && NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(900, 1800) && ScheduleOne.Property.Property.OwnedProperties.Count <= 3 && Business.OwnedBusinesses.Count <= 0)
			{
				if (NetworkSingleton<LevelManager>.Instance.GetFullRank() >= IntroRank)
				{
					SendIntroductionMessage();
				}
				else if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.ElapsedDays >= IntroDaysPlayed)
				{
					SendIntroductionMessage();
				}
				else if (NetworkSingleton<MoneyManager>.Instance.LastCalculatedNetworth >= IntroNetworth)
				{
					SendIntroductionMessage();
				}
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
