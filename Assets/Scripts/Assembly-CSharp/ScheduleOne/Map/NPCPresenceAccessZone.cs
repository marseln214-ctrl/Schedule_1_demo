using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.NPCs;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class NPCPresenceAccessZone : AccessZone
	{
		public const float CooldownTime = 0.5f;

		public Collider DetectionZone;

		public NPC TargetNPC;

		private float timeSinceNPCSensed = float.MaxValue;

		protected override void Awake()
		{
			base.Awake();
		}

		protected virtual void Start()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		protected virtual void MinPass()
		{
			SetIsOpen(DetectionZone.bounds.Contains(TargetNPC.Avatar.CenterPoint));
		}
	}
}
