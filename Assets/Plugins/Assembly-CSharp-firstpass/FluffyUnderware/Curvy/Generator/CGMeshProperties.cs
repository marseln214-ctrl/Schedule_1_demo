using System;
using System.Diagnostics;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGMeshProperties
	{
		[SerializeField]
		private Mesh m_Mesh;

		[SerializeField]
		private Material[] m_Material = new Material[0];

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Translation;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Rotation;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Scale = Vector3.one;

		public Mesh Mesh
		{
			get
			{
				return m_Mesh;
			}
			set
			{
				m_Mesh = value;
				if ((bool)m_Mesh && m_Mesh.subMeshCount != m_Material.Length)
				{
					Array.Resize(ref m_Material, m_Mesh.subMeshCount);
				}
			}
		}

		public Material[] Material
		{
			get
			{
				return m_Material;
			}
			set
			{
				m_Material = value;
			}
		}

		public Vector3 Translation
		{
			get
			{
				return m_Translation;
			}
			set
			{
				m_Translation = value;
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

		public Matrix4x4 Matrix => Matrix4x4.TRS(Translation, Quaternion.Euler(Rotation), Scale);

		public CGMeshProperties()
		{
		}

		public CGMeshProperties(Mesh mesh)
		{
			Mesh = mesh;
			Material = ((mesh != null) ? new Material[mesh.subMeshCount] : new Material[0]);
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public void OnValidate()
		{
			Mesh = m_Mesh;
		}
	}
}
