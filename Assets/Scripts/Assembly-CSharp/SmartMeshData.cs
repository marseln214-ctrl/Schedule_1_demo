using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class SmartMeshData
{
	private Material[] _materials;

	public Mesh mesh { get; private set; }

	public Matrix4x4 transform { get; private set; }

	public IList<Material> materials => new ReadOnlyCollection<Material>(_materials);

	public SmartMeshData(Mesh inMesh, Material[] inMaterials, Matrix4x4 inTransform)
	{
		mesh = inMesh;
		_materials = inMaterials;
		transform = inTransform;
		if (_materials.Length == mesh.subMeshCount)
		{
			return;
		}
		Debug.LogWarning("SmartMeshData has incorrect number of materials. Resizing to match submesh count");
		Material[] array = new Material[mesh.subMeshCount];
		for (int i = 0; i < _materials.Length; i++)
		{
			if (i < _materials.Length)
			{
				array[i] = _materials[i];
			}
			else
			{
				array[i] = null;
			}
		}
		_materials = array;
	}

	public SmartMeshData(Mesh inputMesh, Material[] inputMaterials)
		: this(inputMesh, inputMaterials, Matrix4x4.identity)
	{
	}

	public SmartMeshData(Mesh inputMesh, Material[] inputMaterials, Vector3 position)
		: this(inputMesh, inputMaterials, Matrix4x4.TRS(position, Quaternion.identity, Vector3.one))
	{
	}

	public SmartMeshData(Mesh inputMesh, Material[] inputMaterials, Vector3 position, Quaternion rotation)
		: this(inputMesh, inputMaterials, Matrix4x4.TRS(position, rotation, Vector3.one))
	{
	}

	public SmartMeshData(Mesh inputMesh, Material[] inputMaterials, Vector3 position, Quaternion rotation, Vector3 scale)
		: this(inputMesh, inputMaterials, Matrix4x4.TRS(position, rotation, scale))
	{
	}
}
