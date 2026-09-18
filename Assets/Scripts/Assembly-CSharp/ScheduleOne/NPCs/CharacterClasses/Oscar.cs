using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Persistence;
using ScheduleOne.UI.Shop;
using ScheduleOne.Variables;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.CharacterClasses
{
	public class Oscar : NPC
	{
		public ShopInterface ShopInterface;

		[Header("Settings")]
		public string[] OrderCompletedLines;

		public DialogueContainer GreetingDialogue;

		public string GreetedVariable = "OscarGreeted";

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EOscarAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EOscarAssembly_002DCSharp_002Edll_Excuted;

		protected override void Start()
		{
			base.Start();
			ShopInterface.onOrderCompleted.AddListener(OrderCompleted);
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(Loaded);
		}

		private void OrderCompleted()
		{
			PlayVO(EVOLineType.Thanks);
			dialogueHandler.ShowWorldspaceDialogue(OrderCompletedLines[Random.Range(0, OrderCompletedLines.Length)], 5f);
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
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EOscarAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EOscarAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EOscarAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EOscarAssembly_002DCSharp_002Edll_Excuted = true;
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
