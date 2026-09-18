using System;
using System.Globalization;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public struct SamplePointsPatch : IEquatable<SamplePointsPatch>
	{
		public int Start;

		public int Count;

		public int End
		{
			get
			{
				return Start + Count;
			}
			set
			{
				Count = Mathf.Max(0, value - Start);
			}
		}

		public int TriangleCount => Count * 2;

		public SamplePointsPatch(int start)
		{
			Start = start;
			Count = 0;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Size={0} ({1}-{2}, {3} Tris)", Count, Start, End, TriangleCount);
		}

		public bool Equals(SamplePointsPatch other)
		{
			if (Start == other.Start)
			{
				return Count == other.Count;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is SamplePointsPatch)
			{
				return Equals((SamplePointsPatch)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (Start * 397) ^ Count;
		}

		public static bool operator ==(SamplePointsPatch left, SamplePointsPatch right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SamplePointsPatch left, SamplePointsPatch right)
		{
			return !left.Equals(right);
		}
	}
}
