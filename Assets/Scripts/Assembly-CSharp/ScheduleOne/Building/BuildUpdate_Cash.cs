using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.ObjectScripts.Cash;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Building
{
	public class BuildUpdate_Cash : BuildUpdate_StoredItem
	{
		public int amountIndex;

		protected List<Transform> bills = new List<Transform>();

		private WorldSpaceLabel amountLabel;

		private float placeAmount => Cash.amounts[amountIndex % Cash.amounts.Length];

		private void Start()
		{
			Transform transform = ghostModel.transform.Find("Bills");
			for (int i = 0; i < transform.childCount; i++)
			{
				bills.Add(transform.GetChild(i));
			}
			RefreshGhostModelAppearance();
			amountLabel = new WorldSpaceLabel("Amount", Vector3.zero);
			amountLabel.scale = 1.25f;
		}

		protected override void Update()
		{
			base.Update();
			if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
			{
				amountIndex++;
				RefreshGhostModelAppearance();
			}
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			if (GetRelevantCashBalane() < placeAmount)
			{
				if (GetRelevantCashBalane() < (float)Cash.amounts[0])
				{
					amountIndex = 0;
					RefreshGhostModelAppearance();
					validPosition = false;
					UpdateMaterials();
					amountLabel.text = "Insufficient cash";
					UpdateLabel();
					return;
				}
				while (GetRelevantCashBalane() < placeAmount)
				{
					amountIndex++;
					RefreshGhostModelAppearance();
				}
			}
			amountLabel.text = MoneyManager.FormatAmount(placeAmount);
			UpdateLabel();
		}

		private void UpdateLabel()
		{
			amountLabel.position = ghostModel.transform.position;
			Vector3 vector = PlayerSingleton<PlayerCamera>.Instance.transform.position - ghostModel.transform.position;
			vector.y = 0f;
			vector.Normalize();
			amountLabel.position += vector * 0.2f;
			if (validPosition)
			{
				amountLabel.color = Color.white;
			}
			else
			{
				amountLabel.color = new Color32(byte.MaxValue, 50, 50, byte.MaxValue);
			}
		}

		private void RefreshGhostModelAppearance()
		{
			int billStacksToDisplay = Cash.GetBillStacksToDisplay(placeAmount);
			for (int i = 0; i < bills.Count; i++)
			{
				if (i < billStacksToDisplay)
				{
					bills[i].gameObject.SetActive(value: true);
				}
				else
				{
					bills[i].gameObject.SetActive(value: false);
				}
			}
		}

		protected override void Place()
		{
			float rotation = Vector3.SignedAngle(bestIntersection.storageTile.ownerGrid.transform.forward, storedItemClass.buildPoint.forward, bestIntersection.storageTile.ownerGrid.transform.up);
			CashInstance cashInstance = new CashInstance(itemInstance.Definition, 1);
			cashInstance.SetBalance(placeAmount);
			Singleton<BuildManager>.Instance.CreateStoredItem(cashInstance, bestIntersection.storageTile.ownerGrid.GetComponentInParent<IStorageEntity>(), bestIntersection.storageTile.ownerGrid, GetOriginCoordinate(), rotation);
			mouseUpSincePlace = false;
			PostPlace();
		}

		protected override void PostPlace()
		{
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - placeAmount);
		}

		public override void Stop()
		{
			base.Stop();
			amountLabel.Destroy();
		}

		public float GetRelevantCashBalane()
		{
			return NetworkSingleton<MoneyManager>.Instance.cashBalance;
		}
	}
}
