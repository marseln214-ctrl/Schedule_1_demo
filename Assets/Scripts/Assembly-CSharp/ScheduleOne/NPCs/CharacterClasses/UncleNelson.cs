using System.Collections.Generic;
using ScheduleOne.UI.Phone.Messages;

namespace ScheduleOne.NPCs.CharacterClasses
{
	public class UncleNelson : NPC
	{
		public string InitialMessage_Demo = "I’ve heard you’re in some trouble. Best not to talk over your mobile phone. Go find a payphone.\n- U.N.";

		public string InitialMessage = "You get out alright? Best not to talk over your mobile phone. Go find a payphone.\n- U.N.";

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EUncleNelsonAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EUncleNelsonAssembly_002DCSharp_002Edll_Excuted;

		public void SendInitialMessage()
		{
			if (base.MSGConversation.messageChainHistory.Count <= 0 && base.MSGConversation.messageHistory.Count <= 0)
			{
				base.MSGConversation.SetIsKnown(known: false);
				base.MSGConversation.SendMessageChain(new MessageChain
				{
					Messages = new List<string> { InitialMessage_Demo }
				}, 0f, notify: false);
				base.MSGConversation.SetRead(r: false);
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EUncleNelsonAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EUncleNelsonAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EUncleNelsonAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EUncleNelsonAssembly_002DCSharp_002Edll_Excuted = true;
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
