using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCombiner : MonoBehaviour
{
	private const int Mesh16BitBufferVertexLimit = 65535;

	[SerializeField]
	private bool createMultiMaterialMesh = true;

	[SerializeField]
	private bool combineInactiveChildren;

	[SerializeField]
	private bool deactivateCombinedChildren;

	[SerializeField]
	private bool deactivateCombinedChildrenMeshRenderers;

	[SerializeField]
	private bool generateUVMap;

	[SerializeField]
	private bool destroyCombinedChildren = true;

	[SerializeField]
	private string folderPath = "Prefabs/CombinedMeshes";

	[SerializeField]
	[Tooltip("MeshFilters with Meshes which we don't want to combine into one Mesh.")]
	private MeshFilter[] meshFiltersToSkip = new MeshFilter[0];

	public bool CreateMultiMaterialMesh
	{
		get
		{
			return createMultiMaterialMesh;
		}
		set
		{
			createMultiMaterialMesh = value;
		}
	}

	public bool CombineInactiveChildren
	{
		get
		{
			return combineInactiveChildren;
		}
		set
		{
			combineInactiveChildren = value;
		}
	}

	public bool DeactivateCombinedChildren
	{
		get
		{
			return deactivateCombinedChildren;
		}
		set
		{
			deactivateCombinedChildren = value;
			CheckDeactivateCombinedChildren();
		}
	}

	public bool DeactivateCombinedChildrenMeshRenderers
	{
		get
		{
			return deactivateCombinedChildrenMeshRenderers;
		}
		set
		{
			deactivateCombinedChildrenMeshRenderers = value;
			CheckDeactivateCombinedChildren();
		}
	}

	public bool GenerateUVMap
	{
		get
		{
			return generateUVMap;
		}
		set
		{
			generateUVMap = value;
		}
	}

	public bool DestroyCombinedChildren
	{
		get
		{
			return destroyCombinedChildren;
		}
		set
		{
			destroyCombinedChildren = value;
			CheckDestroyCombinedChildren();
		}
	}

	public string FolderPath
	{
		get
		{
			return folderPath;
		}
		set
		{
			folderPath = value;
		}
	}

	private void CheckDeactivateCombinedChildren()
	{
		if (deactivateCombinedChildren || deactivateCombinedChildrenMeshRenderers)
		{
			destroyCombinedChildren = false;
		}
	}

	private void CheckDestroyCombinedChildren()
	{
		if (destroyCombinedChildren)
		{
			deactivateCombinedChildren = false;
			deactivateCombinedChildrenMeshRenderers = false;
		}
	}

	public void CombineMeshes(bool showCreatedMeshInfo)
	{
		Vector3 localScale = base.transform.localScale;
		int siblingIndex = base.transform.GetSiblingIndex();
		Transform parent = base.transform.parent;
		base.transform.parent = null;
		Quaternion rotation = base.transform.rotation;
		Vector3 position = base.transform.position;
		Vector3 localScale2 = base.transform.localScale;
		base.transform.rotation = Quaternion.identity;
		base.transform.position = Vector3.zero;
		base.transform.localScale = Vector3.one;
		if (!createMultiMaterialMesh)
		{
			CombineMeshesWithSingleMaterial(showCreatedMeshInfo);
		}
		else
		{
			CombineMeshesWithMutliMaterial(showCreatedMeshInfo);
		}
		base.transform.rotation = rotation;
		base.transform.position = position;
		base.transform.localScale = localScale2;
		base.transform.parent = parent;
		base.transform.SetSiblingIndex(siblingIndex);
		base.transform.localScale = localScale;
	}

	private MeshFilter[] GetMeshFiltersToCombine()
	{
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(combineInactiveChildren);
		meshFiltersToSkip = meshFiltersToSkip.Where((MeshFilter meshFilter) => meshFilter != meshFilters[0]).ToArray();
		meshFiltersToSkip = meshFiltersToSkip.Where((MeshFilter meshFilter) => meshFilter != null).ToArray();
		int i;
		for (i = 0; i < meshFiltersToSkip.Length; i++)
		{
			meshFilters = meshFilters.Where((MeshFilter meshFilter) => meshFilter != meshFiltersToSkip[i]).ToArray();
		}
		return meshFilters;
	}

	private void CombineMeshesWithSingleMaterial(bool showCreatedMeshInfo)
	{
		MeshFilter[] meshFiltersToCombine = GetMeshFiltersToCombine();
		CombineInstance[] array = new CombineInstance[meshFiltersToCombine.Length - 1];
		long num = 0L;
		for (int i = 0; i < meshFiltersToCombine.Length - 1; i++)
		{
			array[i].subMeshIndex = 0;
			array[i].mesh = meshFiltersToCombine[i + 1].sharedMesh;
			array[i].transform = meshFiltersToCombine[i + 1].transform.localToWorldMatrix;
			num += array[i].mesh.vertices.Length;
		}
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>(combineInactiveChildren);
		if (componentsInChildren.Length >= 2)
		{
			componentsInChildren[0].sharedMaterials = new Material[1];
			componentsInChildren[0].sharedMaterial = componentsInChildren[1].sharedMaterial;
		}
		else
		{
			componentsInChildren[0].sharedMaterials = new Material[0];
		}
		Mesh mesh = new Mesh();
		mesh.name = base.name;
		if (num > 65535)
		{
			mesh.indexFormat = IndexFormat.UInt32;
		}
		mesh.CombineMeshes(array);
		GenerateUV(mesh);
		meshFiltersToCombine[0].sharedMesh = mesh;
		DeactivateCombinedGameObjects(meshFiltersToCombine);
		if (showCreatedMeshInfo)
		{
			if (num <= 65535)
			{
				Debug.Log("<color=#00cc00><b>Mesh \"" + base.name + "\" was created from " + array.Length + " children meshes and has " + num + " vertices.</b></color>");
			}
			else
			{
				Debug.Log("<color=#ff3300><b>Mesh \"" + base.name + "\" was created from " + array.Length + " children meshes and has " + num + " vertices. Some old devices, like Android with Mali-400 GPU, do not support over 65535 vertices.</b></color>");
			}
		}
	}

	private void CombineMeshesWithMutliMaterial(bool showCreatedMeshInfo)
	{
		MeshFilter[] meshFiltersToCombine = GetMeshFiltersToCombine();
		MeshRenderer[] array = new MeshRenderer[meshFiltersToCombine.Length];
		array[0] = GetComponent<MeshRenderer>();
		List<Material> list = new List<Material>();
		for (int i = 0; i < meshFiltersToCombine.Length - 1; i++)
		{
			array[i + 1] = meshFiltersToCombine[i + 1].GetComponent<MeshRenderer>();
			if (!(array[i + 1] != null))
			{
				continue;
			}
			Material[] sharedMaterials = array[i + 1].sharedMaterials;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				if (!list.Contains(sharedMaterials[j]))
				{
					list.Add(sharedMaterials[j]);
				}
			}
		}
		List<CombineInstance> list2 = new List<CombineInstance>();
		long num = 0L;
		for (int k = 0; k < list.Count; k++)
		{
			List<CombineInstance> list3 = new List<CombineInstance>();
			for (int l = 0; l < meshFiltersToCombine.Length - 1; l++)
			{
				if (!(array[l + 1] != null))
				{
					continue;
				}
				Material[] sharedMaterials2 = array[l + 1].sharedMaterials;
				for (int m = 0; m < sharedMaterials2.Length; m++)
				{
					if (list[k] == sharedMaterials2[m])
					{
						CombineInstance item = default(CombineInstance);
						item.subMeshIndex = m;
						item.mesh = meshFiltersToCombine[l + 1].sharedMesh;
						item.transform = meshFiltersToCombine[l + 1].transform.localToWorldMatrix;
						list3.Add(item);
						num += item.mesh.vertices.Length;
					}
				}
			}
			Mesh mesh = new Mesh();
			if (num > 65535)
			{
				mesh.indexFormat = IndexFormat.UInt32;
			}
			mesh.CombineMeshes(list3.ToArray(), mergeSubMeshes: true);
			CombineInstance item2 = default(CombineInstance);
			item2.subMeshIndex = 0;
			item2.mesh = mesh;
			item2.transform = Matrix4x4.identity;
			list2.Add(item2);
		}
		array[0].sharedMaterials = list.ToArray();
		Mesh mesh2 = new Mesh();
		mesh2.name = base.name;
		if (num > 65535)
		{
			mesh2.indexFormat = IndexFormat.UInt32;
		}
		mesh2.CombineMeshes(list2.ToArray(), mergeSubMeshes: false);
		GenerateUV(mesh2);
		meshFiltersToCombine[0].sharedMesh = mesh2;
		DeactivateCombinedGameObjects(meshFiltersToCombine);
		if (showCreatedMeshInfo)
		{
			if (num <= 65535)
			{
				Debug.Log("<color=#00cc00><b>Mesh \"" + base.name + "\" was created from " + (meshFiltersToCombine.Length - 1) + " children meshes and has " + list2.Count + " submeshes, and " + num + " vertices.</b></color>");
			}
			else
			{
				Debug.Log("<color=#ff3300><b>Mesh \"" + base.name + "\" was created from " + (meshFiltersToCombine.Length - 1) + " children meshes and has " + list2.Count + " submeshes, and " + num + " vertices. Some old devices, like Android with Mali-400 GPU, do not support over 65535 vertices.</b></color>");
			}
		}
	}

	private void DeactivateCombinedGameObjects(MeshFilter[] meshFilters)
	{
		for (int i = 0; i < meshFilters.Length - 1; i++)
		{
			if (!destroyCombinedChildren)
			{
				if (deactivateCombinedChildren)
				{
					meshFilters[i + 1].gameObject.SetActive(value: false);
				}
				if (deactivateCombinedChildrenMeshRenderers)
				{
					MeshRenderer component = meshFilters[i + 1].gameObject.GetComponent<MeshRenderer>();
					if (component != null)
					{
						component.enabled = false;
					}
				}
			}
			else
			{
				Object.DestroyImmediate(meshFilters[i + 1].gameObject);
			}
		}
	}

	private void GenerateUV(Mesh combinedMesh)
	{
	}
}
