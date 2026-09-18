using System.Collections.Generic;
using EasyButtons;
using ScheduleOne.Property.Utilities.Power;
using UnityEngine;

namespace ScheduleOne.Misc
{
	public class UtilityPole : MonoBehaviour
	{
		public UtilityPole previousPole;

		public UtilityPole nextPole;

		public bool Connection1Enabled = true;

		public bool Connection2Enabled = true;

		public float LengthFactor = 1.002f;

		[Header("References")]
		public Transform cable1Connection;

		public Transform cable2Connection;

		public List<Transform> cable1Segments = new List<Transform>();

		public List<Transform> cable2Segments = new List<Transform>();

		[Button]
		public void Orient()
		{
			if (previousPole == null && nextPole == null)
			{
				Console.LogWarning("No neighbour poles!");
			}
			else if (nextPole != null && previousPole != null)
			{
				Vector3 normalized = (base.transform.position - previousPole.transform.position).normalized;
				Vector3 normalized2 = (nextPole.transform.position - base.transform.position).normalized;
				Vector3 normalized3 = (normalized + normalized2).normalized;
				base.transform.rotation = Quaternion.LookRotation(normalized3, Vector3.up);
			}
			else if (previousPole != null)
			{
				Vector3 normalized4 = (base.transform.position - previousPole.transform.position).normalized;
				base.transform.rotation = Quaternion.LookRotation(normalized4, Vector3.up);
			}
			else if (nextPole != null)
			{
				Vector3 normalized5 = (nextPole.transform.position - base.transform.position).normalized;
				base.transform.rotation = Quaternion.LookRotation(normalized5, Vector3.up);
			}
		}

		[Button]
		public void DrawLines()
		{
			if (previousPole == null)
			{
				if (Connection1Enabled)
				{
					foreach (Transform cable1Segment in cable1Segments)
					{
						cable1Segment.gameObject.SetActive(value: false);
					}
				}
				if (!Connection2Enabled)
				{
					return;
				}
				{
					foreach (Transform cable2Segment in cable2Segments)
					{
						cable2Segment.gameObject.SetActive(value: false);
					}
					return;
				}
			}
			if (Connection1Enabled)
			{
				PowerLine.DrawPowerLine(cable1Connection.position, previousPole.cable1Connection.position, cable1Segments, LengthFactor);
				foreach (Transform cable1Segment2 in cable1Segments)
				{
					cable1Segment2.gameObject.SetActive(value: true);
				}
			}
			if (!Connection2Enabled)
			{
				return;
			}
			PowerLine.DrawPowerLine(cable2Connection.position, previousPole.cable2Connection.position, cable2Segments, LengthFactor);
			foreach (Transform cable2Segment2 in cable2Segments)
			{
				cable2Segment2.gameObject.SetActive(value: true);
			}
		}
	}
}
