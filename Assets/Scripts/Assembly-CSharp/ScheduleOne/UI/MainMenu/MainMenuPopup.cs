using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.MainMenu
{
	public class MainMenuPopup : Singleton<MainMenuPopup>
	{
		public class Data
		{
			public string Title;

			public string Description;

			public bool IsBad;

			public Data(string title, string description, bool isBad)
			{
				Title = title;
				Description = description;
				IsBad = isBad;
			}
		}

		public MainMenuScreen Screen;

		public TextMeshProUGUI Title;

		public TextMeshProUGUI Description;

		public void Open(Data data)
		{
			Open(data.Title, data.Description, data.IsBad);
		}

		public void Open(string title, string description, bool isBad)
		{
			Title.color = (isBad ? ((Color)new Color32(byte.MaxValue, 115, 115, byte.MaxValue)) : Color.white);
			Title.text = title;
			Description.text = description;
			Screen.Open(closePrevious: false);
		}
	}
}
