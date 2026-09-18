using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Market
{
	public class VendorZone : MonoBehaviour
	{
		[Header("References")]
		[SerializeField]
		protected BoxCollider zoneCollider;

		[SerializeField]
		protected List<GameObject> doors = new List<GameObject>();

		[Header("Settings")]
		[SerializeField]
		protected int openTime = 600;

		[SerializeField]
		protected int closeTime = 1800;

		public bool isOpen => NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(openTime, closeTime);

		protected virtual void Start()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPassed));
		}

		private void MinPassed()
		{
			if (isOpen)
			{
				SetDoorsActive(a: false);
			}
			else if (!IsPlayerWithinVendorZone())
			{
				SetDoorsActive(a: true);
			}
		}

		private bool IsPlayerWithinVendorZone()
		{
			return zoneCollider.bounds.Contains(PlayerSingleton<PlayerMovement>.Instance.transform.position);
		}

		private void SetDoorsActive(bool a)
		{
			for (int i = 0; i < doors.Count; i++)
			{
				doors[i].SetActive(a);
			}
		}
	}
}
