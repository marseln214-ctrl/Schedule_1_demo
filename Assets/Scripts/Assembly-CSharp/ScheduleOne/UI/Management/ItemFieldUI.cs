using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class ItemFieldUI : MonoBehaviour
	{
		[Header("References")]
		public TextMeshProUGUI FieldLabel;

		public Image IconImg;

		public TextMeshProUGUI SelectionLabel;

		public GameObject NoneSelected;

		public GameObject MultipleSelected;

		public List<ItemField> Fields { get; protected set; } = new List<ItemField>();


		public void Bind(List<ItemField> field)
		{
			Fields = new List<ItemField>();
			Fields.AddRange(field);
			Fields[Fields.Count - 1].onItemChanged.AddListener(Refresh);
			Refresh(Fields[0].SelectedItem);
		}

		private void Refresh(ItemDefinition newVal)
		{
			IconImg.gameObject.SetActive(value: false);
			NoneSelected.gameObject.SetActive(value: false);
			MultipleSelected.gameObject.SetActive(value: false);
			if (AreFieldsUniform())
			{
				if (newVal != null)
				{
					IconImg.sprite = newVal.Icon;
					SelectionLabel.text = newVal.Name;
					IconImg.gameObject.SetActive(value: true);
				}
				else
				{
					NoneSelected.SetActive(value: true);
					SelectionLabel.text = "None";
				}
			}
			else
			{
				MultipleSelected.SetActive(value: true);
				SelectionLabel.text = "Mixed";
			}
		}

		private bool AreFieldsUniform()
		{
			for (int i = 0; i < Fields.Count - 1; i++)
			{
				if (Fields[i].SelectedItem != Fields[i + 1].SelectedItem)
				{
					return false;
				}
			}
			return true;
		}

		public void Clicked()
		{
			List<ItemSelector.Option> list = new List<ItemSelector.Option>();
			ItemSelector.Option selectedOption = null;
			bool flag = AreFieldsUniform();
			if (Fields[0].CanSelectNone)
			{
				list.Add(new ItemSelector.Option("None", null));
				if (flag && Fields[0].SelectedItem == null)
				{
					selectedOption = list[0];
				}
			}
			foreach (ItemDefinition option2 in Fields[0].Options)
			{
				ItemSelector.Option option = new ItemSelector.Option(option2.Name, option2);
				list.Add(option);
				if (flag && Fields[0].SelectedItem == option.Item)
				{
					selectedOption = option;
				}
			}
			Singleton<ManagementInterface>.Instance.ItemSelectorScreen.Initialize(FieldLabel.text, list, selectedOption, OptionSelected);
			Singleton<ManagementInterface>.Instance.ItemSelectorScreen.Open();
		}

		private void OptionSelected(ItemSelector.Option option)
		{
			foreach (ItemField field in Fields)
			{
				field.SetItem(option.Item, network: true);
			}
		}
	}
}
