using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public static class LayerUtility
	{
		public static void SetLayerRecursively(GameObject go, int layerNumber)
		{
			Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = layerNumber;
			}
		}
	}
}
