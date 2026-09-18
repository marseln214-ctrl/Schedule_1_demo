using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.Property.Utilities.Water
{
	public class Tap : NetworkBehaviour, IUsable
	{
		public const float MaxFlowRate = 6f;

		[CompilerGenerated]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public bool _003CIsHeldOpen_003Ek__BackingField;

		[Header("References")]
		public InteractableObject IntObj;

		public Transform CameraPos;

		public Transform WateringCamPos;

		public Collider HandleCollider;

		public Transform Handle;

		public Clickable HandleClickable;

		public ParticleSystem WaterParticles;

		public AudioSourceController SqueakSound;

		public AudioSourceController WaterRunningSound;

		[CompilerGenerated]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CNPCUserObject_003Ek__BackingField;

		[CompilerGenerated]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CPlayerUserObject_003Ek__BackingField;

		private float tapFlow;

		private GameObject wateringCanModel;

		private bool intObjSetThisFrame;

		public SyncVar<bool> syncVar____003CIsHeldOpen_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CNPCUserObject_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CPlayerUserObject_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EProperty_002EUtilities_002EWater_002ETapAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EProperty_002EUtilities_002EWater_002ETapAssembly_002DCSharp_002Edll_Excuted;

		public bool IsHeldOpen
		{
			[CompilerGenerated]
			get
			{
				return _003CIsHeldOpen_003Ek__BackingField;
			}
			[CompilerGenerated]
			set
			{
				_003CIsHeldOpen_003Ek__BackingField = value;

                syncVar____003CIsHeldOpen_003Ek__BackingField.SetValue(value,  true);
			}
		}

		public float ActualFlowRate => 6f * tapFlow;

		public NetworkObject NPCUserObject
		{
			[CompilerGenerated]
			get
			{
				return _003CNPCUserObject_003Ek__BackingField;
			}
			[CompilerGenerated]
			set
			{
				_003CNPCUserObject_003Ek__BackingField = value;

                syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(value,  true);
			}
		}

		public NetworkObject PlayerUserObject
		{
			[CompilerGenerated]
			get
			{
				return _003CPlayerUserObject_003Ek__BackingField;
			}
			[CompilerGenerated]
			set
			{
				_003CPlayerUserObject_003Ek__BackingField = value;

                syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(value,  true);
			}
		}

		public bool SyncAccessor__003CIsHeldOpen_003Ek__BackingField
		{
			get
			{
				return IsHeldOpen;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					IsHeldOpen = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CIsHeldOpen_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		public NetworkObject SyncAccessor__003CNPCUserObject_003Ek__BackingField
		{
			get
			{
				return NPCUserObject;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					NPCUserObject = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		public NetworkObject SyncAccessor__003CPlayerUserObject_003Ek__BackingField
		{
			get
			{
				return PlayerUserObject;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					PlayerUserObject = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EProperty_002EUtilities_002EWater_002ETap_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected virtual void LateUpdate()
		{
			float num = 2f;
			if (SyncAccessor__003CIsHeldOpen_003Ek__BackingField)
			{
				tapFlow = Mathf.Clamp(tapFlow + Time.deltaTime * num, 0f, 1f);
			}
			else
			{
				tapFlow = Mathf.Clamp(tapFlow - Time.deltaTime * num, 0f, 1f);
			}
			UpdateTapVisuals();
			UpdateWaterSound();
			if (!intObjSetThisFrame)
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
			intObjSetThisFrame = false;
		}

		public void SetInteractableObject(string message, InteractableObject.EInteractableState state)
		{
			intObjSetThisFrame = true;
			IntObj.SetMessage(message);
			IntObj.SetInteractableState(state);
		}

		protected void UpdateTapVisuals()
		{
			Handle.transform.localEulerAngles = new Vector3(0f, (0f - tapFlow) * 360f, 0f);
			if (tapFlow > 0f)
			{
				ParticleSystem.MainModule main = WaterParticles.main;
				main.startSize = new ParticleSystem.MinMaxCurve(0.075f * tapFlow, 0.1f * tapFlow);
				if (!WaterParticles.isPlaying)
				{
					WaterParticles.Play();
				}
			}
			else if (WaterParticles.isPlaying)
			{
				WaterParticles.Stop();
			}
		}

		protected void UpdateWaterSound()
		{
			if (tapFlow > 0.01f)
			{
				WaterRunningSound.VolumeMultiplier = tapFlow;
				if (!WaterRunningSound.isPlaying)
				{
					WaterRunningSound.Play();
				}
			}
			else if (WaterRunningSound.isPlaying)
			{
				WaterRunningSound.Stop();
			}
		}

		public void Hovered()
		{
			if (CanInteract())
			{
				IntObj.SetMessage("Fill watering can");
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}

		public void Interacted()
		{
			if (CanInteract())
			{
				new FillWateringCan(this, PlayerSingleton<PlayerInventory>.Instance.equippedSlot?.ItemInstance as WateringCanInstance);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SetPlayerUser(NetworkObject playerObject)
		{
			RpcWriter___Server_SetPlayerUser_3323014238(playerObject);
			RpcLogic___SetPlayerUser_3323014238(playerObject);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SetNPCUser(NetworkObject npcObject)
		{
			RpcWriter___Server_SetNPCUser_3323014238(npcObject);
			RpcLogic___SetNPCUser_3323014238(npcObject);
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public void SetHeldOpen(bool open)
		{
			RpcWriter___Server_SetHeldOpen_1140765316(open);
			RpcLogic___SetHeldOpen_1140765316(open);
		}

		protected virtual bool CanInteract()
		{
			ItemInstance itemInstance = PlayerSingleton<PlayerInventory>.Instance.equippedSlot?.ItemInstance;
			if (itemInstance == null)
			{
				return false;
			}
			if (!(itemInstance is WateringCanInstance wateringCanInstance))
			{
				return false;
			}
			if (wateringCanInstance.CurrentFillAmount >= 15f)
			{
				return false;
			}
			if (((IUsable)this).IsInUse)
			{
				return false;
			}
			return true;
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public void SendWateringCanModel(string ID)
		{
			RpcWriter___Server_SendWateringCanModel_3615296227(ID);
			RpcLogic___SendWateringCanModel_3615296227(ID);
		}

		[ObserversRpc(RunLocally = true)]
		private void CreateWateringCanModel(string ID)
		{
			RpcWriter___Observers_CreateWateringCanModel_3615296227(ID);
			RpcLogic___CreateWateringCanModel_3615296227(ID);
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public void SendClearWateringCanModelModel()
		{
			RpcWriter___Server_SendClearWateringCanModelModel_2166136261();
			RpcLogic___SendClearWateringCanModelModel_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		private void ClearWateringCanModel()
		{
			RpcWriter___Observers_ClearWateringCanModel_2166136261();
			RpcLogic___ClearWateringCanModel_2166136261();
		}

		public GameObject CreateWateringCanModel_Local(string ID, bool force = false)
		{
			if (wateringCanModel != null && !force)
			{
				return null;
			}
			WateringCanDefinition wateringCanDefinition = Registry.GetItem(ID) as WateringCanDefinition;
			if (wateringCanDefinition == null)
			{
				Console.LogWarning("CreateWateringCanModel_Local: WateringCanDefinition not found");
				return null;
			}
			wateringCanModel = Object.Instantiate(wateringCanDefinition.FunctionalWateringCanPrefab, base.transform);
			wateringCanModel.transform.position = WateringCamPos.position;
			wateringCanModel.transform.rotation = WateringCamPos.rotation;
			wateringCanModel.GetComponent<FunctionalWateringCan>().enabled = false;
			return wateringCanModel;
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EProperty_002EUtilities_002EWater_002ETapAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EProperty_002EUtilities_002EWater_002ETapAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CPlayerUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 2u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, PlayerUserObject);
				syncVar____003CNPCUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 1u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, NPCUserObject);
				syncVar____003CIsHeldOpen_003Ek__BackingField = new SyncVar<bool>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, IsHeldOpen);
				RegisterServerRpc(0u, RpcReader___Server_SetPlayerUser_3323014238);
				RegisterServerRpc(1u, RpcReader___Server_SetNPCUser_3323014238);
				RegisterServerRpc(2u, RpcReader___Server_SetHeldOpen_1140765316);
				RegisterServerRpc(3u, RpcReader___Server_SendWateringCanModel_3615296227);
				RegisterObserversRpc(4u, RpcReader___Observers_CreateWateringCanModel_3615296227);
				RegisterServerRpc(5u, RpcReader___Server_SendClearWateringCanModelModel_2166136261);
				RegisterObserversRpc(6u, RpcReader___Observers_ClearWateringCanModel_2166136261);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EProperty_002EUtilities_002EWater_002ETap);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EProperty_002EUtilities_002EWater_002ETapAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EProperty_002EUtilities_002EWater_002ETapAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CPlayerUserObject_003Ek__BackingField.SetRegistered();
				syncVar____003CNPCUserObject_003Ek__BackingField.SetRegistered();
				syncVar____003CIsHeldOpen_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SetPlayerUser_3323014238(NetworkObject playerObject)
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
				writer.WriteNetworkObject(playerObject);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetPlayerUser_3323014238(NetworkObject playerObject)
		{
			if (SyncAccessor__003CPlayerUserObject_003Ek__BackingField != null && SyncAccessor__003CPlayerUserObject_003Ek__BackingField.Owner.IsLocalClient && playerObject != null && !playerObject.Owner.IsLocalClient)
			{
				Singleton<GameInput>.Instance.ExitAll();
			}
			PlayerUserObject = playerObject;
		}

		private void RpcReader___Server_SetPlayerUser_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject playerObject = PooledReader0.ReadNetworkObject();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetPlayerUser_3323014238(playerObject);
			}
		}

		private void RpcWriter___Server_SetNPCUser_3323014238(NetworkObject npcObject)
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
				writer.WriteNetworkObject(npcObject);
				SendServerRpc(1u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetNPCUser_3323014238(NetworkObject npcObject)
		{
			NPCUserObject = npcObject;
		}

		private void RpcReader___Server_SetNPCUser_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject npcObject = PooledReader0.ReadNetworkObject();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetNPCUser_3323014238(npcObject);
			}
		}

		private void RpcWriter___Server_SetHeldOpen_1140765316(bool open)
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
				writer.WriteBoolean(open);
				SendServerRpc(2u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetHeldOpen_1140765316(bool open)
		{
			if (open && !SyncAccessor__003CIsHeldOpen_003Ek__BackingField)
			{
				SqueakSound.Play();
			}
			IsHeldOpen = open;
		}

		private void RpcReader___Server_SetHeldOpen_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			bool open = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetHeldOpen_1140765316(open);
			}
		}

		private void RpcWriter___Server_SendWateringCanModel_3615296227(string ID)
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
				writer.WriteString(ID);
				SendServerRpc(3u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendWateringCanModel_3615296227(string ID)
		{
			CreateWateringCanModel(ID);
		}

		private void RpcReader___Server_SendWateringCanModel_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string iD = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendWateringCanModel_3615296227(iD);
			}
		}

		private void RpcWriter___Observers_CreateWateringCanModel_3615296227(string ID)
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
				writer.WriteString(ID);
				SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___CreateWateringCanModel_3615296227(string ID)
		{
			wateringCanModel = CreateWateringCanModel_Local(ID);
		}

		private void RpcReader___Observers_CreateWateringCanModel_3615296227(PooledReader PooledReader0, Channel channel)
		{
			string iD = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___CreateWateringCanModel_3615296227(iD);
			}
		}

		private void RpcWriter___Server_SendClearWateringCanModelModel_2166136261()
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
				SendServerRpc(5u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendClearWateringCanModelModel_2166136261()
		{
			ClearWateringCanModel();
		}

		private void RpcReader___Server_SendClearWateringCanModelModel_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendClearWateringCanModelModel_2166136261();
			}
		}

		private void RpcWriter___Observers_ClearWateringCanModel_2166136261()
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

		private void RpcLogic___ClearWateringCanModel_2166136261()
		{
			if (wateringCanModel != null)
			{
				Object.Destroy(wateringCanModel);
				wateringCanModel = null;
			}
		}

		private void RpcReader___Observers_ClearWateringCanModel_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ClearWateringCanModel_2166136261();
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EProperty_002EUtilities_002EWater_002ETap(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
			case 2:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(syncVar____003CPlayerUserObject_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				NetworkObject value3 = PooledReader0.ReadNetworkObject();
				syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(value3, Boolean2);
				return true;
			}
			case 1:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(syncVar____003CNPCUserObject_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				NetworkObject value2 = PooledReader0.ReadNetworkObject();
				syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(value2, Boolean2);
				return true;
			}
			case 0:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CIsHeldOpen_003Ek__BackingField.SetValue(syncVar____003CIsHeldOpen_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				bool value = PooledReader0.ReadBoolean();
				syncVar____003CIsHeldOpen_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			default:
				return false;
			}
		}

		private void Awake_UserLogic_ScheduleOne_002EProperty_002EUtilities_002EWater_002ETap_Assembly_002DCSharp_002Edll()
		{
			IntObj.onHovered.AddListener(Hovered);
			IntObj.onInteractStart.AddListener(Interacted);
		}
	}
}
