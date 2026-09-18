using System;
using UnityEngine;

namespace LiquidVolumeFX
{
	public class CylinderManager : MonoBehaviour
	{
		public float startingDelay = 1f;

		public int numCylinders = 16;

		public float scale = 0.2f;

		public float heightMultiplier = 2f;

		public float circleRadius = 1.75f;

		private void Update()
		{
			if (!(Time.time < startingDelay))
			{
				for (int i = 0; i < numCylinders; i++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/CylinderFlask"));
					gameObject.hideFlags = HideFlags.DontSave;
					gameObject.transform.SetParent(base.transform, worldPositionStays: false);
					gameObject.transform.localScale = new Vector3(scale, scale * heightMultiplier, scale);
					float x = Mathf.Cos((float)i / (float)numCylinders * MathF.PI * 2f) * circleRadius;
					float z = Mathf.Sin((float)i / (float)numCylinders * MathF.PI * 2f) * circleRadius;
					gameObject.transform.position = new Vector3(x, -2f, z);
					FlaskAnimator flaskAnimator = gameObject.AddComponent<FlaskAnimator>();
					flaskAnimator.initialPosition = gameObject.transform.position;
					flaskAnimator.finalPosition = gameObject.transform.position + Vector3.up;
					flaskAnimator.duration = 5f + (float)i * 0.5f;
					flaskAnimator.acceleration = 0.001f;
					flaskAnimator.delay = 4f;
					LiquidVolume component = gameObject.GetComponent<LiquidVolume>();
					component.liquidColor1 = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
					component.liquidColor2 = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
					component.turbulence2 = 0f;
					component.refractionBlur = false;
				}
				UnityEngine.Object.Destroy(this);
			}
		}
	}
}
