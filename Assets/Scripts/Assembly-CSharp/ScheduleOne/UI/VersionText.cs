using TMPro;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class VersionText : MonoBehaviour
	{
		private void Awake()
		{
			GetComponent<TextMeshProUGUI>().text = "Free Sample v" + Application.version;
		}
	}
}
