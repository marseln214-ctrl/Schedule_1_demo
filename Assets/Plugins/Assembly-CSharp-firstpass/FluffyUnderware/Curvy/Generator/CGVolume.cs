using System;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.Utils;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(0.08f, 0.4f, 0.75f, 1f)]
	public class CGVolume : CGPath
	{
		public bool CrossClosed;

		public bool CrossSeamless;

		public float CrossFShift;

		public SamplePointsMaterialGroupCollection CrossMaterialGroups;

		private SubArray<Vector3> vertices;

		private SubArray<Vector3> vertexNormals;

		private SubArray<float> crossRelativeDistances;

		private SubArray<float> crossCustomValues;

		private SubArray<Vector2> scales;

		[UsedImplicitly]
		[Obsolete("Do not use this. Use the GetCrossLength method instead")]
		private float[] _segmentLength;

		public SubArray<Vector3> Vertices
		{
			get
			{
				return vertices;
			}
			set
			{
				ArrayPools.Vector3.Free(vertices);
				vertices = value;
			}
		}

		public SubArray<Vector3> VertexNormals
		{
			get
			{
				return vertexNormals;
			}
			set
			{
				ArrayPools.Vector3.Free(vertexNormals);
				vertexNormals = value;
			}
		}

		public SubArray<float> CrossRelativeDistances
		{
			get
			{
				return crossRelativeDistances;
			}
			set
			{
				ArrayPools.Single.Free(crossRelativeDistances);
				crossRelativeDistances = value;
			}
		}

		public SubArray<float> CrossCustomValues
		{
			get
			{
				return crossCustomValues;
			}
			set
			{
				ArrayPools.Single.Free(crossCustomValues);
				crossCustomValues = value;
			}
		}

		public SubArray<Vector2> Scales
		{
			get
			{
				return scales;
			}
			set
			{
				ArrayPools.Vector2.Free(scales);
				scales = value;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use Vertices instead")]
		public Vector3[] Vertex
		{
			get
			{
				return Vertices.CopyToArray(ArrayPools.Vector3);
			}
			set
			{
				Vertices = new SubArray<Vector3>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use VertexNormals instead")]
		public Vector3[] VertexNormal
		{
			get
			{
				return VertexNormals.CopyToArray(ArrayPools.Vector3);
			}
			set
			{
				VertexNormals = new SubArray<Vector3>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use CrossRelativeDistances instead")]
		public float[] CrossF
		{
			get
			{
				return CrossRelativeDistances.CopyToArray(ArrayPools.Single);
			}
			set
			{
				CrossRelativeDistances = new SubArray<float>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use CrossCustomValues instead")]
		public float[] CrossMap
		{
			get
			{
				return CrossCustomValues.CopyToArray(ArrayPools.Single);
			}
			set
			{
				CrossCustomValues = new SubArray<float>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Do not use this. Use the GetCrossLength method instead")]
		public float[] SegmentLength
		{
			get
			{
				if (_segmentLength == null)
				{
					_segmentLength = new float[Count];
				}
				return _segmentLength;
			}
			set
			{
				_segmentLength = value;
			}
		}

		public int CrossSize => crossRelativeDistances.Count;

		public int VertexCount => vertices.Count;

		[UsedImplicitly]
		[Obsolete("Use one of the other constructors")]
		public CGVolume()
		{
		}

		public CGVolume(int samplePoints, CGShape crossShape)
		{
			crossRelativeDistances = ArrayPools.Single.Clone(crossShape.RelativeDistances);
			crossCustomValues = ArrayPools.Single.Clone(crossShape.CustomValues);
			scales = ArrayPools.Vector2.Allocate(samplePoints);
			CrossClosed = crossShape.Closed;
			CrossSeamless = crossShape.Seamless;
			CrossMaterialGroups = new SamplePointsMaterialGroupCollection(crossShape.MaterialGroups);
			vertices = ArrayPools.Vector3.Allocate(CrossSize * samplePoints);
			vertexNormals = ArrayPools.Vector3.Allocate(vertices.Count);
		}

		public CGVolume(CGPath path, CGShape crossShape)
			: base(path)
		{
			crossRelativeDistances = ArrayPools.Single.Clone(crossShape.RelativeDistances);
			crossCustomValues = ArrayPools.Single.Clone(crossShape.CustomValues);
			scales = ArrayPools.Vector2.Allocate(Count);
			CrossClosed = crossShape.Closed;
			CrossSeamless = crossShape.Seamless;
			CrossMaterialGroups = new SamplePointsMaterialGroupCollection(crossShape.MaterialGroups);
			vertices = ArrayPools.Vector3.Allocate(CrossSize * Count);
			vertexNormals = ArrayPools.Vector3.Allocate(vertices.Count);
		}

		public CGVolume(CGVolume source)
			: base(source)
		{
			vertices = ArrayPools.Vector3.Clone(source.vertices);
			vertexNormals = ArrayPools.Vector3.Clone(source.vertexNormals);
			crossRelativeDistances = ArrayPools.Single.Clone(source.crossRelativeDistances);
			crossCustomValues = ArrayPools.Single.Clone(source.crossCustomValues);
			scales = ArrayPools.Vector2.Clone(source.scales);
			CrossClosed = source.Closed;
			CrossSeamless = source.CrossSeamless;
			CrossFShift = source.CrossFShift;
			CrossMaterialGroups = new SamplePointsMaterialGroupCollection(source.CrossMaterialGroups);
		}

		protected override bool Dispose(bool disposing)
		{
			bool num = base.Dispose(disposing);
			if (num)
			{
				ArrayPools.Vector3.Free(vertices);
				ArrayPools.Vector3.Free(vertexNormals);
				ArrayPools.Single.Free(crossRelativeDistances);
				ArrayPools.Single.Free(crossCustomValues);
				ArrayPools.Vector2.Free(scales);
				if (SegmentLength != null)
				{
					ArrayPools.Single.Free(SegmentLength);
				}
			}
			return num;
		}

		[NotNull]
		public static CGVolume Get([CanBeNull] CGVolume data, CGPath path, CGShape crossShape)
		{
			if (data == null)
			{
				return new CGVolume(path, crossShape);
			}
			CGPath.Copy(data, path);
			if (data._segmentLength != null)
			{
				data.SegmentLength = new float[data.Count];
			}
			ArrayPools.Single.Resize(ref data.crossRelativeDistances, crossShape.RelativeDistances.Count, clearNewSpace: false);
			Array.Copy(crossShape.RelativeDistances.Array, 0, data.crossRelativeDistances.Array, 0, crossShape.RelativeDistances.Count);
			ArrayPools.Single.Resize(ref data.crossCustomValues, crossShape.CustomValues.Count, clearNewSpace: false);
			Array.Copy(crossShape.CustomValues.Array, 0, data.crossCustomValues.Array, 0, crossShape.CustomValues.Count);
			ArrayPools.Vector2.Resize(ref data.scales, path.Count, clearNewSpace: false);
			data.CrossClosed = crossShape.Closed;
			data.CrossSeamless = crossShape.Seamless;
			data.CrossMaterialGroups = new SamplePointsMaterialGroupCollection(crossShape.MaterialGroups);
			ArrayPools.Vector3.Resize(ref data.vertices, data.CrossSize * data.Positions.Count, clearNewSpace: false);
			ArrayPools.Vector3.Resize(ref data.vertexNormals, data.vertices.Count, clearNewSpace: false);
			return data;
		}

		public override T Clone<T>()
		{
			return new CGVolume(this) as T;
		}

		public void InterpolateVolume(float f, float crossF, out Vector3 pos, out Vector3 dir, out Vector3 up)
		{
			float pathFrag;
			float crossFrag;
			int vertexIndex = GetVertexIndex(f, crossF, out pathFrag, out crossFrag);
			Vector3 vector = vertices.Array[vertexIndex];
			Vector3 vector2 = vertices.Array[vertexIndex + 1];
			Vector3 vector3 = vertices.Array[vertexIndex + CrossSize];
			Vector3 vector5;
			Vector3 vector6;
			if (pathFrag + crossFrag > 1f)
			{
				Vector3 vector4 = vertices.Array[vertexIndex + CrossSize + 1];
				vector5 = vector4 - vector3;
				vector6 = vector4 - vector2;
				pos = vector3 - vector6 * (1f - pathFrag) + vector5 * crossFrag;
			}
			else
			{
				vector5 = vector2 - vector;
				vector6 = vector3 - vector;
				pos = vector + vector6 * pathFrag + vector5 * crossFrag;
			}
			dir = vector6.normalized;
			up = Vector3.Cross(vector6, vector5);
		}

		public Vector3 InterpolateVolumePosition(float f, float crossF)
		{
			float pathFrag;
			float crossFrag;
			int vertexIndex = GetVertexIndex(f, crossF, out pathFrag, out crossFrag);
			Vector3 vector = vertices.Array[vertexIndex];
			Vector3 vector2 = vertices.Array[vertexIndex + 1];
			Vector3 vector3 = vertices.Array[vertexIndex + CrossSize];
			Vector3 vector5;
			Vector3 vector6;
			if (pathFrag + crossFrag > 1f)
			{
				Vector3 vector4 = vertices.Array[vertexIndex + CrossSize + 1];
				vector5 = vector4 - vector3;
				vector6 = vector4 - vector2;
				return vector3 - vector6 * (1f - pathFrag) + vector5 * crossFrag;
			}
			vector5 = vector2 - vector;
			vector6 = vector3 - vector;
			return vector + vector6 * pathFrag + vector5 * crossFrag;
		}

		public Vector3 InterpolateVolumeDirection(float f, float crossF)
		{
			float pathFrag;
			float crossFrag;
			int vertexIndex = GetVertexIndex(f, crossF, out pathFrag, out crossFrag);
			if (pathFrag + crossFrag > 1f)
			{
				Vector3 vector = vertices.Array[vertexIndex + 1];
				return (vertices.Array[vertexIndex + CrossSize + 1] - vector).normalized;
			}
			Vector3 vector2 = vertices.Array[vertexIndex];
			return (vertices.Array[vertexIndex + CrossSize] - vector2).normalized;
		}

		public Vector3 InterpolateVolumeUp(float f, float crossF)
		{
			float pathFrag;
			float crossFrag;
			int vertexIndex = GetVertexIndex(f, crossF, out pathFrag, out crossFrag);
			Vector3 vector = vertices.Array[vertexIndex + 1];
			Vector3 vector2 = vertices.Array[vertexIndex + CrossSize];
			Vector3 rhs;
			Vector3 lhs;
			if (pathFrag + crossFrag > 1f)
			{
				Vector3 vector3 = vertices.Array[vertexIndex + CrossSize + 1];
				rhs = vector3 - vector2;
				lhs = vector3 - vector;
			}
			else
			{
				Vector3 vector4 = vertices.Array[vertexIndex];
				rhs = vector - vector4;
				lhs = vector2 - vector4;
			}
			return Vector3.Cross(lhs, rhs);
		}

		public float GetCrossLength(float pathF)
		{
			GetSegmentIndices(pathF, out var segment0Index, out var segment1Index, out var frag);
			if (SegmentLength[segment0Index] == 0f)
			{
				SegmentLength[segment0Index] = calcSegmentLength(segment0Index);
			}
			if (SegmentLength[segment1Index] == 0f)
			{
				SegmentLength[segment1Index] = calcSegmentLength(segment1Index);
			}
			return Mathf.LerpUnclamped(SegmentLength[segment0Index], SegmentLength[segment1Index], frag);
		}

		public float CrossFToDistance(float f, float crossF, CurvyClamping crossClamping = CurvyClamping.Clamp)
		{
			return GetCrossLength(f) * CurvyUtility.ClampTF(crossF, crossClamping);
		}

		public float CrossDistanceToF(float f, float distance, CurvyClamping crossClamping = CurvyClamping.Clamp)
		{
			float crossLength = GetCrossLength(f);
			return CurvyUtility.ClampDistance(distance, crossClamping, crossLength) / crossLength;
		}

		[UsedImplicitly]
		[Obsolete("Method will get removed. Copy its content if you still need it")]
		public void GetSegmentIndices(float pathF, out int segment0Index, out int segment1Index, out float frag)
		{
			segment0Index = GetFIndex(Mathf.Repeat(pathF, 1f), out frag);
			segment1Index = segment0Index + 1;
		}

		public int GetSegmentIndex(int segment)
		{
			return segment * CrossSize;
		}

		public int GetCrossFIndex(float crossF, out float frag)
		{
			float num = crossF + CrossFShift;
			num = ((num == 1f) ? num : Mathf.Repeat(num, 1f));
			return getGenericFIndex(crossRelativeDistances, num, out frag);
		}

		public int GetVertexIndex(float pathF, out float pathFrag)
		{
			return GetFIndex(pathF, out pathFrag) * CrossSize;
		}

		public int GetVertexIndex(float pathF, float crossF, out float pathFrag, out float crossFrag)
		{
			int vertexIndex = GetVertexIndex(pathF, out pathFrag);
			int crossFIndex = GetCrossFIndex(crossF, out crossFrag);
			return vertexIndex + crossFIndex;
		}

		public Vector3[] GetSegmentVertices(params int[] segmentIndices)
		{
			SubArray<Vector3> subArray = ArrayPools.Vector3.Allocate(CrossSize * segmentIndices.Length);
			for (int i = 0; i < segmentIndices.Length; i++)
			{
				int sourceIndex = segmentIndices[i] * CrossSize;
				int destinationIndex = i * CrossSize;
				Array.Copy(vertices.Array, sourceIndex, subArray.Array, destinationIndex, CrossSize);
			}
			return subArray.CopyToArray(ArrayPools.Vector3);
		}

		private float calcSegmentLength(int segmentIndex)
		{
			int num = segmentIndex * CrossSize;
			int num2 = num + CrossSize - 1;
			float num3 = 0f;
			for (int i = num; i < num2; i++)
			{
				num3 += (vertices.Array[i + 1] - vertices.Array[i]).magnitude;
			}
			return num3;
		}
	}
}
