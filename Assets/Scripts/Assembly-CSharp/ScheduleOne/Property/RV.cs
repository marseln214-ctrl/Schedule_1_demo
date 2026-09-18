using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Product;
using ScheduleOne.Storage;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Property
{
	public class RV : Property
	{
		public Transform ModelContainer;

		public UnityEvent onSetExploded;

		private bool NetworkInitialize___EarlyScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted;

		protected override void Start()
		{
			base.Start();
			InvokeRepeating("UpdateVariables", 0f, 0.5f);
		}

		private void UpdateVariables()
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			Pot[] componentsInChildren = Container.GetComponentsInChildren<Pot>();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].IsFilledWithSoil)
				{
					num++;
				}
				if (componentsInChildren[i].NormalizedWaterLevel > 0.9f)
				{
					num2++;
				}
				if (componentsInChildren[i].Plant != null)
				{
					num3++;
				}
				if ((bool)componentsInChildren[i].AppliedAdditives.Find((Additive x) => x.AdditiveName == "Speed Grow"))
				{
					num4++;
				}
			}
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("RV_Soil_Pots", num.ToString());
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("RV_Watered_Pots", num2.ToString());
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("RV_Seed_Pots", num3.ToString());
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("RV_SpeedGrow_Pots", num4.ToString());
		}

		public void Ransack()
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			Debug.Log("Ransacking RV");
			foreach (BuildableItem buildableItem in BuildableItems)
			{
				IItemSlotOwner itemSlotOwner = null;
				if (buildableItem is IItemSlotOwner)
				{
					itemSlotOwner = buildableItem as IItemSlotOwner;
				}
				else
				{
					StorageEntity component = buildableItem.GetComponent<StorageEntity>();
					if (component != null)
					{
						itemSlotOwner = component;
					}
				}
				if (itemSlotOwner == null)
				{
					continue;
				}
				for (int i = 0; i < itemSlotOwner.ItemSlots.Count; i++)
				{
					if (itemSlotOwner.ItemSlots[i].ItemInstance != null && itemSlotOwner.ItemSlots[i].ItemInstance is ProductItemInstance)
					{
						itemSlotOwner.ItemSlots[i].SetQuantity(0);
					}
				}
			}
		}

		public void SetExploded()
		{
			if (onSetExploded != null)
			{
				onSetExploded.Invoke();
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted = true;
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
