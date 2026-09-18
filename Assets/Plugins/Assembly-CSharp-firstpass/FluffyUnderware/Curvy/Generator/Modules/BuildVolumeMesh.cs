using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Build/Volume Mesh", ModuleName = "Volume Mesh", Description = "Build a volume mesh")]
	[HelpURL("https://curvyeditor.com/doclink/cgbuildvolumemesh")]
	public class BuildVolumeMesh : CGModule
	{
		private const float DefaultUnscalingOrigin = 0.5f;

		private const int DefaultSplitLength = 100;

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGVolume) })]
		public CGModuleInputSlot InVolume = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGVMesh), Array = true)]
		public CGModuleOutputSlot OutVMesh = new CGModuleOutputSlot();

		[Tab("General")]
		[FieldAction("CBAddMaterial", ActionAttribute.ActionEnum.Callback)]
		[SerializeField]
		[FormerlySerializedAs("m_ReverseNormals")]
		private bool m_ReverseTriOrder;

		[Section("Default/General/UV", true, false, 100)]
		[SerializeField]
		private bool m_GenerateUV = true;

		[SerializeField]
		[Tooltip("When set to true, and if the input Shape Extrusion module is set to apply scaling, the U coordinate of the generated mesh will be modified to compensate that scaling.\nOnly the X component of the scaling is taken into consideration.\nThe unscaling works best on volumes with flat shapes.")]
		[FieldCondition("m_GenerateUV", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		private bool unscaleU;

		[SerializeField]
		[FieldCondition("unscaleU", true, false, ConditionalAttribute.OperatorEnum.AND, "m_GenerateUV", true, false)]
		[Tooltip("When unscaling the U coordinate, this field defines what is the scaling origin.\n0.5 gives usually the best results, but you might need to set it to a different value, usually between 0 and 1")]
		private float unscalingOrigin = 0.5f;

		[SerializeField]
		private bool m_GenerateUV2 = true;

		[Section("Default/General/Split", true, false, 100)]
		[Tooltip("Split the mesh into submeshes")]
		[SerializeField]
		private bool m_Split;

		[Positive(MinValue = 1f)]
		[FieldCondition("m_Split", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private float m_SplitLength = 100f;

		[Group("Default/General/Backward Compatibility", Expanded = false)]
		[Tooltip("Is ignored when Split or Generate UV2 is false.\nIf enabled, UV2s of a split mesh will be computed as in Curvy versions prior to 8.0.0, which had a bug: all the split submeshes used the full range of UV2 coordinates, instead of keeping the same UV2s from the unsplit mesh.")]
		[FieldCondition("IsSplitUV2Togglable", true, false, ActionAttribute.ActionEnum.Enable, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private bool splitUV2;

		[SerializeField]
		[HideInInspector]
		private List<CGMaterialSettingsEx> m_MaterialSettings = new List<CGMaterialSettingsEx>();

		[SerializeField]
		[HideInInspector]
		private Material[] m_Material = new Material[0];

		public bool GenerateUV
		{
			get
			{
				return m_GenerateUV;
			}
			set
			{
				if (m_GenerateUV != value)
				{
					m_GenerateUV = value;
					base.Dirty = true;
				}
			}
		}

		public bool GenerateUV2
		{
			get
			{
				return m_GenerateUV2;
			}
			set
			{
				if (m_GenerateUV2 != value)
				{
					m_GenerateUV2 = value;
					base.Dirty = true;
				}
			}
		}

		public bool UnscaleU
		{
			get
			{
				return unscaleU;
			}
			set
			{
				if (unscaleU != value)
				{
					unscaleU = value;
					base.Dirty = true;
				}
			}
		}

		public float UnscalingOrigin
		{
			get
			{
				return unscalingOrigin;
			}
			set
			{
				if (unscalingOrigin != value)
				{
					unscalingOrigin = value;
					base.Dirty = true;
				}
			}
		}

		public bool ReverseTriOrder
		{
			get
			{
				return m_ReverseTriOrder;
			}
			set
			{
				if (m_ReverseTriOrder != value)
				{
					m_ReverseTriOrder = value;
					base.Dirty = true;
				}
			}
		}

		public bool Split
		{
			get
			{
				return m_Split;
			}
			set
			{
				if (m_Split != value)
				{
					m_Split = value;
					base.Dirty = true;
				}
			}
		}

		public float SplitLength
		{
			get
			{
				return m_SplitLength;
			}
			set
			{
				float num = Mathf.Max(1f, value);
				if (m_SplitLength != num)
				{
					m_SplitLength = num;
					base.Dirty = true;
				}
			}
		}

		public bool SplitUV2
		{
			get
			{
				return splitUV2;
			}
			set
			{
				if (splitUV2 != value)
				{
					splitUV2 = value;
					base.Dirty = true;
				}
			}
		}

		[Obsolete("Use MaterialSettings (with the correct number of Ts) instead")]
		public List<CGMaterialSettingsEx> MaterialSetttings => MaterialSettings;

		public List<CGMaterialSettingsEx> MaterialSettings => m_MaterialSettings;

		public int MaterialCount => m_MaterialSettings.Count;

		private bool IsSplitUV2Togglable
		{
			get
			{
				if (Split)
				{
					return GenerateUV2;
				}
				return false;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (MaterialCount == 0)
			{
				AddMaterial();
			}
		}

		public override void Reset()
		{
			base.Reset();
			GenerateUV = true;
			GenerateUV2 = true;
			UnscaleU = false;
			UnscalingOrigin = 0.5f;
			Split = false;
			SplitLength = 100f;
			SplitUV2 = false;
			ReverseTriOrder = false;
			m_MaterialSettings = new List<CGMaterialSettingsEx>(new CGMaterialSettingsEx[1]
			{
				new CGMaterialSettingsEx()
			});
			m_Material = new Material[1] { CurvyUtility.GetDefaultMaterial() };
		}

		public override void Refresh()
		{
			base.Refresh();
			bool isDataDisposable;
			CGVolume data = InVolume.GetData<CGVolume>(out isDataDisposable, Array.Empty<CGDataRequestParameter>());
			if ((bool)data && data.Count > 0 && data.CrossSize > 0 && data.CrossMaterialGroups.Count > 0)
			{
				List<IntRegion> list = new List<IntRegion>();
				if (Split)
				{
					float num = 0f;
					int num2 = 0;
					for (int i = 0; i < data.Count; i++)
					{
						float num3 = data.FToDistance(data.RelativeDistances.Array[i]);
						if (num3 - num >= SplitLength)
						{
							list.Add(new IntRegion(num2, i));
							num = num3;
							num2 = i;
						}
					}
					if (num2 < data.Count - 1)
					{
						list.Add(new IntRegion(num2, data.Count - 1));
					}
				}
				else
				{
					list.Add(new IntRegion(0, data.Count - 1));
				}
				CGVMesh[] array = new CGVMesh[list.Count];
				List<SamplePointsMaterialGroupCollection> materialIDGroups = getMaterialIDGroups(data);
				for (int j = 0; j < list.Count; j++)
				{
					CGVMesh cGVMesh = CGVMesh.Get(null, data, list[j], GenerateUV, GenerateUV2, ReverseTriOrder);
					build(cGVMesh, data, list[j], materialIDGroups);
					array[j] = cGVMesh;
				}
				OutVMesh.SetDataToCollection(array);
			}
			else
			{
				OutVMesh.ClearData();
			}
			if (isDataDisposable)
			{
				data.Dispose();
			}
		}

		public int AddMaterial()
		{
			m_MaterialSettings.Add(new CGMaterialSettingsEx());
			m_Material = m_Material.Add(CurvyUtility.GetDefaultMaterial());
			base.Dirty = true;
			return MaterialCount;
		}

		public void RemoveMaterial(int index)
		{
			if (validateMaterialIndex(index))
			{
				m_MaterialSettings.RemoveAt(index);
				m_Material = m_Material.RemoveAt(index);
				base.Dirty = true;
			}
		}

		public void SetMaterial(int index, Material mat)
		{
			if (validateMaterialIndex(index) && !(mat == m_Material[index]) && m_Material[index] != mat)
			{
				m_Material[index] = mat;
				base.Dirty = true;
			}
		}

		public Material GetMaterial(int index)
		{
			if (!validateMaterialIndex(index))
			{
				return null;
			}
			return m_Material[index];
		}

		private void build([NotNull] CGVMesh vmesh, CGVolume vol, IntRegion subset, List<SamplePointsMaterialGroupCollection> materialIdGroups)
		{
			prepareSubMeshes(vmesh, materialIdGroups, subset.Length, ref m_Material);
			int num = 0;
			SubArray<int> subArray = ArrayPools.Int32.Allocate(materialIdGroups.Count);
			for (int i = subset.From; i < subset.To; i++)
			{
				for (int j = 0; j < materialIdGroups.Count; j++)
				{
					SamplePointsMaterialGroupCollection samplePointsMaterialGroupCollection = materialIdGroups[j];
					for (int k = 0; k < samplePointsMaterialGroupCollection.Count; k++)
					{
						SamplePointsMaterialGroup samplePointsMaterialGroup = samplePointsMaterialGroupCollection[k];
						if (GenerateUV)
						{
							createMaterialGroupUV(vmesh, vol, samplePointsMaterialGroup, samplePointsMaterialGroupCollection.MaterialID, samplePointsMaterialGroupCollection.AspectCorrectionV, samplePointsMaterialGroupCollection.AspectCorrectionU, i, num);
						}
						if (GenerateUV2)
						{
							createMaterialGroupUV2(vmesh, vol, samplePointsMaterialGroup, i, num);
						}
						for (int l = 0; l < samplePointsMaterialGroup.Patches.Count; l++)
						{
							createPatchTriangles(vmesh.SubMeshes[j].TrianglesList.Array, ref subArray.Array[j], num + samplePointsMaterialGroup.Patches[l].Start, samplePointsMaterialGroup.Patches[l].Count, vol.CrossSize, ReverseTriOrder);
						}
					}
				}
				num += vol.CrossSize;
			}
			for (int m = 0; m < materialIdGroups.Count; m++)
			{
				SamplePointsMaterialGroupCollection samplePointsMaterialGroupCollection = materialIdGroups[m];
				for (int n = 0; n < samplePointsMaterialGroupCollection.Count; n++)
				{
					SamplePointsMaterialGroup samplePointsMaterialGroup = samplePointsMaterialGroupCollection[n];
					if (GenerateUV)
					{
						createMaterialGroupUV(vmesh, vol, samplePointsMaterialGroup, samplePointsMaterialGroupCollection.MaterialID, samplePointsMaterialGroupCollection.AspectCorrectionV, samplePointsMaterialGroupCollection.AspectCorrectionU, subset.To, num);
					}
					if (GenerateUV2)
					{
						createMaterialGroupUV2(vmesh, vol, samplePointsMaterialGroup, subset.To, num);
					}
				}
			}
			ArrayPools.Int32.Free(subArray);
			if (Split && GenerateUV2 && SplitUV2)
			{
				Vector2[] array = vmesh.UV2s.Array;
				float y = array[0].y;
				float y2 = array[vmesh.UV2s.Count - 1].y;
				float num2 = 1f / (y2 - y);
				for (int num3 = 0; num3 < vmesh.UV2s.Count; num3++)
				{
					array[num3].y = (array[num3].y - y) * num2;
				}
			}
		}

		private static void prepareSubMeshes([NotNull] CGVMesh vmesh, List<SamplePointsMaterialGroupCollection> groupsBySubMeshes, int extrusions, ref Material[] materials)
		{
			vmesh.SetSubMeshCount(groupsBySubMeshes.Count);
			for (int i = 0; i < groupsBySubMeshes.Count; i++)
			{
				CGVSubMesh data = vmesh.SubMeshes[i];
				vmesh.SubMeshes[i] = CGVSubMesh.Get(data, groupsBySubMeshes[i].TriangleCount * extrusions * 3, materials[Mathf.Min(groupsBySubMeshes[i].MaterialID, materials.Length - 1)]);
			}
		}

		private void createMaterialGroupUV(CGVMesh vmesh, CGVolume volume, SamplePointsMaterialGroup materialGroup, int matIndex, float aspectCorrectionV, float aspectCorrectionU, int sample, int baseVertex)
		{
			CGMaterialSettingsEx cGMaterialSettingsEx = m_MaterialSettings[matIndex];
			int endVertex = materialGroup.EndVertex;
			bool swapUV = cGMaterialSettingsEx.SwapUV;
			Vector2[] array = vmesh.UVs.Array;
			float[] array2 = volume.CrossCustomValues.Array;
			float num = cGMaterialSettingsEx.UVScale.x * aspectCorrectionU;
			if (UnscaleU)
			{
				num *= volume.Scales.Array[sample].x;
			}
			float num2 = cGMaterialSettingsEx.UVOffset.y + volume.RelativeDistances.Array[sample] * cGMaterialSettingsEx.UVScale.y * aspectCorrectionV;
			for (int i = materialGroup.StartVertex; i <= endVertex; i++)
			{
				float num3 = (UnscaleU ? (cGMaterialSettingsEx.UVOffset.x + unscalingOrigin + (array2[i] - unscalingOrigin) * num) : (cGMaterialSettingsEx.UVOffset.x + array2[i] * num));
				array[baseVertex + i].x = (swapUV ? num2 : num3);
				array[baseVertex + i].y = (swapUV ? num3 : num2);
			}
		}

		private void createMaterialGroupUV2(CGVMesh vmesh, CGVolume volume, SamplePointsMaterialGroup materialGroup, int sample, int baseVertex)
		{
			int endVertex = materialGroup.EndVertex;
			Vector2[] array = vmesh.UV2s.Array;
			for (int i = materialGroup.StartVertex; i <= endVertex; i++)
			{
				array[baseVertex + i].x = volume.CrossRelativeDistances.Array[i];
				array[baseVertex + i].y = volume.RelativeDistances.Array[sample];
			}
		}

		private static void createPatchTriangles(int[] triangles, ref int triIdx, int curVTIndex, int patchSize, int crossSize, bool reverse)
		{
			int num = (reverse ? 1 : 0);
			int num2 = 1 - num;
			int num3 = curVTIndex + crossSize;
			for (int i = 0; i < patchSize; i++)
			{
				triangles[triIdx + num] = curVTIndex + i;
				triangles[triIdx + num2] = num3 + i;
				triangles[triIdx + 2] = curVTIndex + i + 1;
				triangles[triIdx + num + 3] = curVTIndex + i + 1;
				triangles[triIdx + num2 + 3] = num3 + i;
				triangles[triIdx + 5] = num3 + i + 1;
				triIdx += 6;
			}
		}

		private List<SamplePointsMaterialGroupCollection> getMaterialIDGroups(CGVolume volume)
		{
			Dictionary<int, SamplePointsMaterialGroupCollection> dictionary = new Dictionary<int, SamplePointsMaterialGroupCollection>();
			for (int i = 0; i < volume.CrossMaterialGroups.Count; i++)
			{
				int num;
				if (volume.CrossMaterialGroups[i].MaterialID <= MaterialCount - 1)
				{
					num = volume.CrossMaterialGroups[i].MaterialID;
				}
				else
				{
					UIMessages.Add($"Input Volume is using material id {volume.CrossMaterialGroups[i].MaterialID}, which has no associate Material in this module. Use the 'Add Material Group'");
					num = MaterialCount - 1;
				}
				if (!dictionary.TryGetValue(num, out var value))
				{
					value = new SamplePointsMaterialGroupCollection();
					value.MaterialID = num;
					dictionary.Add(num, value);
				}
				value.Add(volume.CrossMaterialGroups[i]);
			}
			List<SamplePointsMaterialGroupCollection> list = new List<SamplePointsMaterialGroupCollection>();
			foreach (SamplePointsMaterialGroupCollection value2 in dictionary.Values)
			{
				value2.CalculateAspectCorrection(volume, MaterialSettings[value2.MaterialID]);
				list.Add(value2);
			}
			return list;
		}

		private bool validateMaterialIndex(int index)
		{
			if (index < 0 || index >= m_MaterialSettings.Count)
			{
				Debug.LogError("TriangulateTube: Invalid Material Index!");
				return false;
			}
			return true;
		}
	}
}
