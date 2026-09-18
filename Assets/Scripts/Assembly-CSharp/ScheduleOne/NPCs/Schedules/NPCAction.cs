using System;
using FishNet;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules
{
	[Serializable]
	public abstract class NPCAction : NetworkBehaviour
	{
		public const int MAX_CONSECUTIVE_PATHING_FAILURES = 5;

		[SerializeField]
		protected int priority;

		[Header("Timing Settings")]
		public int StartTime;

		protected NPC npc;

		protected NPCScheduleManager schedule;

		public Action onEnded;

		protected int consecutivePathingFailures;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted;

		protected string ActionName => "ActionName";

		public bool IsEvent => this is NPCEvent;

		public bool IsSignal => this is NPCSignal;

		public bool IsActive { get; protected set; }

		public bool HasStarted { get; protected set; }

		public virtual int Priority => priority;

		protected NPCMovement movement => npc.Movement;

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCAction_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			GetReferences();
		}

		private void GetReferences()
		{
			if (npc == null)
			{
				npc = GetComponentInParent<NPC>();
			}
			if (schedule == null)
			{
				schedule = GetComponentInParent<NPCScheduleManager>();
			}
		}

		protected virtual void Start()
		{
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPassed));
		}

		public virtual void Started()
		{
			GetReferences();
			if (schedule.DEBUG_MODE)
			{
				Debug.Log(GetName() + " started");
			}
			IsActive = true;
			schedule.ActiveAction = this;
			HasStarted = true;
		}

		public virtual void LateStarted()
		{
			GetReferences();
			if (schedule.DEBUG_MODE)
			{
				Debug.Log(GetName() + " late started");
			}
			IsActive = true;
			schedule.ActiveAction = this;
			HasStarted = true;
		}

		public virtual void JumpTo()
		{
			GetReferences();
			if (schedule.DEBUG_MODE)
			{
				Debug.Log(GetName() + " jumped to");
			}
			IsActive = true;
			schedule.ActiveAction = this;
			HasStarted = true;
		}

		public virtual void End()
		{
			GetReferences();
			if (schedule.DEBUG_MODE)
			{
				Debug.Log(GetName() + " ended");
			}
			IsActive = false;
			schedule.ActiveAction = null;
			HasStarted = false;
			if (onEnded != null)
			{
				onEnded();
			}
		}

		public virtual void Interrupt()
		{
			GetReferences();
			if (schedule.DEBUG_MODE)
			{
				Debug.Log(GetName() + " interrupted");
			}
			IsActive = false;
			schedule.ActiveAction = null;
			if (!schedule.PendingActions.Contains(this))
			{
				schedule.PendingActions.Add(this);
			}
		}

		public virtual void Resume()
		{
			GetReferences();
			if (schedule.DEBUG_MODE)
			{
				Debug.Log(GetName() + " resumed");
			}
			IsActive = true;
			schedule.ActiveAction = this;
			if (schedule.PendingActions.Contains(this))
			{
				schedule.PendingActions.Remove(this);
			}
		}

		public virtual void ResumeFailed()
		{
			GetReferences();
			if (schedule.DEBUG_MODE)
			{
				Debug.Log(GetName() + " resume failed");
			}
			HasStarted = false;
			if (schedule.PendingActions.Contains(this))
			{
				schedule.PendingActions.Remove(this);
			}
		}

		public virtual void Skipped()
		{
			GetReferences();
			if (schedule.DEBUG_MODE)
			{
				Debug.Log(base.gameObject.name + " skipped");
			}
			IsActive = false;
			HasStarted = false;
		}

		public virtual void ActiveUpdate()
		{
		}

		public virtual void ActiveMinPassed()
		{
		}

		public virtual void PendingMinPassed()
		{
			if (HasStarted && !IsActive && !NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, GetEndTime()))
			{
				ResumeFailed();
			}
		}

		public virtual void MinPassed()
		{
		}

		public virtual bool ShouldStart()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return false;
			}
			return true;
		}

		public abstract string GetName();

		public abstract string GetTimeDescription();

		public abstract int GetEndTime();

		protected void SetDestination(Vector3 position, bool teleportIfFail = true)
		{
			if (InstanceFinder.IsServer)
			{
				if (teleportIfFail && consecutivePathingFailures >= 5 && !movement.CanGetTo(position))
				{
					Console.LogWarning(npc.fullName + " too many pathing failures. Warping to " + position.ToString());
					movement.Warp(position);
					WalkCallback(NPCMovement.WalkResult.Success);
				}
				movement.SetDestination(position, WalkCallback);
			}
		}

		protected virtual void WalkCallback(NPCMovement.WalkResult result)
		{
			if (IsActive)
			{
				if (result == NPCMovement.WalkResult.Failed)
				{
					consecutivePathingFailures++;
				}
				else
				{
					consecutivePathingFailures = 0;
				}
				if (schedule.DEBUG_MODE)
				{
					Console.Log("Walk callback result: " + result);
				}
			}
		}

		public virtual void SetStartTime(int startTime)
		{
			StartTime = startTime;
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCAction_Assembly_002DCSharp_002Edll()
		{
			GetReferences();
		}
	}
}
