using EasyButtons;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class SetRendererMaterial : MonoBehaviour
	{
		public Material Material;

		[Button]
		public void SetMaterial()
		{
			MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				Material[] sharedMaterials = meshRenderer.sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					sharedMaterials[j] = Material;
				}
				meshRenderer.sharedMaterials = sharedMaterials;
			}
		}
	}
}
