using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.ThirdParty.LibTessDotNet;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using ToolBuddy.Pooling.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Build/Volume Caps", ModuleName = "Volume Caps", Description = "Build volume caps")]
	[HelpURL("https://curvyeditor.com/doclink/cgbuildvolumecaps")]
	public class BuildVolumeCaps : CGModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGVolume) })]
		public CGModuleInputSlot InVolume = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGVolume) }, Optional = true, Array = true)]
		public CGModuleInputSlot InVolumeHoles = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGVMesh), Array = true)]
		public CGModuleOutputSlot OutVMesh = new CGModuleOutputSlot();

		[Tab("General")]
		[SerializeField]
		private CGYesNoAuto m_StartCap = CGYesNoAuto.Auto;

		[SerializeField]
		private CGYesNoAuto m_EndCap = CGYesNoAuto.Auto;

		[SerializeField]
		[FormerlySerializedAs("m_ReverseNormals")]
		private bool m_ReverseTriOrder;

		[SerializeField]
		private bool m_GenerateUV = true;

		[SerializeField]
		private bool m_GenerateUV2 = true;

		[Tab("Start Cap")]
		[Inline]
		[SerializeField]
		private CGMaterialSettings m_StartMaterialSettings = new CGMaterialSettings();

		[Label("Material", "")]
		[SerializeField]
		private Material m_StartMaterial;

		[Tab("End Cap")]
		[SerializeField]
		private bool m_CloneStartCap = true;

		[AsGroup(null, Invisible = true)]
		[GroupCondition("m_CloneStartCap", false, false)]
		[SerializeField]
		private CGMaterialSettings m_EndMaterialSettings = new CGMaterialSettings();

		[Group("Default/End Cap")]
		[Label("Material", "")]
		[FieldCondition("m_CloneStartCap", false, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private Material m_EndMaterial;

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

		public CGYesNoAuto StartCap
		{
			get
			{
				return m_StartCap;
			}
			set
			{
				if (m_StartCap != value)
				{
					m_StartCap = value;
					base.Dirty = true;
				}
			}
		}

		public Material StartMaterial
		{
			get
			{
				return m_StartMaterial;
			}
			set
			{
				if (m_StartMaterial != value)
				{
					m_StartMaterial = value;
					base.Dirty = true;
				}
			}
		}

		public CGMaterialSettings StartMaterialSettings => m_StartMaterialSettings;

		public CGYesNoAuto EndCap
		{
			get
			{
				return m_EndCap;
			}
			set
			{
				if (m_EndCap != value)
				{
					m_EndCap = value;
					base.Dirty = true;
				}
			}
		}

		public bool CloneStartCap
		{
			get
			{
				return m_CloneStartCap;
			}
			set
			{
				if (m_CloneStartCap != value)
				{
					m_CloneStartCap = value;
					base.Dirty = true;
				}
			}
		}

		public CGMaterialSettings EndMaterialSettings => m_EndMaterialSettings;

		public Material EndMaterial
		{
			get
			{
				return m_EndMaterial;
			}
			set
			{
				if (m_EndMaterial != value)
				{
					m_EndMaterial = value;
					base.Dirty = true;
				}
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (StartMaterial == null)
			{
				StartMaterial = CurvyUtility.GetDefaultMaterial();
			}
			if (EndMaterial == null)
			{
				EndMaterial = CurvyUtility.GetDefaultMaterial();
			}
		}

		public override void Reset()
		{
			base.Reset();
			StartCap = CGYesNoAuto.Auto;
			EndCap = CGYesNoAuto.Auto;
			ReverseTriOrder = false;
			GenerateUV = true;
			GenerateUV2 = true;
			m_StartMaterialSettings = new CGMaterialSettings();
			m_EndMaterialSettings = new CGMaterialSettings();
			StartMaterial = CurvyUtility.GetDefaultMaterial();
			EndMaterial = CurvyUtility.GetDefaultMaterial();
			CloneStartCap = true;
		}

		public override void Refresh()
		{
			base.Refresh();
			bool isDataDisposable;
			CGVolume data = InVolume.GetData<CGVolume>(out isDataDisposable, Array.Empty<CGDataRequestParameter>());
			bool isDataDisposable2;
			List<CGVolume> allData = InVolumeHoles.GetAllData<CGVolume>(out isDataDisposable2, Array.Empty<CGDataRequestParameter>());
			if ((bool)data)
			{
				bool flag = StartCap == CGYesNoAuto.Yes || (StartCap == CGYesNoAuto.Auto && !data.Seamless);
				bool flag2 = EndCap == CGYesNoAuto.Yes || (EndCap == CGYesNoAuto.Auto && !data.Seamless);
				if (!flag && !flag2)
				{
					OutVMesh.ClearData();
					return;
				}
				CGVMesh cGVMesh = new CGVMesh();
				SubArray<Vector3> subArray = ArrayPools.Vector3.Allocate(0);
				cGVMesh.AddSubMesh(new CGVSubMesh());
				CGVSubMesh cGVSubMesh = cGVMesh.SubMeshes[0];
				if (flag)
				{
					Tess tess = new Tess();
					tess.UsePooling = true;
					tess.AddContour(make2DSegment(data, 0));
					for (int i = 0; i < allData.Count; i++)
					{
						if (allData[i].Count < 3)
						{
							OutVMesh.ClearData();
							UIMessages.Add("Hole Cross has less than 3 Vertices: Can't create Caps!");
							return;
						}
						tess.AddContour(make2DSegment(allData[i], 0));
					}
					tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
					ArrayPools.Vector3.Free(subArray);
					subArray = UnityLibTessUtility.ContourVerticesToPositions(tess.Vertices);
					int num = 0;
					cGVMesh.Vertices = applyMatrix(subArray, getMatrix(data, num, inverse: true), out var bounds);
					SubArray<Vector3> normalsList = ArrayPools.Vector3.Allocate(cGVMesh.Vertices.Count);
					Vector3 vector = -data.Directions.Array[num];
					for (int j = 0; j < normalsList.Count; j++)
					{
						normalsList.Array[j] = vector;
					}
					cGVMesh.NormalsList = normalsList;
					cGVSubMesh.Material = StartMaterial;
					cGVSubMesh.TrianglesList = tess.ElementsArray.Value;
					if (ReverseTriOrder)
					{
						flipTris(cGVSubMesh.TrianglesList, 0, cGVSubMesh.TrianglesList.Count);
					}
					if (GenerateUV)
					{
						cGVMesh.UVs = ArrayPools.Vector2.Allocate(subArray.Count);
						applyUV(subArray, cGVMesh.UVs, 0, subArray.Count, StartMaterialSettings, bounds);
					}
					if (GenerateUV2)
					{
						cGVMesh.UV2s = ArrayPools.Vector2.Allocate(subArray.Count);
						applyUV2(subArray, cGVMesh.UV2s, 0, subArray.Count, bounds);
					}
				}
				if (flag2)
				{
					Tess tess2 = new Tess();
					tess2.UsePooling = true;
					tess2.AddContour(make2DSegment(data, data.Count - 1));
					for (int k = 0; k < allData.Count; k++)
					{
						if (allData[k].Count < 3)
						{
							OutVMesh.ClearData();
							UIMessages.Add("Hole Cross has <3 Vertices: Can't create Caps!");
							return;
						}
						tess2.AddContour(make2DSegment(allData[k], allData[k].Count - 1));
					}
					tess2.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
					SubArray<Vector3> subArray2 = UnityLibTessUtility.ContourVerticesToPositions(tess2.Vertices);
					int count = cGVMesh.Vertices.Count;
					int num2 = data.Count - 1;
					Bounds bounds2;
					SubArray<Vector3> subArray3 = applyMatrix(subArray2, getMatrix(data, num2, inverse: true), out bounds2);
					SubArray<Vector3> vertices = ArrayPools.Vector3.Allocate(cGVMesh.Vertices.Count + subArray3.Count);
					Array.Copy(cGVMesh.Vertices.Array, 0, vertices.Array, 0, cGVMesh.Vertices.Count);
					Array.Copy(subArray3.Array, 0, vertices.Array, cGVMesh.Vertices.Count, subArray3.Count);
					cGVMesh.Vertices = vertices;
					ArrayPools.Vector3.Free(subArray3);
					SubArray<Vector3> subArray4 = ArrayPools.Vector3.Allocate(count);
					Vector3 vector2 = data.Directions.Array[num2];
					for (int l = 0; l < subArray4.Count; l++)
					{
						subArray4.Array[l] = vector2;
					}
					SubArray<Vector3> normalsList2 = ArrayPools.Vector3.Allocate(cGVMesh.NormalsList.Count + subArray4.Count);
					Array.Copy(cGVMesh.NormalsList.Array, 0, normalsList2.Array, 0, cGVMesh.NormalsList.Count);
					Array.Copy(subArray4.Array, 0, normalsList2.Array, cGVMesh.NormalsList.Count, subArray4.Count);
					cGVMesh.NormalsList = normalsList2;
					ArrayPools.Vector3.Free(subArray4);
					SubArray<int> value = tess2.ElementsArray.Value;
					if (!ReverseTriOrder)
					{
						flipTris(value, 0, value.Count);
					}
					for (int m = 0; m < value.Count; m++)
					{
						value.Array[m] += count;
					}
					if (!CloneStartCap && StartMaterial != EndMaterial)
					{
						cGVMesh.AddSubMesh(new CGVSubMesh(value, EndMaterial));
					}
					else
					{
						cGVSubMesh.Material = StartMaterial;
						SubArray<int> trianglesList = ArrayPools.Int32.Allocate(cGVSubMesh.TrianglesList.Count + value.Count);
						Array.Copy(cGVSubMesh.TrianglesList.Array, 0, trianglesList.Array, 0, cGVSubMesh.TrianglesList.Count);
						Array.Copy(value.Array, 0, trianglesList.Array, cGVSubMesh.TrianglesList.Count, value.Count);
						cGVSubMesh.TrianglesList = trianglesList;
					}
					if (GenerateUV)
					{
						SubArray<Vector2> uVs = ArrayPools.Vector2.Allocate(cGVMesh.UVs.Count + subArray2.Count);
						Array.Copy(cGVMesh.UVs.Array, 0, uVs.Array, 0, cGVMesh.UVs.Count);
						cGVMesh.UVs = uVs;
						applyUV(subArray2, cGVMesh.UVs, subArray.Count, subArray2.Count, CloneStartCap ? StartMaterialSettings : EndMaterialSettings, bounds2);
					}
					if (GenerateUV2)
					{
						SubArray<Vector2> uV2s = ArrayPools.Vector2.Allocate(cGVMesh.UV2s.Count + subArray2.Count);
						Array.Copy(cGVMesh.UV2s.Array, 0, uV2s.Array, 0, cGVMesh.UV2s.Count);
						cGVMesh.UV2s = uV2s;
						applyUV2(subArray2, cGVMesh.UV2s, subArray.Count, subArray2.Count, bounds2);
					}
					ArrayPools.Vector3.Free(subArray2);
				}
				ArrayPools.Vector3.Free(subArray);
				OutVMesh.SetDataToElement(cGVMesh);
			}
			if (isDataDisposable)
			{
				data.Dispose();
			}
			if (isDataDisposable2)
			{
				allData.ForEach(delegate(CGVolume h)
				{
					h.Dispose();
				});
			}
		}

		private static Matrix4x4 getMatrix(CGVolume vol, int index, bool inverse)
		{
			if (inverse)
			{
				Quaternion q = Quaternion.LookRotation(vol.Directions.Array[index], vol.Normals.Array[index]);
				return Matrix4x4.TRS(vol.Positions.Array[index], q, Vector3.one);
			}
			Quaternion quaternion = Quaternion.Inverse(Quaternion.LookRotation(vol.Directions.Array[index], vol.Normals.Array[index]));
			return Matrix4x4.TRS(-(quaternion * vol.Positions.Array[index]), quaternion, Vector3.one);
		}

		private static void flipTris(SubArray<int> indices, int start, int end)
		{
			for (int i = start; i < end; i += 3)
			{
				int num = indices.Array[i];
				indices.Array[i] = indices.Array[i + 2];
				indices.Array[i + 2] = num;
			}
		}

		private static SubArray<Vector3> applyMatrix(SubArray<Vector3> vt, Matrix4x4 matrix, out Bounds bounds)
		{
			SubArray<Vector3> result = ArrayPools.Vector3.Allocate(vt.Count);
			float num = float.MaxValue;
			float num2 = float.MaxValue;
			float num3 = float.MinValue;
			float num4 = float.MinValue;
			for (int i = 0; i < vt.Count; i++)
			{
				num = Mathf.Min(vt.Array[i].x, num);
				num2 = Mathf.Min(vt.Array[i].y, num2);
				num3 = Mathf.Max(vt.Array[i].x, num3);
				num4 = Mathf.Max(vt.Array[i].y, num4);
				result.Array[i] = matrix.MultiplyPoint3x4(vt.Array[i]);
			}
			Vector3 size = new Vector3(Mathf.Abs(num3 - num), Mathf.Abs(num4 - num2));
			bounds = new Bounds(new Vector3(num + size.x / 2f, num2 + size.y / 2f, 0f), size);
			return result;
		}

		private static ContourVertex[] make2DSegment(CGVolume vol, int segmentIndex)
		{
			Matrix4x4 matrix = getMatrix(vol, segmentIndex, inverse: false);
			int segmentIndex2 = vol.GetSegmentIndex(segmentIndex);
			ContourVertex[] array = new ContourVertex[vol.CrossSize];
			for (int i = 0; i < vol.CrossSize; i++)
			{
				array[i] = matrix.MultiplyPoint3x4(vol.Vertices.Array[segmentIndex2 + i]).ContourVertex();
			}
			return array;
		}

		private static void applyUV(SubArray<Vector3> vts, SubArray<Vector2> uvArray, int index, int count, CGMaterialSettings mat, Bounds bounds)
		{
			float x = bounds.size.x;
			float y = bounds.size.y;
			float x2 = bounds.min.x;
			float y2 = bounds.min.y;
			float num = mat.UVScale.x;
			float num2 = mat.UVScale.y;
			switch (mat.KeepAspect)
			{
			case CGKeepAspectMode.ScaleU:
			{
				float num5 = x * mat.UVScale.y;
				float num6 = y * mat.UVScale.x;
				num *= num5 / num6;
				break;
			}
			case CGKeepAspectMode.ScaleV:
			{
				float num3 = x * mat.UVScale.y;
				float num4 = y * mat.UVScale.x;
				num2 *= num4 / num3;
				break;
			}
			}
			bool swapUV = mat.SwapUV;
			if (mat.UVRotation != 0f)
			{
				float f = mat.UVRotation * (MathF.PI / 180f);
				float num7 = Mathf.Sin(f);
				float num8 = Mathf.Cos(f);
				float num9 = num * 0.5f;
				float num10 = num2 * 0.5f;
				Vector2 vector = default(Vector2);
				for (int i = 0; i < count; i++)
				{
					float num11 = (vts.Array[i].x - x2) / x * num;
					float num12 = (vts.Array[i].y - y2) / y * num2;
					float num13 = num11 - num9;
					float num14 = num12 - num10;
					num11 = num8 * num13 - num7 * num14 + num9 + mat.UVOffset.x;
					num12 = num7 * num13 + num8 * num14 + num10 + mat.UVOffset.y;
					int num15 = i + index;
					vector.x = (swapUV ? num12 : num11);
					vector.y = (swapUV ? num11 : num12);
					uvArray.Array[num15] = vector;
				}
			}
			else
			{
				Vector2 vector2 = default(Vector2);
				for (int j = 0; j < count; j++)
				{
					float num11 = mat.UVOffset.x + (vts.Array[j].x - x2) / x * num;
					float num12 = mat.UVOffset.y + (vts.Array[j].y - y2) / y * num2;
					int num16 = j + index;
					vector2.x = (swapUV ? num12 : num11);
					vector2.y = (swapUV ? num11 : num12);
					uvArray.Array[num16] = vector2;
				}
			}
		}

		private static void applyUV2(SubArray<Vector3> vertice, SubArray<Vector2> uv2Array, int index, int count, Bounds bounds)
		{
			float num = 1f / bounds.size.x;
			float num2 = 1f / bounds.size.y;
			float x = bounds.min.x;
			float y = bounds.min.y;
			Vector2 vector = default(Vector2);
			for (int i = 0; i < count; i++)
			{
				vector.x = (vertice.Array[i].x - x) * num;
				vector.y = (vertice.Array[i].y - y) * num2;
				uv2Array.Array[i + index] = vector;
			}
		}
	}
}
