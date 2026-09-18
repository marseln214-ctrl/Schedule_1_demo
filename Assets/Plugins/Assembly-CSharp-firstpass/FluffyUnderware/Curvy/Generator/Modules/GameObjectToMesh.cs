using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Convert/GameObject To Mesh", ModuleName = "GameObject To Mesh", Description = "Converts GameObjects to Volume Meshes")]
	[HelpURL("https://curvyeditor.com/doclink/cggameobject2mesh")]
	public class GameObjectToMesh : CGModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGGameObject) }, Array = true)]
		public CGModuleInputSlot InGameObjects = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGVMesh), Array = true)]
		public CGModuleOutputSlot OutVMesh = new CGModuleOutputSlot();

		[SerializeField]
		[Tooltip("Whether to include or not the meshes from the input Game Objects' children")]
		private bool useChildrenMeshes;

		[SerializeField]
		[Tooltip("Forces the output mesh to be centered")]
		private bool centerMesh;

		public bool UseChildrenMeshes
		{
			get
			{
				return useChildrenMeshes;
			}
			set
			{
				if (value != useChildrenMeshes)
				{
					useChildrenMeshes = value;
					base.Dirty = true;
				}
			}
		}

		public bool CenterMesh
		{
			get
			{
				return centerMesh;
			}
			set
			{
				if (value != centerMesh)
				{
					centerMesh = value;
					base.Dirty = true;
				}
			}
		}

		public override void Reset()
		{
			base.Reset();
			UseChildrenMeshes = false;
			CenterMesh = false;
		}

		public override void Refresh()
		{
			base.Refresh();
			if (!OutVMesh.IsLinked)
			{
				return;
			}
			bool isDataDisposable;
			List<CGGameObject> allData = InGameObjects.GetAllData<CGGameObject>(out isDataDisposable, Array.Empty<CGDataRequestParameter>());
			List<CGVMesh> list = new List<CGVMesh>(allData.Count);
			foreach (CGGameObject item in allData)
			{
				GameObject @object = item.Object;
				if (@object == null)
				{
					continue;
				}
				Mesh mesh;
				Material[] materials2;
				if (UseChildrenMeshes)
				{
					mesh = CombineMeshFilters(@object.GetComponentsInChildren<MeshFilter>(includeInactive: false), out var materials, @object.transform.worldToLocalMatrix, UIMessages);
					materials2 = materials.ToArray();
				}
				else
				{
					MeshFilter component = @object.GetComponent<MeshFilter>();
					if (component == null)
					{
						UIMessages.Add("GameObject '" + @object.name + "' has no Mesh Filter associated to it. If you want to use Mesh Filters in its children, set the 'Use Children Mesh' parameter to true");
						continue;
					}
					mesh = component.sharedMesh;
					MeshRenderer component2 = component.gameObject.GetComponent<MeshRenderer>();
					if (component2 == null)
					{
						UIMessages.Add("GameObject '" + @object.name + "' has a Mesh Filter but no Mesh Renderer associated to it. No material will be assigned to this mesh");
						materials2 = new Material[0];
					}
					else
					{
						materials2 = component2.sharedMaterials;
					}
				}
				Matrix4x4 matrix = item.Matrix;
				if (centerMesh)
				{
					matrix *= Matrix4x4.Translate(-mesh.bounds.center);
				}
				if (!mesh.isReadable)
				{
					UIMessages.Add("GameObject '" + @object.name + "' has a mesh '" + mesh.name + "' that is not readable. Please set the 'Read/Write Enabled' parameter to true in the mesh model import settings");
				}
				list.Add(new CGVMesh(mesh, materials2, matrix));
			}
			OutVMesh.SetDataToCollection(list.ToArray());
			if (!isDataDisposable)
			{
				return;
			}
			foreach (CGGameObject item2 in allData)
			{
				item2.Dispose();
			}
		}

		public static Mesh CombineMeshFilters(MeshFilter[] meshFilters, out List<Material> materials, Matrix4x4 originTrs, [CanBeNull] List<string> errorMessages)
		{
			List<CombineInstance> list = new List<CombineInstance>(meshFilters.Length);
			materials = new List<Material>(meshFilters.Length);
			List<Material> list2 = new List<Material>(1);
			int num = 0;
			int num2 = 0;
			Mesh mesh = new Mesh();
			foreach (MeshFilter meshFilter in meshFilters)
			{
				Mesh sharedMesh = meshFilter.sharedMesh;
				if (!sharedMesh.isReadable)
				{
					errorMessages?.Add("Mesh '" + sharedMesh.name + "' is not readable. Please set the 'Read/Write Enabled' parameter to true in the mesh model import settings.");
				}
				for (int j = 0; j < sharedMesh.subMeshCount; j++)
				{
					list.Add(new CombineInstance
					{
						transform = originTrs * meshFilter.transform.localToWorldMatrix,
						mesh = sharedMesh,
						subMeshIndex = j
					});
					num2 += sharedMesh.vertexCount;
				}
				num += sharedMesh.vertexCount;
				MeshRenderer component = meshFilter.gameObject.GetComponent<MeshRenderer>();
				if (component == null)
				{
					errorMessages?.Add("GameObject '" + meshFilter.gameObject.name + "' has a Mesh Filter but no Mesh Renderer associated to it. No material will be assigned to this mesh");
					for (int k = 0; k < sharedMesh.subMeshCount; k++)
					{
						materials.Add(null);
					}
				}
				else
				{
					component.GetSharedMaterials(list2);
					materials.AddRange(list2);
				}
			}
			mesh.indexFormat = ((num2 >= 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
			mesh.CombineMeshes(list.ToArray(), mergeSubMeshes: false);
			IndexFormat indexFormat = ((num >= 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
			if (mesh.indexFormat != indexFormat)
			{
				mesh.indexFormat = indexFormat;
			}
			return mesh;
		}
	}
}
