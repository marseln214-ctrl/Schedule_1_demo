using TMPro;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class VMSBoard : MonoBehaviour
	{
		[Header("References")]
		public TextMeshProUGUI Label;

		public void SetText(string text, Color col)
		{
			Label.text = text;
			Label.color = col;
		}

		public void SetText(string text)
		{
			SetText(text, new Color32(byte.MaxValue, 215, 50, byte.MaxValue));
		}
	}
}
