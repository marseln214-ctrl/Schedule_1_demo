using MTAssets.UltimateLODSystem.MeshSimplifier;
using UnityEngine;

namespace MTAssets.UltimateLODSystem
{
	[AddComponentMenu("")]
	public class UltimateLevelOfDetailGlobal : MonoBehaviour
	{
		private static bool enableGlobalUlodSystem = true;

		private static float lodDistanceMultiplier = 1f;

		public static Camera currentCameraThatIsOnTopOfScreenInThisScene = null;

		public static bool isGlobalULodSystemEnabled()
		{
			return enableGlobalUlodSystem;
		}

		public static void EnableGlobalULodSystem(bool enable)
		{
			enableGlobalUlodSystem = enable;
		}

		public static void SetGlobalLodDistanceMultiplier(float multiplier)
		{
			lodDistanceMultiplier = multiplier;
		}

		public static float GetGlobalLodDistanceMultiplier()
		{
			return lodDistanceMultiplier;
		}

		public static Mesh GetSimplifiedVersionOfThisMesh(Mesh meshToSimplify, bool isSkinnedMesh, bool skinnedAnimsCompatibilityMode, bool simplificationDestroyerMode, bool preventArtifacts, float percentOfVerticesOfSimplifyiedVersion)
		{
			float num = 1E-05f;
			MTAssets.UltimateLODSystem.MeshSimplifier.MeshSimplifier meshSimplifier = new MTAssets.UltimateLODSystem.MeshSimplifier.MeshSimplifier();
			SimplificationOptions simplificationOptions = default(SimplificationOptions);
			if (!simplificationDestroyerMode)
			{
				simplificationOptions.Agressiveness = 7.0;
				num = 1f;
			}
			if (simplificationDestroyerMode)
			{
				simplificationOptions.Agressiveness = 12.0;
				num = 0.4f;
			}
			if (preventArtifacts)
			{
				simplificationOptions.EnableSmartLink = true;
			}
			if (!preventArtifacts)
			{
				simplificationOptions.EnableSmartLink = false;
			}
			simplificationOptions.MaxIterationCount = 100;
			simplificationOptions.PreserveBorderEdges = false;
			simplificationOptions.PreserveSurfaceCurvature = false;
			simplificationOptions.PreserveUVFoldoverEdges = false;
			simplificationOptions.PreserveUVSeamEdges = false;
			simplificationOptions.VertexLinkDistance = double.Epsilon;
			meshSimplifier.SimplificationOptions = simplificationOptions;
			meshSimplifier.Initialize(meshToSimplify);
			meshSimplifier.SimplifyMesh(percentOfVerticesOfSimplifyiedVersion / 100f * num);
			Mesh mesh = meshSimplifier.ToMesh();
			if (isSkinnedMesh && skinnedAnimsCompatibilityMode)
			{
				mesh.bindposes = meshToSimplify.bindposes;
			}
			return mesh;
		}
	}
}
