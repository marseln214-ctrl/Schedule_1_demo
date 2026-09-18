using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public struct CGSpot : IEquatable<CGSpot>
	{
		[SerializeField]
		[Label("Index", "")]
		private int m_Index;

		[SerializeField]
		[VectorEx("Position", "", Options = AttributeOptionsFlags.Compact, Precision = 4)]
		private Vector3 m_Position;

		[SerializeField]
		[VectorEx("Rotation", "", Options = AttributeOptionsFlags.Compact, Precision = 4)]
		private Quaternion m_Rotation;

		[SerializeField]
		[VectorEx("Scale", "", Options = AttributeOptionsFlags.Compact, Precision = 4)]
		private Vector3 m_Scale;

		public int Index => m_Index;

		public Vector3 Position
		{
			get
			{
				return m_Position;
			}
			set
			{
				m_Position = value;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return m_Rotation;
			}
			set
			{
				m_Rotation = value;
			}
		}

		public Vector3 Scale
		{
			get
			{
				return m_Scale;
			}
			set
			{
				m_Scale = value;
			}
		}

		public Matrix4x4 Matrix => Matrix4x4.TRS(m_Position, m_Rotation, m_Scale);

		public CGSpot(int index)
			: this(index, Vector3.zero, Quaternion.identity, Vector3.one)
		{
		}

		public CGSpot(int index, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			m_Index = index;
			m_Position = position;
			m_Rotation = rotation;
			m_Scale = scale;
		}

		public void ToTransform(Transform transform)
		{
			transform.localPosition = Position;
			transform.localRotation = Rotation;
			transform.localScale = Scale;
		}

		public bool Equals(CGSpot other)
		{
			if (m_Index == other.m_Index && m_Position.Equals(other.m_Position) && m_Rotation.Equals(other.m_Rotation))
			{
				return m_Scale.Equals(other.m_Scale);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is CGSpot)
			{
				return Equals((CGSpot)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((m_Index * 397) ^ m_Position.GetHashCode()) * 397) ^ m_Rotation.GetHashCode()) * 397) ^ m_Scale.GetHashCode();
		}

		public static bool operator ==(CGSpot left, CGSpot right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(CGSpot left, CGSpot right)
		{
			return !left.Equals(right);
		}
	}
}
