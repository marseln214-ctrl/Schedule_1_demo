using TMPro;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class FeedbackFormPopup : MonoBehaviour
	{
		public TextMeshProUGUI Label;

		public bool AutoClose = true;

		private float closeTime;

		public void Open(string text)
		{
			if (Label != null)
			{
				Label.text = text;
			}
			base.gameObject.SetActive(value: true);
			closeTime = Time.unscaledTime + 4f;
		}

		public void Close()
		{
			base.gameObject.SetActive(value: false);
		}

		private void Update()
		{
			if (AutoClose && Time.unscaledTime > closeTime)
			{
				Close();
			}
		}
	}
}
