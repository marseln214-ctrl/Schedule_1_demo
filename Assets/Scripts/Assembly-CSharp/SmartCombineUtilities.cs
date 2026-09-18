using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SmartCombineUtilities
{
	private class SmartSubmeshData
	{
		public Mesh mesh { get; private set; }

		public IList<CombineInstance> combineInstances { get; private set; }

		public SmartSubmeshData()
		{
			combineInstances = new List<CombineInstance>();
		}

		public void CombineSubmeshes()
		{
			if (mesh == null)
			{
				mesh = new Mesh();
			}
			else
			{
				mesh.Clear();
			}
			mesh.CombineMeshes(combineInstances.ToArray(), mergeSubMeshes: true, useMatrices: true);
		}
	}

	public static void CombineMeshesSmart(this Mesh mesh, SmartMeshData[] meshData, out Material[] materials)
	{
		IDictionary<Material, SmartSubmeshData> dictionary = new Dictionary<Material, SmartSubmeshData>();
		IList<CombineInstance> list = new List<CombineInstance>();
		foreach (SmartMeshData smartMeshData in meshData)
		{
			IList<Material> materials2 = smartMeshData.materials;
			for (int j = 0; j < smartMeshData.mesh.subMeshCount; j++)
			{
				SmartSubmeshData smartSubmeshData = null;
				if (dictionary.ContainsKey(materials2[j]))
				{
					smartSubmeshData = dictionary[materials2[j]];
				}
				else
				{
					smartSubmeshData = new SmartSubmeshData();
					dictionary.Add(materials2[j], smartSubmeshData);
				}
				CombineInstance item = default(CombineInstance);
				item.mesh = smartMeshData.mesh;
				item.subMeshIndex = j;
				item.transform = smartMeshData.transform;
				smartSubmeshData.combineInstances.Add(item);
			}
		}
		foreach (SmartSubmeshData value in dictionary.Values)
		{
			value.CombineSubmeshes();
			CombineInstance item2 = default(CombineInstance);
			item2.mesh = value.mesh;
			item2.subMeshIndex = 0;
			list.Add(item2);
		}
		mesh.Clear();
		mesh.CombineMeshes(list.ToArray(), mergeSubMeshes: false, useMatrices: false);
		mesh.Optimize();
		materials = dictionary.Keys.ToArray();
	}
}
