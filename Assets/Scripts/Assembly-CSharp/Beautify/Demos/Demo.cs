using Beautify.Universal;
using UnityEngine;
using UnityEngine.UI;

namespace Beautify.Demos
{
	public class Demo : MonoBehaviour
	{
		public Texture lutTexture;

		//private void Start()
		//{
		//	UpdateText();
		//}

		//private void Update()
		//{
		//	if (Input.GetKeyDown(KeyCode.J))
		//	{
		//		BeautifySettings.settings.bloomIntensity.value += 0.1f;
		//	}
		//	if (Input.GetKeyDown(KeyCode.T) || Input.GetMouseButtonDown(0))
		//	{
		//		BeautifySettings.settings.disabled.value = !BeautifySettings.settings.disabled.value;
		//		UpdateText();
		//	}
		//	if (Input.GetKeyDown(KeyCode.B))
		//	{
		//		BeautifySettings.Blink(0.2f);
		//	}
		//	if (Input.GetKeyDown(KeyCode.C))
		//	{
		//		BeautifySettings.settings.compareMode.value = !BeautifySettings.settings.compareMode.value;
		//	}
		//	if (Input.GetKeyDown(KeyCode.N))
		//	{
		//		BeautifySettings.settings.nightVision.Override(!BeautifySettings.settings.nightVision.value);
		//	}
		//	if (Input.GetKeyDown(KeyCode.F))
		//	{
		//		if (BeautifySettings.settings.blurIntensity.overrideState)
		//		{
		//			BeautifySettings.settings.blurIntensity.overrideState = false;
		//		}
		//		else
		//		{
		//			BeautifySettings.settings.blurIntensity.Override(4f);
		//		}
		//	}
		//	if (Input.GetKeyDown(KeyCode.Alpha1))
		//	{
		//		BeautifySettings.settings.brightness.Override(0.1f);
		//	}
		//	if (Input.GetKeyDown(KeyCode.Alpha2))
		//	{
		//		BeautifySettings.settings.brightness.Override(0.5f);
		//	}
		//	if (Input.GetKeyDown(KeyCode.Alpha3))
		//	{
		//		BeautifySettings.settings.brightness.overrideState = false;
		//	}
		//	if (Input.GetKeyDown(KeyCode.Alpha4))
		//	{
		//		BeautifySettings.settings.outline.Override(x: true);
		//		BeautifySettings.settings.outlineColor.Override(Color.cyan);
		//		BeautifySettings.settings.outlineCustomize.Override(x: true);
		//		BeautifySettings.settings.outlineSpread.Override(1.5f);
		//	}
		//	if (Input.GetKeyDown(KeyCode.Alpha5))
		//	{
		//		BeautifySettings.settings.outline.overrideState = false;
		//	}
		//	if (Input.GetKeyDown(KeyCode.Alpha6))
		//	{
		//		BeautifySettings.settings.lut.Override(x: true);
		//		BeautifySettings.settings.lutIntensity.Override(1f);
		//		BeautifySettings.settings.lutTexture.Override(lutTexture);
		//	}
		//	if (Input.GetKeyDown(KeyCode.Alpha7))
		//	{
		//		BeautifySettings.settings.lut.Override(x: false);
		//	}
		//}

		//private void UpdateText()
		//{
		//	if (BeautifySettings.settings.disabled.value)
		//	{
		//		GameObject.Find("Beautify").GetComponent<Text>().text = "Beautify OFF";
		//	}
		//	else
		//	{
		//		GameObject.Find("Beautify").GetComponent<Text>().text = "Beautify ON";
		//	}
		//}
	}
}
