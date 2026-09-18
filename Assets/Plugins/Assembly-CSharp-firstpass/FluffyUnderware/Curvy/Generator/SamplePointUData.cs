using System;
using System.Globalization;
using JetBrains.Annotations;

namespace FluffyUnderware.Curvy.Generator
{
	public struct SamplePointUData : IEquatable<SamplePointUData>
	{
		public int Vertex;

		public bool UVEdge;

		public bool HardEdge;

		public float FirstU;

		public float SecondU;

		[UsedImplicitly]
		[Obsolete("Use other constructors")]
		public SamplePointUData(int vertexIndex, bool uvEdge, float firstU, float secondU)
			: this(vertexIndex, uvEdge, hardEdge: false, firstU, secondU)
		{
		}

		public SamplePointUData(int vertexIndex, bool uvEdge, bool hardEdge, float firstU, float secondU)
		{
			Vertex = vertexIndex;
			UVEdge = uvEdge;
			HardEdge = hardEdge;
			FirstU = firstU;
			SecondU = secondU;
		}

		public SamplePointUData(int vertexIndex, ControlPointOption controlPointsOption)
			: this(vertexIndex, controlPointsOption.UVEdge, controlPointsOption.HardEdge, controlPointsOption.FirstU, controlPointsOption.SecondU)
		{
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "SamplePointUData (Vertex={0}, UVEdge={1}, HardEdge={4}, FirstU={2}, SecondU={3}", Vertex, UVEdge, FirstU, SecondU, HardEdge);
		}

		public bool Equals(SamplePointUData other)
		{
			if (Vertex == other.Vertex && UVEdge == other.UVEdge && HardEdge == other.HardEdge && FirstU.Equals(other.FirstU))
			{
				return SecondU.Equals(other.SecondU);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is SamplePointUData)
			{
				return Equals((SamplePointUData)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((Vertex * 397) ^ UVEdge.GetHashCode()) * 397) ^ HardEdge.GetHashCode()) * 397) ^ FirstU.GetHashCode()) * 397) ^ SecondU.GetHashCode();
		}

		public static bool operator ==(SamplePointUData left, SamplePointUData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SamplePointUData left, SamplePointUData right)
		{
			return !left.Equals(right);
		}
	}
}
