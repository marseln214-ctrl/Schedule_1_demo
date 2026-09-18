using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.UI.Phone;
using ScheduleOne.Variables;

namespace ScheduleOne.NPCs.CharacterClasses
{
	public class Shirley : Supplier
	{
		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EShirleyAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EShirleyAssembly_002DCSharp_002Edll_Excuted;

		protected override void DeaddropConfirmed(List<PhoneShopInterface.CartEntry> cart, float totalPrice)
		{
			base.DeaddropConfirmed(cart, totalPrice);
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ShirleyDeaddropOrders", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("ShirleyDeaddropOrders") + 1f).ToString());
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EShirleyAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EShirleyAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EShirleyAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EShirleyAssembly_002DCSharp_002Edll_Excuted = true;
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
