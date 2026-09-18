using EasyButtons;
using Pathfinding;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class CleanNodeLinks : MonoBehaviour
	{
		[Button]
		public void Clean()
		{
			NodeLink[] componentsInChildren = GetComponentsInChildren<NodeLink>();
			foreach (NodeLink nodeLink in componentsInChildren)
			{
				if (nodeLink.End == null)
				{
					Console.Log("Destroying link: " + nodeLink.name);
					Object.DestroyImmediate(nodeLink);
				}
			}
		}
	}
}
