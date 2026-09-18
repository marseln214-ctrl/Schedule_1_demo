using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Shop;
using ScheduleOne.Variables;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.CharacterClasses
{
	public class Dan : NPC
	{
		public ShopInterface ShopInterface;

		[Header("Settings")]
		public string[] OrderCompletedLines;

		public UnityEvent onGreeting;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EDanAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EDanAssembly_002DCSharp_002Edll_Excuted;

		protected override void Start()
		{
			base.Start();
			ShopInterface.onOrderCompleted.AddListener(OrderCompleted);
		}

		private void OrderCompleted()
		{
			if (NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("Dan_Greeting_Done"))
			{
				PlayVO(EVOLineType.Thanks);
				dialogueHandler.ShowWorldspaceDialogue(OrderCompletedLines[Random.Range(0, OrderCompletedLines.Length)], 5f);
				return;
			}
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Dan_Greeting_Done", true.ToString());
			PlayVO(EVOLineType.Question);
			if (onGreeting != null)
			{
				onGreeting.Invoke();
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EDanAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EDanAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EDanAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EDanAssembly_002DCSharp_002Edll_Excuted = true;
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
