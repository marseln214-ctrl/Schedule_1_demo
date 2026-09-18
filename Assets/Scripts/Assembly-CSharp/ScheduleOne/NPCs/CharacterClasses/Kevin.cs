using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.Persistence;
using ScheduleOne.Variables;

namespace ScheduleOne.NPCs.CharacterClasses
{
	public class Kevin : NPC
	{
		private bool offerSent;

		private Customer customer;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EKevinAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EKevinAssembly_002DCSharp_002Edll_Excuted;

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002ECharacterClasses_002EKevin_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		private void Loaded()
		{
			offerSent = NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("Kevin_First_Contract_Sent");
		}

		protected override void MinPass()
		{
			base.MinPass();
			_ = Singleton<LoadManager>.Instance.IsGameLoaded;
		}

		private void SendFirstOffer()
		{
			Console.Log("Sending first offer from Kevin");
			if (!RelationData.Unlocked)
			{
				RelationData.Unlock(NPCRelationData.EUnlockType.Recommendation, notify: false);
			}
			offerSent = true;
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Kevin_First_Contract_Sent", true.ToString());
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EKevinAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EKevinAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EKevinAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EKevinAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ECharacterClasses_002EKevin_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			customer = GetComponent<Customer>();
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(Loaded);
			RelationData.SetRelationship(2f);
		}
	}
}
