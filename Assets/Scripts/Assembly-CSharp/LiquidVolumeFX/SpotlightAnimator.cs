using UnityEngine;

namespace LiquidVolumeFX
{
	public class SpotlightAnimator : MonoBehaviour
	{
		public float lightOnDelay = 2f;

		public float targetIntensity = 3.5f;

		public float initialIntensity;

		public float duration = 3f;

		public float nextColorInterval = 2f;

		public float colorChangeDuration = 2f;

		private Light spotLight;

		private float lastColorChange;

		private float colorChangeStarted;

		private Color currentColor;

		private Color nextColor;

		private bool changingColor;

		private void Awake()
		{
			spotLight = GetComponent<Light>();
			spotLight.intensity = 0f;
		}

		private void Update()
		{
			if (Time.time < lightOnDelay)
			{
				return;
			}
			float t = (Time.time - lightOnDelay) / duration;
			spotLight.intensity = Mathf.Lerp(initialIntensity, targetIntensity, t);
			if (!(Time.time - lastColorChange > nextColorInterval))
			{
				return;
			}
			if (changingColor)
			{
				t = (Time.time - colorChangeStarted) / colorChangeDuration;
				if (t >= 1f)
				{
					changingColor = false;
					lastColorChange = Time.time;
				}
				spotLight.color = Color.Lerp(currentColor, nextColor, t);
			}
			else
			{
				currentColor = spotLight.color;
				nextColor = new Color(Mathf.Clamp01(Random.value + 0.25f), Mathf.Clamp01(Random.value + 0.25f), Mathf.Clamp01(Random.value + 0.25f), 1f);
				changingColor = true;
				colorChangeStarted = Time.time;
			}
		}
	}
}
