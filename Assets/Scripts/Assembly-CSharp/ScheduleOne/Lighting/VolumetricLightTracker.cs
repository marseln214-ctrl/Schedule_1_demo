using ScheduleOne.DevUtilities;
using UnityEngine;
using VLB;

namespace ScheduleOne.Lighting
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Light))]
	[RequireComponent(typeof(VolumetricLightBeamSD))]
	public class VolumetricLightTracker : MonoBehaviour
	{
		public bool Override;

		public bool Enabled;

		public Light light;

		public OptimizedLight optimizedLight;

		public VolumetricLightBeamSD beam;

		public VolumetricDustParticles dust;

		private void OnValidate()
		{
			if (light == null)
			{
				light = GetComponent<Light>();
			}
			if (optimizedLight == null)
			{
				optimizedLight = GetComponent<OptimizedLight>();
			}
			if (beam == null)
			{
				beam = GetComponent<VolumetricLightBeamSD>();
			}
			if (dust == null)
			{
				dust = GetComponent<VolumetricDustParticles>();
			}
		}

		private void LateUpdate()
		{
			if (Override)
			{
				beam.enabled = Enabled;
			}
			else if (optimizedLight != null)
			{
				beam.enabled = optimizedLight.Enabled;
			}
			else if (light != null)
			{
				beam.enabled = light.enabled;
			}
			if (dust != null)
			{
				dust.enabled = beam.enabled;
			}
		}
	}
}
