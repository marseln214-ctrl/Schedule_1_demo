using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.ATM
{
	public class ATMInterface : MonoBehaviour
	{
		[Header("References")]
		[SerializeField]
		protected Canvas canvas;

		[SerializeField]
		protected ScheduleOne.Money.ATM atm;

		[SerializeField]
		protected AudioSourceController CompleteSound;

		[Header("Menu")]
		[SerializeField]
		protected RectTransform menuScreen;

		[SerializeField]
		protected Text menu_TitleText;

		[SerializeField]
		protected Button menu_DepositButton;

		[SerializeField]
		protected Button menu_WithdrawButton;

		[Header("Top bar")]
		[SerializeField]
		protected Text depositLimitText;

		[SerializeField]
		protected Text onlineBalanceText;

		[SerializeField]
		protected Text cleanCashText;

		[SerializeField]
		protected RectTransform depositLimitContainer;

		[Header("Amount screen")]
		[SerializeField]
		protected RectTransform amountSelectorScreen;

		[SerializeField]
		protected Text amountSelectorTitle;

		[SerializeField]
		protected List<Button> amountButtons = new List<Button>();

		[SerializeField]
		protected Text amountLabelText;

		[SerializeField]
		protected RectTransform amountBackground;

		[SerializeField]
		protected RectTransform selectedButtonIndicator;

		[SerializeField]
		protected Button confirmAmountButton;

		[SerializeField]
		protected Text confirmButtonText;

		[Header("Processing screen")]
		[SerializeField]
		protected RectTransform processingScreen;

		[SerializeField]
		protected RectTransform processingScreenIndicator;

		[Header("Success screen")]
		[SerializeField]
		protected RectTransform successScreen;

		[SerializeField]
		protected Text successScreenSubtitle;

		[SerializeField]
		protected Button doneButton;

		private RectTransform activeScreen;

		public static int[] amounts = new int[6] { 20, 50, 100, 500, 1000, 5000 };

		private bool depositing = true;

		private int selectedAmountIndex;

		private float selectedAmount;

		public bool isOpen { get; protected set; }

		private float relevantBalance
		{
			get
			{
				if (!depositing)
				{
					return NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance;
				}
				return NetworkSingleton<MoneyManager>.Instance.cashBalance;
			}
		}

		private static float remainingAllowedDeposit => float.MaxValue;

		private void Awake()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		}

		private void OnDestroy()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		}

		protected virtual void Start()
		{
			GameInput.RegisterExitListener(Exit, 2);
			activeScreen = menuScreen;
			canvas.enabled = false;
			for (int i = 0; i < amountButtons.Count; i++)
			{
				int fuckYou = i;
				amountButtons[i].onClick.AddListener(delegate
				{
					AmountSelected(fuckYou);
				});
				if (i == amountButtons.Count - 1)
				{
					amountButtons[i].transform.Find("Text").GetComponent<Text>().text = "ALL ()";
				}
				else
				{
					amountButtons[i].transform.Find("Text").GetComponent<Text>().text = MoneyManager.FormatAmount(amounts[i]);
				}
			}
			depositLimitContainer.gameObject.SetActive(value: false);
		}

		private void PlayerSpawned()
		{
			canvas.worldCamera = PlayerSingleton<PlayerCamera>.Instance.Camera;
		}

		protected virtual void Update()
		{
			if (!isOpen)
			{
				return;
			}
			onlineBalanceText.text = MoneyManager.FormatAmount(NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance);
			cleanCashText.text = MoneyManager.FormatAmount(NetworkSingleton<MoneyManager>.Instance.cashBalance);
			depositLimitText.text = MoneyManager.FormatAmount(ScheduleOne.Money.ATM.AmountDepositedToday) + " / " + MoneyManager.FormatAmount(float.MaxValue);
			if (ScheduleOne.Money.ATM.AmountDepositedToday >= float.MaxValue)
			{
				depositLimitText.color = new Color32(byte.MaxValue, 75, 75, byte.MaxValue);
			}
			else
			{
				depositLimitText.color = Color.white;
			}
			if (activeScreen == amountSelectorScreen)
			{
				if (depositing)
				{
					amountButtons[amountButtons.Count - 1].transform.Find("Text").GetComponent<Text>().text = "MAX (" + MoneyManager.FormatAmount(Mathf.Min(NetworkSingleton<MoneyManager>.Instance.cashBalance, remainingAllowedDeposit)) + ")";
				}
				UpdateAvailableAmounts();
				confirmAmountButton.interactable = relevantBalance > 0f;
				if (depositing)
				{
					if (selectedAmountIndex == amounts.Length)
					{
						confirmButtonText.text = "DEPOSIT ALL";
					}
					else
					{
						confirmButtonText.text = "DEPOSIT " + MoneyManager.FormatAmount(selectedAmount);
					}
				}
				else
				{
					confirmButtonText.text = "WITHDRAW " + MoneyManager.FormatAmount(selectedAmount);
				}
				if (relevantBalance < GetAmountFromIndex(selectedAmountIndex, depositing))
				{
					DefaultAmountSelection();
				}
			}
			if (activeScreen == menuScreen)
			{
				menu_DepositButton.interactable = ScheduleOne.Money.ATM.AmountDepositedToday < float.MaxValue;
			}
			if (activeScreen == processingScreen)
			{
				processingScreenIndicator.localEulerAngles = new Vector3(0f, 0f, processingScreenIndicator.localEulerAngles.z - Time.deltaTime * 360f);
			}
		}

		protected virtual void LateUpdate()
		{
			if (isOpen && activeScreen == amountSelectorScreen)
			{
				if (selectedAmountIndex == -1)
				{
					selectedButtonIndicator.gameObject.SetActive(value: false);
					return;
				}
				selectedButtonIndicator.anchoredPosition = amountButtons[selectedAmountIndex].GetComponent<RectTransform>().anchoredPosition;
				selectedButtonIndicator.gameObject.SetActive(value: true);
			}
		}

		public virtual void SetIsOpen(bool o)
		{
			if (o != isOpen)
			{
				isOpen = o;
				canvas.enabled = isOpen;
				EventSystem.current.SetSelectedGameObject(null);
				if (isOpen)
				{
					PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
					Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
					PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
					SetActiveScreen(menuScreen);
				}
				else
				{
					atm.Exit();
					PlayerSingleton<PlayerCamera>.Instance.LockMouse();
					Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
					PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				}
			}
		}

		public virtual void Exit(ExitAction action)
		{
			if (!action.used && isOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				if (activeScreen == menuScreen || activeScreen == successScreen)
				{
					SetIsOpen(o: false);
				}
				else if (activeScreen == amountSelectorScreen)
				{
					SetActiveScreen(menuScreen);
				}
			}
		}

		public void SetActiveScreen(RectTransform screen)
		{
			menuScreen.gameObject.SetActive(value: false);
			amountSelectorScreen.gameObject.SetActive(value: false);
			processingScreen.gameObject.SetActive(value: false);
			successScreen.gameObject.SetActive(value: false);
			activeScreen = screen;
			activeScreen.gameObject.SetActive(value: true);
			if (activeScreen == menuScreen)
			{
				menu_TitleText.text = "Hello, " + Player.Local.PlayerName;
				menu_DepositButton.Select();
			}
			else if (activeScreen == amountSelectorScreen)
			{
				UpdateAvailableAmounts();
				DefaultAmountSelection();
			}
			else if (activeScreen == successScreen)
			{
				doneButton.Select();
			}
		}

		private void DefaultAmountSelection()
		{
			if (amountButtons[0].interactable)
			{
				amountButtons[0].Select();
				AmountSelected(0);
				return;
			}
			if (amountButtons[amountButtons.Count - 1].interactable && relevantBalance > 0f)
			{
				amountButtons[amountButtons.Count - 1].Select();
				AmountSelected(amountButtons.Count - 1);
				return;
			}
			AmountSelected(-1);
			for (int i = 0; i < amountButtons.Count; i++)
			{
			}
		}

		public void DepositButtonPressed()
		{
			amountSelectorTitle.text = "Select amount to deposit";
			depositing = true;
			SetActiveScreen(amountSelectorScreen);
		}

		public void WithdrawButtonPressed()
		{
			amountSelectorTitle.text = "Select amount to withdraw";
			depositing = false;
			amountButtons[amountButtons.Count - 1].transform.Find("Text").GetComponent<Text>().text = MoneyManager.FormatAmount(amounts[amounts.Length - 1]);
			SetActiveScreen(amountSelectorScreen);
		}

		public void CancelAmountSelection()
		{
			SetActiveScreen(menuScreen);
		}

		public void AmountSelected(int amountIndex)
		{
			selectedAmountIndex = amountIndex;
			SetSelectedAmount(GetAmountFromIndex(amountIndex, depositing));
		}

		private void SetSelectedAmount(float amount)
		{
			float num = 0f;
			num = ((!depositing) ? NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance : Mathf.Min(NetworkSingleton<MoneyManager>.Instance.cashBalance, remainingAllowedDeposit));
			selectedAmount = Mathf.Clamp(amount, 0f, num);
			amountLabelText.text = MoneyManager.FormatAmount(selectedAmount);
		}

		public static float GetAmountFromIndex(int index, bool depositing)
		{
			if (index == -1 || index >= amounts.Length)
			{
				return 0f;
			}
			if (depositing && index == amounts.Length - 1)
			{
				return Mathf.Min(NetworkSingleton<MoneyManager>.Instance.cashBalance, remainingAllowedDeposit);
			}
			return amounts[index];
		}

		private void UpdateAvailableAmounts()
		{
			for (int i = 0; i < amounts.Length; i++)
			{
				if (depositing && i == amounts.Length - 1)
				{
					amountButtons[amountButtons.Count - 1].interactable = relevantBalance > 0f && remainingAllowedDeposit > 0f;
					break;
				}
				if (depositing)
				{
					amountButtons[i].interactable = relevantBalance >= (float)amounts[i] && ScheduleOne.Money.ATM.AmountDepositedToday + (float)amounts[i] <= float.MaxValue;
				}
				else
				{
					amountButtons[i].interactable = relevantBalance >= (float)amounts[i];
				}
			}
		}

		public void AmountConfirmed()
		{
			StartCoroutine(ProcessTransaction(selectedAmount, depositing));
		}

		public void ChangeAmount(float amount)
		{
			selectedAmountIndex = -1;
			SetSelectedAmount(selectedAmount + amount);
		}

		protected IEnumerator ProcessTransaction(float amount, bool depositing)
		{
			SetActiveScreen(processingScreen);
			yield return new WaitForSeconds(1f);
			CompleteSound.Play();
			if (depositing)
			{
				if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= amount)
				{
					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - amount);
					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Cash Deposit", amount, 1f, string.Empty);
					ScheduleOne.Money.ATM.AmountDepositedToday += amount;
					successScreenSubtitle.text = "You have deposited " + MoneyManager.FormatAmount(amount);
					SetActiveScreen(successScreen);
				}
				else
				{
					SetActiveScreen(menuScreen);
				}
			}
			else if (NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= amount)
			{
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(amount);
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Cash Withdrawal", 0f - amount, 1f, string.Empty);
				successScreenSubtitle.text = "You have withdrawn " + MoneyManager.FormatAmount(amount);
				SetActiveScreen(successScreen);
			}
			else
			{
				SetActiveScreen(menuScreen);
			}
		}

		public void DoneButtonPressed()
		{
			SetIsOpen(o: false);
		}

		public void ReturnToMenuButtonPressed()
		{
			SetActiveScreen(menuScreen);
		}
	}
}
