using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.AvatarFramework.Customization
{
	public abstract class ACSelection<T> : MonoBehaviour where T : Object
	{
		[Header("References")]
		public GameObject ButtonPrefab;

		[Header("Settings")]
		public int PropertyIndex;

		public List<T> Options = new List<T>();

		public bool Nullable = true;

		public int DefaultOptionIndex;

		protected List<GameObject> buttons = new List<GameObject>();

		protected int SelectedOptionIndex = -1;

		public UnityEvent<T> onValueChange;

		public UnityEvent<T, int> onValueChangeWithIndex;

		protected virtual void Awake()
		{
			for (int i = 0; i < Options.Count; i++)
			{
				GameObject gameObject = Object.Instantiate(ButtonPrefab, base.transform);
				gameObject.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = GetOptionLabel(i);
				buttons.Add(gameObject);
				int index = i;
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					SelectOption(index);
				});
			}
		}

		public void SelectOption(int index, bool notify = true)
		{
			int selectedOptionIndex = SelectedOptionIndex;
			if (index != SelectedOptionIndex)
			{
				if (SelectedOptionIndex != -1)
				{
					SetButtonHighlighted(SelectedOptionIndex, h: false);
				}
				SelectedOptionIndex = index;
				SetButtonHighlighted(SelectedOptionIndex, h: true);
			}
			else if (Nullable)
			{
				SetButtonHighlighted(SelectedOptionIndex, h: false);
				SelectedOptionIndex = -1;
			}
			if (selectedOptionIndex != SelectedOptionIndex && notify)
			{
				CallValueChange();
			}
		}

		public abstract void CallValueChange();

		public abstract string GetOptionLabel(int index);

		public abstract int GetAssetPathIndex(string path);

		private void SetButtonHighlighted(int buttonIndex, bool h)
		{
			if (buttonIndex != -1)
			{
				buttons[buttonIndex].transform.Find("Indicator").gameObject.SetActive(h);
			}
		}
	}
}
