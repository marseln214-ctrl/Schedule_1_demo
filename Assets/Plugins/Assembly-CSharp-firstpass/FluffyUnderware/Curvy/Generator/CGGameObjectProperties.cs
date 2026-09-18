using System;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGGameObjectProperties
	{
		[SerializeField]
		[CanBeNull]
		private GameObject m_Object;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Translation;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Rotation;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Scale = Vector3.one;

		[CanBeNull]
		public GameObject Object
		{
			get
			{
				return m_Object;
			}
			set
			{
				m_Object = value;
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

		public CGGameObjectProperties()
		{
		}

		public CGGameObjectProperties(GameObject gameObject)
		{
			Object = gameObject;
		}
	}
}
