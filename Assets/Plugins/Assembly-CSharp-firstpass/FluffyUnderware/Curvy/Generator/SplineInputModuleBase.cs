using System;
using System.Collections.Generic;
using System.Globalization;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public abstract class SplineInputModuleBase : CGModule
	{
		[Tab("General")]
		[SerializeField]
		[Tooltip("Makes this module use the cached approximations of the spline's positions and tangents")]
		private bool m_UseCache;

		[Tooltip("Whether to use local or global coordinates of the input's control points.\r\nUsing the global space when the input's transform is updating every frame will lead to the generator refreshing too frequently")]
		[SerializeField]
		private bool m_UseGlobalSpace;

		[Tab("Range")]
		[SerializeField]
		protected CurvySplineSegment m_StartCP;

		[FieldCondition("m_StartCP", null, true, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		[SerializeField]
		protected CurvySplineSegment m_EndCP;

		public bool UseCache
		{
			get
			{
				return m_UseCache;
			}
			set
			{
				if (m_UseCache != value)
				{
					m_UseCache = value;
					base.Dirty = true;
				}
			}
		}

		public CurvySplineSegment StartCP
		{
			get
			{
				return m_StartCP;
			}
			set
			{
				if (m_StartCP != value)
				{
					m_StartCP = value;
					ValidateStartAndEndCps();
					base.Dirty = true;
				}
			}
		}

		public CurvySplineSegment EndCP
		{
			get
			{
				return m_EndCP;
			}
			set
			{
				if (m_EndCP != value)
				{
					m_EndCP = value;
					ValidateStartAndEndCps();
					base.Dirty = true;
				}
			}
		}

		public bool UseGlobalSpace
		{
			get
			{
				return m_UseGlobalSpace;
			}
			set
			{
				if (m_UseGlobalSpace != value)
				{
					m_UseGlobalSpace = value;
					base.Dirty = true;
				}
			}
		}

		public override bool IsConfigured
		{
			get
			{
				if (base.IsConfigured)
				{
					return InputSpline != null;
				}
				return false;
			}
		}

		public override bool IsInitialized
		{
			get
			{
				if (base.IsInitialized)
				{
					if (!(InputSpline == null))
					{
						return InputSpline.IsInitialized;
					}
					return true;
				}
				return false;
			}
		}

		public bool PathIsClosed
		{
			get
			{
				if (IsConfigured)
				{
					return getPathClosed(InputSpline);
				}
				return false;
			}
		}

		protected abstract CurvySpline InputSpline { get; set; }

		public void SetRange(CurvySplineSegment rangeStart, CurvySplineSegment rangeEnd)
		{
			if (StartCP != rangeStart || EndCP != rangeEnd)
			{
				m_StartCP = rangeStart;
				m_EndCP = rangeEnd;
				ValidateStartAndEndCps();
				base.Dirty = true;
			}
		}

		public void ClearRange()
		{
			SetRange(null, null);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 250f;
			OnSplineAssigned();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if ((bool)InputSpline)
			{
				InputSpline.OnRefresh.RemoveListener(OnSplineRefreshed);
				InputSpline.OnInitialized.RemoveListener(OnSplineInitialized);
				CurvySpline inputSpline = InputSpline;
				inputSpline.OnGlobalCoordinatesChanged = (Action<CurvySpline>)Delegate.Remove(inputSpline.OnGlobalCoordinatesChanged, new Action<CurvySpline>(OnInputSplineCoordinatesChanged));
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			ValidateStartAndEndCps();
			if (base.IsActiveAndEnabled)
			{
				OnSplineAssigned();
			}
		}

		public override void Reset()
		{
			base.Reset();
			InputSpline = null;
			UseCache = false;
			StartCP = null;
			EndCP = null;
			UseGlobalSpace = false;
		}

		private void OnSplineRefreshed(CurvySplineEventArgs e)
		{
			if (base.IsActiveAndEnabled)
			{
				if (InputSpline == e.Spline)
				{
					ForceRefresh();
				}
				else
				{
					e.Spline.OnRefresh.RemoveListener(OnSplineRefreshed);
				}
			}
		}

		private void OnSplineInitialized(CurvySplineEventArgs e)
		{
			if (base.IsActiveAndEnabled)
			{
				if (InputSpline == e.Spline)
				{
					ValidateStartAndEndCps();
					base.Dirty = true;
				}
				else
				{
					e.Spline.OnInitialized.RemoveListener(OnSplineInitialized);
				}
			}
		}

		private void OnInputSplineCoordinatesChanged(CurvySpline sender)
		{
			if (!base.IsActiveAndEnabled)
			{
				return;
			}
			if (InputSpline == sender)
			{
				if (UseGlobalSpace)
				{
					ForceRefresh();
				}
			}
			else
			{
				CurvySpline inputSpline = InputSpline;
				inputSpline.OnGlobalCoordinatesChanged = (Action<CurvySpline>)Delegate.Remove(inputSpline.OnGlobalCoordinatesChanged, new Action<CurvySpline>(OnInputSplineCoordinatesChanged));
			}
		}

		private void ForceRefresh()
		{
			base.Dirty = true;
		}

		private bool getPathClosed(CurvySpline spline)
		{
			if (!spline || !spline.Closed)
			{
				return false;
			}
			return EndCP == null;
		}

		[CanBeNull]
		protected CGData GetSplineData(CurvySpline spline, bool fullPath, CGDataRequestRasterization raster, CGDataRequestMetaCGOptions options)
		{
			if (spline == null || spline.Count == 0)
			{
				return null;
			}
			float num = (((bool)StartCP && (bool)EndCP) ? (EndCP.Distance - StartCP.Distance) : spline.Length);
			float num2 = num * raster.Start;
			if ((bool)StartCP)
			{
				num2 = (num2 + StartCP.Distance) % spline.Length;
			}
			float num3 = num2 + num * raster.RasterizedRelativeLength;
			float num4 = CurvySpline.CalculateSamplingPointsPerUnit(raster.Resolution, spline.MaxPointsPerUnit);
			float num5 = num3 - num2;
			float num6 = Mathf.Min(num5 / (num * raster.RasterizedRelativeLength * num4), num5 / 3f);
			CGShape cGShape = (fullPath ? new CGPath() : new CGShape());
			cGShape.Length = num3 - num2;
			cGShape.SourceIsManaged = IsManagedResource(spline);
			cGShape.Closed = spline.Closed;
			cGShape.Seamless = spline.Closed && raster.RasterizedRelativeLength == 1f;
			if (cGShape.Length == 0f)
			{
				return cGShape;
			}
			List<ControlPointOption> list;
			int initialMaterialID;
			float initialMaxStep;
			if ((bool)options)
			{
				list = CGUtility.GetControlPointsWithOptions(options, spline, num2, num3, raster.Mode == CGDataRequestRasterization.ModeEnum.Optimized, out initialMaterialID, out initialMaxStep);
			}
			else
			{
				list = new List<ControlPointOption>();
				initialMaterialID = 0;
				initialMaxStep = float.MaxValue;
			}
			float tf = spline.DistanceToTF(num2);
			float startTF = tf;
			float num7 = ((num3 > spline.Length && spline.Closed) ? (spline.DistanceToTF(num3 - spline.Length) + 1f) : spline.DistanceToTF(num3));
			float currentDistance = num2;
			int num8 = raster.Mode switch
			{
				CGDataRequestRasterization.ModeEnum.Even => Mathf.Max(20, Mathf.CeilToInt(1.1f * (num3 - num2) / num6)), 
				CGDataRequestRasterization.ModeEnum.Optimized => Mathf.Max(20, Mathf.CeilToInt(0.2f * (num3 - num2) / num6)), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			SubArrayList<Vector3> positionList = new SubArrayList<Vector3>(num8, ArrayPools.Vector3);
			SubArrayList<float> relativeFList = new SubArrayList<float>(num8, ArrayPools.Single);
			SubArrayList<float> sourceFList = new SubArrayList<float>(num8, ArrayPools.Single);
			SubArrayList<Vector3> tangentList = new SubArrayList<Vector3>(fullPath ? num8 : 0, ArrayPools.Vector3);
			SubArrayList<Vector3> upList = new SubArrayList<Vector3>(fullPath ? num8 : 0, ArrayPools.Vector3);
			List<DuplicateSamplePoint> list2 = new List<DuplicateSamplePoint>();
			List<SamplePointUData> list3 = new List<SamplePointUData>();
			bool duplicatePoint = false;
			SamplePointsMaterialGroup currentMaterialGroup = new SamplePointsMaterialGroup(initialMaterialID);
			SamplePointsPatch currentPatch = new SamplePointsPatch(0);
			CurvyClamping clamping = (cGShape.Closed ? CurvyClamping.Loop : CurvyClamping.Clamp);
			int num9 = 2000000;
			CGDataRequestRasterization.ModeEnum mode = raster.Mode;
			if (mode != 0)
			{
				if (mode == CGDataRequestRasterization.ModeEnum.Optimized)
				{
					float stepDist = num6 / spline.Length;
					float angleThreshold = raster.AngleThreshold;
					Vector3 position;
					Vector3 tangent;
					if (UseCache)
					{
						spline.InterpolateAndGetTangentFast(tf, out position, out tangent);
					}
					else
					{
						spline.InterpolateAndGetTangent(tf, out position, out tangent);
					}
					while (tf < num7 && num9-- > 0)
					{
						AddPoint(currentDistance / spline.Length, (currentDistance - num2) / cGShape.Length, fullPath, position, tangent, spline.GetOrientationUpFast(tf % 1f), ref sourceFList, ref relativeFList, ref positionList, ref tangentList, ref upList);
						float stopTF = ((list.Count > 0) ? list[0].TF : num7);
						bool flag = MoveByAngleExt(spline, UseCache, ref tf, initialMaxStep, angleThreshold, out position, out tangent, stopTF, cGShape.Closed, stepDist);
						currentDistance = spline.TFToDistance(tf, clamping);
						if (currentDistance < num2)
						{
							currentDistance += spline.Length;
						}
						if (Mathf.Approximately(tf, num7) || tf > num7)
						{
							currentDistance = num3;
							num7 = (cGShape.Closed ? DTMath.Repeat(num7, 1f) : Mathf.Clamp01(num7));
							if (fullPath)
							{
								if (UseCache)
								{
									spline.InterpolateAndGetTangentFast(num7, out position, out tangent);
								}
								else
								{
									spline.InterpolateAndGetTangent(num7, out position, out tangent);
								}
							}
							else
							{
								position = (UseCache ? spline.InterpolateFast(num7) : spline.Interpolate(num7));
							}
							AddPoint(currentDistance / spline.Length, (currentDistance - num2) / cGShape.Length, fullPath, position, tangent, spline.GetOrientationUpFast(num7), ref sourceFList, ref relativeFList, ref positionList, ref tangentList, ref upList);
							break;
						}
						if (flag)
						{
							if (list.Count <= 0)
							{
								AddPoint(currentDistance / spline.Length, (currentDistance - num2) / cGShape.Length, fullPath, position, tangent, spline.GetOrientationUpFast(tf), ref sourceFList, ref relativeFList, ref positionList, ref tangentList, ref upList);
								break;
							}
							ControlPointOption options2 = list[0];
							ProcessControlPointOptions(options2, positionList.Count, cGShape.MaterialGroups, list3, list2, ref currentMaterialGroup, ref currentPatch, out currentDistance, out duplicatePoint);
							list.RemoveAt(0);
							initialMaxStep = options2.MaxStepDistance;
							if (duplicatePoint)
							{
								AddPoint(currentDistance / spline.Length, (currentDistance - num2) / cGShape.Length, fullPath, position, tangent, spline.GetOrientationUpFast(tf), ref sourceFList, ref relativeFList, ref positionList, ref tangentList, ref upList);
							}
						}
					}
					if (num9 <= 0)
					{
						Debug.LogError("[Curvy] He's dead, Jim! Deadloop in SplineInputModuleBase.GetSplineData (Optimized)! Please send a bug report.");
					}
					currentPatch.End = positionList.Count - 1;
					currentMaterialGroup.Patches.Add(currentPatch);
					if (list.Count > 0 && list[0].UVShift)
					{
						list3.Add(new SamplePointUData(positionList.Count - 1, list[0]));
					}
					if (cGShape.Closed)
					{
						list2.Add(new DuplicateSamplePoint(positionList.Count - 1, 0, spline[0].GetMetadata<MetaCGOptions>(autoCreate: true).CorrectedHardEdge));
					}
					FillData(cGShape, currentMaterialGroup, sourceFList, relativeFList, fullPath, positionList, tangentList, upList, UseGlobalSpace, spline.transform, base.Generator.transform);
				}
			}
			else
			{
				while (currentDistance <= num3 && --num9 > 0)
				{
					tf = spline.DistanceToTF(spline.ClampDistance(currentDistance, clamping));
					float num10 = (currentDistance - num2) / cGShape.Length;
					if (Mathf.Approximately(1f, num10))
					{
						num10 = 1f;
					}
					float localF;
					CurvySplineSegment curvySplineSegment = spline.TFToSegment(tf, out localF, CurvyClamping.Clamp);
					Vector3 position2;
					Vector3 tangent2;
					Vector3 up;
					if (fullPath)
					{
						if (UseCache)
						{
							curvySplineSegment.InterpolateAndGetTangentFast(localF, out position2, out tangent2);
						}
						else
						{
							curvySplineSegment.InterpolateAndGetTangent(localF, out position2, out tangent2);
						}
						up = curvySplineSegment.GetOrientationUpFast(localF);
					}
					else
					{
						position2 = (UseCache ? curvySplineSegment.InterpolateFast(localF) : curvySplineSegment.Interpolate(localF));
						tangent2 = Vector3.zero;
						up = Vector3.zero;
					}
					AddPoint(currentDistance / spline.Length, num10, fullPath, position2, tangent2, up, ref sourceFList, ref relativeFList, ref positionList, ref tangentList, ref upList);
					if (duplicatePoint)
					{
						AddPoint(currentDistance / spline.Length, num10, fullPath, position2, tangent2, up, ref sourceFList, ref relativeFList, ref positionList, ref tangentList, ref upList);
						duplicatePoint = false;
					}
					currentDistance += num6;
					if (list.Count > 0 && currentDistance >= list[0].Distance)
					{
						ProcessControlPointOptions(list[0], positionList.Count, cGShape.MaterialGroups, list3, list2, ref currentMaterialGroup, ref currentPatch, out currentDistance, out duplicatePoint);
						list.RemoveAt(0);
					}
					if (currentDistance > num3 && num10 < 1f)
					{
						currentDistance = num3;
					}
				}
				if (num9 <= 0)
				{
					Debug.LogError("[Curvy] He's dead, Jim! Deadloop in SplineInputModuleBase.GetSplineData (Even)! Please send a bug report.");
				}
				currentPatch.End = positionList.Count - 1;
				currentMaterialGroup.Patches.Add(currentPatch);
				if (cGShape.Closed)
				{
					list2.Add(new DuplicateSamplePoint(positionList.Count - 1, 0, spline[0].GetMetadata<MetaCGOptions>(autoCreate: true).CorrectedHardEdge));
				}
				FillData(cGShape, currentMaterialGroup, sourceFList, relativeFList, fullPath, positionList, tangentList, upList, UseGlobalSpace, spline.transform, base.Generator.transform);
			}
			cGShape.CustomValues = ArrayPools.Single.Clone(cGShape.RelativeDistances);
			cGShape.DuplicatePoints = list2;
			if (!fullPath)
			{
				cGShape.RecalculateNormals();
				if (list3.Count > 0)
				{
					CalculateExtendedUV(spline, startTF, num7, list3, cGShape);
					if (spline.Closed)
					{
						UIMessages.Add("Extended UV features (UV Edge, Explicit U) are used in the Meta CG Options of a closed spline. Those features are supported only for open splines");
					}
				}
			}
			return cGShape;
		}

		private static void ProcessControlPointOptions(ControlPointOption options, int positionsCount, List<SamplePointsMaterialGroup> shapeMaterialGroups, List<SamplePointUData> extendedUVData, List<DuplicateSamplePoint> duplicatePoints, ref SamplePointsMaterialGroup currentMaterialGroup, ref SamplePointsPatch currentPatch, out float currentDistance, out bool duplicatePoint)
		{
			if (options.UVEdge || options.UVShift)
			{
				extendedUVData.Add(new SamplePointUData(positionsCount, options));
			}
			currentDistance = options.Distance;
			duplicatePoint = options.HardEdge || options.MaterialID != currentMaterialGroup.MaterialID || options.UVEdge;
			if (duplicatePoint)
			{
				duplicatePoints.Add(new DuplicateSamplePoint(positionsCount, positionsCount + 1, options.HardEdge));
				currentPatch.End = positionsCount;
				currentMaterialGroup.Patches.Add(currentPatch);
				if (currentMaterialGroup.MaterialID != options.MaterialID)
				{
					shapeMaterialGroups.Add(currentMaterialGroup);
					currentMaterialGroup = new SamplePointsMaterialGroup(options.MaterialID);
				}
				currentPatch = new SamplePointsPatch(positionsCount + 1);
				if (options.UVEdge || options.UVShift)
				{
					extendedUVData.Add(new SamplePointUData(positionsCount + 1, options));
				}
			}
		}

		private static void FillData(CGShape dataToFill, SamplePointsMaterialGroup materialGroup, SubArrayList<float> sourceFs, SubArrayList<float> relativeFs, bool isFullPath, SubArrayList<Vector3> positions, SubArrayList<Vector3> tangents, SubArrayList<Vector3> normals, bool considerSplineTransform, Transform splineTransform, Transform generatorTransform)
		{
			if (considerSplineTransform)
			{
				Vector3[] array = positions.Array;
				for (int i = 0; i < positions.Count; i++)
				{
					array[i] = generatorTransform.InverseTransformPoint(splineTransform.TransformPoint(array[i]));
				}
				if (isFullPath)
				{
					Vector3[] array2 = normals.Array;
					Vector3[] array3 = tangents.Array;
					for (int j = 0; j < tangents.Count; j++)
					{
						array3[j] = generatorTransform.InverseTransformDirection(splineTransform.TransformDirection(array3[j]));
					}
					for (int k = 0; k < normals.Count; k++)
					{
						array2[k] = generatorTransform.InverseTransformDirection(splineTransform.TransformDirection(array2[k]));
					}
				}
			}
			dataToFill.MaterialGroups.Add(materialGroup);
			dataToFill.SourceRelativeDistances = sourceFs.ToSubArray();
			dataToFill.RelativeDistances = relativeFs.ToSubArray();
			dataToFill.Positions = positions.ToSubArray();
			if (isFullPath)
			{
				((CGPath)dataToFill).Directions = tangents.ToSubArray();
				dataToFill.Normals = normals.ToSubArray();
			}
		}

		private static void AddPoint(float sourceF, float relativeF, bool isFullPath, Vector3 position, Vector3 tangent, Vector3 up, ref SubArrayList<float> sourceFList, ref SubArrayList<float> relativeFList, ref SubArrayList<Vector3> positionList, ref SubArrayList<Vector3> tangentList, ref SubArrayList<Vector3> upList)
		{
			sourceF = (sourceF.Approximately(1f) ? 1f : (sourceF % 1f));
			sourceFList.Add(sourceF);
			positionList.Add(position);
			relativeFList.Add(relativeF);
			if (isFullPath)
			{
				tangentList.Add(tangent);
				upList.Add(up);
			}
		}

		private static bool MoveByAngleExt(CurvySpline spline, bool useCache, ref float tf, float maxDistance, float maxAngle, out Vector3 pos, out Vector3 tan, float stopTF, bool loop, float stepDist)
		{
			if (!loop)
			{
				tf = Mathf.Clamp01(tf);
			}
			float tf2 = (loop ? (tf % 1f) : tf);
			CurvySplineSegment curvySplineSegment = spline.TFToSegment(tf2, out var localF, CurvyClamping.Clamp);
			if (useCache)
			{
				curvySplineSegment.InterpolateAndGetTangentFast(localF, out pos, out tan);
			}
			else
			{
				curvySplineSegment.InterpolateAndGetTangent(localF, out pos, out tan);
			}
			Vector3 vector = pos;
			Vector3 from = tan;
			float num = 0f;
			float num2 = 0f;
			if (stopTF < tf && loop)
			{
				stopTF += 1f;
			}
			bool flag = false;
			Vector3 vector2 = default(Vector3);
			while (tf < stopTF && !flag)
			{
				tf = Mathf.Min(stopTF, tf + stepDist);
				tf2 = (loop ? (tf % 1f) : tf);
				curvySplineSegment = spline.TFToSegment(tf2, out localF, CurvyClamping.Clamp);
				if (useCache)
				{
					curvySplineSegment.InterpolateAndGetTangentFast(localF, out pos, out tan);
				}
				else
				{
					curvySplineSegment.InterpolateAndGetTangent(localF, out pos, out tan);
				}
				vector2.x = pos.x - vector.x;
				vector2.y = pos.y - vector.y;
				vector2.z = pos.z - vector.z;
				num += vector2.magnitude;
				float num3 = Vector3.Angle(from, tan);
				num2 += num3;
				if (num >= maxDistance || num2 >= maxAngle || (num3 == 0f && num2 > 0f))
				{
					flag = true;
					continue;
				}
				vector = pos;
				from = tan;
			}
			return Mathf.Approximately(tf, stopTF);
		}

		private static void CalculateExtendedUV(CurvySpline spline, float startTF, float endTF, List<SamplePointUData> ext, CGShape data)
		{
			CurvySplineSegment cp;
			MetaCGOptions metaCGOptions = findPreviousReferenceCPOptions(spline, startTF, out cp);
			CurvySplineSegment cp2;
			MetaCGOptions metaCGOptions2 = findNextReferenceCPOptions(spline, startTF, out cp2);
			float num = ((!(spline.FirstVisibleControlPoint == cp2)) ? cp2.Distance : spline.Length);
			float t = (data.SourceRelativeDistances.Array[0] * spline.Length - cp.Distance) / (num - cp.Distance);
			float firstU = Mathf.LerpUnclamped(metaCGOptions.GetDefinedFirstU(0f), metaCGOptions2.GetDefinedFirstU(0f), t);
			float definedSecondU = metaCGOptions.GetDefinedSecondU(0f);
			ext.Insert(0, new SamplePointUData(0, startTF == 0f && metaCGOptions.CorrectedUVEdge, startTF == 0f && metaCGOptions.CorrectedUVEdge, firstU, definedSecondU));
			if (ext[ext.Count - 1].Vertex < data.Count - 1)
			{
				CurvySplineSegment cp3;
				MetaCGOptions metaCGOptions3 = findPreviousReferenceCPOptions(spline, endTF, out cp3);
				CurvySplineSegment cp4;
				MetaCGOptions metaCGOptions4 = findNextReferenceCPOptions(spline, endTF, out cp4);
				float t2;
				float b;
				if (spline.FirstVisibleControlPoint == cp4)
				{
					t2 = (data.SourceRelativeDistances.Array[data.Count - 1] * spline.Length - cp3.Distance) / (spline.Length - cp3.Distance);
					b = (metaCGOptions4.CorrectedUVEdge ? metaCGOptions4.FirstU : ((ext.Count <= 1) ? 1f : ((float)(Mathf.FloorToInt(ext[ext.Count - 1].UVEdge ? ext[ext.Count - 1].SecondU : ext[ext.Count - 1].FirstU) + 1))));
				}
				else
				{
					t2 = (data.SourceRelativeDistances.Array[data.Count - 1] * spline.Length - cp3.Distance) / (cp4.Distance - cp3.Distance);
					b = metaCGOptions4.GetDefinedFirstU(1f);
				}
				ext.Add(new SamplePointUData(data.Count - 1, uvEdge: false, hardEdge: false, Mathf.LerpUnclamped(metaCGOptions3.GetDefinedSecondU(0f), b, t2), 0f));
			}
			float num2 = 0f;
			float num3 = (ext[0].UVEdge ? ext[0].SecondU : ext[0].FirstU);
			float firstU2 = ext[1].FirstU;
			float num4 = data.RelativeDistances.Array[ext[1].Vertex] - data.RelativeDistances.Array[ext[0].Vertex];
			int num5 = 1;
			for (int i = 0; i < data.Count - 1; i++)
			{
				float num6 = (data.RelativeDistances.Array[i] - num2) / num4;
				data.CustomValues.Array[i] = (firstU2 - num3) * num6 + num3;
				if (ext[num5].Vertex == i && i + 1 < data.Count - 1)
				{
					float num7 = data.RelativeDistances.Array[ext[num5 + 1].Vertex];
					float num8 = data.RelativeDistances.Array[ext[num5].Vertex];
					if (num7.Approximately(num8))
					{
						num3 = (ext[num5].UVEdge ? ext[num5].SecondU : ext[num5].FirstU);
						num5++;
						num8 = num7;
						num7 = data.RelativeDistances.Array[ext[num5 + 1].Vertex];
					}
					else
					{
						num3 = ext[num5].FirstU;
					}
					firstU2 = ext[num5 + 1].FirstU;
					num4 = num7 - num8;
					num2 = data.RelativeDistances.Array[i];
					num5++;
				}
			}
			data.CustomValues.Array[data.Count - 1] = ext[ext.Count - 1].FirstU;
		}

		private static MetaCGOptions findPreviousReferenceCPOptions(CurvySpline spline, float tf, out CurvySplineSegment cp)
		{
			cp = spline.TFToSegment(tf);
			MetaCGOptions metadata;
			do
			{
				metadata = cp.GetMetadata<MetaCGOptions>(autoCreate: true);
				if (spline.FirstVisibleControlPoint == cp)
				{
					return metadata;
				}
				cp = spline.GetPreviousSegment(cp);
			}
			while ((bool)cp && !metadata.CorrectedUVEdge && !metadata.ExplicitU);
			return metadata;
		}

		private static MetaCGOptions findNextReferenceCPOptions(CurvySpline spline, float tf, out CurvySplineSegment cp)
		{
			cp = spline.TFToSegment(tf, out var _);
			MetaCGOptions metadata;
			do
			{
				cp = spline.GetNextControlPoint(cp);
				metadata = cp.GetMetadata<MetaCGOptions>(autoCreate: true);
				if (!spline.Closed && spline.LastVisibleControlPoint == cp)
				{
					return metadata;
				}
			}
			while (!metadata.CorrectedUVEdge && !metadata.ExplicitU && !(spline.FirstSegment == cp));
			return metadata;
		}

		protected virtual void OnSplineAssigned()
		{
			if ((bool)InputSpline)
			{
				InputSpline.OnRefresh.AddListenerOnce(OnSplineRefreshed);
				InputSpline.OnInitialized.AddListenerOnce(OnSplineInitialized);
				CurvySpline inputSpline = InputSpline;
				inputSpline.OnGlobalCoordinatesChanged = (Action<CurvySpline>)Delegate.Remove(inputSpline.OnGlobalCoordinatesChanged, new Action<CurvySpline>(OnInputSplineCoordinatesChanged));
				CurvySpline inputSpline2 = InputSpline;
				inputSpline2.OnGlobalCoordinatesChanged = (Action<CurvySpline>)Delegate.Combine(inputSpline2.OnGlobalCoordinatesChanged, new Action<CurvySpline>(OnInputSplineCoordinatesChanged));
			}
		}

		protected void ValidateStartAndEndCps()
		{
			if (!(InputSpline == null) && InputSpline.IsInitialized)
			{
				if ((bool)m_StartCP && m_StartCP.Spline != InputSpline)
				{
					DTLog.LogError(string.Format(CultureInfo.InvariantCulture, "[Curvy] Input module {0}: StartCP is not part of the input spline ({1})", base.name, InputSpline.name), this);
					m_StartCP = null;
				}
				if ((bool)m_EndCP && m_EndCP.Spline != InputSpline)
				{
					DTLog.LogError(string.Format(CultureInfo.InvariantCulture, "[Curvy] Input module {0}: EndCP is not part of the input spline ({1})", base.name, InputSpline.name), this);
					m_EndCP = null;
				}
				if (m_EndCP != null && m_StartCP != null && InputSpline.GetControlPointIndex(m_EndCP) <= InputSpline.GetControlPointIndex(m_StartCP))
				{
					DTLog.LogError(string.Format(CultureInfo.InvariantCulture, "[Curvy] Input module {0}: EndCP has an index ({1}) less or equal than StartCP ({2})", base.name, InputSpline.GetControlPointIndex(m_EndCP), InputSpline.GetControlPointIndex(m_StartCP)), this);
					m_EndCP = null;
				}
			}
		}
	}
}
