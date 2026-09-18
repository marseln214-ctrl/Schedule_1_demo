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
	public class ObjectFieldUI : MonoBehaviour
	{
		[Header("References")]
		public string InstructionText = "Select <ObjectType>";

		public string ExtendedInstructionText = string.Empty;

		public TextMeshProUGUI FieldLabel;

		public Image IconImg;

		public TextMeshProUGUI SelectionLabel;

		public GameObject NoneSelected;

		public GameObject MultipleSelected;

		public RectTransform ClearButton;

		public List<ObjectField> Fields { get; protected set; } = new List<ObjectField>();


		public void Bind(List<ObjectField> field)
		{
			Fields = new List<ObjectField>();
			Fields.AddRange(field);
			Fields[Fields.Count - 1].onObjectChanged.AddListener(Refresh);
			Refresh(Fields[0].SelectedObject);
		}

		private void Refresh(BuildableItem newVal)
		{
			IconImg.gameObject.SetActive(value: false);
			NoneSelected.gameObject.SetActive(value: false);
			MultipleSelected.gameObject.SetActive(value: false);
			if (AreFieldsUniform())
			{
				if (newVal != null)
				{
					IconImg.sprite = newVal.ItemInstance.Icon;
					SelectionLabel.text = newVal.ItemInstance.Name;
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
			ObjectField objectField = Fields.FirstOrDefault((ObjectField x) => x.SelectedObject != null);
			ClearButton.gameObject.SetActive(objectField != null);
		}

		private bool AreFieldsUniform()
		{
			for (int i = 0; i < Fields.Count - 1; i++)
			{
				if (Fields[i].SelectedObject != Fields[i + 1].SelectedObject)
				{
					return false;
				}
			}
			return true;
		}

		public void Clicked()
		{
			BuildableItem buildableItem = null;
			if (AreFieldsUniform())
			{
				buildableItem = Fields[0].SelectedObject;
			}
			List<BuildableItem> list = new List<BuildableItem>();
			if (buildableItem != null)
			{
				list.Add(buildableItem);
			}
			List<Transform> list2 = new List<Transform>();
			for (int i = 0; i < Fields.Count; i++)
			{
				if (Fields[i].DrawTransitLine)
				{
					list2.Add(Fields[i].ParentConfig.Configurable.UIPoint);
				}
			}
			Singleton<ManagementInterface>.Instance.ObjectSelector.Open(InstructionText, ExtendedInstructionText, 1, list, Fields[0].TypeRequirements, ObjectValid, ObjectsSelected, list2);
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
			ObjectSelected((objs.Count > 0) ? objs[0] : null);
		}

		private void ObjectSelected(BuildableItem obj)
		{
			if (obj != null && Fields[0].TypeRequirements.Count > 0 && !Fields[0].TypeRequirements.Contains(obj.GetType()))
			{
				Console.LogError("Wrong Object type selection");
				return;
			}
			foreach (ObjectField field in Fields)
			{
				field.SetObject(obj, network: true);
			}
		}

		public void ClearClicked()
		{
			ObjectSelected(null);
		}
	}
}
