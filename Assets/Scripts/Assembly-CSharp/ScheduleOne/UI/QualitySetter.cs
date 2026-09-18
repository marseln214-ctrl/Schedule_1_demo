using TMPro;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class QualitySetter : MonoBehaviour
	{
		private void Awake()
		{
			GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate(int x)
			{
				SetQuality(x);
			});
		}

		private void SetQuality(int quality)
		{
			Console.Log("Setting quality to " + quality);
			QualitySettings.SetQualityLevel(quality);
		}
	}
}
