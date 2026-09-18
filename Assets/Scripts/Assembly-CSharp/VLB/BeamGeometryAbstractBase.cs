using UnityEngine;

namespace VLB
{
	public abstract class BeamGeometryAbstractBase : MonoBehaviour
	{
		protected Matrix4x4 m_ColorGradientMatrix;

		protected Material m_CustomMaterial;

		public MeshRenderer meshRenderer { get; protected set; }

		public MeshFilter meshFilter { get; protected set; }

		public Mesh coneMesh { get; protected set; }

		protected abstract VolumetricLightBeamAbstractBase GetMaster();

		private void Start()
		{
			DestroyInvalidOwner();
		}

		private void OnDestroy()
		{
			if ((bool)m_CustomMaterial)
			{
				Object.DestroyImmediate(m_CustomMaterial);
				m_CustomMaterial = null;
			}
		}

		private void DestroyInvalidOwner()
		{
			if (!GetMaster())
			{
				DestroyBeamGeometryGameObject(this);
			}
		}

		public static void DestroyBeamGeometryGameObject(BeamGeometryAbstractBase beamGeom)
		{
			if ((bool)beamGeom)
			{
				Object.DestroyImmediate(beamGeom.gameObject);
			}
		}
	}
}
