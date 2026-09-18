using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;

namespace ScheduleOne.Money
{
	public class CashSlot : HotbarSlot
	{
		public const float MAX_CASH_PER_SLOT = 1000f;

		public override void ClearStoredInstance(bool _internal = false)
		{
			(base.ItemInstance as CashInstance).SetBalance(0f, blockClear: true);
		}

		public override bool CanSlotAcceptCash()
		{
			return true;
		}
	}
}
