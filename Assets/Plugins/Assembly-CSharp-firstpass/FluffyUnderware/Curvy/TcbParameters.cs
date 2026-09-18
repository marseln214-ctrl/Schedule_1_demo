using System;

namespace FluffyUnderware.Curvy
{
	public struct TcbParameters : IEquatable<TcbParameters>
	{
		public float StartTension { get; set; }

		public float EndTension { get; set; }

		public float StartContinuity { get; set; }

		public float EndContinuity { get; set; }

		public float StartBias { get; set; }

		public float EndBias { get; set; }

		public bool Equals(TcbParameters other)
		{
			if (StartTension.Equals(other.StartTension) && EndTension.Equals(other.EndTension) && StartContinuity.Equals(other.StartContinuity) && EndContinuity.Equals(other.EndContinuity) && StartBias.Equals(other.StartBias))
			{
				return EndBias.Equals(other.EndBias);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is TcbParameters other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((((StartTension.GetHashCode() * 397) ^ EndTension.GetHashCode()) * 397) ^ StartContinuity.GetHashCode()) * 397) ^ EndContinuity.GetHashCode()) * 397) ^ StartBias.GetHashCode()) * 397) ^ EndBias.GetHashCode();
		}

		public static bool operator ==(TcbParameters left, TcbParameters right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TcbParameters left, TcbParameters right)
		{
			return !left.Equals(right);
		}
	}
}
