using System;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.Utils;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(0.73f, 0.87f, 0.98f, 1f)]
	public class CGShape : CGData
	{
		public List<SamplePointsMaterialGroup> MaterialGroups;

		public bool SourceIsManaged;

		public bool Closed;

		public bool Seamless;

		public float Length;

		private SubArray<float> relativeDistances;

		private SubArray<float> sourceRelativeDistances;

		private SubArray<Vector3> positions;

		private SubArray<Vector3> normals;

		private SubArray<float> customValues;

		private float mCacheLastF = float.MaxValue;

		private int mCacheLastIndex;

		private float mCacheLastFrag;

		public SubArray<float> RelativeDistances
		{
			get
			{
				return relativeDistances;
			}
			set
			{
				ArrayPools.Single.Free(relativeDistances);
				relativeDistances = value;
			}
		}

		public SubArray<float> SourceRelativeDistances
		{
			get
			{
				return sourceRelativeDistances;
			}
			set
			{
				ArrayPools.Single.Free(sourceRelativeDistances);
				sourceRelativeDistances = value;
			}
		}

		public SubArray<Vector3> Positions
		{
			get
			{
				return positions;
			}
			set
			{
				ArrayPools.Vector3.Free(positions);
				positions = value;
			}
		}

		public SubArray<Vector3> Normals
		{
			get
			{
				return normals;
			}
			set
			{
				ArrayPools.Vector3.Free(normals);
				normals = value;
			}
		}

		public SubArray<float> CustomValues
		{
			get
			{
				return customValues;
			}
			set
			{
				ArrayPools.Single.Free(customValues);
				customValues = value;
			}
		}

		public List<DuplicateSamplePoint> DuplicatePoints { get; set; }

		[UsedImplicitly]
		[Obsolete("Use RelativeDistances instead")]
		public float[] F
		{
			get
			{
				return RelativeDistances.CopyToArray(ArrayPools.Single);
			}
			set
			{
				RelativeDistances = new SubArray<float>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use SourceRelativeDistances instead")]
		public float[] SourceF
		{
			get
			{
				return SourceRelativeDistances.CopyToArray(ArrayPools.Single);
			}
			set
			{
				SourceRelativeDistances = new SubArray<float>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use Positions instead")]
		public Vector3[] Position
		{
			get
			{
				return Positions.CopyToArray(ArrayPools.Vector3);
			}
			set
			{
				Positions = new SubArray<Vector3>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use Normals instead")]
		public Vector3[] Normal
		{
			get
			{
				return Normals.CopyToArray(ArrayPools.Vector3);
			}
			set
			{
				Normals = new SubArray<Vector3>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use CustomValues instead")]
		public float[] Map
		{
			get
			{
				return CustomValues.CopyToArray(ArrayPools.Single);
			}
			set
			{
				CustomValues = new SubArray<float>(value);
			}
		}

		public override int Count => relativeDistances.Count;

		public CGShape()
		{
			sourceRelativeDistances = ArrayPools.Single.Allocate(0);
			relativeDistances = ArrayPools.Single.Allocate(0);
			positions = ArrayPools.Vector3.Allocate(0);
			normals = ArrayPools.Vector3.Allocate(0);
			customValues = ArrayPools.Single.Allocate(0);
			DuplicatePoints = new List<DuplicateSamplePoint>();
			MaterialGroups = new List<SamplePointsMaterialGroup>();
		}

		public CGShape(CGShape source)
		{
			positions = ArrayPools.Vector3.Clone(source.positions);
			normals = ArrayPools.Vector3.Clone(source.normals);
			customValues = ArrayPools.Single.Clone(source.customValues);
			DuplicatePoints = new List<DuplicateSamplePoint>(source.DuplicatePoints);
			relativeDistances = ArrayPools.Single.Clone(source.relativeDistances);
			sourceRelativeDistances = ArrayPools.Single.Clone(source.sourceRelativeDistances);
			MaterialGroups = new List<SamplePointsMaterialGroup>(source.MaterialGroups.Count);
			foreach (SamplePointsMaterialGroup materialGroup in source.MaterialGroups)
			{
				MaterialGroups.Add(materialGroup.Clone());
			}
			Closed = source.Closed;
			Seamless = source.Seamless;
			Length = source.Length;
			SourceIsManaged = source.SourceIsManaged;
		}

		protected override bool Dispose(bool disposing)
		{
			bool num = base.Dispose(disposing);
			if (num)
			{
				ArrayPools.Single.Free(sourceRelativeDistances);
				ArrayPools.Single.Free(relativeDistances);
				ArrayPools.Vector3.Free(positions);
				ArrayPools.Vector3.Free(normals);
				ArrayPools.Single.Free(customValues);
			}
			return num;
		}

		public override T Clone<T>()
		{
			return new CGShape(this) as T;
		}

		public static void Copy(CGShape dest, CGShape source)
		{
			ArrayPools.Vector3.Resize(ref dest.positions, source.positions.Count);
			Array.Copy(source.positions.Array, 0, dest.positions.Array, 0, source.positions.Count);
			ArrayPools.Vector3.Resize(ref dest.normals, source.normals.Count);
			Array.Copy(source.normals.Array, 0, dest.normals.Array, 0, source.normals.Count);
			ArrayPools.Single.Resize(ref dest.customValues, source.customValues.Count);
			Array.Copy(source.customValues.Array, 0, dest.customValues.Array, 0, source.customValues.Count);
			ArrayPools.Single.Resize(ref dest.relativeDistances, source.relativeDistances.Count);
			Array.Copy(source.relativeDistances.Array, 0, dest.relativeDistances.Array, 0, source.relativeDistances.Count);
			ArrayPools.Single.Resize(ref dest.sourceRelativeDistances, source.sourceRelativeDistances.Count);
			Array.Copy(source.sourceRelativeDistances.Array, 0, dest.sourceRelativeDistances.Array, 0, source.sourceRelativeDistances.Count);
			dest.DuplicatePoints.Clear();
			dest.DuplicatePoints.AddRange(source.DuplicatePoints);
			dest.MaterialGroups = source.MaterialGroups.Select((SamplePointsMaterialGroup g) => g.Clone()).ToList();
			dest.Closed = source.Closed;
			dest.Seamless = source.Seamless;
			dest.Length = source.Length;
		}

		public void Copy(CGShape source)
		{
			Copy(this, source);
		}

		public float DistanceToF(float distance)
		{
			return Mathf.Clamp(distance, 0f, Length) / Length;
		}

		public float FToDistance(float f)
		{
			return Mathf.Clamp01(f) * Length;
		}

		public int GetFIndex(float f, out float frag)
		{
			if (mCacheLastF != f)
			{
				mCacheLastF = f;
				float fValue = ((f == 1f) ? f : (f % 1f));
				mCacheLastIndex = getGenericFIndex(relativeDistances, fValue, out mCacheLastFrag);
			}
			frag = mCacheLastFrag;
			return mCacheLastIndex;
		}

		public Vector3 InterpolatePosition(float f)
		{
			float frag;
			int fIndex = GetFIndex(f, out frag);
			return positions.Array[fIndex].LerpUnclamped(positions.Array[fIndex + 1], frag);
		}

		public Vector3 InterpolateUp(float f)
		{
			float frag;
			int fIndex = GetFIndex(f, out frag);
			return Vector3.SlerpUnclamped(normals.Array[fIndex], normals.Array[fIndex + 1], frag);
		}

		public void Interpolate(float f, out Vector3 position, out Vector3 up)
		{
			float frag;
			int fIndex = GetFIndex(f, out frag);
			position = positions.Array[fIndex].LerpUnclamped(positions.Array[fIndex + 1], frag);
			up = Vector3.SlerpUnclamped(normals.Array[fIndex], normals.Array[fIndex + 1], frag);
		}

		public void Move(ref float f, ref int direction, float speed, CurvyClamping clamping)
		{
			f = CurvyUtility.ClampTF(f + speed * (float)direction, ref direction, clamping);
		}

		public void MoveBy(ref float f, ref int direction, float speedDist, CurvyClamping clamping)
		{
			float distance = CurvyUtility.ClampDistance(FToDistance(f) + speedDist * (float)direction, ref direction, clamping, Length);
			f = DistanceToF(distance);
		}

		public virtual void Recalculate()
		{
			Length = 0f;
			SubArray<float> subArray = ArrayPools.Single.Allocate(Count);
			for (int i = 1; i < Count; i++)
			{
				subArray.Array[i] = subArray.Array[i - 1] + positions.Array[i].Subtraction(positions.Array[i - 1]).magnitude;
			}
			if (Count > 0)
			{
				Length = subArray.Array[Count - 1];
				if (Length > 0f)
				{
					relativeDistances.Array[0] = 0f;
					float num = 1f / Length;
					for (int j = 1; j < Count - 1; j++)
					{
						relativeDistances.Array[j] = subArray.Array[j] * num;
					}
					relativeDistances.Array[Count - 1] = 1f;
				}
				else
				{
					ArrayPools.Single.ResizeAndClear(ref relativeDistances, Count);
				}
			}
			ArrayPools.Single.Free(subArray);
		}

		[UsedImplicitly]
		[Obsolete("Use another overload of RecalculateNormals instead")]
		public void RecalculateNormals(List<int> softEdges)
		{
			if (normals.Count != positions.Count)
			{
				ArrayPools.Vector3.Resize(ref normals, positions.Count);
			}
			for (int i = 0; i < MaterialGroups.Count; i++)
			{
				for (int j = 0; j < MaterialGroups[i].Patches.Count; j++)
				{
					SamplePointsPatch samplePointsPatch = MaterialGroups[i].Patches[j];
					Vector3 normalized;
					for (int k = 0; k < samplePointsPatch.Count; k++)
					{
						int num = samplePointsPatch.Start + k;
						normalized = (positions.Array[num + 1] - positions.Array[num]).normalized;
						normals.Array[num] = new Vector3(0f - normalized.y, normalized.x, 0f);
					}
					normalized = (positions.Array[samplePointsPatch.End] - positions.Array[samplePointsPatch.End - 1]).normalized;
					normals.Array[samplePointsPatch.End] = new Vector3(0f - normalized.y, normalized.x, 0f);
				}
			}
			for (int l = 0; l < softEdges.Count; l++)
			{
				int num2 = softEdges.ToArray()[l] - 1;
				if (num2 < 0)
				{
					num2 = positions.Count - 1;
				}
				int num3 = num2 - 1;
				if (num3 < 0)
				{
					num3 = positions.Count - 1;
				}
				int num4 = softEdges.ToArray()[l] + 1;
				if (num4 == positions.Count)
				{
					num4 = 0;
				}
				normals.Array[softEdges.ToArray()[l]] = Vector3.Slerp(normals.Array[num3], normals.Array[num4], 0.5f);
				normals.Array[num2] = normals.Array[softEdges.ToArray()[l]];
			}
		}

		public void RecalculateNormals([NotNull] CurvySpline spline)
		{
			if (normals.Count != positions.Count)
			{
				ArrayPools.Vector3.Resize(ref normals, positions.Count);
			}
			Vector3[] array = normals.Array;
			float[] array2 = SourceRelativeDistances.Array;
			for (int i = 0; i < MaterialGroups.Count; i++)
			{
				for (int j = 0; j < MaterialGroups[i].Patches.Count; j++)
				{
					SamplePointsPatch samplePointsPatch = MaterialGroups[i].Patches[j];
					for (int k = 0; k < samplePointsPatch.Count; k++)
					{
						int num = samplePointsPatch.Start + k;
						array[num] = spline.GetOrientationUpFast(spline.DistanceToTF(spline.Length * array2[num]));
					}
					array[samplePointsPatch.End] = spline.GetOrientationUpFast(spline.DistanceToTF(spline.Length * array2[samplePointsPatch.End]));
				}
			}
			foreach (DuplicateSamplePoint duplicatePoint in DuplicatePoints)
			{
				if (duplicatePoint.IsHardEdge)
				{
					int startIndex = duplicatePoint.StartIndex;
					array[startIndex] = array[Math.Max(0, startIndex - 1)];
				}
			}
		}

		public void RecalculateNormals()
		{
			if (normals.Count != positions.Count)
			{
				ArrayPools.Vector3.Resize(ref normals, positions.Count);
			}
			Vector3[] array = positions.Array;
			Vector3[] array2 = normals.Array;
			for (int i = 0; i < MaterialGroups.Count; i++)
			{
				for (int j = 0; j < MaterialGroups[i].Patches.Count; j++)
				{
					SamplePointsPatch samplePointsPatch = MaterialGroups[i].Patches[j];
					Vector3 normalized;
					for (int k = 0; k < samplePointsPatch.Count; k++)
					{
						int num = samplePointsPatch.Start + k;
						normalized = (array[num + 1] - array[num]).normalized;
						array2[num] = new Vector3(0f - normalized.y, normalized.x, 0f);
					}
					normalized = (array[samplePointsPatch.End] - array[samplePointsPatch.End - 1]).normalized;
					array2[samplePointsPatch.End] = new Vector3(0f - normalized.y, normalized.x, 0f);
				}
			}
			foreach (DuplicateSamplePoint duplicatePoint in DuplicatePoints)
			{
				if (!duplicatePoint.IsHardEdge)
				{
					int num2 = duplicatePoint.EndIndex - 1;
					if (num2 < 0)
					{
						num2 = positions.Count - 1;
					}
					int num3 = num2 - 1;
					if (num3 < 0)
					{
						num3 = positions.Count - 1;
					}
					int num4 = duplicatePoint.EndIndex + 1;
					if (num4 == positions.Count)
					{
						num4 = 0;
					}
					array2[duplicatePoint.EndIndex] = Vector3.Slerp(array2[num3], array2[num4], 0.5f);
					array2[num2] = array2[duplicatePoint.EndIndex];
				}
			}
		}
	}
}
