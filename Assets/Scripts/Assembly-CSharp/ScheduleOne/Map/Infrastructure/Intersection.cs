using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Map.Infrastructure
{
	public class Intersection : MonoBehaviour
	{
		[Header("References")]
		[SerializeField]
		protected List<TrafficLight> path1Lights = new List<TrafficLight>();

		[SerializeField]
		protected List<TrafficLight> path2Lights = new List<TrafficLight>();

		[SerializeField]
		protected List<GameObject> path1Obstacles = new List<GameObject>();

		[SerializeField]
		protected List<GameObject> path2Obstacles = new List<GameObject>();

		[Header("Settings")]
		[SerializeField]
		protected float path1Time = 10f;

		[SerializeField]
		protected float path2Time = 10f;

		[SerializeField]
		protected float timeOffset;

		protected virtual void Start()
		{
			Singleton<CoroutineService>.Instance.StartCoroutine(Run());
		}

		protected IEnumerator Run()
		{
			while (true)
			{
				SetPath1Lights(TrafficLight.State.Green);
				SetPath2Lights(TrafficLight.State.Red);
				if (timeOffset != 0f)
				{
					yield return new WaitForSecondsRealtime(Mathf.Abs(timeOffset));
					timeOffset = 0f;
				}
				yield return new WaitForSecondsRealtime(path1Time);
				SetPath1Lights(TrafficLight.State.Orange);
				yield return new WaitForSecondsRealtime(TrafficLight.amberTime);
				SetPath1Lights(TrafficLight.State.Red);
				yield return new WaitForSecondsRealtime(1f);
				SetPath2Lights(TrafficLight.State.Green);
				yield return new WaitForSecondsRealtime(path2Time);
				SetPath2Lights(TrafficLight.State.Orange);
				yield return new WaitForSecondsRealtime(TrafficLight.amberTime);
				SetPath2Lights(TrafficLight.State.Red);
				yield return new WaitForSecondsRealtime(1f);
			}
		}

		protected void SetPath1Lights(TrafficLight.State state)
		{
			foreach (TrafficLight path1Light in path1Lights)
			{
				path1Light.state = state;
			}
			if (state == TrafficLight.State.Green)
			{
				foreach (GameObject path1Obstacle in path1Obstacles)
				{
					path1Obstacle.gameObject.SetActive(value: false);
				}
				return;
			}
			foreach (GameObject path1Obstacle2 in path1Obstacles)
			{
				path1Obstacle2.gameObject.SetActive(value: true);
			}
		}

		protected void SetPath2Lights(TrafficLight.State state)
		{
			foreach (TrafficLight path2Light in path2Lights)
			{
				path2Light.state = state;
			}
			if (state == TrafficLight.State.Green)
			{
				foreach (GameObject path2Obstacle in path2Obstacles)
				{
					path2Obstacle.gameObject.SetActive(value: false);
				}
				return;
			}
			foreach (GameObject path2Obstacle2 in path2Obstacles)
			{
				path2Obstacle2.gameObject.SetActive(value: true);
			}
		}
	}
}
