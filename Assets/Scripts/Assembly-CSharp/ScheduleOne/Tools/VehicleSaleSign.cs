using ScheduleOne.Money;
using ScheduleOne.Vehicles;
using TMPro;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class VehicleSaleSign : MonoBehaviour
	{
		public TextMeshPro NameLabel;

		public TextMeshPro PriceLabel;

		private void Awake()
		{
			LandVehicle componentInParent = GetComponentInParent<LandVehicle>();
			if (componentInParent != null)
			{
				NameLabel.text = componentInParent.VehicleName;
				PriceLabel.text = MoneyManager.FormatAmount(componentInParent.VehiclePrice);
			}
		}
	}
}
