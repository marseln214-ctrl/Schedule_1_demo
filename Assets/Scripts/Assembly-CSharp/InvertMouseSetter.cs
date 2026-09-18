using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.UI;

public class InvertMouseSetter : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<Toggle>().onValueChanged.AddListener(delegate(bool x)
		{
			Singleton<Settings>.Instance.InvertMouse = x;
		});
	}
}
