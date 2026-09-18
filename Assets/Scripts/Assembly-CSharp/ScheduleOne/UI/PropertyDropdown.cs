using System;
using System.Collections.Generic;
using ScheduleOne.Property;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class PropertyDropdown : MonoBehaviour
	{
		public ScheduleOne.Property.Property selectedProperty;

		private TMP_Dropdown TMP_dropdown;

		private Dropdown dropdown;

		private Dictionary<int, ScheduleOne.Property.Property> intToProperty = new Dictionary<int, ScheduleOne.Property.Property>();

		public Action onSelectionChanged;

		protected virtual void Awake()
		{
			List<string> list = new List<string>();
			list.Add("None");
			TMP_dropdown = GetComponent<TMP_Dropdown>();
			if (TMP_dropdown != null)
			{
				TMP_dropdown.onValueChanged.AddListener(ValueChanged);
				TMP_dropdown.AddOptions(list);
			}
			dropdown = GetComponent<Dropdown>();
			if (dropdown != null)
			{
				dropdown.onValueChanged.AddListener(ValueChanged);
				dropdown.AddOptions(list);
			}
			intToProperty.Add(0, null);
			ScheduleOne.Property.Property.onPropertyAcquired = (ScheduleOne.Property.Property.PropertyChange)Delegate.Combine(ScheduleOne.Property.Property.onPropertyAcquired, new ScheduleOne.Property.Property.PropertyChange(PropertyAcquired));
		}

		private void PropertyAcquired(ScheduleOne.Property.Property p)
		{
			List<string> list = new List<string>();
			list.Add(p.PropertyName);
			if (dropdown != null)
			{
				intToProperty.Add(dropdown.options.Count, p);
				dropdown.AddOptions(list);
			}
			if (TMP_dropdown != null)
			{
				intToProperty.Add(TMP_dropdown.options.Count, p);
				TMP_dropdown.AddOptions(list);
			}
		}

		private void ValueChanged(int newVal)
		{
			selectedProperty = intToProperty[newVal];
			if (onSelectionChanged != null)
			{
				onSelectionChanged();
			}
		}
	}
}
