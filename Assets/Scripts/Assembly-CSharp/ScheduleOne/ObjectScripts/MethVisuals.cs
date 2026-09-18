using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class MethVisuals : MonoBehaviour
	{
		public MeshRenderer[] Meshes;

		public void Setup(MethDefinition definition)
		{
			MeshRenderer[] meshes = Meshes;
			for (int i = 0; i < meshes.Length; i++)
			{
				meshes[i].material = definition.CrystalMaterial;
			}
		}
	}
}
