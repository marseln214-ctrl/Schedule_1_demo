using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCreator
{
	public class CharacterCreatorMenu : MonoBehaviour
	{
		[Serializable]
		public class Window
		{
			public string Name;

			public RectTransform Container;

			public void Open()
			{
				Container.gameObject.SetActive(value: true);
			}

			public void Close()
			{
				Container.gameObject.SetActive(value: false);
			}
		}

		public Window[] Windows;

		[Header("References")]
		public TextMeshProUGUI CategoryLabel;

		public Button BackButton;

		public Button NextButton;

		private int openWindowIndex;

		private Window openWindow;

		public void Awake()
		{
			Window[] windows = Windows;
			for (int i = 0; i < windows.Length; i++)
			{
				windows[i].Close();
			}
			OpenWindow(0);
		}

		public void OpenWindow(int index)
		{
			if (openWindow != null)
			{
				openWindow.Close();
			}
			openWindowIndex = index;
			openWindow = Windows[index];
			openWindow.Open();
			CategoryLabel.text = openWindow.Name;
			BackButton.interactable = index > 0;
			NextButton.interactable = index < Windows.Length - 1;
		}

		public void Back()
		{
			OpenWindow(openWindowIndex - 1);
		}

		public void Next()
		{
			OpenWindow(openWindowIndex + 1);
		}
	}
}
