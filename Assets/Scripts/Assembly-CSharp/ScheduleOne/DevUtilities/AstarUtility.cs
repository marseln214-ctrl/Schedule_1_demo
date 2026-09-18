using Pathfinding;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class AstarUtility : MonoBehaviour
	{
		public static Vector3 GetClosestPointOnGraph(Vector3 point, string GraphName)
		{
			NNConstraint nNConstraint = new NNConstraint();
			nNConstraint.graphMask = GraphMask.FromGraphName(GraphName);
			return AstarPath.active.GetNearest(point, nNConstraint).position;
		}
	}
}
