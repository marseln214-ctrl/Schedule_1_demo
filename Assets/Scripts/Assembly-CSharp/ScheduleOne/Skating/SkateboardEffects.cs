using UnityEngine;

namespace ScheduleOne.Skating
{
	[RequireComponent(typeof(Skateboard))]
	public class SkateboardEffects : MonoBehaviour
	{
		private Skateboard skateboard;

		[Header("References")]
		public TrailRenderer[] Trails;

		private float trailsOpacity;

		private void Awake()
		{
			skateboard = GetComponent<Skateboard>();
			trailsOpacity = Trails[0].startColor.a;
		}

		private void FixedUpdate()
		{
			TrailRenderer[] trails = Trails;
			foreach (TrailRenderer obj in trails)
			{
				Color startColor = obj.startColor;
				startColor.a = trailsOpacity * Mathf.Clamp01(skateboard.CurrentSpeed_Kmh / skateboard.TopSpeed_Kmh);
				obj.startColor = startColor;
			}
		}
	}
}
