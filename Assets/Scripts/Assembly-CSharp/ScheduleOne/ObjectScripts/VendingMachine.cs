using System;
using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts
{
	public class VendingMachine : NetworkBehaviour, IGUIDRegisterable, IGenericSaveable
	{
		public static List<VendingMachine> AllMachines = new List<VendingMachine>();

		public const float COST = 2f;

		public const int REPAIR_TIME_DAYS = 0;

		public const float IMPACT_THRESHOLD_FREE_ITEM = 50f;

		public const float IMPACT_THRESHOLD_FREE_ITEM_CHANCE = 0.33f;

		public const float IMPACT_THRESHOLD_BREAK = 165f;

		public const int MIN_CASH_DROP = 1;

		public const int MAX_CASH_DROP = 4;

		[Header("Settings")]
		public int LitStartTime = 1700;

		public int LitOnEndTime = 800;

		public ItemPickup CukePrefab;

		public CashPickup CashPrefab;

		[Header("References")]
		public MeshRenderer DoorMesh;

		public MeshRenderer BodyMesh;

		public Material DoorOffMat;

		public Material DoorOnMat;

		public Material BodyOffMat;

		public Material BodyOnMat;

		public OptimizedLight[] Lights;

		public AudioSourceController PaySound;

		public AudioSourceController DispenseSound;

		public Animation Anim;

		public Transform ItemSpawnPoint;

		public InteractableObject IntObj;

		public Transform AccessPoint;

		public PhysicsDamageable Damageable;

		public Transform CashSpawnPoint;

		public UnityEvent onBreak;

		public UnityEvent onRepair;

		private bool isLit;

		private bool purchaseInProgress;

		private float timeOnLastFreeItem;

		[SerializeField]
		protected string BakedGUID = string.Empty;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted;

		public bool IsBroken { get; protected set; }

		public int DaysUntilRepair { get; protected set; }

		public ItemPickup lastDroppedItem { get; protected set; }

		public Guid GUID { get; protected set; }

		[Button]
		public void RegenerateGUID()
		{
			BakedGUID = Guid.NewGuid().ToString();
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EObjectScripts_002EVendingMachine_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		private void Start()
		{
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance._onSleepStart.RemoveListener(DayPass);
			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance._onSleepStart.AddListener(DayPass);
			SetLit(lit: false);
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

		private void OnDestroy()
		{
			if (AllMachines.Contains(this))
			{
				AllMachines.Remove(this);
			}
			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.InstanceExists)
			{
				NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance._onSleepStart.RemoveListener(DayPass);
			}
		}

		private void MinPass()
		{
			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(LitStartTime, LitOnEndTime) && !IsBroken)
			{
				if (!isLit)
				{
					SetLit(lit: true);
				}
			}
			else if (isLit)
			{
				SetLit(lit: false);
			}
		}

		public void DayPass()
		{
			if (IsBroken)
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
			if (purchaseInProgress)
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
			else if (IsBroken)
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
			else if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= 2f)
			{
				IntObj.SetMessage("Purchase Cuke");
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				IntObj.SetMessage("Not enough cash");
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			}
		}

		public void Interacted()
		{
			if (!purchaseInProgress && !IsBroken && NetworkSingleton<MoneyManager>.Instance.cashBalance >= 2f)
			{
				LocalPurchase();
			}
		}

		private void LocalPurchase()
		{
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(-2f);
			SendPurchase();
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendPurchase()
		{
			RpcWriter___Server_SendPurchase_2166136261();
			RpcLogic___SendPurchase_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		public void PurchaseRoutine()
		{
			RpcWriter___Observers_PurchaseRoutine_2166136261();
			RpcLogic___PurchaseRoutine_2166136261();
		}

		[ServerRpc(RequireOwnership = false)]
		public void DropItem()
		{
			RpcWriter___Server_DropItem_2166136261();
		}

		public void RemoveLastDropped()
		{
			if (lastDroppedItem != null && lastDroppedItem.gameObject != null)
			{
				lastDroppedItem.Destroy();
				lastDroppedItem = null;
			}
		}

		private void Impacted(Impact impact)
		{
			if (impact.ImpactForce < 50f || IsBroken)
			{
				return;
			}
			if (impact.ImpactForce >= 165f)
			{
				SendBreak();
				if (impact.ImpactSource == Player.Local.NetworkObject)
				{
					Player.Local.VisualState.ApplyState("vandalism", PlayerVisualState.EVisualState.Vandalizing);
					Player.Local.VisualState.RemoveState("vandalism", 2f);
				}
				StartCoroutine(BreakRoutine());
			}
			else if (UnityEngine.Random.value < 0.33f && Time.time - timeOnLastFreeItem > 10f)
			{
				timeOnLastFreeItem = Time.time;
				StartCoroutine(Drop());
			}
			IEnumerator BreakRoutine()
			{
				int cashDrop = UnityEngine.Random.Range(1, 5);
				for (int i = 0; i < cashDrop; i++)
				{
					DropCash();
					yield return new WaitForSeconds(0.25f);
				}
			}
			IEnumerator Drop()
			{
				DispenseSound.Play();
				yield return new WaitForSeconds(0.65f);
				DropItem();
			}
		}

		private void SetLit(bool lit)
		{
			isLit = lit;
			if (isLit)
			{
				Material[] materials = DoorMesh.materials;
				materials[1] = DoorOnMat;
				DoorMesh.materials = materials;
				Material[] materials2 = BodyMesh.materials;
				materials2[1] = BodyOnMat;
				BodyMesh.materials = materials2;
			}
			else
			{
				Material[] materials3 = DoorMesh.materials;
				materials3[1] = DoorOffMat;
				DoorMesh.materials = materials3;
				Material[] materials4 = BodyMesh.materials;
				materials4[1] = BodyOffMat;
				BodyMesh.materials = materials4;
			}
			for (int i = 0; i < Lights.Length; i++)
			{
				Lights[i].Enabled = isLit;
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
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SendPurchase_2166136261);
				RegisterObserversRpc(1u, RpcReader___Observers_PurchaseRoutine_2166136261);
				RegisterServerRpc(2u, RpcReader___Server_DropItem_2166136261);
				RegisterServerRpc(3u, RpcReader___Server_SendBreak_2166136261);
				RegisterObserversRpc(4u, RpcReader___Observers_Break_328543758);
				RegisterTargetRpc(5u, RpcReader___Target_Break_328543758);
				RegisterObserversRpc(6u, RpcReader___Observers_Repair_2166136261);
				RegisterServerRpc(7u, RpcReader___Server_DropCash_2166136261);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendPurchase_2166136261()
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

		public void RpcLogic___SendPurchase_2166136261()
		{
			PurchaseRoutine();
		}

		private void RpcReader___Server_SendPurchase_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendPurchase_2166136261();
			}
		}

		private void RpcWriter___Observers_PurchaseRoutine_2166136261()
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

		public void RpcLogic___PurchaseRoutine_2166136261()
		{
			if (!purchaseInProgress)
			{
				purchaseInProgress = true;
				Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				PaySound.Play();
				DispenseSound.Play();
				Anim.Play();
				yield return new WaitForSeconds(0.65f);
				if (base.IsServer)
				{
					DropItem();
				}
				purchaseInProgress = false;
			}
		}

		private void RpcReader___Observers_PurchaseRoutine_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___PurchaseRoutine_2166136261();
			}
		}

		private void RpcWriter___Server_DropItem_2166136261()
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
				SendServerRpc(2u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___DropItem_2166136261()
		{
			ItemPickup itemPickup = UnityEngine.Object.Instantiate(CukePrefab, ItemSpawnPoint.position, ItemSpawnPoint.rotation);
			Spawn(itemPickup.gameObject);
			lastDroppedItem = itemPickup;
		}

		private void RpcReader___Server_DropItem_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized)
			{
				RpcLogic___DropItem_2166136261();
			}
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
				SendServerRpc(3u, writer, channel, DataOrderType.Default);
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
				SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___Break_328543758(NetworkConnection conn)
		{
			if (!IsBroken)
			{
				IsBroken = true;
				SetLit(lit: false);
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
				SendTargetRpc(5u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
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
				SendObserversRpc(6u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
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
				SendServerRpc(7u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___DropCash_2166136261()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(CashPrefab.gameObject, CashSpawnPoint.position, CashSpawnPoint.rotation);
			gameObject.GetComponent<Rigidbody>().AddForce(CashSpawnPoint.forward * UnityEngine.Random.Range(1.5f, 2.5f), ForceMode.VelocityChange);
			gameObject.GetComponent<Rigidbody>().AddTorque(UnityEngine.Random.insideUnitSphere * 2f, ForceMode.VelocityChange);
			Spawn(gameObject.gameObject);
			PaySound.Play();
		}

		private void RpcReader___Server_DropCash_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized)
			{
				RpcLogic___DropCash_2166136261();
			}
		}

		private void Awake_UserLogic_ScheduleOne_002EObjectScripts_002EVendingMachine_Assembly_002DCSharp_002Edll()
		{
			if (!AllMachines.Contains(this))
			{
				AllMachines.Add(this);
			}
			PhysicsDamageable damageable = Damageable;
			damageable.onImpacted = (Action<Impact>)Delegate.Combine(damageable.onImpacted, new Action<Impact>(Impacted));
			GUID = new Guid(BakedGUID);
			GUIDManager.RegisterObject(this);
		}
	}
}
