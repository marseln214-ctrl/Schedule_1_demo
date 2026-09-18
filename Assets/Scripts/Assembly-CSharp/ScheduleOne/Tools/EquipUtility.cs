using EasyButtons;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Equipping;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class EquipUtility : MonoBehaviour
	{
		public AvatarEquippable Equippable;

		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.Q))
			{
				Equip();
			}
		}

		[Button]
		public void Equip()
		{
			GetComponent<ScheduleOne.AvatarFramework.Avatar>().SetEquippable(Equippable.AssetPath);
		}

		[Button]
		public void Unequip()
		{
			GetComponent<ScheduleOne.AvatarFramework.Avatar>().SetEquippable(string.Empty);
		}
	}
}
