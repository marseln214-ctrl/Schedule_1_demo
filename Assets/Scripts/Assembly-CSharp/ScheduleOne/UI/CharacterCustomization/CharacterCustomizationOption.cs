using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.Money;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCustomization
{
	public class CharacterCustomizationOption : MonoBehaviour
	{
		public string Name = "Option";

		public string Label = "AssetPath or Label";

		public float Price;

		public bool RequireLevel;

		public FullRank RequiredLevel = new FullRank(ERank.Street_Rat, 1);

		[Header("References")]
		public TextMeshProUGUI NameLabel;

		public TextMeshProUGUI PriceLabel;

		public TextMeshProUGUI LevelLabel;

		public RectTransform LockDisplay;

		public Button MainButton;

		public Button BuyButton;

		public RectTransform SelectionIndicator;

		[Header("Events")]
		public UnityEvent onSelect;

		public UnityEvent onDeselect;

		public UnityEvent onPurchase;

		private bool selected;

		public bool purchased { get; private set; }

		private bool purchaseable
		{
			get
			{
				if (RequireLevel)
				{
					return RequiredLevel <= NetworkSingleton<LevelManager>.Instance.GetFullRank();
				}
				return true;
			}
		}

		private void Awake()
		{
			NameLabel.text = Name;
			if (Price > 0f)
			{
				PriceLabel.text = MoneyManager.FormatAmount(Price);
			}
			else
			{
				PriceLabel.text = "Free";
			}
			UpdatePriceColor();
			LevelLabel.text = RequiredLevel.ToString();
			MainButton.onClick.AddListener(Selected);
			BuyButton.onClick.AddListener(Purchased);
		}

		private void OnValidate()
		{
			base.gameObject.name = Name;
		}

		private void FixedUpdate()
		{
			BuyButton.interactable = NetworkSingleton<MoneyManager>.Instance.cashBalance >= Price;
		}

		private void Start()
		{
			UpdateUI();
		}

		private void Selected()
		{
			SetSelected(_selected: true);
		}

		private void Purchased()
		{
			if (purchaseable)
			{
				if (onPurchase != null)
				{
					onPurchase.Invoke();
				}
				if (Price > 0f)
				{
					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - Price);
				}
				SetPurchased(_purchased: true);
			}
		}

		private void UpdatePriceColor()
		{
			if (Price > 0f)
			{
				if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= Price)
				{
					PriceLabel.color = (ColorUtility.TryParseHtmlString("#46CB4F", out var color) ? color : Color.white);
				}
				else
				{
					PriceLabel.color = new Color32(200, 75, 70, byte.MaxValue);
				}
			}
			else
			{
				PriceLabel.color = (ColorUtility.TryParseHtmlString("#46CB4F", out var color2) ? color2 : Color.white);
			}
		}

		public void SetSelected(bool _selected)
		{
			selected = _selected;
			SelectionIndicator.gameObject.SetActive(selected);
			NameLabel.rectTransform.offsetMin = new Vector2(selected ? 30f : 10f, NameLabel.rectTransform.offsetMin.y);
			UpdateUI();
			if (selected)
			{
				if (onSelect != null)
				{
					onSelect.Invoke();
				}
			}
			else if (onDeselect != null)
			{
				onDeselect.Invoke();
			}
		}

		public void SetPurchased(bool _purchased)
		{
			purchased = _purchased;
			BuyButton.gameObject.SetActive(!purchased);
			PriceLabel.gameObject.SetActive(!purchased);
			if (_purchased)
			{
				SetSelected(_selected: true);
			}
			UpdateUI();
		}

		private void UpdateUI()
		{
			LockDisplay.gameObject.SetActive(!purchaseable);
			PriceLabel.gameObject.SetActive(purchaseable && !purchased);
			BuyButton.gameObject.SetActive(purchaseable && !purchased);
			UpdatePriceColor();
		}

		public void ParentCategoryClosed()
		{
			if (selected && !purchased)
			{
				SetSelected(_selected: false);
			}
			else if (purchased && !selected)
			{
				SetSelected(_selected: true);
			}
		}

		public void SiblingOptionSelected(CharacterCustomizationOption option)
		{
			if (option != this && selected)
			{
				SetSelected(_selected: false);
			}
		}

		public void SiblingOptionPurchased(CharacterCustomizationOption option)
		{
			if (option != this && purchased)
			{
				SetPurchased(_purchased: false);
			}
		}
	}
}
