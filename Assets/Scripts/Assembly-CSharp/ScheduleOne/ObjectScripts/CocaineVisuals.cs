using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class CocaineVisuals : MonoBehaviour
	{
		public MeshRenderer[] Meshes;

		public void Setup(CocaineDefinition definition)
		{
			MeshRenderer[] meshes = Meshes;
			for (int i = 0; i < meshes.Length; i++)
			{
				meshes[i].material = definition.RockMaterial;
			}
		}
	}
}
