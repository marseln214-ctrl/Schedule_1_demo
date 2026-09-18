using UnityEngine;

namespace RadiantGI.Demos
{
	public class ShootGlowingBalls : MonoBehaviour
	{
		public int count;

		public Transform center;

		public GameObject glowingBall;

		private void Start()
		{
			for (int i = 0; i < count; i++)
			{
				GameObject obj = Object.Instantiate(glowingBall, center.position + Vector3.right * Random.Range(-4, 4) + Vector3.up * (5f + (float)i), Quaternion.identity);
				Color color = Random.ColorHSV();
				float value = Random.value;
				if (value < 0.33f)
				{
					color.r *= 0.2f;
				}
				else if (value < 0.66f)
				{
					color.g *= 0.2f;
				}
				else
				{
					color.b *= 0.2f;
				}
				Renderer component = obj.GetComponent<Renderer>();
				component.transform.localScale = Vector3.one * Random.Range(0.65f, 1f);
				component.material.color = color;
				component.material.SetColor("_EmissionColor", color * 2f);
			}
		}
	}
}
