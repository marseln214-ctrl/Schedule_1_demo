using System.Collections.Generic;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class NumberFieldUI : MonoBehaviour
	{
		[Header("References")]
		public TextMeshProUGUI FieldLabel;

		public Slider Slider;

		public TextMeshProUGUI ValueLabel;

		public TextMeshProUGUI MinValueLabel;

		public TextMeshProUGUI MaxValueLabel;

		public List<NumberField> Fields { get; protected set; } = new List<NumberField>();


		public void Bind(List<NumberField> field)
		{
			Fields = new List<NumberField>();
			Fields.AddRange(field);
			Fields[Fields.Count - 1].onItemChanged.AddListener(Refresh);
			MinValueLabel.text = Fields[0].MinValue.ToString();
			MaxValueLabel.text = Fields[0].MaxValue.ToString();
			Slider.minValue = Fields[0].MinValue;
			Slider.maxValue = Fields[0].MaxValue;
			Slider.wholeNumbers = Fields[0].WholeNumbers;
			Slider.onValueChanged.AddListener(ValueChanged);
			Refresh(Fields[0].Value);
		}

		private void Refresh(float newVal)
		{
			if (AreFieldsUniform())
			{
				ValueLabel.text = newVal.ToString();
			}
			else
			{
				ValueLabel.text = "#";
			}
			Slider.SetValueWithoutNotify(newVal);
		}

		private bool AreFieldsUniform()
		{
			for (int i = 0; i < Fields.Count - 1; i++)
			{
				if (Fields[i].Value != Fields[i + 1].Value)
				{
					return false;
				}
			}
			return true;
		}

		public void ValueChanged(float value)
		{
			for (int i = 0; i < Fields.Count; i++)
			{
				Fields[i].SetValue(value, network: true);
			}
		}
	}
}
