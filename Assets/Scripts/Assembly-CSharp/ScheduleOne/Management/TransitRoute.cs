using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.UI.Management;
using UnityEngine;

namespace ScheduleOne.Management
{
	public class TransitRoute
	{
		protected TransitLineVisuals visuals;

		public ITransitEntity Source { get; set; }

		public ITransitEntity Destination { get; set; }

		public TransitRoute(ITransitEntity source, ITransitEntity destination)
		{
			Source = source;
			Destination = destination;
			Debug.DrawLine(source.LinkOriginPoint, destination.LinkOriginPoint, Color.green, 10f);
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onUpdate = (Action)Delegate.Combine(instance.onUpdate, new Action(Update));
		}

		public void Destroy()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onUpdate = (Action)Delegate.Remove(instance.onUpdate, new Action(Update));
			if (visuals != null)
			{
				UnityEngine.Object.Destroy(visuals.gameObject);
			}
		}

		public void SetVisualsActive(bool active)
		{
			if (visuals == null)
			{
				visuals = UnityEngine.Object.Instantiate(Singleton<ManagementWorldspaceCanvas>.Instance.TransitRouteVisualsPrefab.gameObject, GameObject.Find("_Temp").transform).GetComponent<TransitLineVisuals>();
			}
			visuals.gameObject.SetActive(active);
			if (active)
			{
				Update();
			}
		}

		private void Update()
		{
			if (!(visuals == null) && visuals.gameObject.activeSelf)
			{
				Vector3.Distance(Source.LinkOriginPoint, Destination.LinkOriginPoint);
				visuals.SetSourcePosition(Source.LinkOriginPoint);
				visuals.SetDestinationPosition(Destination.LinkOriginPoint);
			}
		}
	}
}
