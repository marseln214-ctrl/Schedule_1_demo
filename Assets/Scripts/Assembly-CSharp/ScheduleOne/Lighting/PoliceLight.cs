using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Lighting
{
	public class PoliceLight : MonoBehaviour
	{
		public bool IsOn;

		[Header("References")]
		public MeshRenderer[] RedMeshes;

		public MeshRenderer[] BlueMeshes;

		public OptimizedLight[] RedLights;

		public OptimizedLight[] BlueLights;

		public AudioSourceController Siren;

		[Header("Settings")]
		public float CycleDuration = 0.5f;

		public Material RedOffMat;

		public Material RedOnMat;

		public Material BlueOffMat;

		public Material BlueOnMat;

		public AnimationCurve RedBrightnessCurve;

		public AnimationCurve BlueBrightnessCurve;

		public float LightBrightness = 5f;

		private Coroutine cycleRoutine;

		public void SetIsOn(bool isOn)
		{
			IsOn = isOn;
		}

		private void FixedUpdate()
		{
			if (IsOn)
			{
				if (!Siren.isPlaying)
				{
					Siren.Play();
				}
				if (cycleRoutine == null)
				{
					cycleRoutine = StartCoroutine(CycleCoroutine());
				}
			}
			else if (Siren.isPlaying)
			{
				Siren.Stop();
			}
		}

		protected IEnumerator CycleCoroutine()
		{
			OptimizedLight[] redLights = RedLights;
			foreach (OptimizedLight obj in redLights)
			{
				obj.light.intensity = 0f;
				obj.Enabled = true;
			}
			redLights = BlueLights;
			foreach (OptimizedLight obj2 in redLights)
			{
				obj2.light.intensity = 0f;
				obj2.Enabled = true;
			}
			float time = 0f;
			MeshRenderer[] redMeshes;
			while (IsOn)
			{
				time += Time.deltaTime;
				float time2 = time / CycleDuration % 1f;
				float num = RedBrightnessCurve.Evaluate(time2);
				float num2 = BlueBrightnessCurve.Evaluate(time2);
				redLights = RedLights;
				for (int i = 0; i < redLights.Length; i++)
				{
					redLights[i].light.intensity = num * LightBrightness;
				}
				redMeshes = RedMeshes;
				for (int i = 0; i < redMeshes.Length; i++)
				{
					redMeshes[i].material = ((num > 0f) ? RedOnMat : RedOffMat);
				}
				redLights = BlueLights;
				for (int i = 0; i < redLights.Length; i++)
				{
					redLights[i].light.intensity = num2 * LightBrightness;
				}
				redMeshes = BlueMeshes;
				for (int i = 0; i < redMeshes.Length; i++)
				{
					redMeshes[i].material = ((num2 > 0f) ? BlueOnMat : BlueOffMat);
				}
				yield return new WaitForEndOfFrame();
			}
			redLights = RedLights;
			foreach (OptimizedLight obj3 in redLights)
			{
				obj3.light.intensity = 0f;
				obj3.Enabled = false;
			}
			redMeshes = RedMeshes;
			for (int i = 0; i < redMeshes.Length; i++)
			{
				redMeshes[i].material = RedOffMat;
			}
			redLights = BlueLights;
			foreach (OptimizedLight obj4 in redLights)
			{
				obj4.light.intensity = 0f;
				obj4.Enabled = false;
			}
			redMeshes = BlueMeshes;
			for (int i = 0; i < redMeshes.Length; i++)
			{
				redMeshes[i].material = BlueOffMat;
			}
			cycleRoutine = null;
		}
	}
}
