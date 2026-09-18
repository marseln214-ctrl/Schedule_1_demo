using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTAssets.UltimateLODSystem
{
	[AddComponentMenu("MT Assets/Ultimate LOD System/Ultimate Level Of Detail Optimizer")]
	public class UltimateLevelOfDetailOptimizer : MonoBehaviour
	{
		private WaitForSecondsRealtime DELAY_BETWEEN_OPTIMIZATION_UPDATES = new WaitForSecondsRealtime(0.2f);

		private WaitForSecondsRealtime DELAY_BETWEEN_GAMEOBJECTS_STATE_CHANGE = new WaitForSecondsRealtime(0.05f);

		private float ADITIONAL_CULLING_DISTANCE_OFFSET = 10f;

		private RuntimeInstancesDetector runtimeInstancesDetector;

		private int[] instructionsToMakeOnUlods = new int[0];

		[HideInInspector]
		public bool enableOptimizationTasks = true;

		[HideInInspector]
		public List<UltimateLevelOfDetail> ulodsToBeIgnored = new List<UltimateLevelOfDetail>();

		public void Awake()
		{
			StartCoroutine(UlodOptimizationLoop());
		}

		private bool isThisUlodPresentOnUlodsToBeIgnored(UltimateLevelOfDetail ulod)
		{
			bool result = false;
			foreach (UltimateLevelOfDetail item in ulodsToBeIgnored)
			{
				if (item == ulod)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		private IEnumerator UlodOptimizationLoop()
		{
			yield return new WaitForSecondsRealtime(0.1f);
			GameObject gameObject = GameObject.Find("Ultimate LOD Data");
			runtimeInstancesDetector = gameObject.GetComponent<RuntimeInstancesDetector>();
			runtimeInstancesDetector.RegisterNewUlodOptimizerInThisScene(this);
			while (true)
			{
				yield return DELAY_BETWEEN_OPTIMIZATION_UPDATES;
				if (enableOptimizationTasks)
				{
					if (instructionsToMakeOnUlods.Length != runtimeInstancesDetector.instancesOfUlodInThisScene.Count)
					{
						instructionsToMakeOnUlods = new int[runtimeInstancesDetector.instancesOfUlodInThisScene.Count];
					}
					for (int j = 0; j < instructionsToMakeOnUlods.Length; j++)
					{
						bool num = isThisUlodPresentOnUlodsToBeIgnored(runtimeInstancesDetector.instancesOfUlodInThisScene[j]);
						if (num)
						{
							instructionsToMakeOnUlods[j] = 2;
						}
						if (!num)
						{
							instructionsToMakeOnUlods[j] = -1;
						}
					}
					for (int k = 0; k < runtimeInstancesDetector.instancesOfUlodInThisScene.Count; k++)
					{
						if (runtimeInstancesDetector.instancesOfUlodInThisScene[k].cullingMode != 0 && runtimeInstancesDetector.instancesOfUlodInThisScene[k].cullingMode != 0 && runtimeInstancesDetector.instancesOfUlodInThisScene[k].isMeshesCurrentScannedAndLodsWorkingInThisComponent() && instructionsToMakeOnUlods[k] != 2)
						{
							if (Vector3.Distance(base.gameObject.transform.position, runtimeInstancesDetector.instancesOfUlodInThisScene[k].transform.position) > runtimeInstancesDetector.instancesOfUlodInThisScene[k].minDistanceOfViewForCull + ADITIONAL_CULLING_DISTANCE_OFFSET && runtimeInstancesDetector.instancesOfUlodInThisScene[k].gameObject.activeSelf)
							{
								instructionsToMakeOnUlods[k] = 0;
							}
							if (Vector3.Distance(base.gameObject.transform.position, runtimeInstancesDetector.instancesOfUlodInThisScene[k].transform.position) <= runtimeInstancesDetector.instancesOfUlodInThisScene[k].minDistanceOfViewForCull + ADITIONAL_CULLING_DISTANCE_OFFSET && !runtimeInstancesDetector.instancesOfUlodInThisScene[k].gameObject.activeSelf)
							{
								instructionsToMakeOnUlods[k] = 1;
							}
						}
					}
					for (int i = 0; i < runtimeInstancesDetector.instancesOfUlodInThisScene.Count; i++)
					{
						if (instructionsToMakeOnUlods[i] != -1)
						{
							if (instructionsToMakeOnUlods[i] == 0)
							{
								runtimeInstancesDetector.instancesOfUlodInThisScene[i].gameObject.SetActive(value: false);
								instructionsToMakeOnUlods[i] = -1;
							}
							if (instructionsToMakeOnUlods[i] == 1)
							{
								runtimeInstancesDetector.instancesOfUlodInThisScene[i].gameObject.SetActive(value: true);
								instructionsToMakeOnUlods[i] = -1;
							}
							yield return DELAY_BETWEEN_GAMEOBJECTS_STATE_CHANGE;
						}
					}
				}
				if (enableOptimizationTasks)
				{
					continue;
				}
				foreach (UltimateLevelOfDetail item in runtimeInstancesDetector.instancesOfUlodInThisScene)
				{
					if (!isThisUlodPresentOnUlodsToBeIgnored(item))
					{
						item.gameObject.SetActive(value: true);
					}
					yield return DELAY_BETWEEN_GAMEOBJECTS_STATE_CHANGE;
				}
			}
		}
	}
}
