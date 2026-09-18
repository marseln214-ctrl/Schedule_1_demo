using ScheduleOne.Economy;
using UnityEngine;

namespace ScheduleOne.Doors
{
	public class DealerStaticDoor : StaticDoor
	{
		public Dealer Dealer;

		protected override bool IsKnockValid(out string message)
		{
			if (Building.OccupantCount == 0 && Vector3.Distance(base.transform.position, Dealer.transform.position) > 2f)
			{
				message = Dealer.FirstName + " is out dealing";
				return false;
			}
			return base.IsKnockValid(out message);
		}
	}
}
