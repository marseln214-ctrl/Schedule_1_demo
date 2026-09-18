using ScheduleOne.StationFramework;
using UnityEngine;

namespace ScheduleOne.Product
{
	public class LiquidMethVisuals : MonoBehaviour
	{
		public MeshRenderer StaticLiquidMesh;

		public LiquidContainer LiquidContainer;

		public ParticleSystem PourParticles;

		public void Setup(LiquidMethDefinition def)
		{
			if (!(def == null))
			{
				if (StaticLiquidMesh != null)
				{
					StaticLiquidMesh.material.color = def.StaticLiquidColor;
				}
				if (LiquidContainer != null)
				{
					LiquidContainer.SetLiquidColor(def.LiquidVolumeColor);
				}
				if (PourParticles != null)
				{
					ParticleSystem.MainModule main = PourParticles.main;
					main.startColor = def.PourParticlesColor;
				}
			}
		}
	}
}
