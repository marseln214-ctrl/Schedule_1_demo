using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCustomization
{
	public class CharacterCustomizationCategory : MonoBehaviour
	{
		public string CategoryName;

		[Header("References")]
		public TextMeshProUGUI TitleText;

		public Button BackButton;

		public ScrollRect ScrollRect;

		private CharacterCustomizationUI ui;

		private CharacterCustomizationOption[] options;

		public UnityEvent onOpen;

		public UnityEvent onClose;

		private void Awake()
		{
			ui = GetComponentInParent<CharacterCustomizationUI>();
			options = GetComponentsInChildren<CharacterCustomizationOption>(includeInactive: true);
			TitleText.text = CategoryName;
			BackButton.onClick.AddListener(Back);
			for (int i = 0; i < options.Length; i++)
			{
				CharacterCustomizationOption option = options[i];
				options[i].onSelect.AddListener(delegate
				{
					OptionSelected(option);
				});
				options[i].onDeselect.AddListener(delegate
				{
					OptionDeselected(option);
				});
				options[i].onPurchase.AddListener(delegate
				{
					OptionPurchased(option);
				});
			}
			for (int j = 0; j < options.Length; j++)
			{
				for (int k = j + 1; k < options.Length; k++)
				{
					if (options[k].Price < options[j].Price)
					{
						_ = options[j].transform;
						options[j].transform.SetSiblingIndex(k);
						options[k].transform.SetSiblingIndex(j);
					}
				}
			}
		}

		public void Open()
		{
			bool flag = false;
			for (int i = 0; i < options.Length; i++)
			{
				if (ui.IsOptionCurrentlyApplied(options[i]))
				{
					flag = true;
					options[i].SetPurchased(_purchased: true);
				}
				else
				{
					options[i].SetPurchased(_purchased: false);
					options[i].SetSelected(_selected: false);
				}
			}
			if (!flag && options.Length != 0)
			{
				options[0].SetPurchased(_purchased: true);
			}
			ScrollRect.verticalScrollbar.value = 1f;
			if (onOpen != null)
			{
				onOpen.Invoke();
			}
		}

		public void Back()
		{
			ui.SetActiveCategory(null);
			for (int i = 0; i < options.Length; i++)
			{
				options[i].ParentCategoryClosed();
			}
			if (onClose != null)
			{
				onClose.Invoke();
			}
		}

		private void OptionSelected(CharacterCustomizationOption option)
		{
			ui.OptionSelected(option);
			for (int i = 0; i < options.Length; i++)
			{
				options[i].SiblingOptionSelected(option);
			}
		}

		private void OptionDeselected(CharacterCustomizationOption option)
		{
			ui.OptionDeselected(option);
		}

		private void OptionPurchased(CharacterCustomizationOption option)
		{
			ui.OptionPurchased(option);
			for (int i = 0; i < options.Length; i++)
			{
				options[i].SiblingOptionPurchased(option);
			}
		}
	}
}
