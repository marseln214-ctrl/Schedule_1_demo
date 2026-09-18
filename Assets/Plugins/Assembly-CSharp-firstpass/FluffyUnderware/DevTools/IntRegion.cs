using System;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[Serializable]
	public struct IntRegion : IEquatable<IntRegion>
	{
		public int From;

		public int To;

		public bool SimpleValue;

		public static IntRegion ZeroOne => new IntRegion(0, 1);

		public bool Positive => From <= To;

		public int Low
		{
			get
			{
				if (!Positive)
				{
					return To;
				}
				return From;
			}
			set
			{
				if (Positive)
				{
					From = value;
				}
				else
				{
					To = value;
				}
			}
		}

		public int High
		{
			get
			{
				if (!Positive)
				{
					return From;
				}
				return To;
			}
			set
			{
				if (Positive)
				{
					To = value;
				}
				else
				{
					From = value;
				}
			}
		}

		public int Random => UnityEngine.Random.Range(From, To);

		public int Length => To - From;

		public int LengthPositive
		{
			get
			{
				if (!Positive)
				{
					return From - To;
				}
				return To - From;
			}
		}

		public IntRegion(int value)
		{
			From = value;
			To = value;
			SimpleValue = true;
		}

		public IntRegion(int A, int B)
		{
			From = A;
			To = B;
			SimpleValue = false;
		}

		public void MakePositive()
		{
			if (To < From)
			{
				int to = To;
				int from = From;
				From = to;
				To = from;
			}
		}

		public void Clamp(int low, int high)
		{
			Low = Mathf.Clamp(Low, low, high);
			High = Mathf.Clamp(High, low, high);
		}

		public override string ToString()
		{
			return $"({From}-{To})";
		}

		public override int GetHashCode()
		{
			return From.GetHashCode() ^ (To.GetHashCode() << 2);
		}

		public bool Equals(IntRegion other)
		{
			if (From.Equals(other.From))
			{
				return To.Equals(other.To);
			}
			return false;
		}

		public override bool Equals(object other)
		{
			if (!(other is IntRegion intRegion))
			{
				return false;
			}
			if (From.Equals(intRegion.From))
			{
				return To.Equals(intRegion.To);
			}
			return false;
		}

		public static IntRegion operator +(IntRegion a, IntRegion b)
		{
			return new IntRegion(a.From + b.From, a.To + b.To);
		}

		public static IntRegion operator -(IntRegion a, IntRegion b)
		{
			return new IntRegion(a.From - b.From, a.To - b.To);
		}

		public static IntRegion operator -(IntRegion a)
		{
			return new IntRegion(-a.From, -a.To);
		}

		public static IntRegion operator *(IntRegion a, int v)
		{
			return new IntRegion(a.From * v, a.To * v);
		}

		public static IntRegion operator *(int v, IntRegion a)
		{
			return new IntRegion(a.From * v, a.To * v);
		}

		public static IntRegion operator /(IntRegion a, int v)
		{
			return new IntRegion(a.From / v, a.To / v);
		}

		public static bool operator ==(IntRegion lhs, IntRegion rhs)
		{
			if (lhs.From == rhs.From && lhs.To == rhs.To)
			{
				return lhs.SimpleValue != rhs.SimpleValue;
			}
			return false;
		}

		public static bool operator !=(IntRegion lhs, IntRegion rhs)
		{
			if (lhs.From == rhs.From && lhs.To == rhs.To)
			{
				return lhs.SimpleValue != rhs.SimpleValue;
			}
			return true;
		}
	}
}
