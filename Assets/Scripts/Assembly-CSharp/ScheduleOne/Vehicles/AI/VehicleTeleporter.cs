using Pathfinding;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI
{
	[RequireComponent(typeof(LandVehicle))]
	public class VehicleTeleporter : MonoBehaviour
	{
		public void MoveToGraph(bool resetRotation = true)
		{
			NNConstraint nNConstraint = new NNConstraint();
			nNConstraint.graphMask = GraphMask.FromGraphName("General Vehicle Graph");
			NNInfo nearest = AstarPath.active.GetNearest(base.transform.position, nNConstraint);
			base.transform.position = nearest.position + base.transform.up * GetComponent<LandVehicle>().boundingBoxDimensions.y / 2f;
			if (resetRotation)
			{
				base.transform.rotation = Quaternion.identity;
			}
		}

		public void MoveToRoadNetwork(bool resetRotation = true)
		{
			NNConstraint nNConstraint = new NNConstraint();
			nNConstraint.graphMask = GraphMask.FromGraphName("Road Nodes");
			NNInfo nearest = AstarPath.active.GetNearest(base.transform.position, nNConstraint);
			base.transform.position = nearest.position + base.transform.up * GetComponent<LandVehicle>().boundingBoxDimensions.y / 2f;
			if (resetRotation)
			{
				base.transform.rotation = Quaternion.identity;
			}
		}
	}
}
