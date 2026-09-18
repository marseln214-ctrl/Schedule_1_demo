using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.Modification;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class VehicleModMenu : Singleton<VehicleModMenu>
	{
		public static float repaintCost = 500f;

		[Header("References")]
		[SerializeField]
		protected Canvas canvas;

		[SerializeField]
		protected RectTransform buttonContainer;

		[SerializeField]
		protected RectTransform tempIndicator;

		[SerializeField]
		protected RectTransform permIndicator;

		[SerializeField]
		protected Button confirmButton_Cash;

		[SerializeField]
		protected TextMeshProUGUI confirmText_Cash;

		[SerializeField]
		protected Button confirmButton_Online;

		[SerializeField]
		protected TextMeshProUGUI confirmText_Online;

		[Header("Prefabs")]
		[SerializeField]
		protected GameObject buttonPrefab;

		protected LandVehicle currentVehicle;

		protected List<RectTransform> colorButtons = new List<RectTransform>();

		protected Dictionary<EVehicleColor, RectTransform> colorToButton = new Dictionary<EVehicleColor, RectTransform>();

		protected EVehicleColor selectedColor = EVehicleColor.White;

		public bool isOpen => canvas.enabled;

		protected override void Awake()
		{
			base.Awake();
			confirmText_Cash.text = "Confirm (" + MoneyManager.FormatAmount(repaintCost) + " Cash)";
			confirmText_Online.text = "Confirm (" + MoneyManager.FormatAmount(repaintCost) + " Online)";
		}

		protected override void Start()
		{
			base.Start();
			for (int i = 0; i < Singleton<VehicleColors>.Instance.colorLibrary.Count; i++)
			{
				RectTransform component = Object.Instantiate(buttonPrefab, buttonContainer).GetComponent<RectTransform>();
				component.anchoredPosition = new Vector2((0.5f + (float)colorButtons.Count) * component.sizeDelta.x, component.anchoredPosition.y);
				component.Find("Image").GetComponent<Image>().color = Singleton<VehicleColors>.Instance.colorLibrary[i].UIColor;
				EVehicleColor c = Singleton<VehicleColors>.Instance.colorLibrary[i].color;
				colorButtons.Add(component);
				colorToButton.Add(c, component);
				component.GetComponent<Button>().onClick.AddListener(delegate
				{
					ColorClicked(c);
				});
			}
			buttonContainer.anchoredPosition = new Vector2((0f - colorButtons[0].sizeDelta.x) * (float)colorButtons.Count * 0.5f, buttonContainer.anchoredPosition.y);
			Close();
		}

		protected virtual void Update()
		{
			if (isOpen)
			{
				UpdateConfirmButton();
			}
		}

		private void UpdateConfirmButton()
		{
			bool flag = NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= repaintCost;
			bool flag2 = NetworkSingleton<MoneyManager>.Instance.cashBalance >= repaintCost;
			confirmButton_Cash.interactable = flag2 && selectedColor != currentVehicle.color;
			confirmButton_Online.interactable = flag && selectedColor != currentVehicle.color;
		}

		public void Open(LandVehicle vehicle)
		{
			currentVehicle = vehicle;
			canvas.enabled = true;
			selectedColor = vehicle.color;
			RefreshSelectionIndicator();
			Singleton<HUD>.Instance.ShowTopScreenText("Repainting vehicle...");
			UpdateConfirmButton();
		}

		public void Close()
		{
			if (currentVehicle != null)
			{
				currentVehicle.StopColorOverride();
			}
			currentVehicle = null;
			canvas.enabled = false;
			Singleton<HUD>.Instance.HideTopScreenText();
		}

		public void ColorClicked(EVehicleColor col)
		{
			selectedColor = col;
			currentVehicle.OverrideColor(col);
			RefreshSelectionIndicator();
			UpdateConfirmButton();
		}

		private void RefreshSelectionIndicator()
		{
			tempIndicator.position = colorToButton[selectedColor].position;
			permIndicator.position = colorToButton[currentVehicle.color].position;
		}

		public void ConfirmButtonClicked(bool payWithCash)
		{
			if (payWithCash)
			{
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - repaintCost);
			}
			else
			{
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Vehicle repaint", 0f - repaintCost, 1f, string.Empty);
			}
			currentVehicle.color = selectedColor;
			currentVehicle.StopColorOverride();
			RefreshSelectionIndicator();
		}
	}
}
