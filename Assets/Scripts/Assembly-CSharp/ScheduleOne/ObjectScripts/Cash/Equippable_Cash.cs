using System;
using System.Collections.Generic;
using ScheduleOne.Building;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.ObjectScripts.Cash
{
	public class Equippable_Cash : Equippable_Viewmodel
	{
		private int amountIndex;

		[Header("References")]
		public Transform Container_Under100;

		public List<Transform> SingleNotes;

		public Transform Container_100_300;

		public List<Transform> Under300Stacks;

		public Transform Container_300Plus;

		public List<Transform> PlusStacks;

		protected override void Update()
		{
			base.Update();
			if (!lookingAtStorageObject && GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
			{
				amountIndex++;
			}
		}

		protected override void StartBuildingStoredItem()
		{
			isBuildingStoredItem = true;
			Singleton<BuildManager>.Instance.StartPlacingCash(itemInstance);
		}

		protected override void StopBuildingStoredItem()
		{
			isBuildingStoredItem = false;
			Singleton<BuildManager>.Instance.StopBuilding();
		}

		public override void Equip(ItemInstance item)
		{
			base.Equip(item);
			item.onDataChanged = (Action)Delegate.Combine(item.onDataChanged, new Action(UpdateCashVisuals));
			UpdateCashVisuals();
		}

		public override void Unequip()
		{
			base.Unequip();
			ItemInstance obj = itemInstance;
			obj.onDataChanged = (Action)Delegate.Remove(obj.onDataChanged, new Action(UpdateCashVisuals));
		}

		private void UpdateCashVisuals()
		{
			if (!(itemInstance is CashInstance { Balance: var balance }))
			{
				Container_100_300.gameObject.SetActive(value: false);
				Container_300Plus.gameObject.SetActive(value: false);
				Container_Under100.gameObject.SetActive(value: false);
				return;
			}
			float num;
			if (balance < 100f)
			{
				num = Mathf.Round(balance / 10f) * 10f;
				int num2 = Mathf.Clamp(Mathf.RoundToInt(num / 10f), 0, 10);
				if (num > 0f)
				{
					num2 = Mathf.Max(1, num2);
				}
				Container_100_300.gameObject.SetActive(value: false);
				Container_300Plus.gameObject.SetActive(value: false);
				Container_Under100.gameObject.SetActive(value: true);
				for (int i = 0; i < SingleNotes.Count; i++)
				{
					if (i < num2)
					{
						SingleNotes[i].gameObject.SetActive(value: true);
					}
					else
					{
						SingleNotes[i].gameObject.SetActive(value: false);
					}
				}
				return;
			}
			num = Mathf.Floor(balance / 100f) * 100f;
			Container_Under100.gameObject.SetActive(value: false);
			if (num < 400f)
			{
				Container_300Plus.gameObject.SetActive(value: false);
				Container_100_300.gameObject.SetActive(value: true);
				for (int j = 0; j < Under300Stacks.Count; j++)
				{
					if ((float)j < num / 100f)
					{
						Under300Stacks[j].gameObject.SetActive(value: true);
					}
					else
					{
						Under300Stacks[j].gameObject.SetActive(value: false);
					}
				}
				return;
			}
			Container_100_300.gameObject.SetActive(value: false);
			Container_300Plus.gameObject.SetActive(value: true);
			for (int k = 0; k < PlusStacks.Count; k++)
			{
				if ((float)k < num / 100f)
				{
					PlusStacks[k].gameObject.SetActive(value: true);
				}
				else
				{
					PlusStacks[k].gameObject.SetActive(value: false);
				}
			}
		}
	}
}
