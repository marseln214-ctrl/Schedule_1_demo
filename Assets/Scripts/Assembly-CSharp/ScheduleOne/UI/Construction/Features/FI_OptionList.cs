using System.Collections.Generic;
using ScheduleOne.Construction.Features;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Construction.Features
{
	public class FI_OptionList : FI_Base
	{
		public class Option
		{
			public string optionLabel;

			public Color optionColor;

			public float optionPrice;

			public Option(string _optionLabel, Color _optionColor, float _optionPrice)
			{
				optionLabel = _optionLabel;
				optionColor = _optionColor;
				optionPrice = _optionPrice;
			}
		}

		[Header("References")]
		[SerializeField]
		protected RectTransform buttonContainer;

		[SerializeField]
		protected Button buyButton;

		[SerializeField]
		protected TextMeshProUGUI buyButtonText;

		[SerializeField]
		protected RectTransform bar;

		[Header("Prefab")]
		[SerializeField]
		protected GameObject buttonPrefab;

		public UnityEvent<int> onSelectionChanged;

		public UnityEvent<int> onSelectionPurchased;

		private List<Option> options = new List<Option>();

		public OptionListFeature specificFeature;

		private int selectionIndex;

		public virtual void Initialize(OptionListFeature _feature, List<Option> _options)
		{
			base.Initialize(_feature);
			specificFeature = _feature;
			options.AddRange(_options);
			selectionIndex = specificFeature.SyncAccessor_ownedOptionIndex;
			for (int i = 0; i < options.Count; i++)
			{
				Button component = Object.Instantiate(buttonPrefab, buttonContainer).GetComponent<Button>();
				component.GetComponent<Image>().color = options[i].optionColor;
				component.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = options[i].optionLabel;
				int index = i;
				component.onClick.AddListener(delegate
				{
					Select(index);
				});
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(buttonContainer);
			bar.anchoredPosition = new Vector2(bar.anchoredPosition.x, buttonContainer.GetChild(buttonContainer.childCount - 1).GetComponent<RectTransform>().anchoredPosition.y - 35f);
			UpdateSelection();
		}

		public override void Close()
		{
			Select(specificFeature.SyncAccessor_ownedOptionIndex);
			base.Close();
		}

		public void BuyButtonClicked()
		{
			if (!(NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance < options[selectionIndex].optionPrice))
			{
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction(Singleton<ConstructionMenu>.Instance.SelectedConstructable.ConstructableName + ": " + feature.featureName, 0f - options[selectionIndex].optionPrice, 1f, string.Empty);
				if (onSelectionPurchased != null)
				{
					onSelectionPurchased.Invoke(selectionIndex);
				}
				UpdateSelection();
			}
		}

		public void Select(int index)
		{
			selectionIndex = Mathf.Clamp(index, 0, options.Count - 1);
			if (onSelectionChanged != null)
			{
				onSelectionChanged.Invoke(selectionIndex);
			}
			UpdateSelection();
		}

		private void UpdateSelection()
		{
			for (int i = 0; i < buttonContainer.childCount; i++)
			{
				buttonContainer.GetChild(i).Find("SelectionIndicator").gameObject.SetActive(value: false);
				buttonContainer.GetChild(i).Find("OwnedIndicator").gameObject.SetActive(value: false);
			}
			buttonContainer.GetChild(selectionIndex).Find("SelectionIndicator").gameObject.SetActive(value: true);
			buttonContainer.GetChild(specificFeature.SyncAccessor_ownedOptionIndex).Find("OwnedIndicator").gameObject.SetActive(value: true);
			if (selectionIndex != specificFeature.SyncAccessor_ownedOptionIndex)
			{
				buyButtonText.text = "Buy (" + MoneyManager.FormatAmount(options[selectionIndex].optionPrice) + ")";
				buyButton.gameObject.SetActive(value: true);
			}
			else
			{
				buyButton.gameObject.SetActive(value: false);
			}
		}
	}
}
