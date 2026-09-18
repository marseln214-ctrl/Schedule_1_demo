using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public abstract class TRSModuleBase : CGModule
	{
		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Transpose;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Rotation;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Scale = Vector3.one;

		public Vector3 Transpose
		{
			get
			{
				return m_Transpose;
			}
			set
			{
				if (m_Transpose != value)
				{
					m_Transpose = value;
					base.Dirty = true;
				}
			}
		}

		public Vector3 Rotation
		{
			get
			{
				return m_Rotation;
			}
			set
			{
				if (m_Rotation != value)
				{
					m_Rotation = value;
					base.Dirty = true;
				}
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
				if (m_Scale != value)
				{
					m_Scale = value;
					base.Dirty = true;
				}
			}
		}

		public Matrix4x4 Matrix => Matrix4x4.TRS(Transpose, Quaternion.Euler(Rotation), Scale);

		protected Matrix4x4 ApplyTrsOnShape([NotNull] CGShape shape)
		{
			Matrix4x4 matrix = Matrix;
			Matrix4x4 result = Matrix4x4.TRS(Transpose, Quaternion.Euler(Rotation), Vector3.one);
			for (int i = 0; i < shape.Count; i++)
			{
				shape.Positions.Array[i] = matrix.MultiplyPoint3x4(shape.Positions.Array[i]);
				shape.Normals.Array[i] = result.MultiplyVector(shape.Normals.Array[i]);
			}
			if (Scale != Vector3.one)
			{
				shape.Recalculate();
			}
			return result;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 250f;
			Properties.LabelWidth = 50f;
		}

		public override void Reset()
		{
			base.Reset();
			Transpose = Vector3.zero;
			Rotation = Vector3.zero;
			Scale = Vector3.one;
		}
	}
}
