using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using UnityEngine;

namespace ScheduleOne.Quests
{
	public class StateMachine : MonoBehaviour
	{
		public static Action OnStateChange;

		private static bool stateChanged;

		private void Start()
		{
			Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(Clean);
		}

		private void Update()
		{
			if (stateChanged)
			{
				OnStateChange?.Invoke();
				stateChanged = false;
			}
		}

		private void Clean()
		{
			Debug.Log("Clearing state change...");
			OnStateChange = null;
		}

		public static void ChangeState()
		{
			stateChanged = true;
		}
	}
}
