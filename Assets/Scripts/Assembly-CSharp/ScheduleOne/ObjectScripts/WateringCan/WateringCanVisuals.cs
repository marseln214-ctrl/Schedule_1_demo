using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.ObjectScripts.WateringCan
{
	public class WateringCanVisuals : MonoBehaviour
	{
		public ParticleSystem OverflowParticles;

		public Transform WaterTransform;

		public float WaterMaxY;

		public float WaterMinY;

		public Transform SideWaterTransform;

		public float SideWaterMinScale;

		public float SideWaterMaxScale;

		public AudioSourceController FillSound;

		public virtual void SetFillLevel(float normalizedFillLevel)
		{
			WaterTransform.localPosition = new Vector3(WaterTransform.localPosition.x, Mathf.Lerp(WaterMinY, WaterMaxY, normalizedFillLevel), WaterTransform.localPosition.z);
			SideWaterTransform.localScale = new Vector3(Mathf.Lerp(SideWaterMinScale, SideWaterMaxScale, normalizedFillLevel), SideWaterTransform.localScale.y, SideWaterTransform.localScale.z);
			SideWaterTransform.localPosition = new Vector3(SideWaterTransform.localPosition.x, SideWaterTransform.localPosition.y, (0f - SideWaterTransform.localScale.x) * 0.5f);
		}

		public void SetOverflowParticles(bool enabled)
		{
			if (enabled)
			{
				if (!OverflowParticles.isPlaying)
				{
					OverflowParticles.Play();
				}
			}
			else if (OverflowParticles.isPlaying)
			{
				OverflowParticles.Stop();
			}
		}
	}
}
