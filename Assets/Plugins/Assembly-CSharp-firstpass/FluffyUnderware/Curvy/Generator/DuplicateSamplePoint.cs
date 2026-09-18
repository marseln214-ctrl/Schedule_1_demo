using System;

namespace FluffyUnderware.Curvy.Generator
{
	public readonly struct DuplicateSamplePoint : IEquatable<DuplicateSamplePoint>
	{
		public int StartIndex { get; }

		public int EndIndex { get; }

		public bool IsHardEdge { get; }

		public DuplicateSamplePoint(int startIndex, int endIndex, bool isHardEdge)
		{
			StartIndex = startIndex;
			EndIndex = endIndex;
			IsHardEdge = isHardEdge;
		}

		public bool Equals(DuplicateSamplePoint other)
		{
			if (StartIndex == other.StartIndex && EndIndex == other.EndIndex)
			{
				return IsHardEdge == other.IsHardEdge;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is DuplicateSamplePoint other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((StartIndex * 397) ^ EndIndex) * 397) ^ IsHardEdge.GetHashCode();
		}

		public static bool operator ==(DuplicateSamplePoint left, DuplicateSamplePoint right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DuplicateSamplePoint left, DuplicateSamplePoint right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}", "StartIndex", StartIndex, "EndIndex", EndIndex, "IsHardEdge", IsHardEdge);
		}
	}
}
