using System;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[Serializable]
	public struct FloatRegion : IEquatable<FloatRegion>
	{
		public float From;

		public float To;

		public bool SimpleValue;

		public static FloatRegion ZeroOne => new FloatRegion(0f, 1f);

		public bool Positive => From <= To;

		public float Low
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

		public float High
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

		public float Random => UnityEngine.Random.Range(From, To);

		public float Next
		{
			get
			{
				if (SimpleValue)
				{
					return From;
				}
				return Random;
			}
		}

		public float Length => To - From;

		public float LengthPositive
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

		public FloatRegion(float value)
		{
			From = value;
			To = value;
			SimpleValue = true;
		}

		public FloatRegion(float A, float B)
		{
			From = A;
			To = B;
			SimpleValue = false;
		}

		public void MakePositive()
		{
			if (To < From)
			{
				float to = To;
				float from = From;
				From = to;
				To = from;
			}
		}

		public void Clamp(float low, float high)
		{
			Low = Mathf.Clamp(Low, low, high);
			High = Mathf.Clamp(High, low, high);
		}

		public override string ToString()
		{
			return $"({From:F2}-{To:F2})";
		}

		public override int GetHashCode()
		{
			return From.GetHashCode() ^ (To.GetHashCode() << 2);
		}

		public bool Equals(FloatRegion other)
		{
			if (From.Equals(other.From))
			{
				return To.Equals(other.To);
			}
			return false;
		}

		public override bool Equals(object other)
		{
			if (!(other is FloatRegion floatRegion))
			{
				return false;
			}
			if (From.Equals(floatRegion.From))
			{
				return To.Equals(floatRegion.To);
			}
			return false;
		}

		public static FloatRegion operator +(FloatRegion a, FloatRegion b)
		{
			return new FloatRegion(a.From + b.From, a.To + b.To);
		}

		public static FloatRegion operator -(FloatRegion a, FloatRegion b)
		{
			return new FloatRegion(a.From - b.From, a.To - b.To);
		}

		public static FloatRegion operator -(FloatRegion a)
		{
			return new FloatRegion(0f - a.From, 0f - a.To);
		}

		public static FloatRegion operator *(FloatRegion a, float v)
		{
			return new FloatRegion(a.From * v, a.To * v);
		}

		public static FloatRegion operator *(float v, FloatRegion a)
		{
			return new FloatRegion(a.From * v, a.To * v);
		}

		public static FloatRegion operator /(FloatRegion a, float v)
		{
			return new FloatRegion(a.From / v, a.To / v);
		}

		public static bool operator ==(FloatRegion lhs, FloatRegion rhs)
		{
			if (lhs.SimpleValue == rhs.SimpleValue && Mathf.Approximately(lhs.From, rhs.From))
			{
				return Mathf.Approximately(lhs.To, rhs.To);
			}
			return false;
		}

		public static bool operator !=(FloatRegion lhs, FloatRegion rhs)
		{
			if (lhs.SimpleValue == rhs.SimpleValue && Mathf.Approximately(lhs.From, rhs.From))
			{
				return !Mathf.Approximately(lhs.To, rhs.To);
			}
			return true;
		}
	}
}
