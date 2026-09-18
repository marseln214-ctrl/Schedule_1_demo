using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdvancedPeopleSystem
{
	public class CharacterCustomizationCombiner
	{
		private class MeshInstance
		{
			public Dictionary<Material, List<CombineInstanceWithSM>> combine_instances = new Dictionary<Material, List<CombineInstanceWithSM>>();

			public List<Material> unique_materials = new List<Material>();

			public Mesh combined_new_mesh = new Mesh();

			public List<Vector3> combined_vertices = new List<Vector3>();

			public List<Vector2> combined_uv = new List<Vector2>();

			public List<Vector2> combined_uv2 = new List<Vector2>();

			public List<Vector2> combined_uv3 = new List<Vector2>();

			public List<Vector2> combined_uv4 = new List<Vector2>();

			public List<Vector3> normals = new List<Vector3>();

			public List<Vector4> tangents = new List<Vector4>();

			public Dictionary<Material, List<int>> combined_submesh_indices = new Dictionary<Material, List<int>>();

			public List<BoneWeight> combined_bone_weights = new List<BoneWeight>();

			public List<string> blendShapeNames = new List<string>();

			public List<float> blendShapeValues = new List<float>();

			public Dictionary<Mesh, int> vertex_offset_map = new Dictionary<Mesh, int>();

			public int vertex_index_offset;

			public int current_material_index;
		}

		private struct CombineInstanceWithSM
		{
			public SkinnedMeshRenderer skinnedMesh;

			public CombineInstance instance;
		}

		private struct BlendWeightData
		{
			public Vector3[] deltaVerts;

			public Vector3[] deltaNormals;

			public Vector3[] deltaTangents;
		}

		private static Matrix4x4[] bindPoses;

		private static List<MeshInstance> LODMeshInstances;

		private static CharacterCustomization currentCharacter;

		private static bool useExportToAnotherObject = false;

		private static bool BlendshapeTransferWork = false;

		private static Action<List<SkinnedMeshRenderer>> _callback;

		private static List<SkinnedMeshRenderer> returnSkinnedMeshes = new List<SkinnedMeshRenderer>();

		public static List<SkinnedMeshRenderer> MakeCombinedMeshes(CharacterCustomization character, GameObject exportInCustomObject = null, float blendshapeAddDelay = 0.001f, Action<List<SkinnedMeshRenderer>> callback = null)
		{
			returnSkinnedMeshes.Clear();
			if (character.IsBaked())
			{
				Debug.LogError("Character is already combined!");
				return null;
			}
			if (callback != null)
			{
				_callback = callback;
			}
			BlendshapeTransferWork = false;
			useExportToAnotherObject = exportInCustomObject != null;
			if (!useExportToAnotherObject)
			{
				character.CurrentCombinerState = CombinerState.InProgressCombineMesh;
			}
			currentCharacter = character;
			bindPoses = character.GetCharacterPart("Head").skinnedMesh[0].sharedMesh.bindposes;
			LODMeshInstances = new List<MeshInstance>();
			for (int i = 0; i < character.MaxLODLevels - character.MinLODLevels + 1; i++)
			{
				LODMeshInstances.Add(new MeshInstance());
				foreach (SkinnedMeshRenderer item3 in character.GetAllMeshesByLod(i))
				{
					SelectMeshes(item3, i);
				}
			}
			SkinnedMeshRenderer original = character.GetCharacterPart("Combined").skinnedMesh[0];
			List<SkinnedMeshRenderer> list = character.GetCharacterPart("Combined").skinnedMesh;
			if (exportInCustomObject != null)
			{
				List<SkinnedMeshRenderer> list2 = new List<SkinnedMeshRenderer>();
				for (int j = 0; j < character.MaxLODLevels - character.MinLODLevels + 1; j++)
				{
					SkinnedMeshRenderer item = UnityEngine.Object.Instantiate(original, exportInCustomObject.transform);
					list2.Add(item);
				}
				list = list2;
			}
			for (int k = 0; k < LODMeshInstances.Count; k++)
			{
				MeshInstance meshInstance = LODMeshInstances[k];
				for (int l = 0; l < meshInstance.unique_materials.Count; l++)
				{
					Material key = meshInstance.unique_materials[l];
					List<CombineInstanceWithSM> list3 = meshInstance.combine_instances[meshInstance.unique_materials[l]];
					for (int m = 0; m < list3.Count; m++)
					{
						CombineInstanceWithSM combineInstanceWithSM = list3[m];
						if (!meshInstance.vertex_offset_map.ContainsKey(combineInstanceWithSM.instance.mesh))
						{
							meshInstance.combined_vertices.AddRange(combineInstanceWithSM.instance.mesh.vertices);
							if (combineInstanceWithSM.instance.mesh.uv.Length == 0)
							{
								meshInstance.combined_uv.AddRange(new Vector2[combineInstanceWithSM.instance.mesh.vertexCount]);
							}
							else
							{
								meshInstance.combined_uv.AddRange(combineInstanceWithSM.instance.mesh.uv);
							}
							if (combineInstanceWithSM.instance.mesh.uv2.Length == 0)
							{
								meshInstance.combined_uv2.AddRange(new Vector2[combineInstanceWithSM.instance.mesh.vertexCount]);
							}
							else
							{
								meshInstance.combined_uv2.AddRange(combineInstanceWithSM.instance.mesh.uv2);
							}
							if (combineInstanceWithSM.instance.mesh.uv3.Length == 0)
							{
								meshInstance.combined_uv3.AddRange(new Vector2[combineInstanceWithSM.instance.mesh.vertexCount]);
							}
							else
							{
								meshInstance.combined_uv3.AddRange(combineInstanceWithSM.instance.mesh.uv3);
							}
							meshInstance.normals.AddRange(combineInstanceWithSM.instance.mesh.normals);
							meshInstance.combined_bone_weights.AddRange(combineInstanceWithSM.instance.mesh.boneWeights);
							meshInstance.vertex_offset_map[combineInstanceWithSM.instance.mesh] = meshInstance.vertex_index_offset;
							meshInstance.vertex_index_offset += combineInstanceWithSM.instance.mesh.vertexCount;
						}
						int num = meshInstance.vertex_offset_map[combineInstanceWithSM.instance.mesh];
						int[] triangles = combineInstanceWithSM.instance.mesh.GetTriangles(combineInstanceWithSM.instance.subMeshIndex);
						for (int n = 0; n < triangles.Length; n++)
						{
							triangles[n] += num;
						}
						if (!meshInstance.combined_submesh_indices.ContainsKey(key))
						{
							meshInstance.combined_submesh_indices.Add(key, triangles.ToList());
						}
						else
						{
							meshInstance.combined_submesh_indices[key].AddRange(triangles);
						}
						for (int num2 = 0; num2 < combineInstanceWithSM.instance.mesh.blendShapeCount; num2++)
						{
							string blendShapeName = combineInstanceWithSM.instance.mesh.GetBlendShapeName(num2);
							if (!meshInstance.blendShapeNames.Contains(blendShapeName))
							{
								meshInstance.blendShapeNames.Add(blendShapeName);
								meshInstance.blendShapeValues.Add(combineInstanceWithSM.skinnedMesh.GetBlendShapeWeight(num2));
							}
						}
					}
				}
				meshInstance.combined_new_mesh.vertices = meshInstance.combined_vertices.ToArray();
				meshInstance.combined_new_mesh.uv = meshInstance.combined_uv.ToArray();
				if (meshInstance.combined_uv2.Count > 0)
				{
					meshInstance.combined_new_mesh.uv2 = meshInstance.combined_uv2.ToArray();
				}
				if (meshInstance.combined_uv3.Count > 0)
				{
					meshInstance.combined_new_mesh.uv3 = meshInstance.combined_uv3.ToArray();
				}
				if (meshInstance.combined_uv4.Count > 0)
				{
					meshInstance.combined_new_mesh.uv4 = meshInstance.combined_uv4.ToArray();
				}
				meshInstance.combined_new_mesh.boneWeights = meshInstance.combined_bone_weights.ToArray();
				meshInstance.combined_new_mesh.name = $"APP_CombinedMesh_lod{k}";
				meshInstance.combined_new_mesh.subMeshCount = meshInstance.unique_materials.Count;
				for (int num3 = 0; num3 < meshInstance.unique_materials.Count; num3++)
				{
					meshInstance.combined_new_mesh.SetTriangles(meshInstance.combined_submesh_indices[meshInstance.unique_materials[num3]], num3);
				}
				meshInstance.combined_new_mesh.SetNormals(meshInstance.normals);
				meshInstance.combined_new_mesh.RecalculateTangents();
				if (!useExportToAnotherObject && character.CurrentCombinerState != CombinerState.InProgressBlendshapeTransfer)
				{
					character.CurrentCombinerState = CombinerState.InProgressBlendshapeTransfer;
				}
				character.StartCoroutine(BlendshapeTransfer(meshInstance, blendshapeAddDelay, list[k], k, exportInCustomObject == null));
			}
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				list[num4].name = $"combinemesh_lod{num4}";
				list[num4].sharedMesh = LODMeshInstances[num4].combined_new_mesh;
				list[num4].sharedMesh.bindposes = bindPoses;
				list[num4].sharedMaterials = LODMeshInstances[num4].unique_materials.ToArray();
				list[num4].updateWhenOffscreen = true;
			}
			returnSkinnedMeshes.AddRange(list);
			BlendshapeTransferWork = true;
			return returnSkinnedMeshes;
			static void SelectMeshes(SkinnedMeshRenderer mesh, int LOD)
			{
				if (mesh != null)
				{
					for (int num5 = 0; num5 < mesh.sharedMaterials.Length; num5++)
					{
						Material material = mesh.sharedMaterials[num5];
						Mesh sharedMesh = mesh.sharedMesh;
						if (!(sharedMesh == null) && mesh.gameObject.activeSelf && mesh.enabled && sharedMesh.vertexCount != 0 && sharedMesh.subMeshCount - 1 >= num5)
						{
							if (!LODMeshInstances[LOD].combine_instances.ContainsKey(material))
							{
								LODMeshInstances[LOD].combine_instances.Add(material, new List<CombineInstanceWithSM>());
								LODMeshInstances[LOD].unique_materials.Add(material);
							}
							CombineInstanceWithSM item2 = default(CombineInstanceWithSM);
							CombineInstance instance = default(CombineInstance);
							instance.transform = Matrix4x4.identity;
							instance.subMeshIndex = num5;
							instance.mesh = sharedMesh;
							item2.instance = instance;
							item2.skinnedMesh = mesh;
							LODMeshInstances[LOD].combine_instances[material].Add(item2);
						}
					}
				}
			}
		}

		private static IEnumerator BlendshapeTransfer(MeshInstance meshInstance, float waitTime, SkinnedMeshRenderer smr, int lod, bool yieldUse = true)
		{
			yield return new WaitWhile(() => !BlendshapeTransferWork);
			CharacterCustomization characterSystem = currentCharacter;
			for (int bs = 0; bs < meshInstance.blendShapeNames.Count; bs++)
			{
				int num = 0;
				BlendWeightData blendWeightData = default(BlendWeightData);
				blendWeightData.deltaNormals = new Vector3[meshInstance.combined_new_mesh.vertexCount];
				blendWeightData.deltaTangents = new Vector3[meshInstance.combined_new_mesh.vertexCount];
				blendWeightData.deltaVerts = new Vector3[meshInstance.combined_new_mesh.vertexCount];
				foreach (KeyValuePair<Material, List<CombineInstanceWithSM>> combine_instance in meshInstance.combine_instances)
				{
					foreach (CombineInstanceWithSM item in combine_instance.Value)
					{
						CombineInstance instance = item.instance;
						if (instance.subMeshIndex <= 0)
						{
							instance = item.instance;
							int vertexCount = instance.mesh.vertexCount;
							Vector3[] array = new Vector3[vertexCount];
							Vector3[] array2 = new Vector3[vertexCount];
							Vector3[] array3 = new Vector3[vertexCount];
							instance = item.instance;
							if (instance.mesh.GetBlendShapeIndex(meshInstance.blendShapeNames[bs]) != -1)
							{
								instance = item.instance;
								int blendShapeIndex = instance.mesh.GetBlendShapeIndex(meshInstance.blendShapeNames[bs]);
								instance = item.instance;
								Mesh mesh = instance.mesh;
								instance = item.instance;
								mesh.GetBlendShapeFrameVertices(blendShapeIndex, instance.mesh.GetBlendShapeFrameCount(blendShapeIndex) - 1, array, array2, array3);
								Array.Copy(array, 0, blendWeightData.deltaVerts, num, vertexCount);
								Array.Copy(array2, 0, blendWeightData.deltaNormals, num, vertexCount);
								Array.Copy(array3, 0, blendWeightData.deltaTangents, num, vertexCount);
							}
							num += vertexCount;
						}
					}
				}
				smr.sharedMesh.AddBlendShapeFrame(meshInstance.blendShapeNames[bs], 100f, blendWeightData.deltaVerts, blendWeightData.deltaNormals, blendWeightData.deltaTangents);
				smr.SetBlendShapeWeight(bs, meshInstance.blendShapeValues[bs]);
				if (waitTime > 0f && yieldUse)
				{
					yield return new WaitForSecondsRealtime(waitTime);
				}
			}
			if (lod == characterSystem.MaxLODLevels - characterSystem.MinLODLevels)
			{
				if (!useExportToAnotherObject)
				{
					characterSystem.CurrentCombinerState = CombinerState.Combined;
				}
				_callback?.Invoke(returnSkinnedMeshes);
				_callback = null;
			}
		}
	}
}
