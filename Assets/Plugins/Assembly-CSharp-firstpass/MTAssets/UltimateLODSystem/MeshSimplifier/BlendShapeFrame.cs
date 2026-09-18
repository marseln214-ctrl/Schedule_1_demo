using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MTAssets.UltimateLODSystem.MeshSimplifier
{
	[Serializable]
	[StructLayout(LayoutKind.Auto)]
	public struct BlendShapeFrame
	{
		public float FrameWeight;

		public Vector3[] DeltaVertices;

		public Vector3[] DeltaNormals;

		public Vector3[] DeltaTangents;

		public BlendShapeFrame(float frameWeight, Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents)
		{
			FrameWeight = frameWeight;
			DeltaVertices = deltaVertices;
			DeltaNormals = deltaNormals;
			DeltaTangents = deltaTangents;
		}
	}
}
