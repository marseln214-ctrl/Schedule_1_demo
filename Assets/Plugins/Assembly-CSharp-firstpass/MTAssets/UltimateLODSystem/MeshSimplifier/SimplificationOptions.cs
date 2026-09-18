using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MTAssets.UltimateLODSystem.MeshSimplifier
{
	[Serializable]
	[StructLayout(LayoutKind.Auto)]
	public struct SimplificationOptions
	{
		public static readonly SimplificationOptions Default = new SimplificationOptions
		{
			PreserveBorderEdges = false,
			PreserveUVSeamEdges = false,
			PreserveUVFoldoverEdges = false,
			PreserveSurfaceCurvature = false,
			EnableSmartLink = true,
			VertexLinkDistance = double.Epsilon,
			MaxIterationCount = 100,
			Agressiveness = 7.0,
			ManualUVComponentCount = false,
			UVComponentCount = 2
		};

		[Tooltip("If the border edges should be preserved.")]
		public bool PreserveBorderEdges;

		[Tooltip("If the UV seam edges should be preserved.")]
		public bool PreserveUVSeamEdges;

		[Tooltip("If the UV foldover edges should be preserved.")]
		public bool PreserveUVFoldoverEdges;

		[Tooltip("If the discrete curvature of the mesh surface be taken into account during simplification. Taking surface curvature into account can result in very good quality mesh simplification, but it can slow the simplification process significantly.")]
		public bool PreserveSurfaceCurvature;

		[Tooltip("If a feature for smarter vertex linking should be enabled, reducing artifacts at the cost of slower simplification.")]
		public bool EnableSmartLink;

		[Tooltip("The maximum distance between two vertices in order to link them.")]
		public double VertexLinkDistance;

		[Tooltip("The maximum iteration count. Higher number is more expensive but can bring you closer to your target quality.")]
		public int MaxIterationCount;

		[Tooltip("The agressiveness of the mesh simplification. Higher number equals higher quality, but more expensive to run.")]
		public double Agressiveness;

		[Tooltip("If a manual UV component count should be used (set by UV Component Count below), instead of the automatic detection.")]
		public bool ManualUVComponentCount;

		[Range(0f, 4f)]
		[Tooltip("The UV component count. The same UV component count will be used on all UV channels.")]
		public int UVComponentCount;
	}
}
