using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.Money;
using ScheduleOne.ObjectScripts.Cash;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class LaunderingInterface : MonoBehaviour
	{
		protected const float fovOverride = 65f;

		protected const float lerpTime = 0.15f;

		protected const int minLaunderAmount = 10;

		[Header("References")]
		[SerializeField]
		protected Transform cameraPosition;

		[SerializeField]
		protected InteractableObject intObj;

		[SerializeField]
		protected Button launderButton;

		[SerializeField]
		protected GameObject amountSelectorScreen;

		[SerializeField]
		protected Slider amountSlider;

		[SerializeField]
		protected TMP_InputField amountInputField;

		[SerializeField]
		protected RectTransform notchContainer;

		[SerializeField]
		protected TextMeshProUGUI currentTotalAmountLabel;

		[SerializeField]
		protected TextMeshProUGUI launderCapacityLabel;

		[SerializeField]
		protected TextMeshProUGUI insufficientCashLabel;

		[SerializeField]
		protected RectTransform entryContainer;

		[SerializeField]
		protected RectTransform noEntries;

		public CashStackVisuals[] CashStacks;

		[Header("Prefabs")]
		[SerializeField]
		protected GameObject timelineNotchPrefab;

		[SerializeField]
		protected GameObject entryPrefab;

		[Header("UI references")]
		[SerializeField]
		protected Canvas canvas;

		private int selectedAmountToLaunder;

		private Dictionary<LaunderingOperation, RectTransform> operationToNotch = new Dictionary<LaunderingOperation, RectTransform>();

		private List<RectTransform> notches = new List<RectTransform>();

		private bool ignoreSliderChange = true;

		private Dictionary<LaunderingOperation, RectTransform> operationToEntry = new Dictionary<LaunderingOperation, RectTransform>();

		protected int maxLaunderAmount => (int)Mathf.Min(business.appliedLaunderLimit, NetworkSingleton<MoneyManager>.Instance.cashBalance);

		public Business business { get; private set; }

		public bool isOpen => canvas.gameObject.activeSelf;

		public void Initialize(Business bus)
		{
			business = bus;
			intObj.onHovered.AddListener(Hovered);
			intObj.onInteractStart.AddListener(Interacted);
			launderCapacityLabel.text = MoneyManager.FormatAmount(business.LaunderCapacity);
			canvas.gameObject.SetActive(value: false);
			noEntries.gameObject.SetActive(operationToEntry.Count == 0);
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, (Action)delegate
			{
				canvas.worldCamera = PlayerSingleton<PlayerCamera>.Instance.Camera;
			});
			Business.onOperationStarted = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationStarted, new Action<LaunderingOperation>(CreateEntry));
			Business.onOperationStarted = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationStarted, new Action<LaunderingOperation>(UpdateCashStacks));
			Business.onOperationFinished = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationFinished, new Action<LaunderingOperation>(RemoveEntry));
			Business.onOperationFinished = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationFinished, new Action<LaunderingOperation>(UpdateCashStacks));
			GameInput.RegisterExitListener(Exit, 5);
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
			CloseAmountSelector();
		}

		private void OnDestroy()
		{
		}

		protected virtual void MinPass()
		{
		}

		protected void Exit(ExitAction exit)
		{
			if (!exit.used && isOpen)
			{
				if (amountSelectorScreen.gameObject.activeSelf)
				{
					exit.used = true;
					CloseAmountSelector();
				}
				else if (exit.exitType == ExitType.Escape)
				{
					exit.used = true;
					Close();
				}
			}
		}

		protected void UpdateTimeline()
		{
			foreach (LaunderingOperation launderingOperation in business.LaunderingOperations)
			{
				if (!operationToNotch.ContainsKey(launderingOperation))
				{
					RectTransform component = UnityEngine.Object.Instantiate(timelineNotchPrefab, notchContainer).GetComponent<RectTransform>();
					component.Find("Amount").GetComponent<TextMeshProUGUI>().text = MoneyManager.FormatAmount(launderingOperation.amount);
					operationToNotch.Add(launderingOperation, component);
					notches.Add(component);
				}
			}
			List<RectTransform> list = (from x in operationToNotch
				where business.LaunderingOperations.Contains(x.Key)
				select x.Value).ToList();
			for (int i = 0; i < notches.Count; i++)
			{
				if (!list.Contains(notches[i]))
				{
					UnityEngine.Object.Destroy(notches[i].gameObject);
					notches.RemoveAt(i);
					i--;
				}
			}
			foreach (LaunderingOperation launderingOperation2 in business.LaunderingOperations)
			{
				operationToNotch[launderingOperation2].anchoredPosition = new Vector2(notchContainer.rect.width * (float)launderingOperation2.minutesSinceStarted / (float)launderingOperation2.completionTime_Minutes, operationToNotch[launderingOperation2].anchoredPosition.y);
			}
		}

		protected void UpdateCurrentTotal()
		{
			currentTotalAmountLabel.text = MoneyManager.FormatAmount(business.currentLaunderTotal);
		}

		private void CreateEntry(LaunderingOperation op)
		{
			if (!operationToEntry.ContainsKey(op))
			{
				RectTransform component = UnityEngine.Object.Instantiate(entryPrefab, entryContainer).GetComponent<RectTransform>();
				component.SetAsLastSibling();
				component.Find("BusinessLabel").GetComponent<TextMeshProUGUI>().text = op.business.PropertyName;
				component.Find("AmountLabel").GetComponent<TextMeshProUGUI>().text = MoneyManager.FormatAmount(op.amount);
				operationToEntry.Add(op, component);
				UpdateEntryTimes();
				noEntries.gameObject.SetActive(operationToEntry.Count == 0);
			}
		}

		private void RemoveEntry(LaunderingOperation op)
		{
			UnityEngine.Object.Destroy(operationToEntry[op].gameObject);
			operationToEntry.Remove(op);
			noEntries.gameObject.SetActive(operationToEntry.Count == 0);
		}

		private void UpdateEntryTimes()
		{
			foreach (LaunderingOperation item in operationToEntry.Keys.ToList())
			{
				int num = item.completionTime_Minutes - item.minutesSinceStarted;
				if (num > 60)
				{
					int num2 = Mathf.CeilToInt((float)num / 60f);
					operationToEntry[item].Find("TimeLabel").GetComponent<TextMeshProUGUI>().text = num2 + " hours";
				}
				else
				{
					operationToEntry[item].Find("TimeLabel").GetComponent<TextMeshProUGUI>().text = num + " minutes";
				}
			}
		}

		private void UpdateCashStacks(LaunderingOperation op)
		{
			float num = business.currentLaunderTotal;
			for (int i = 0; i < CashStacks.Length; i++)
			{
				if (num <= 0f)
				{
					CashStacks[i].ShowAmount(0f);
					continue;
				}
				float num2 = Mathf.Min(num, 1000f);
				CashStacks[i].ShowAmount(num2);
				num -= num2;
			}
		}

		private void RefreshLaunderButton()
		{
			launderButton.interactable = business.currentLaunderTotal < business.LaunderCapacity && NetworkSingleton<MoneyManager>.Instance.cashBalance > 10f;
			if (business.currentLaunderTotal >= business.LaunderCapacity)
			{
				insufficientCashLabel.text = "The business is already at maximum laundering capacity.";
				insufficientCashLabel.gameObject.SetActive(value: true);
			}
			else if (NetworkSingleton<MoneyManager>.Instance.cashBalance <= 10f)
			{
				insufficientCashLabel.text = "You need at least " + MoneyManager.FormatAmount(10f) + " cash to launder.";
				insufficientCashLabel.gameObject.SetActive(value: true);
			}
			else
			{
				insufficientCashLabel.gameObject.SetActive(value: false);
			}
		}

		public void OpenAmountSelector()
		{
			amountSelectorScreen.gameObject.SetActive(value: true);
			int num = (selectedAmountToLaunder = Mathf.Clamp(100, 10, maxLaunderAmount));
			amountSlider.minValue = 10f;
			amountSlider.maxValue = maxLaunderAmount;
			amountSlider.SetValueWithoutNotify(num);
			amountInputField.SetTextWithoutNotify(num.ToString());
		}

		public void CloseAmountSelector()
		{
			amountSelectorScreen.gameObject.SetActive(value: false);
		}

		public void ConfirmAmount()
		{
			int num = Mathf.Clamp(selectedAmountToLaunder, 10, maxLaunderAmount);
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(-num);
			business.StartLaunderingOperation(num);
			float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("LaunderingOperationsStarted");
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("LaunderingOperationsStarted", (value + 1f).ToString());
			UpdateTimeline();
			UpdateCurrentTotal();
			RefreshLaunderButton();
			CloseAmountSelector();
		}

		public void SliderValueChanged()
		{
			if (ignoreSliderChange)
			{
				ignoreSliderChange = false;
				return;
			}
			selectedAmountToLaunder = (int)amountSlider.value;
			amountInputField.SetTextWithoutNotify(selectedAmountToLaunder.ToString());
		}

		public void InputValueChanged()
		{
			selectedAmountToLaunder = Mathf.Clamp(int.Parse(amountInputField.text), 10, maxLaunderAmount);
			amountInputField.SetTextWithoutNotify(selectedAmountToLaunder.ToString());
			amountSlider.SetValueWithoutNotify(selectedAmountToLaunder);
		}

		public void Hovered()
		{
			if (!business.IsOwned || isOpen)
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
			else if (business.IsOwned && !isOpen)
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				intObj.SetMessage("Manage business");
			}
		}

		public void Interacted()
		{
			if (business.IsOwned && !isOpen)
			{
				Open();
			}
		}

		public virtual void Open()
		{
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(cameraPosition.transform.position, cameraPosition.rotation, 0.15f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(65f, 0.15f);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			RefreshLaunderButton();
			UpdateTimeline();
			UpdateCurrentTotal();
			base.gameObject.SetActive(value: true);
		}

		public virtual void Close()
		{
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.15f);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.15f);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			base.gameObject.SetActive(value: false);
		}
	}
}
