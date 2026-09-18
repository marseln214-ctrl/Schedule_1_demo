using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.GameTime;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Employees
{
	public class Employee : NPC
	{
		public class NoWorkReason
		{
			public string Reason;

			public string Fix;

			public int Priority;

			public NoWorkReason(string reason, string fix, int priority)
			{
				Reason = reason;
				Fix = fix;
				Priority = priority;
			}
		}

		[CompilerGenerated]
		[SyncVar]
		public bool _003CPaidForToday_003Ek__BackingField;

		[SerializeField]
		protected EEmployeeType Type;

		[Header("Payment")]
		public float SigningFee = 500f;

		public float DailyWage = 100f;

		[Header("References")]
		public IdleBehaviour WaitOutside;

		public IdleBehaviour IdleBehaviour;

		public MoveItemBehaviour MoveItemBehaviour;

		public DialogueContainer BedNotAssignedDialogue;

		public DialogueContainer NotPaidDialogue;

		public DialogueContainer WorkIssueDialogueTemplate;

		private List<NoWorkReason> WorkIssues = new List<NoWorkReason>();

		protected bool initialized;

		public SyncVar<bool> syncVar____003CPaidForToday_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted;

		public ScheduleOne.Property.Property AssignedProperty { get; protected set; }

		public int EmployeeIndex { get; protected set; }

		public bool PaidForToday
		{
			[CompilerGenerated]
			get
			{
				return _003CPaidForToday_003Ek__BackingField;
			}
			[CompilerGenerated]
			private set
			{
                _003CPaidForToday_003Ek__BackingField = value;
                syncVar____003CPaidForToday_003Ek__BackingField.SetValue(value, true);
            }
        }

		public bool IsWaitingOutside => WaitOutside.Active;

		public bool IsMale { get; private set; } = true;


		protected int AppearanceIndex { get; private set; }

		public EEmployeeType EmployeeType => Type;

		public int TimeSinceLastWorked { get; private set; }

		public bool SyncAccessor__003CPaidForToday_003Ek__BackingField
		{
			get
			{
				return PaidForToday;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					PaidForToday = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CPaidForToday_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		protected override void Start()
		{
			base.Start();
			DialogueController.DialogueChoice dialogueChoice = new DialogueController.DialogueChoice();
			dialogueChoice.ChoiceText = "Why aren't you working?";
			dialogueChoice.Enabled = true;
			dialogueChoice.shouldShowCheck = ShouldShowNoWorkDialogue;
			dialogueChoice.onChoosen.AddListener(OnNotWorkingDialogue);
			dialogueHandler.GetComponent<DialogueController>().AddDialogueChoice(dialogueChoice);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (!connection.IsLocalClient)
			{
				Initialize(connection, FirstName, LastName, ID, base.GUID.ToString(), AssignedProperty.PropertyCode, IsMale, AppearanceIndex);
			}
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public virtual void Initialize(NetworkConnection conn, string firstName, string lastName, string id, string guid, string propertyID, bool male, int appearanceIndex)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_Initialize_2260823878(conn, firstName, lastName, id, guid, propertyID, male, appearanceIndex);
				RpcLogic___Initialize_2260823878(conn, firstName, lastName, id, guid, propertyID, male, appearanceIndex);
			}
			else
			{
				RpcWriter___Target_Initialize_2260823878(conn, firstName, lastName, id, guid, propertyID, male, appearanceIndex);
			}
		}

		protected virtual void AssignProperty(ScheduleOne.Property.Property prop)
		{
			AssignedProperty = prop;
			EmployeeIndex = AssignedProperty.RegisterEmployee(this);
			movement.Warp(prop.NPCSpawnPoint.position);
			WaitOutside.IdlePoint = prop.EmployeeIdlePoints[EmployeeIndex];
			IdleBehaviour.IdlePoint = prop.EmployeeIdlePoints[EmployeeIndex];
		}

		protected virtual void InitializeInfo(string firstName, string lastName, string id)
		{
			FirstName = firstName;
			LastName = lastName;
			ID = id;
			NetworkSingleton<EmployeeManager>.Instance.RegisterName(firstName + " " + lastName);
		}

		protected virtual void InitializeAppearance(bool male, int index)
		{
			IsMale = male;
			AppearanceIndex = index;
			EmployeeManager.EmployeeAppearance appearance = NetworkSingleton<EmployeeManager>.Instance.GetAppearance(male, index);
			Avatar.LoadNakedSettings(appearance.Settings, 100);
			MugshotSprite = appearance.Mugshot;
			VoiceOverEmitter.Database = NetworkSingleton<EmployeeManager>.Instance.GetVoice(male, index);
			int num = (FirstName + LastName).GetHashCode() / 1000;
			VoiceOverEmitter.PitchMultiplier = 0.9f + (float)(num % 10) / 10f * 0.2f;
			NetworkSingleton<EmployeeManager>.Instance.RegisterAppearance(male, index);
			float num2 = (male ? 0.8f : 1.3f);
			float num3 = 0.2f;
			float num4 = (0f - num3) / 2f + Mathf.Clamp01((float)(FirstName.GetHashCode() % 10) / 10f) * num3;
			num2 += num4;
			VoiceOverEmitter.PitchMultiplier = num2;
		}

		protected virtual void OnDestroy()
		{
			if (InstanceFinder.IsServer)
			{
				ScheduleOne.GameTime.TimeManager.onSleepEnd = (Action<int>)Delegate.Remove(ScheduleOne.GameTime.TimeManager.onSleepEnd, new Action<int>(OnSleepEnd));
			}
			NetworkSingleton<EmployeeManager>.Instance.AllEmployees.Remove(this);
		}

		protected virtual void UpdateBehaviour()
		{
		}

		protected void MarkIsWorking()
		{
			TimeSinceLastWorked = 0;
		}

		private void SetWaitOutside(bool wait)
		{
			if (wait)
			{
				if (!WaitOutside.Enabled)
				{
					WaitOutside.Enable_Networked(null);
				}
			}
			else if (WaitOutside.Enabled || WaitOutside.Active)
			{
				WaitOutside.Disable_Networked(null);
				WaitOutside.End_Networked(null);
			}
		}

		protected virtual bool ShouldIdle()
		{
			return false;
		}

		protected override bool ShouldNoticeGeneralCrime(Player player)
		{
			return false;
		}

		protected override void MinPass()
		{
			base.MinPass();
			TimeSinceLastWorked++;
			WorkIssues.Clear();
		}

		private void OnSleepEnd(int sleepTime)
		{
			PaidForToday = false;
		}

		public void SetIsPaid()
		{
			PaidForToday = true;
		}

		public override bool ShouldSave()
		{
			return false;
		}

		public override string GetSaveString()
		{
			return new EmployeeData(ID, AssignedProperty.PropertyCode, FirstName, LastName, IsMale, AppearanceIndex, base.transform.position, base.transform.rotation, base.GUID, SyncAccessor__003CPaidForToday_003Ek__BackingField).GetJson();
		}

		public virtual BedItem GetBed()
		{
			Console.LogError("GETBED NOT IMPLEMENTED");
			return null;
		}

		public bool IsPayAvailable()
		{
			BedItem bed = GetBed();
			if (bed == null)
			{
				return false;
			}
			return bed.GetCashSum() >= DailyWage;
		}

		public void RemoveDailyWage()
		{
			Console.Log("Removing daily wage");
			BedItem bed = GetBed();
			if (!(bed == null) && bed.GetCashSum() >= DailyWage)
			{
				bed.RemoveCash(DailyWage);
			}
		}

		public virtual bool GetWorkIssue(out DialogueContainer notWorkingReason)
		{
			if (GetBed() == null)
			{
				notWorkingReason = BedNotAssignedDialogue;
				return true;
			}
			if (!SyncAccessor__003CPaidForToday_003Ek__BackingField)
			{
				notWorkingReason = NotPaidDialogue;
				return true;
			}
			if (TimeSinceLastWorked >= 5 && WorkIssues.Count > 0)
			{
				notWorkingReason = UnityEngine.Object.Instantiate(WorkIssueDialogueTemplate);
				notWorkingReason.GetDialogueNodeByLabel("ENTRY").DialogueText = WorkIssues[0].Reason;
				if (!string.IsNullOrEmpty(WorkIssues[0].Fix))
				{
					notWorkingReason.GetDialogueNodeByLabel("FIX").DialogueText = WorkIssues[0].Fix;
				}
				else
				{
					notWorkingReason.GetDialogueNodeByLabel("ENTRY").choices = new DialogueChoiceData[0];
				}
				return true;
			}
			notWorkingReason = null;
			return false;
		}

		public virtual void SetIdle(bool idle)
		{
			if (idle && !IdleBehaviour.Enabled)
			{
				IdleBehaviour.Enable_Networked(null);
			}
			else if (!idle && IdleBehaviour.Enabled)
			{
				IdleBehaviour.Disable_Networked(null);
			}
		}

		[ObserversRpc(RunLocally = true)]
		public void SubmitNoWorkReason(string reason, string fix, int priority = 0)
		{
			RpcWriter___Observers_SubmitNoWorkReason_15643032(reason, fix, priority);
			RpcLogic___SubmitNoWorkReason_15643032(reason, fix, priority);
		}

		private bool ShouldShowNoWorkDialogue(bool enabled)
		{
			DialogueContainer notWorkingReason;
			if (WaitOutside.Active || IdleBehaviour.Active)
			{
				return GetWorkIssue(out notWorkingReason);
			}
			return false;
		}

		private void OnNotWorkingDialogue()
		{
			if (GetWorkIssue(out var notWorkingReason))
			{
				dialogueHandler.InitializeDialogue(notWorkingReason);
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar____003CPaidForToday_003Ek__BackingField = new SyncVar<bool>(this, 1u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, PaidForToday);
				RegisterObserversRpc(34u, RpcReader___Observers_Initialize_2260823878);
				RegisterTargetRpc(35u, RpcReader___Target_Initialize_2260823878);
				RegisterObserversRpc(36u, RpcReader___Observers_SubmitNoWorkReason_15643032);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EEmployees_002EEmployee);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar____003CPaidForToday_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_Initialize_2260823878(NetworkConnection conn, string firstName, string lastName, string id, string guid, string propertyID, bool male, int appearanceIndex)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteString(firstName);
				writer.WriteString(lastName);
				writer.WriteString(id);
				writer.WriteString(guid);
				writer.WriteString(propertyID);
				writer.WriteBoolean(male);
				writer.WriteInt32(appearanceIndex);
				SendObserversRpc(34u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public virtual void RpcLogic___Initialize_2260823878(NetworkConnection conn, string firstName, string lastName, string id, string guid, string propertyID, bool male, int appearanceIndex)
		{
			_ = initialized;
		}

		private void RpcReader___Observers_Initialize_2260823878(PooledReader PooledReader0, Channel channel)
		{
			string firstName = PooledReader0.ReadString();
			string lastName = PooledReader0.ReadString();
			string id = PooledReader0.ReadString();
			string guid = PooledReader0.ReadString();
			string propertyID = PooledReader0.ReadString();
			bool male = PooledReader0.ReadBoolean();
			int appearanceIndex = PooledReader0.ReadInt32();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Initialize_2260823878(null, firstName, lastName, id, guid, propertyID, male, appearanceIndex);
			}
		}

		private void RpcWriter___Target_Initialize_2260823878(NetworkConnection conn, string firstName, string lastName, string id, string guid, string propertyID, bool male, int appearanceIndex)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteString(firstName);
				writer.WriteString(lastName);
				writer.WriteString(id);
				writer.WriteString(guid);
				writer.WriteString(propertyID);
				writer.WriteBoolean(male);
				writer.WriteInt32(appearanceIndex);
				SendTargetRpc(35u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_Initialize_2260823878(PooledReader PooledReader0, Channel channel)
		{
			string firstName = PooledReader0.ReadString();
			string lastName = PooledReader0.ReadString();
			string id = PooledReader0.ReadString();
			string guid = PooledReader0.ReadString();
			string propertyID = PooledReader0.ReadString();
			bool male = PooledReader0.ReadBoolean();
			int appearanceIndex = PooledReader0.ReadInt32();
			if (base.IsClientInitialized)
			{
				RpcLogic___Initialize_2260823878(base.LocalConnection, firstName, lastName, id, guid, propertyID, male, appearanceIndex);
			}
		}

		private void RpcWriter___Observers_SubmitNoWorkReason_15643032(string reason, string fix, int priority = 0)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteString(reason);
				writer.WriteString(fix);
				writer.WriteInt32(priority);
				SendObserversRpc(36u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SubmitNoWorkReason_15643032(string reason, string fix, int priority = 0)
		{
			NoWorkReason noWorkReason = new NoWorkReason(reason, fix, priority);
			for (int i = 0; i < WorkIssues.Count; i++)
			{
				if (WorkIssues[i].Priority < noWorkReason.Priority)
				{
					WorkIssues.Insert(i, noWorkReason);
					return;
				}
			}
			WorkIssues.Add(noWorkReason);
		}

		private void RpcReader___Observers_SubmitNoWorkReason_15643032(PooledReader PooledReader0, Channel channel)
		{
			string reason = PooledReader0.ReadString();
			string fix = PooledReader0.ReadString();
			int priority = PooledReader0.ReadInt32();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SubmitNoWorkReason_15643032(reason, fix, priority);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EEmployees_002EEmployee(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 1)
			{
				if (PooledReader0 == null)
				{
                    syncVar____003CPaidForToday_003Ek__BackingField.SetValue(syncVar____003CPaidForToday_003Ek__BackingField.GetValue(calledByUser: true), true);
					return true;
				}
				bool value = PooledReader0.ReadBoolean();
                syncVar____003CPaidForToday_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			return false;
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
