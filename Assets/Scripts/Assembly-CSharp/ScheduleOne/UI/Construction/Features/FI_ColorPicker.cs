using ScheduleOne.Construction.Features;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Construction.Features
{
	public class FI_ColorPicker : FI_Base
	{
		[Header("References")]
		[SerializeField]
		protected RectTransform colorButtonContainer;

		[SerializeField]
		protected Button buyButton;

		[SerializeField]
		protected TextMeshProUGUI buyButtonText;

		[SerializeField]
		protected TextMeshProUGUI colorLabel;

		[SerializeField]
		protected RectTransform bar;

		[Header("Prefab")]
		[SerializeField]
		protected GameObject colorButtonPrefab;

		public UnityEvent<ColorFeature.NamedColor> onSelectionChanged;

		public UnityEvent<ColorFeature.NamedColor> onSelectionPurchased;

		private ColorFeature specificFeature;

		private int selectionIndex;

		public override void Initialize(Feature _feature)
		{
			base.Initialize(_feature);
			specificFeature = feature as ColorFeature;
			selectionIndex = specificFeature.SyncAccessor_ownedColorIndex;
			for (int i = 0; i < specificFeature.colors.Count; i++)
			{
				Button component = Object.Instantiate(colorButtonPrefab, colorButtonContainer).GetComponent<Button>();
				component.GetComponent<Image>().color = specificFeature.colors[i].color;
				int index = i;
				component.onClick.AddListener(delegate
				{
					Select(index);
				});
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(colorButtonContainer);
			bar.anchoredPosition = new Vector2(bar.anchoredPosition.x, colorButtonContainer.GetChild(colorButtonContainer.childCount - 1).GetComponent<RectTransform>().anchoredPosition.y - 35f);
			UpdateSelection();
		}

		public override void Close()
		{
			Select(specificFeature.SyncAccessor_ownedColorIndex);
			base.Close();
		}

		public void BuyButtonClicked()
		{
			if (!(NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance < specificFeature.colors[selectionIndex].price))
			{
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction(Singleton<ConstructionMenu>.Instance.SelectedConstructable.ConstructableName + ": " + feature.featureName, 0f - specificFeature.colors[selectionIndex].price, 1f, string.Empty);
				if (onSelectionPurchased != null)
				{
					onSelectionPurchased.Invoke(specificFeature.colors[selectionIndex]);
				}
				UpdateSelection();
			}
		}

		public void Select(int index)
		{
			selectionIndex = Mathf.Clamp(index, 0, specificFeature.colors.Count - 1);
			if (onSelectionChanged != null)
			{
				onSelectionChanged.Invoke(specificFeature.colors[selectionIndex]);
			}
			UpdateSelection();
		}

		private void UpdateSelection()
		{
			colorLabel.text = specificFeature.colors[selectionIndex].colorName;
			for (int i = 0; i < colorButtonContainer.childCount; i++)
			{
				colorButtonContainer.GetChild(i).Find("SelectionIndicator").gameObject.SetActive(value: false);
				colorButtonContainer.GetChild(i).Find("OwnedIndicator").gameObject.SetActive(value: false);
			}
			colorButtonContainer.GetChild(selectionIndex).Find("SelectionIndicator").gameObject.SetActive(value: true);
			colorButtonContainer.GetChild(specificFeature.SyncAccessor_ownedColorIndex).Find("OwnedIndicator").gameObject.SetActive(value: true);
			if (selectionIndex != specificFeature.SyncAccessor_ownedColorIndex)
			{
				buyButtonText.text = "Buy (" + MoneyManager.FormatAmount(specificFeature.colors[selectionIndex].price) + ")";
				buyButton.gameObject.SetActive(value: true);
			}
			else
			{
				buyButton.gameObject.SetActive(value: false);
			}
		}
	}
}
