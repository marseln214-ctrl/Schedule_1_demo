using System;
using UnityEngine;

namespace LiquidVolumeFX
{
	public class CubeSpawn : MonoBehaviour
	{
		public int instances = 150;

		public float radius = 2f;

		public float jitter = 0.5f;

		public float expansion = 0.04f;

		public float laps = 2f;

		private void Start()
		{
			for (int i = 1; i <= instances; i++)
			{
				GameObject obj = UnityEngine.Object.Instantiate(base.gameObject);
				obj.GetComponent<CubeSpawn>().enabled = false;
				obj.name = "Cube" + i;
				float f = (float)i / (float)instances * MathF.PI * 2f * laps;
				float num = (float)i * expansion;
				float x = Mathf.Cos(f) * (radius + num);
				float z = Mathf.Sin(f) * (radius + num);
				Vector3 vector = UnityEngine.Random.insideUnitSphere * jitter;
				obj.transform.position = base.transform.position + new Vector3(x, 0f, z) + vector;
				obj.transform.localScale *= 1f - UnityEngine.Random.value * jitter;
			}
		}
	}
}
