using UnityEngine;
using UnityEngine.UI;

namespace Funly.SkyStudio
{
	[RequireComponent(typeof(RawImage))]
	public class LoadOverheadDepthTexture : MonoBehaviour
	{
		private WeatherDepthCamera m_RainCamera;

		private void Start()
		{
			m_RainCamera = Object.FindObjectOfType<WeatherDepthCamera>();
		}

		private void Update()
		{
		}
	}
}
