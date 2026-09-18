using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIScaleSetter : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<Slider>().onValueChanged.AddListener(delegate(float x)
		{
			ScheduleOne.UI.CanvasScaler.SetScaleFactor(x);
		});
	}
}
