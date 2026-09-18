using System;
using System.Collections;
using EasyButtons;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.ATM;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Money
{
	public class ATM : NetworkBehaviour, IGUIDRegisterable, IGenericSaveable
	{
		public const bool DepositLimitEnabled = false;

		public const float DailyDepositLimit = float.MaxValue;

		public const float IMPACT_THRESHOLD_BREAK = 165f;

		public const int REPAIR_TIME_DAYS = 0;

		public const int MIN_CASH_DROP = 2;

		public const int MAX_CASH_DROP = 8;

		public static float AmountDepositedToday = 0f;

		public CashPickup CashPrefab;

		[Header("References")]
		[SerializeField]
		protected InteractableObject intObj;

		[SerializeField]
		protected Transform camPos;

		[SerializeField]
		protected ATMInterface interfaceATM;

		public Transform AccessPoint;

		public Transform CashSpawnPoint;

		public PhysicsDamageable Damageable;

		[Header("Settings")]
		public static float viewLerpTime = 0.15f;

		[SerializeField]
		protected string BakedGUID = string.Empty;

		public UnityEvent onBreak;

		public UnityEvent onRepair;

		private bool NetworkInitialize___EarlyScheduleOne_002EMoney_002EATMAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EMoney_002EATMAssembly_002DCSharp_002Edll_Excuted;

		public bool IsBroken { get; protected set; }

		public int DaysUntilRepair { get; protected set; }

		public bool isInUse { get; protected set; }

		public Guid GUID { get; protected set; }

		[Button]
		public void RegenerateGUID()
		{
			BakedGUID = Guid.NewGuid().ToString();
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EMoney_002EATM_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected virtual void Start()
		{
			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance._onSleepStart.RemoveListener(DayPass);
			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance._onSleepStart.AddListener(DayPass);
			((IGenericSaveable)this).InitializeSaveable();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (IsBroken)
			{
				Break(connection);
			}
		}

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
		}

		public void DayPass()
		{
			AmountDepositedToday = 0f;
			if (InstanceFinder.IsServer && IsBroken)
			{
				DaysUntilRepair--;
				if (DaysUntilRepair <= 0)
				{
					Repair();
				}
			}
		}

		public void Hovered()
		{
			if (isInUse || IsBroken)
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
				return;
			}
			intObj.SetMessage("Use ATM");
			intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}

		public void Interacted()
		{
			if (!isInUse && !IsBroken)
			{
				Enter();
			}
		}

		public void Enter()
		{
			isInUse = true;
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, viewLerpTime);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(camPos.position, camPos.rotation, viewLerpTime);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			interfaceATM.SetIsOpen(o: true);
		}

		public void Exit()
		{
			isInUse = false;
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(viewLerpTime);
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(viewLerpTime);
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		}

		private void Impacted(Impact impact)
		{
			if (!IsBroken && impact.ImpactForce >= 165f)
			{
				SendBreak();
				if (impact.ImpactSource == Player.Local.NetworkObject)
				{
					Player.Local.VisualState.ApplyState("vandalism", PlayerVisualState.EVisualState.Vandalizing);
					Player.Local.VisualState.RemoveState("vandalism", 2f);
				}
				StartCoroutine(BreakRoutine());
			}
			IEnumerator BreakRoutine()
			{
				int cashDrop = UnityEngine.Random.Range(2, 9);
				for (int i = 0; i < cashDrop; i++)
				{
					DropCash();
					yield return new WaitForSeconds(0.2f);
				}
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		private void SendBreak()
		{
			RpcWriter___Server_SendBreak_2166136261();
			RpcLogic___SendBreak_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void Break(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_Break_328543758(conn);
				RpcLogic___Break_328543758(conn);
			}
			else
			{
				RpcWriter___Target_Break_328543758(conn);
			}
		}

		[ObserversRpc]
		private void Repair()
		{
			RpcWriter___Observers_Repair_2166136261();
		}

		[ServerRpc(RequireOwnership = false)]
		private void DropCash()
		{
			RpcWriter___Server_DropCash_2166136261();
		}

		public void Load(GenericSaveData data)
		{
			bool @bool = data.GetBool("broken");
			if (@bool)
			{
				Break(null);
			}
			IsBroken = @bool;
			DaysUntilRepair = data.GetInt("daysUntilRepair");
		}

		public GenericSaveData GetSaveData()
		{
			GenericSaveData genericSaveData = new GenericSaveData(GUID.ToString());
			genericSaveData.Add("broken", IsBroken);
			genericSaveData.Add("daysUntilRepair", DaysUntilRepair);
			return genericSaveData;
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EMoney_002EATMAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EMoney_002EATMAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SendBreak_2166136261);
				RegisterObserversRpc(1u, RpcReader___Observers_Break_328543758);
				RegisterTargetRpc(2u, RpcReader___Target_Break_328543758);
				RegisterObserversRpc(3u, RpcReader___Observers_Repair_2166136261);
				RegisterServerRpc(4u, RpcReader___Server_DropCash_2166136261);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EMoney_002EATMAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EMoney_002EATMAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendBreak_2166136261()
		{
			if (!base.IsClientInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendBreak_2166136261()
		{
			DaysUntilRepair = 0;
			Break(null);
		}

		private void RpcReader___Server_SendBreak_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendBreak_2166136261();
			}
		}

		private void RpcWriter___Observers_Break_328543758(NetworkConnection conn)
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
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___Break_328543758(NetworkConnection conn)
		{
			if (!IsBroken)
			{
				IsBroken = true;
				onBreak?.Invoke();
			}
		}

		private void RpcReader___Observers_Break_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Break_328543758(null);
			}
		}

		private void RpcWriter___Target_Break_328543758(NetworkConnection conn)
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
				SendTargetRpc(2u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_Break_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___Break_328543758(base.LocalConnection);
			}
		}

		private void RpcWriter___Observers_Repair_2166136261()
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
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___Repair_2166136261()
		{
			if (IsBroken)
			{
				IsBroken = false;
				onRepair?.Invoke();
			}
		}

		private void RpcReader___Observers_Repair_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___Repair_2166136261();
			}
		}

		private void RpcWriter___Server_DropCash_2166136261()
		{
			if (!base.IsClientInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				SendServerRpc(4u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___DropCash_2166136261()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(CashPrefab.gameObject, CashSpawnPoint.position, CashSpawnPoint.rotation);
			gameObject.GetComponent<Rigidbody>().AddForce(CashSpawnPoint.forward * UnityEngine.Random.Range(1.5f, 2.5f), ForceMode.VelocityChange);
			gameObject.GetComponent<Rigidbody>().AddTorque(UnityEngine.Random.insideUnitSphere * 2f, ForceMode.VelocityChange);
			Spawn(gameObject.gameObject);
		}

		private void RpcReader___Server_DropCash_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized)
			{
				RpcLogic___DropCash_2166136261();
			}
		}

		private void Awake_UserLogic_ScheduleOne_002EMoney_002EATM_Assembly_002DCSharp_002Edll()
		{
			PhysicsDamageable damageable = Damageable;
			damageable.onImpacted = (Action<Impact>)Delegate.Combine(damageable.onImpacted, new Action<Impact>(Impacted));
			GUID = new Guid(BakedGUID);
			GUIDManager.RegisterObject(this);
		}
	}
}
