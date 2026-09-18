using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.AvatarFramework.Customization
{
	public class ACWindow : MonoBehaviour
	{
		[Header("Settings")]
		public string WindowTitle;

		public ACWindow Predecessor;

		[Header("References")]
		public TextMeshProUGUI TitleText;

		public Button BackButton;

		private void Start()
		{
			TitleText.text = WindowTitle;
			if (Predecessor != null)
			{
				BackButton.onClick.AddListener(Close);
				BackButton.gameObject.SetActive(value: true);
			}
			else
			{
				BackButton.gameObject.SetActive(value: false);
			}
			if (Predecessor != null)
			{
				base.gameObject.SetActive(value: false);
			}
		}

		public void Open()
		{
			base.gameObject.SetActive(value: true);
		}

		public void Close()
		{
			base.gameObject.SetActive(value: false);
			if (Predecessor != null)
			{
				Predecessor.Open();
			}
		}
	}
}
