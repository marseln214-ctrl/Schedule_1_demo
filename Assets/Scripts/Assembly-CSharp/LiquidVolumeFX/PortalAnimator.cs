using UnityEngine;

namespace LiquidVolumeFX
{
	public class PortalAnimator : MonoBehaviour
	{
		public float delay = 2f;

		public float duration = 1f;

		public float delayFadeOut = 4f;

		private Vector3 scale;

		private void Start()
		{
			scale = base.transform.localScale;
			base.transform.localScale = Vector3.zero;
		}

		private void Update()
		{
			if (!(Time.time < delay))
			{
				float value = ((!(Time.time > delayFadeOut)) ? ((Time.time - delay) / duration) : (1f - (Time.time - delayFadeOut) / duration));
				base.transform.localScale = Mathf.Clamp01(value) * scale;
			}
		}
	}
}
