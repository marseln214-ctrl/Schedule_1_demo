using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Persistence;
using ScheduleOne.Variables;

namespace ScheduleOne.NPCs
{
	public class Stan : NPC
	{
		public DialogueContainer GreetingDialogue;

		public string GreetedVariable = "StanGreeted";

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted;

		protected override void Start()
		{
			base.Start();
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(Loaded);
		}

		private void Loaded()
		{
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(Loaded);
			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>(GreetedVariable))
			{
				EnableGreeting();
			}
		}

		private void EnableGreeting()
		{
			dialogueHandler.GetComponent<DialogueController>().OverrideContainer = GreetingDialogue;
			dialogueHandler.onConversationStart.AddListener(SetGreeted);
		}

		private void SetGreeted()
		{
			dialogueHandler.onConversationStart.RemoveListener(SetGreeted);
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(GreetedVariable, true.ToString());
			dialogueHandler.GetComponent<DialogueController>().OverrideContainer = null;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted = true;
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
