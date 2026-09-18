using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Storage
{
	public class Safe : StorageEntity
	{
		private bool NetworkInitialize___EarlyScheduleOne_002EStorage_002ESafeAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EStorage_002ESafeAssembly_002DCSharp_002Edll_Excuted;

		public float GetCash()
		{
			float num = 0f;
			for (int i = 0; i < base.ItemSlots.Count; i++)
			{
				if (base.ItemSlots[i].ItemInstance != null && base.ItemSlots[i].ItemInstance is CashInstance)
				{
					CashInstance cashInstance = base.ItemSlots[i].ItemInstance as CashInstance;
					num += cashInstance.Balance;
				}
			}
			return num;
		}

		public void RemoveCash(float amount)
		{
			amount = Mathf.Abs(amount);
			float num = amount;
			for (int i = 0; i < base.ItemSlots.Count; i++)
			{
				if (base.ItemSlots[i].ItemInstance != null && base.ItemSlots[i].ItemInstance is CashInstance)
				{
					CashInstance obj = base.ItemSlots[i].ItemInstance as CashInstance;
					float num2 = Mathf.Min(obj.Balance, num);
					obj.ChangeBalance(0f - num2);
					num -= num2;
				}
				if (num <= 0f)
				{
					break;
				}
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EStorage_002ESafeAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EStorage_002ESafeAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EStorage_002ESafeAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EStorage_002ESafeAssembly_002DCSharp_002Edll_Excuted = true;
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
