using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.NPCs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class NPCFieldUI : MonoBehaviour
	{
		[Header("References")]
		public TextMeshProUGUI FieldLabel;

		public Image IconImg;

		public TextMeshProUGUI SelectionLabel;

		public GameObject NoneSelected;

		public GameObject MultipleSelected;

		public RectTransform ClearButton;

		public List<NPCField> Fields { get; protected set; } = new List<NPCField>();


		public void Bind(List<NPCField> field)
		{
			Fields = new List<NPCField>();
			Fields.AddRange(field);
			Fields[Fields.Count - 1].onNPCChanged.AddListener(Refresh);
			Refresh(Fields[0].SelectedNPC);
		}

		private void Refresh(NPC newVal)
		{
			IconImg.gameObject.SetActive(value: false);
			NoneSelected.gameObject.SetActive(value: false);
			MultipleSelected.gameObject.SetActive(value: false);
			if (AreFieldsUniform())
			{
				if (newVal != null)
				{
					IconImg.sprite = newVal.MugshotSprite;
					SelectionLabel.text = newVal.FirstName + "\n" + newVal.LastName;
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
			NPCField nPCField = Fields.FirstOrDefault((NPCField x) => x.SelectedNPC != null);
			ClearButton.gameObject.SetActive(nPCField != null);
		}

		private bool AreFieldsUniform()
		{
			for (int i = 0; i < Fields.Count - 1; i++)
			{
				if (Fields[i].SelectedNPC != Fields[i + 1].SelectedNPC)
				{
					return false;
				}
			}
			return true;
		}

		public void Clicked()
		{
			AreFieldsUniform();
			Singleton<ManagementInterface>.Instance.NPCSelector.Open("Select " + FieldLabel.text, Fields[0].TypeRequirement, NPCSelected);
		}

		public void NPCSelected(NPC npc)
		{
			if (npc != null && npc.GetType() != Fields[0].TypeRequirement)
			{
				Console.LogError("Wrong NPC type selection");
				return;
			}
			foreach (NPCField field in Fields)
			{
				field.SetNPC(npc, network: true);
			}
		}

		public void ClearClicked()
		{
			NPCSelected(null);
		}
	}
}
