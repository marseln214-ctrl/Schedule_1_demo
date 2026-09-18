using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class ButtonRequireInputFields : MonoBehaviour
	{
		[Serializable]
		public class Input
		{
			public TMP_InputField InputField;

			public RectTransform ErrorMessage;
		}

		public List<Input> Inputs;

		public Button Button;

		public void Update()
		{
			Button.interactable = true;
			foreach (Input input in Inputs)
			{
				if (string.IsNullOrEmpty(input.InputField.text))
				{
					input.ErrorMessage.gameObject.SetActive(value: true);
					Button.interactable = false;
				}
				else
				{
					input.ErrorMessage.gameObject.SetActive(value: false);
				}
			}
		}
	}
}
