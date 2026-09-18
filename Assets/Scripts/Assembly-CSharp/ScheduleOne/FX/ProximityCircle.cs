using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.FX
{
	public class ProximityCircle : MonoBehaviour
	{
		[Header("References")]
		public DecalProjector Circle;

		private bool enabledThisFrame;

		private void LateUpdate()
		{
			if (!enabledThisFrame)
			{
				SetAlpha(0f);
				enabledThisFrame = false;
			}
			enabledThisFrame = false;
		}

		public void SetRadius(float rad)
		{
			Circle.size = new Vector3(rad * 2f, rad * 2f, 3f);
		}

		public void SetAlpha(float alpha)
		{
			enabledThisFrame = true;
			Circle.fadeFactor = alpha;
			Circle.enabled = alpha > 0f;
		}

		public void SetColor(Color col)
		{
			Circle.material.color = col;
		}
	}
}
