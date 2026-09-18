using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Property.Utilities.Power
{
	public class PowerNode : MonoBehaviour
	{
		public bool poweredNode;

		public bool consumptionNode;

		public bool isConnectedToPower;

		[Header("References")]
		[SerializeField]
		protected Transform connectionPoint;

		public List<PowerLine> connections = new List<PowerLine>();

		public Transform pConnectionPoint => connectionPoint;

		public bool IsConnectedTo(PowerNode node)
		{
			for (int i = 0; i < connections.Count; i++)
			{
				if (connections[i].nodeA == this)
				{
					if (connections[i].nodeB == node)
					{
						return true;
					}
				}
				else if (connections[i].nodeA == node)
				{
					return true;
				}
			}
			return false;
		}

		public void RecalculatePowerNetwork()
		{
			List<PowerNode> connectedNodes = GetConnectedNodes(new List<PowerNode>());
			bool flag = false;
			foreach (PowerNode item in connectedNodes)
			{
				if (item.poweredNode)
				{
					flag = true;
				}
			}
			foreach (PowerNode item2 in connectedNodes)
			{
				if (flag)
				{
					item2.isConnectedToPower = true;
				}
				else
				{
					item2.isConnectedToPower = false;
				}
			}
		}

		public List<PowerNode> GetConnectedNodes(List<PowerNode> exclusions)
		{
			List<PowerNode> list = new List<PowerNode>();
			list.Add(this);
			exclusions.Add(this);
			for (int i = 0; i < connections.Count; i++)
			{
				if (!exclusions.Contains(connections[i].GetOtherNode(this)))
				{
					List<PowerNode> connectedNodes = connections[i].GetOtherNode(this).GetConnectedNodes(exclusions);
					exclusions.AddRange(connectedNodes);
					list.AddRange(connectedNodes);
				}
			}
			return list;
		}
	}
}
