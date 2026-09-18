using ScheduleOne.ConstructableScripts;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Misc
{
	public class ToggleableLight : MonoBehaviour
	{
		public bool isOn;

		[Header("References")]
		[SerializeField]
		protected OptimizedLight[] lightSources;

		[SerializeField]
		protected MeshRenderer[] lightSurfacesMeshes;

		public int MaterialIndex;

		[Header("Materials")]
		[SerializeField]
		protected Material lightOnMat;

		[SerializeField]
		protected Material lightOffMat;

		private Constructable_GridBased constructable;

		private bool lightsApplied;

		protected virtual void Awake()
		{
			constructable = GetComponentInParent<Constructable_GridBased>();
			SetLights(isOn);
		}

		private void OnValidate()
		{
			if (isOn != lightsApplied)
			{
				SetLights(isOn);
			}
		}

		protected virtual void Update()
		{
			if (isOn != lightsApplied)
			{
				SetLights(isOn);
			}
		}

		public void TurnOn()
		{
			isOn = true;
			Update();
		}

		public void TurnOff()
		{
			isOn = false;
			Update();
		}

		protected virtual void SetLights(bool active)
		{
			lightsApplied = isOn;
			OptimizedLight[] array = lightSources;
			foreach (OptimizedLight optimizedLight in array)
			{
				if (!(optimizedLight == null))
				{
					optimizedLight.Enabled = active;
				}
			}
			Material material = (active ? lightOnMat : lightOffMat);
			MeshRenderer[] array2 = lightSurfacesMeshes;
			foreach (MeshRenderer meshRenderer in array2)
			{
				if (!(meshRenderer == null))
				{
					Material[] sharedMaterials = meshRenderer.sharedMaterials;
					sharedMaterials[MaterialIndex] = material;
					meshRenderer.materials = sharedMaterials;
				}
			}
		}
	}
}
