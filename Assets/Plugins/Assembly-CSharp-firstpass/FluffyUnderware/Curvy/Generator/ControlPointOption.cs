using System;

namespace FluffyUnderware.Curvy.Generator
{
	public struct ControlPointOption : IEquatable<ControlPointOption>
	{
		public float TF;

		public float Distance;

		public bool Include;

		public int MaterialID;

		public bool HardEdge;

		public float MaxStepDistance;

		public bool UVEdge;

		public bool UVShift;

		public float FirstU;

		public float SecondU;

		public ControlPointOption(float tf, float dist, bool includeAnyways, int materialID, bool hardEdge, float maxStepDistance, bool uvEdge, bool uvShift, float firstU, float secondU)
		{
			TF = tf;
			Distance = dist;
			Include = includeAnyways;
			MaterialID = materialID;
			HardEdge = hardEdge;
			if (maxStepDistance == 0f)
			{
				MaxStepDistance = float.MaxValue;
			}
			else
			{
				MaxStepDistance = maxStepDistance;
			}
			UVEdge = uvEdge;
			UVShift = uvShift;
			FirstU = firstU;
			SecondU = secondU;
		}

		public bool Equals(ControlPointOption other)
		{
			if (TF.Equals(other.TF) && Distance.Equals(other.Distance) && Include == other.Include && MaterialID == other.MaterialID && HardEdge == other.HardEdge && MaxStepDistance.Equals(other.MaxStepDistance) && UVEdge == other.UVEdge && UVShift == other.UVShift && FirstU.Equals(other.FirstU))
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
			if (obj is ControlPointOption)
			{
				return Equals((ControlPointOption)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((((((((((((TF.GetHashCode() * 397) ^ Distance.GetHashCode()) * 397) ^ Include.GetHashCode()) * 397) ^ MaterialID) * 397) ^ HardEdge.GetHashCode()) * 397) ^ MaxStepDistance.GetHashCode()) * 397) ^ UVEdge.GetHashCode()) * 397) ^ UVShift.GetHashCode()) * 397) ^ FirstU.GetHashCode()) * 397) ^ SecondU.GetHashCode();
		}

		public static bool operator ==(ControlPointOption left, ControlPointOption right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ControlPointOption left, ControlPointOption right)
		{
			return !left.Equals(right);
		}
	}
}
