using TMPro;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class CrosshairText : MonoBehaviour
	{
		public TextMeshProUGUI Label;

		private bool setThisFrame;

		private void Awake()
		{
			Hide();
		}

		private void LateUpdate()
		{
			if (!setThisFrame)
			{
				Label.enabled = false;
			}
			setThisFrame = false;
		}

		public void Show(string text, Color col = default(Color))
		{
			setThisFrame = true;
			Label.color = ((col != default(Color)) ? col : Color.white);
			Label.text = text;
			Label.enabled = true;
		}

		public void Hide()
		{
			Label.enabled = false;
		}
	}
}
