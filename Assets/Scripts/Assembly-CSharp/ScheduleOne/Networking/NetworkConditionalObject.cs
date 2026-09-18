using System;
using FishNet;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Networking
{
	public class NetworkConditionalObject : MonoBehaviour
	{
		public enum ECondition
		{
			All = 0,
			HostOnly = 1
		}

		public ECondition condition;

		private void Awake()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(Check));
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Check));
		}

		public void Check()
		{
			ECondition eCondition = condition;
			if (eCondition != 0 && eCondition == ECondition.HostOnly && !InstanceFinder.IsHost)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
