using System.Collections.Generic;
using UnityEngine;

namespace MTAssets.UltimateLODSystem
{
	[AddComponentMenu("")]
	public class RuntimeInstancesDetector : MonoBehaviour
	{
		[HideInInspector]
		public List<UltimateLevelOfDetail> instancesOfUlodInThisScene = new List<UltimateLevelOfDetail>();

		[HideInInspector]
		public List<UltimateLevelOfDetailOptimizer> instancesOfUlodOptimizerInThisScene = new List<UltimateLevelOfDetailOptimizer>();

		public void RegisterNewUlodOptimizerInThisScene(UltimateLevelOfDetailOptimizer optimizer)
		{
			instancesOfUlodOptimizerInThisScene.Add(optimizer);
			if (instancesOfUlodOptimizerInThisScene.Count > 1)
			{
				Debug.LogWarning("It has been identified that there is more than one \"Ultimate Level Of Detail Optimizer\" component in this scene. It is highly recommended that there is only one active component in the scene to avoid optimization problems and conflicts.");
			}
		}
	}
}
