using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Money;
using ScheduleOne.Property;
using ScheduleOne.Property.Utilities.Power;
using ScheduleOne.Property.Utilities.Water;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone
{
	public class UtilitiesApp : App<UtilitiesApp>
	{
		[Header("References")]
		[SerializeField]
		protected Text water_Usage;

		[SerializeField]
		protected Text water_Cost;

		[SerializeField]
		protected Text water_Total;

		[SerializeField]
		protected Text electricity_Usage;

		[SerializeField]
		protected Text electricity_Cost;

		[SerializeField]
		protected Text electricity_Total;

		[SerializeField]
		protected Text dumpster_Count;

		[SerializeField]
		protected Text dumpster_EmptyCost;

		[SerializeField]
		protected Text dumpster_Total;

		[SerializeField]
		protected Button dumpsterButton;

		[SerializeField]
		protected PropertyDropdown propertySelector;

		private ScheduleOne.Property.Property selectedProperty;

		protected override void Awake()
		{
			base.Awake();
			water_Cost.text = "Cost per litre: $" + WaterManager.pricePerL;
			electricity_Cost.text = "Cost per kWh $" + PowerManager.pricePerkWh;
			TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
			timeManager.onMinutePass = (Action)Delegate.Combine(timeManager.onMinutePass, new Action(RefreshShownValues));
			TimeManager timeManager2 = NetworkSingleton<TimeManager>.Instance;
			timeManager2.onDayPass = (Action)Delegate.Combine(timeManager2.onDayPass, new Action(OnDayPass));
			PropertyDropdown propertyDropdown = propertySelector;
			propertyDropdown.onSelectionChanged = (Action)Delegate.Combine(propertyDropdown.onSelectionChanged, new Action(RefreshShownValues));
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
			timeManager.onMinutePass = (Action)Delegate.Remove(timeManager.onMinutePass, new Action(RefreshShownValues));
			TimeManager timeManager2 = NetworkSingleton<TimeManager>.Instance;
			timeManager2.onDayPass = (Action)Delegate.Remove(timeManager2.onDayPass, new Action(OnDayPass));
		}

		protected override void Update()
		{
			base.Update();
			if (base.isOpen)
			{
				selectedProperty = propertySelector.selectedProperty;
			}
		}

		protected virtual void RefreshShownValues()
		{
			if (base.isOpen)
			{
				selectedProperty = propertySelector.selectedProperty;
				water_Usage.text = "Water usage: " + Round(Singleton<WaterManager>.Instance.GetTotalUsage(), 1f) + " litres";
				water_Total.text = "Total cost: " + MoneyManager.FormatAmount(Round(Singleton<WaterManager>.Instance.GetTotalUsage() * WaterManager.pricePerL, 2f), showDecimals: true);
				electricity_Usage.text = "Electricity usage: " + Round(Singleton<PowerManager>.Instance.GetTotalUsage(), 2f) + " kWh";
				electricity_Total.text = "Total cost: " + MoneyManager.FormatAmount(Round(Singleton<PowerManager>.Instance.GetTotalUsage() * PowerManager.pricePerkWh, 2f), showDecimals: true);
			}
		}

		protected virtual void OnDayPass()
		{
		}

		private float Round(float n, float decimals)
		{
			return Mathf.Round(n * Mathf.Pow(10f, decimals)) / Mathf.Pow(10f, decimals);
		}

		public override void SetOpen(bool open)
		{
			base.SetOpen(open);
			if (open)
			{
				RefreshShownValues();
			}
		}
	}
}
