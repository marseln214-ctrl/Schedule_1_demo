using System;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.GameTime;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs
{
	[RequireComponent(typeof(NPCHealth))]
	public class NPCHealth : NetworkBehaviour
	{
		public const int REVIVE_DAYS = 3;

		[CompilerGenerated]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public float _003CHealth_003Ek__BackingField;

		[Header("Settings")]
		public float MaxHealth = 100f;

		private NPC npc;

		public UnityEvent onDie;

		public UnityEvent onKnockedOut;

		public SyncVar<float> syncVar____003CHealth_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCHealthAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCHealthAssembly_002DCSharp_002Edll_Excuted;

		//public float Health
		//{
		//	[CompilerGenerated]
		//	get
		//	{
		//		return SyncAccessor__003CHealth_003Ek__BackingField;
		//	}
		//	[CompilerGenerated]
		//	private set
		//	{
		//		syncVar____003CHealth_003Ek__BackingField.SetValue(value,  true);
		//	}
		//}

        public float Health
        {
            get
            {
                return _003CHealth_003Ek__BackingField;
            }
            protected set
            {
                _003CHealth_003Ek__BackingField = value;

                syncVar____003CHealth_003Ek__BackingField.SetValue(value, true);
            }
        }

        public bool IsDead { get; private set; }

		public int DaysPassedSinceDeath { get; private set; }

		public float SyncAccessor__003CHealth_003Ek__BackingField
		{
			get
			{
				return Health;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					Health = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CHealth_003Ek__BackingField.SetValue(value, value != 0);
				}
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCHealth_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		private void OnDestroy()
		{
			ScheduleOne.GameTime.TimeManager.onSleepStart = (Action)Delegate.Remove(ScheduleOne.GameTime.TimeManager.onSleepStart, new Action(SleepStart));
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			Health = MaxHealth;
		}

		public void Load(NPCHealthData healthData)
		{
			DaysPassedSinceDeath = healthData.DaysPassedSinceDeath;
		}

		public void SleepStart()
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			_ = IsDead;
			if (!npc.IsConscious)
			{
				if (IsDead)
				{
					DaysPassedSinceDeath++;
					if (DaysPassedSinceDeath >= 3 || npc.IsImportant)
					{
						Revive();
					}
				}
				else
				{
					Revive();
				}
			}
			if (npc.IsConscious)
			{
				Health = MaxHealth;
			}
		}

		public void TakeDamage(float damage, bool isLethal = true)
		{
			if (npc.IsConscious)
			{
				Console.Log(npc.fullName + " has taken " + damage + " damage.");
				SyncAccessor__003CHealth_003Ek__BackingField -= damage;
				if (SyncAccessor__003CHealth_003Ek__BackingField <= 0f)
				{
					Health = 0f;
					KnockOut();
				}
			}
		}

		public virtual void Die()
		{
			Console.LogWarning("NPCs can't die in demo; knocking out instead.");
			KnockOut();
		}

		public virtual void KnockOut()
		{
			Console.Log(npc.fullName + " has been knocked out.");
			npc.behaviour.UnconsciousBehaviour.Enable_Networked(null);
			if (onKnockedOut != null)
			{
				onKnockedOut.Invoke();
			}
		}

		public virtual void Revive()
		{
			Console.Log(npc.fullName + " has been revived.");
			IsDead = false;
			Health = MaxHealth;
			npc.behaviour.DeadBehaviour.SendDisable();
			npc.behaviour.UnconsciousBehaviour.SendDisable();
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCHealthAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCHealthAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CHealth_003Ek__BackingField = new SyncVar<float>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, Health);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002ENPCs_002ENPCHealth);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCHealthAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCHealthAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CHealth_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public virtual bool ReadSyncVar___ScheduleOne_002ENPCs_002ENPCHealth(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 0)
			{
				if (PooledReader0 == null)
				{
					syncVar____003CHealth_003Ek__BackingField.SetValue(syncVar____003CHealth_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				float value = PooledReader0.ReadSingle();
				syncVar____003CHealth_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			return false;
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCHealth_Assembly_002DCSharp_002Edll()
		{
			npc = GetComponent<NPC>();
			ScheduleOne.GameTime.TimeManager.onSleepStart = (Action)Delegate.Combine(ScheduleOne.GameTime.TimeManager.onSleepStart, new Action(SleepStart));
		}
	}
}
