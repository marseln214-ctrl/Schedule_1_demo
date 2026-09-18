using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySetter : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<Slider>().onValueChanged.AddListener(delegate(float x)
		{
			Singleton<Settings>.Instance.LookSensitivity = x;
		});
	}
}
