using System;
using System.Collections.Generic;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Property.Utilities.Power
{
	public class PowerLine : Constructable
	{
		public static int powerLine_MinSegments = 3;

		public static int powerLine_MaxSegments = 10;

		public static float maxLineLength = 25f;

		public static float updateThreshold = 0.1f;

		public PowerNode nodeA;

		public PowerNode nodeB;

		public float LengthFactor = 1.002f;

		protected List<Transform> segments = new List<Transform>();

		private Vector3 nodeA_LastUpdatePos = Vector3.zero;

		private Vector3 nodeB_LastUpdatePos = Vector3.zero;

		private bool NetworkInitialize___EarlyScheduleOne_002EProperty_002EUtilities_002EPower_002EPowerLineAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EProperty_002EUtilities_002EPower_002EPowerLineAssembly_002DCSharp_002Edll_Excuted;

		public void InitializePowerLine(PowerNode a, PowerNode b)
		{
			nodeA = a;
			nodeB = b;
			nodeA.connections.Add(this);
			nodeB.connections.Add(this);
			nodeA.RecalculatePowerNetwork();
			for (int i = 0; i < powerLine_MaxSegments; i++)
			{
				Transform transform = UnityEngine.Object.Instantiate(Singleton<PowerManager>.Instance.powerLineSegmentPrefab, base.transform).transform;
				transform.gameObject.SetActive(value: false);
				segments.Add(transform);
			}
			RefreshVisuals();
		}

		public override void DestroyConstructable(bool callOnServer = true)
		{
			if (nodeA != null)
			{
				nodeA.connections.Remove(this);
			}
			if (nodeB != null)
			{
				nodeB.connections.Remove(this);
			}
			if (nodeA != null)
			{
				nodeA.RecalculatePowerNetwork();
			}
			if (nodeB != null)
			{
				nodeB.RecalculatePowerNetwork();
			}
			base.DestroyConstructable(callOnServer);
		}

		protected virtual void LateUpdate()
		{
			if (nodeA == null || nodeB == null)
			{
				DestroyConstructable();
				return;
			}
			if (Vector3.Distance(nodeA_LastUpdatePos, nodeA.pConnectionPoint.transform.position) > updateThreshold)
			{
				RefreshVisuals();
			}
			if (Vector3.Distance(nodeB_LastUpdatePos, nodeB.pConnectionPoint.transform.position) > updateThreshold)
			{
				RefreshVisuals();
			}
			if (Vector3.Distance(nodeA.pConnectionPoint.transform.position, nodeB.pConnectionPoint.transform.position) > maxLineLength)
			{
				DestroyConstructable();
			}
		}

		private void RefreshVisuals()
		{
			nodeA_LastUpdatePos = nodeA.pConnectionPoint.transform.position;
			nodeB_LastUpdatePos = nodeB.pConnectionPoint.transform.position;
			int segmentCount = GetSegmentCount(nodeA_LastUpdatePos, nodeB_LastUpdatePos);
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < segments.Count; i++)
			{
				if (i < segmentCount)
				{
					segments[i].gameObject.SetActive(value: true);
					list.Add(segments[i]);
				}
				else
				{
					segments[i].gameObject.SetActive(value: false);
				}
			}
			DrawPowerLine(nodeA_LastUpdatePos, nodeB_LastUpdatePos, list, LengthFactor);
			RefreshBoundingBox();
		}

		private void RefreshBoundingBox()
		{
			boundingBox.transform.position = (nodeA.pConnectionPoint.transform.position + nodeB.pConnectionPoint.transform.position) / 2f;
			boundingBox.transform.LookAt(nodeA.transform.position);
			boundingBox.size = new Vector3(0.1f, 0.5f, Vector3.Distance(nodeA.pConnectionPoint.transform.position, nodeB.pConnectionPoint.transform.position));
		}

		public PowerNode GetOtherNode(PowerNode firstNode)
		{
			if (firstNode == nodeA)
			{
				return nodeB;
			}
			if (firstNode == nodeB)
			{
				return nodeA;
			}
			return null;
		}

		public void SetVisible(bool v)
		{
			for (int i = 0; i < segments.Count; i++)
			{
				segments[i].gameObject.SetActive(v);
			}
		}

		public override Vector3 GetCosmeticCenter()
		{
			return (nodeA.transform.position + nodeB.transform.position) / 2f;
		}

		public static bool CanNodesBeConnected(PowerNode nodeA, PowerNode nodeB)
		{
			if (nodeA == nodeB)
			{
				return false;
			}
			if (nodeA == null || nodeB == null)
			{
				return false;
			}
			if (nodeA.IsConnectedTo(nodeB))
			{
				return false;
			}
			if (Vector3.Distance(nodeA.pConnectionPoint.transform.position, nodeB.pConnectionPoint.transform.position) > maxLineLength)
			{
				return false;
			}
			return true;
		}

		public static int GetSegmentCount(Vector3 startPoint, Vector3 endPoint)
		{
			float num = Vector3.Distance(startPoint, endPoint);
			int num2 = (int)((float)(powerLine_MaxSegments - powerLine_MinSegments) * Mathf.Clamp(num / 20f, 0f, 1f));
			return powerLine_MinSegments + num2;
		}

		public static void DrawPowerLine(Vector3 startPoint, Vector3 endPoint, List<Transform> segments, float lengthFactor)
		{
			PositionSegments(GetCatenaryPoints(startPoint, endPoint, segments.Count, lengthFactor), segments);
		}

		private static void PositionSegments(List<Vector3> points, List<Transform> segments)
		{
			for (int i = 0; i < segments.Count; i++)
			{
				segments[i].transform.position = (points[i] + points[i + 1]) / 2f;
				segments[i].transform.forward = points[i + 1] - points[i];
				segments[i].localScale = new Vector3(segments[i].localScale.x, segments[i].localScale.y, Vector3.Distance(points[i], points[i + 1]));
			}
		}

		private static List<Vector3> GetCatenaryPoints(Vector3 startPoint, Vector3 endPoint, int pointCount, float l)
		{
			Vector3 vector = startPoint;
			Vector3 b = endPoint;
			List<Vector3> list = new List<Vector3>();
			l *= Vector3.Distance(startPoint, endPoint);
			Vector3 vector2 = endPoint - startPoint;
			vector2.y = 0f;
			vector2 = vector2.normalized;
			_ = Vector3.up;
			endPoint.y -= startPoint.y;
			endPoint.x = Vector3.Distance(new Vector3(startPoint.x, 0f, startPoint.z), new Vector3(endPoint.x, 0f, endPoint.z));
			startPoint = Vector3.zero;
			float num = endPoint.y - startPoint.y;
			float num2 = endPoint.x - startPoint.x;
			int num3 = 0;
			float num4 = 0.01f * Mathf.Pow(Mathf.Clamp(Vector3.Distance(vector, b), 1f, float.MaxValue), 2f);
			float num5 = 1f;
			do
			{
				num5 += num4;
				num3++;
			}
			while ((double)Mathf.Sqrt(Mathf.Pow(l, 2f) - Mathf.Pow(num, 2f)) < (double)(2f * num5) * System.Math.Sinh(num2 / (2f * num5)));
			int num6 = 0;
			float num7 = 0.001f;
			float num8 = num5 - num4;
			float num9 = num5;
			do
			{
				num6++;
				num5 = (num8 + num9) / 2f;
				if ((double)Mathf.Sqrt(Mathf.Pow(l, 2f) - Mathf.Pow(num, 2f)) < (double)(2f * num5) * System.Math.Sinh(num2 / (2f * num5)))
				{
					num8 = num5;
				}
				else
				{
					num9 = num5;
				}
			}
			while (num9 - num8 > num7);
			float num10 = (startPoint.x + endPoint.x - num5 * Mathf.Log((l + num) / (l - num))) / 2f;
			float num11 = (float)(System.Math.Cosh((double)num2 / (2.0 * (double)num5)) / System.Math.Sinh((double)num2 / (2.0 * (double)num5)));
			float num12 = (startPoint.y + endPoint.y - l * num11) / 2f;
			float num13 = endPoint.x / (float)pointCount;
			List<Vector2> list2 = new List<Vector2>();
			for (int i = 0; i <= pointCount; i++)
			{
				float num14 = num13 * (float)i;
				float y = (float)((double)num5 * System.Math.Cosh((num14 - num10) / num5) + (double)num12);
				list2.Add(new Vector2(num14, y));
			}
			for (int j = 0; j < list2.Count; j++)
			{
				Vector3 item = vector + vector2 * list2[j].x;
				item.y = vector.y + list2[j].y;
				list.Add(item);
			}
			return list;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EProperty_002EUtilities_002EPower_002EPowerLineAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EProperty_002EUtilities_002EPower_002EPowerLineAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EProperty_002EUtilities_002EPower_002EPowerLineAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EProperty_002EUtilities_002EPower_002EPowerLineAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
