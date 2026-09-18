using ScheduleOne.UI.Shop;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.CharacterClasses
{
	public class Steve : NPC
	{
		public ShopInterface ShopInterface;

		[Header("Settings")]
		public string[] OrderCompletedLines;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ESteveAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ESteveAssembly_002DCSharp_002Edll_Excuted;

		protected override void Start()
		{
			base.Start();
			ShopInterface.onOrderCompleted.AddListener(OrderCompleted);
		}

		private void OrderCompleted()
		{
			PlayVO(EVOLineType.Thanks);
			dialogueHandler.ShowWorldspaceDialogue(OrderCompletedLines[Random.Range(0, OrderCompletedLines.Length)], 5f);
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ESteveAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ESteveAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ESteveAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ESteveAssembly_002DCSharp_002Edll_Excuted = true;
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
