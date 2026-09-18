using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class ObjectListFieldUI : MonoBehaviour
	{
		[Header("References")]
		public string FieldText = "Objects";

		public string InstructionText = "Select <ObjectType>";

		public string ExtendedInstructionText = string.Empty;

		public TextMeshProUGUI FieldLabel;

		public GameObject NoneSelected;

		public GameObject MultipleSelected;

		public RectTransform[] Entries;

		public Button Button;

		public GameObject EditIcon;

		public GameObject NoMultiEdit;

		public List<ObjectListField> Fields { get; protected set; } = new List<ObjectListField>();


		public void Bind(List<ObjectListField> field)
		{
			Fields = new List<ObjectListField>();
			Fields.AddRange(field);
			Fields[Fields.Count - 1].onListChanged.AddListener(Refresh);
			Refresh(Fields[0].SelectedObjects);
			if (field.Count == 1)
			{
				EditIcon.gameObject.SetActive(value: true);
				NoMultiEdit.gameObject.SetActive(value: false);
				Button.interactable = true;
			}
			else
			{
				EditIcon.gameObject.SetActive(value: false);
				NoMultiEdit.gameObject.SetActive(value: true);
				Button.interactable = false;
			}
		}

		private void Refresh(List<BuildableItem> newVal)
		{
			NoneSelected.gameObject.SetActive(value: false);
			MultipleSelected.gameObject.SetActive(value: false);
			bool flag = AreFieldsUniform();
			if (flag)
			{
				if (Fields[0].SelectedObjects.Count == 0)
				{
					NoneSelected.SetActive(value: true);
				}
			}
			else
			{
				MultipleSelected.SetActive(value: true);
			}
			if (Fields.Count == 1)
			{
				FieldLabel.text = FieldText + " (" + newVal.Count + "/" + Fields[0].MaxItems + ")";
			}
			else
			{
				FieldLabel.text = FieldText;
			}
			for (int i = 0; i < Entries.Length; i++)
			{
				if (flag && Fields[0].SelectedObjects.Count > i)
				{
					Entries[i].Find("Title").GetComponent<TextMeshProUGUI>().text = Fields[0].SelectedObjects[i].ItemInstance.Name;
					Entries[i].Find("Title").gameObject.SetActive(value: true);
				}
				else
				{
					Entries[i].Find("Title").gameObject.SetActive(value: false);
				}
			}
		}

		private bool AreFieldsUniform()
		{
			for (int i = 0; i < Fields.Count - 1; i++)
			{
				if (!Fields[i].SelectedObjects.SequenceEqual(Fields[i + 1].SelectedObjects))
				{
					return false;
				}
			}
			return true;
		}

		public void Clicked()
		{
			List<BuildableItem> list = new List<BuildableItem>();
			if (AreFieldsUniform())
			{
				list.AddRange(Fields[0].SelectedObjects);
			}
			Singleton<ManagementInterface>.Instance.ObjectSelector.Open(InstructionText, ExtendedInstructionText, Fields[0].MaxItems, list, Fields[0].TypeRequirements, ObjectValid, ObjectsSelected);
		}

		private bool ObjectValid(BuildableItem obj, out string reason)
		{
			string text = string.Empty;
			for (int i = 0; i < Fields.Count; i++)
			{
				if (Fields[i].objectFilter == null || Fields[i].objectFilter(obj, out reason))
				{
					reason = string.Empty;
					return true;
				}
				text = reason;
			}
			reason = text;
			return false;
		}

		public void ObjectsSelected(List<BuildableItem> objs)
		{
			foreach (ObjectListField field in Fields)
			{
				new List<BuildableItem>().AddRange(objs);
				field.SetList(objs, network: true);
			}
		}
	}
}
