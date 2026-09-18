using System;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/Deform Mesh", ModuleName = "Deform Mesh", Description = "Deform a mesh following a path")]
	[HelpURL("https://curvyeditor.com/doclink/cgdeformmesh")]
	public class DeformMesh : ScalingModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGVMesh) }, Array = true, Name = "VMesh")]
		public CGModuleInputSlot InVMeshes = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path", DisplayName = "Volume/Rasterized Path")]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGSpots) }, Array = true, Name = "Spots", Optional = true)]
		public CGModuleInputSlot InSpots = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGVMesh), Name = "VMesh", Array = true)]
		public CGModuleOutputSlot OutVMeshes = new CGModuleOutputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGSpots), Array = true, Name = "Spots")]
		public CGModuleOutputSlot OutSpots = new CGModuleOutputSlot();

		[Tab("General")]
		[SerializeField]
		[Tooltip("Stretch the meshes to make them fit the end of the path")]
		private bool stretchToEnd;

		private readonly ThreadPoolWorker<CGSpot> threadWorker = new ThreadPoolWorker<CGSpot>();

		public bool StretchToEnd
		{
			get
			{
				return stretchToEnd;
			}
			set
			{
				if (stretchToEnd != value)
				{
					stretchToEnd = value;
					base.Dirty = true;
				}
			}
		}

		public override void Reset()
		{
			base.Reset();
			StretchToEnd = false;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			threadWorker.Dispose();
		}

		public override void Refresh()
		{
			base.Refresh();
			bool isDataDisposable;
			CGPath data = InPath.GetData<CGPath>(out isDataDisposable, Array.Empty<CGDataRequestParameter>());
			bool isDataDisposable2;
			List<CGVMesh> allData = InVMeshes.GetAllData<CGVMesh>(out isDataDisposable2, Array.Empty<CGDataRequestParameter>());
			CGData[] dataToCollection;
			CGSpots dataToElement;
			if (allData.Count != 0)
			{
				bool isDataDisposable3;
				List<CGSpots> allData2 = InSpots.GetAllData<CGSpots>(out isDataDisposable3, Array.Empty<CGDataRequestParameter>());
				bool arrayIsCopy;
				SubArray<CGSpot>? subArray = ToOneDimensionalArray(allData2, out arrayIsCopy);
				if (subArray.HasValue && subArray.Value.Count != 0)
				{
					int count = subArray.Value.Count;
					CGSpot[] array = subArray.Value.Array;
					bool flag = true;
					for (int i = 0; i < count; i++)
					{
						int index = array[i].Index;
						if (index < 0 || index >= allData.Count)
						{
							UIMessages.Add($"Spot #{i} has an invalid Index value of '{index}'. An index can't be greater or equal to the number of input Meshes, which is '{allData.Count}'");
							flag = false;
							break;
						}
					}
					if (flag)
					{
						CGVMesh[] array2 = new CGVMesh[count];
						SubArray<CGSpot> subArray2 = ArrayPools.CGSpot.Allocate(count);
						ScaleParameters scaleParameters = new ScaleParameters(base.ScaleMode, base.ScaleReference, base.ScaleUniform, base.ScaleOffset, base.ScaleX, base.ScaleY, base.ScaleMultiplierX, base.ScaleMultiplierY);
						DeformMeshes(allData, subArray.Value, subArray2, array2, data, StretchToEnd, threadWorker, scaleParameters);
						CGData[] array3 = array2;
						dataToCollection = array3;
						dataToElement = new CGSpots(subArray2);
					}
					else
					{
						CGData[] array3 = Array.Empty<CGVMesh>();
						dataToCollection = array3;
						dataToElement = new CGSpots();
					}
				}
				else
				{
					CGData[] array3 = Array.Empty<CGVMesh>();
					dataToCollection = array3;
					dataToElement = new CGSpots();
				}
				if (arrayIsCopy)
				{
					ArrayPools.CGSpot.Free(subArray.Value);
				}
				if (isDataDisposable3)
				{
					allData2.ForEach(delegate(CGSpots s)
					{
						s.Dispose();
					});
				}
			}
			else
			{
				CGData[] array3 = Array.Empty<CGVMesh>();
				dataToCollection = array3;
				dataToElement = new CGSpots();
			}
			OutVMeshes.SetDataToCollection(dataToCollection);
			OutSpots.SetDataToElement(dataToElement);
			if (isDataDisposable)
			{
				data.Dispose();
			}
			if (isDataDisposable2)
			{
				allData.ForEach(delegate(CGVMesh m)
				{
					m.Dispose();
				});
			}
		}

		public static void DeformMeshes([NotNull] List<CGVMesh> inputMeshes, SubArray<CGSpot> inputSpots, SubArray<CGSpot> outputSpots, [NotNull] CGVMesh[] outputMeshes, [NotNull] CGPath path, bool stretchToEnd, ThreadPoolWorker<CGSpot> threadPoolWorker)
		{
			ScaleParameters scaleParameters = new ScaleParameters(ScaleMode.Simple, CGReferenceMode.Self, scaleUniform: true, 0f, 1f, 1f, AnimationCurve.Linear(0f, 1f, 1f, 1f), AnimationCurve.Linear(0f, 1f, 1f, 1f));
			DeformMeshes(inputMeshes, inputSpots, outputSpots, outputMeshes, path, stretchToEnd, threadPoolWorker, scaleParameters);
		}

		public static void DeformMeshes([NotNull] List<CGVMesh> inputMeshes, SubArray<CGSpot> inputSpots, SubArray<CGSpot> outputSpots, [NotNull] CGVMesh[] outputMeshes, [NotNull] CGPath path, bool stretchToEnd, ThreadPoolWorker<CGSpot> threadPoolWorker, ScaleParameters scaleParameters)
		{
			if (inputMeshes == null)
			{
				throw new ArgumentNullException("inputMeshes");
			}
			if (outputMeshes == null)
			{
				throw new ArgumentNullException("outputMeshes");
			}
			if (outputSpots.Array == null)
			{
				throw new ArgumentNullException("outputSpots");
			}
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (inputSpots.Count == 0)
			{
				throw new ArgumentException("input spots should have at least one element", "inputSpots");
			}
			if (inputMeshes.Count == 0)
			{
				throw new ArgumentException("input meshes should have at least one element", "inputMeshes");
			}
			bool isCurveEvaluationNeeded = IsCurveEvaluationNeeded(scaleParameters);
			CGSpot[] array = inputSpots.Array;
			int count = inputSpots.Count;
			for (int i = 0; i < count; i++)
			{
				CGVMesh cGVMesh = inputMeshes[array[i].Index];
				CGVMesh cGVMesh2 = new CGVMesh(cGVMesh.Count, cGVMesh.HasUV, cGVMesh.HasUV2, cGVMesh.HasNormals, cGVMesh.HasTangents);
				if (cGVMesh.HasUV)
				{
					Array.Copy(cGVMesh.UVs.Array, 0, cGVMesh2.UVs.Array, 0, cGVMesh.UVs.Count);
				}
				if (cGVMesh.HasUV2)
				{
					Array.Copy(cGVMesh.UV2s.Array, 0, cGVMesh2.UV2s.Array, 0, cGVMesh.UV2s.Count);
				}
				cGVMesh2.SubMeshes = new CGVSubMesh[cGVMesh.SubMeshes.Length];
				for (int j = 0; j < cGVMesh.SubMeshes.Length; j++)
				{
					cGVMesh2.SubMeshes[j] = new CGVSubMesh(cGVMesh.SubMeshes[j]);
				}
				outputMeshes[i] = cGVMesh2;
			}
			float smallestVertexDistance;
			float stretchingAdditionalDistanceRatio;
			if (stretchToEnd)
			{
				CGSpot cGSpot = array[0];
				float spotDistance = GetSpotDistance(path, cGSpot.Position, path.Positions.Array, path.Count - 1, path.RelativeDistances.Array, path.Length);
				CGVMesh cGVMesh3 = inputMeshes[cGSpot.Index];
				if (cGVMesh3.Count == 0)
				{
					smallestVertexDistance = 0f;
				}
				else
				{
					SubArray<int> cachedSortedVertexIndices = cGVMesh3.GetCachedSortedVertexIndices();
					smallestVertexDistance = spotDistance + cGVMesh3.Vertices.Array[cachedSortedVertexIndices.Array[0]].z;
				}
				CGSpot cGSpot2 = array[count - 1];
				float spotDistance2 = GetSpotDistance(path, cGSpot2.Position, path.Positions.Array, path.Count - 1, path.RelativeDistances.Array, path.Length);
				CGVMesh cGVMesh4 = inputMeshes[cGSpot2.Index];
				float num;
				if (cGVMesh4.Count == 0)
				{
					num = 0f;
				}
				else
				{
					SubArray<int> cachedSortedVertexIndices2 = cGVMesh4.GetCachedSortedVertexIndices();
					num = spotDistance2 + cGVMesh4.Vertices.Array[cachedSortedVertexIndices2.Array[cGVMesh4.Vertices.Count - 1]].z;
				}
				float num2 = num - smallestVertexDistance;
				stretchingAdditionalDistanceRatio = ((num2 > 0f) ? ((path.Length - num) / num2) : 0f);
			}
			else
			{
				smallestVertexDistance = (stretchingAdditionalDistanceRatio = float.NaN);
			}
			Action<CGSpot, int, int> action = delegate(CGSpot spot, int spotIndex, int elementsCount)
			{
				int count2 = path.Count;
				Vector3[] array2 = path.Directions.Array;
				Vector3[] array3 = path.Normals.Array;
				float[] array4 = path.RelativeDistances.Array;
				Vector3[] array5 = path.Positions.Array;
				CGVMesh cGVMesh5 = inputMeshes[spot.Index];
				Vector3[] array6 = cGVMesh5.Vertices.Array;
				Vector3[] array7 = cGVMesh5.NormalsList.Array;
				Vector4[] array8 = cGVMesh5.TangentsList.Array;
				int count3 = cGVMesh5.Vertices.Count;
				int count4 = cGVMesh5.NormalsList.Count;
				int count5 = cGVMesh5.TangentsList.Count;
				int[] array9 = cGVMesh5.GetCachedSortedVertexIndices().Array;
				CGVMesh obj = outputMeshes[spotIndex];
				Vector3[] array10 = obj.Vertices.Array;
				Vector3[] array11 = obj.NormalsList.Array;
				Vector4[] array12 = obj.TangentsList.Array;
				obj.Name = cGVMesh5.Name;
				int num3 = count2 - 1;
				float length = path.Length;
				float num4 = 1f / length;
				Vector3 position = spot.Position;
				Vector3 scale = spot.Scale;
				float z = spot.Scale.z;
				float num5 = 1f / spot.Scale.x;
				float num6 = 1f / spot.Scale.y;
				float num7 = 1f / spot.Scale.z;
				float x = position.x;
				float y = position.y;
				float z2 = position.z;
				float spotDistance3 = GetSpotDistance(path, position, array5, num3, array4, length);
				outputSpots.Array[spotIndex] = new CGSpot(spotIndex, position, Quaternion.identity, scale);
				float num8 = float.NaN;
				Vector2 vector = Vector3.zero;
				Vector3 vector2 = Vector3.zero;
				float num9 = float.NaN;
				float num10 = float.NaN;
				float num11 = float.NaN;
				float num12 = float.NaN;
				float num13 = float.NaN;
				float num14 = float.NaN;
				float num15 = float.NaN;
				float num16 = float.NaN;
				float num17 = float.NaN;
				Vector2 vector4 = default(Vector2);
				Vector3 vector5 = default(Vector3);
				Vector3 vector8 = default(Vector3);
				Vector3 vector10 = default(Vector3);
				Vector4 vector12 = default(Vector4);
				for (int k = 0; k < count3; k++)
				{
					int num18 = array9[k];
					Vector3 vector3 = array6[num18];
					vector4.x = vector3.x;
					vector4.y = vector3.y;
					float z3 = vector3.z;
					Vector2 vector6;
					float num19;
					float num20;
					float num21;
					float num22;
					float num23;
					float num24;
					float num25;
					float num26;
					float num27;
					if (k > 0 && num8 == z3)
					{
						vector5 = vector2;
						vector6 = vector;
						num19 = num9;
						num20 = num10;
						num21 = num11;
						num22 = num12;
						num23 = num13;
						num24 = num14;
						num25 = num15;
						num26 = num16;
						num27 = num17;
					}
					else
					{
						float num28 = spotDistance3 + vector3.z * z;
						if (stretchToEnd)
						{
							num28 += (num28 - smallestVertexDistance) * stretchingAdditionalDistanceRatio;
						}
						float num29 = num28 * num4;
						if (path.Seamless)
						{
							for (; num29 < 0f; num29 += 1f)
							{
							}
							while (num29 > 1f)
							{
								num29 -= 1f;
							}
						}
						else if (num29 < 0f)
						{
							num29 = 0f;
						}
						else if (num29 > 1f)
						{
							num29 = 1f;
						}
						int num30 = CurvyUtility.InterpolationSearch(array4, count2, num29);
						float t;
						if (num30 == num3)
						{
							num30--;
							t = 1f;
						}
						else
						{
							t = (num29 - array4[num30]) / (array4[num30 + 1] - array4[num30]);
						}
						int num31 = Math.Min(num30 + 1, num3);
						Vector3 vector7 = Vector3.LerpUnclamped(array5[num30], array5[num31], t);
						switch (scaleParameters.ScaleMode)
						{
						case ScaleMode.Advanced:
							if (isCurveEvaluationNeeded)
							{
								float relativeDistance = ScalingModule.GetRelativeDistance(num30, scaleParameters.ScaleReference, path.RelativeDistances, path.SourceRelativeDistances);
								lock (scaleParameters)
								{
									vector6 = ScalingModule.GetAdvancedScale(relativeDistance, scaleParameters.ScaleOffset, scaleParameters.ScaleUniform, scaleParameters.ScaleX, scaleParameters.ScaleMultiplierX, scaleParameters.ScaleY, scaleParameters.ScaleMultiplierY);
								}
							}
							else
							{
								vector6 = ScalingModule.GetSimpleScale(scaleParameters.ScaleUniform, scaleParameters.ScaleX, scaleParameters.ScaleY);
							}
							break;
						case ScaleMode.Simple:
							vector6 = ScalingModule.GetSimpleScale(scaleParameters.ScaleUniform, scaleParameters.ScaleX, scaleParameters.ScaleY);
							break;
						default:
							throw new ArgumentOutOfRangeException();
						}
						vector5.x = (vector7.x - x) * num5;
						vector5.y = (vector7.y - y) * num6;
						vector5.z = (vector7.z - z2) * num7;
						Quaternion a = Quaternion.LookRotation(array2[num30], array3[num30]);
						Quaternion b = Quaternion.LookRotation(array2[num31], array3[num31]);
						Quaternion quaternion = Quaternion.LerpUnclamped(a, b, t);
						float num32 = quaternion.x * 2f;
						float num33 = quaternion.y * 2f;
						float num34 = quaternion.z * 2f;
						num19 = quaternion.x * num32;
						num20 = quaternion.y * num33;
						num21 = quaternion.z * num34;
						num22 = quaternion.x * num33;
						num23 = quaternion.x * num34;
						num24 = quaternion.y * num34;
						num25 = quaternion.w * num32;
						num26 = quaternion.w * num33;
						num27 = quaternion.w * num34;
						num8 = z3;
						vector = vector6;
						vector2 = vector5;
						num9 = num19;
						num10 = num20;
						num11 = num21;
						num12 = num22;
						num13 = num23;
						num14 = num24;
						num15 = num25;
						num16 = num26;
						num17 = num27;
					}
					vector8.x = vector5.x + (1f - (num20 + num21)) * vector6.x * vector4.x + (num22 - num27) * vector6.y * vector4.y;
					vector8.y = vector5.y + (num22 + num27) * vector6.x * vector4.x + (1f - (num19 + num21)) * vector6.y * vector4.y;
					vector8.z = vector5.z + (num23 - num26) * vector6.x * vector4.x + (num24 + num25) * vector6.y * vector4.y;
					array10[num18] = vector8;
					if (count4 > num18)
					{
						Vector3 vector9 = array7[num18];
						vector10.x = (1f - (num20 + num21)) * vector9.x + (num22 - num27) * vector9.y + (num23 + num26) * vector9.z;
						vector10.y = (num22 + num27) * vector9.x + (1f - (num19 + num21)) * vector9.y + (num24 - num25) * vector9.z;
						vector10.z = (num23 - num26) * vector9.x + (num24 + num25) * vector9.y + (1f - (num19 + num20)) * vector9.z;
						array11[num18] = vector10;
					}
					if (count5 > num18)
					{
						Vector4 vector11 = array8[num18];
						vector12.x = (1f - (num20 + num21)) * vector11.x + (num22 - num27) * vector11.y + (num23 + num26) * vector11.z;
						vector12.y = (num22 + num27) * vector11.x + (1f - (num19 + num21)) * vector11.y + (num24 - num25) * vector11.z;
						vector12.z = (num23 - num26) * vector11.x + (num24 + num25) * vector11.y + (1f - (num19 + num20)) * vector11.z;
						vector12.w = vector11.w;
						array12[num18] = vector12;
					}
				}
			};
			threadPoolWorker.ParallelFor(action, array, count);
		}

		private static bool IsCurveEvaluationNeeded(ScaleParameters scaleParameters)
		{
			return scaleParameters.ScaleMode switch
			{
				ScaleMode.Simple => false, 
				ScaleMode.Advanced => scaleParameters.ScaleUniform ? (!scaleParameters.ScaleMultiplierX.ValueIsOne()) : (!scaleParameters.ScaleMultiplierX.ValueIsOne() || !scaleParameters.ScaleMultiplierY.ValueIsOne()), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		private static float GetSpotDistance(CGPath path, Vector3 spotPosition, Vector3[] pathPoints, int maxIndex, float[] pathRelativeDistances, float pathLength)
		{
			CurvyUtility.GetNearestPointIndex(spotPosition, pathPoints, path.Positions.Count, out var index, out var fragement);
			int num = Math.Min(index + 1, maxIndex);
			return Mathf.LerpUnclamped(pathRelativeDistances[index], pathRelativeDistances[num], fragement) * pathLength;
		}

		private static SubArray<CGSpot>? ToOneDimensionalArray(List<CGSpots> spotsList, out bool arrayIsCopy)
		{
			SubArray<CGSpot>? result;
			switch (spotsList.Count)
			{
			case 1:
				if (spotsList[0] != null)
				{
					result = new SubArray<CGSpot>(spotsList[0].Spots.Array, spotsList[0].Spots.Count);
					arrayIsCopy = false;
				}
				else
				{
					result = null;
					arrayIsCopy = false;
				}
				break;
			case 0:
				result = null;
				arrayIsCopy = false;
				break;
			default:
			{
				result = ArrayPools.CGSpot.Allocate(spotsList.Where((CGSpots s) => s != null).Sum((CGSpots s) => s.Count));
				arrayIsCopy = true;
				CGSpot[] array = result.Value.Array;
				int num = 0;
				foreach (CGSpots spots in spotsList)
				{
					if (spots != null)
					{
						Array.Copy(spots.Spots.Array, 0, array, num, spots.Spots.Count);
						num += spots.Spots.Count;
					}
				}
				break;
			}
			}
			return result;
		}
	}
}
