using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Equipping
{
	public class Equippable : MonoBehaviour
	{
		protected ItemInstance itemInstance;

		public bool CanInteractWhenEquipped = true;

		public virtual void Equip(ItemInstance item)
		{
			itemInstance = item;
			PlayerSingleton<PlayerInventory>.Instance.SetEquippable(this);
		}

		public virtual void Unequip()
		{
			PlayerSingleton<PlayerInventory>.Instance.SetEquippable(null);
			Object.Destroy(base.gameObject);
		}
	}
}
