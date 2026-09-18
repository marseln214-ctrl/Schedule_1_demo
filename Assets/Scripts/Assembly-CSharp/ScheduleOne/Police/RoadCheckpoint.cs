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
using ScheduleOne.Misc;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Vehicles;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Police
{
	public class RoadCheckpoint : NetworkBehaviour
	{
		public enum ECheckpointState
		{
			Disabled = 0,
			Enabled = 1
		}

		public const float MAX_TIME_OPEN = 15f;

		protected ECheckpointState appliedState;

		[CompilerGenerated]
		[SyncVar(SendRate = 0.25f)]
		public bool _003CGate1Open_003Ek__BackingField;

		[CompilerGenerated]
		[SyncVar(SendRate = 0.25f)]
		public bool _003CGate2Open_003Ek__BackingField;

		public List<NPC> AssignedNPCs = new List<NPC>();

		[Header("Settings")]
		public EStealthLevel MaxStealthLevel;

		public bool OpenForNPCs = true;

		public bool EnabledOnStart;

		[Header("References")]
		[SerializeField]
		protected GameObject container;

		public CarStopper Stopper1;

		public CarStopper Stopper2;

		public VehicleDetector SearchArea1;

		public VehicleDetector SearchArea2;

		public VehicleObstacle VehicleObstacle1;

		public VehicleObstacle VehicleObstacle2;

		public VehicleDetector NPCVehicleDetectionArea1;

		public VehicleDetector NPCVehicleDetectionArea2;

		public VehicleDetector ImmediateVehicleDetector;

		public Rigidbody[] TrafficCones;

		public Transform[] StandPoints;

		protected Dictionary<Rigidbody, Tuple<Vector3, Quaternion>> trafficConeOriginalTransforms = new Dictionary<Rigidbody, Tuple<Vector3, Quaternion>>();

		private float timeSinceGate1Open;

		private bool vehicleDetectedSinceGate1Open;

		private float timeSinceGate2Open;

		private bool vehicleDetectedSinceGate2Open;

		public UnityEvent<Player> onPlayerWalkThrough;

		public SyncVar<bool> syncVar____003CGate1Open_003Ek__BackingField;

		public SyncVar<bool> syncVar____003CGate2Open_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted;

		public ECheckpointState ActivationState { get; protected set; }

		//public bool Gate1Open
		//{
		//	[CompilerGenerated]
		//	get
		//	{
		//		return SyncAccessor__003CGate1Open_003Ek__BackingField;
		//	}
		//	[CompilerGenerated]
		//	protected set
		//	{
		//		syncVar____003CGate1Open_003Ek__BackingField.SetValue(value, true);
		//	}
		//}

        public bool Gate1Open
        {
            get
            {
                return _003CGate1Open_003Ek__BackingField;
            }
            protected set
            {
                _003CGate1Open_003Ek__BackingField = value;

                syncVar____003CGate1Open_003Ek__BackingField.SetValue(value, true);
            }
        }

        //      public bool Gate2Open
        //{
        //	[CompilerGenerated]
        //	get
        //	{
        //		return SyncAccessor__003CGate2Open_003Ek__BackingField;
        //	}
        //	[CompilerGenerated]
        //	protected set
        //	{
        //		syncVar____003CGate2Open_003Ek__BackingField.SetValue(value, true);
        //	}
        //}

        public bool Gate2Open
        {
            get
            {
                return _003CGate2Open_003Ek__BackingField;
            }
            protected set
            {
                _003CGate2Open_003Ek__BackingField = value;

                syncVar____003CGate2Open_003Ek__BackingField.SetValue(value, true);
            }
        }

        public bool SyncAccessor__003CGate1Open_003Ek__BackingField
		{
			get
			{
				return Gate1Open;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					Gate1Open = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CGate1Open_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		public bool SyncAccessor__003CGate2Open_003Ek__BackingField
		{
			get
			{
				return Gate2Open;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					Gate2Open = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CGate2Open_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EPolice_002ERoadCheckpoint_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected virtual void Update()
		{
			if (ActivationState != 0)
			{
				VehicleObstacle1.gameObject.SetActive(!SyncAccessor__003CGate1Open_003Ek__BackingField);
				VehicleObstacle2.gameObject.SetActive(!SyncAccessor__003CGate2Open_003Ek__BackingField);
				Stopper1.isActive = !SyncAccessor__003CGate1Open_003Ek__BackingField;
				Stopper2.isActive = !SyncAccessor__003CGate2Open_003Ek__BackingField;
			}
			if (ActivationState != appliedState)
			{
				ApplyState();
			}
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (OpenForNPCs)
			{
				if (NPCVehicleDetectionArea1.closestVehicle != null && NPCVehicleDetectionArea1.closestVehicle.OccupantNPCs[0] != null)
				{
					if (!SyncAccessor__003CGate1Open_003Ek__BackingField)
					{
						SetGate1Open(o: true);
					}
					if (!SyncAccessor__003CGate2Open_003Ek__BackingField)
					{
						SetGate2Open(o: true);
					}
				}
				if (NPCVehicleDetectionArea2.closestVehicle != null && NPCVehicleDetectionArea2.closestVehicle.OccupantNPCs[0] != null)
				{
					if (!SyncAccessor__003CGate1Open_003Ek__BackingField)
					{
						SetGate1Open(o: true);
					}
					if (!SyncAccessor__003CGate2Open_003Ek__BackingField)
					{
						SetGate2Open(o: true);
					}
				}
			}
			if (ActivationState != 0)
			{
				if (SyncAccessor__003CGate1Open_003Ek__BackingField)
				{
					timeSinceGate1Open += Time.deltaTime;
					if (ImmediateVehicleDetector.vehicles.Count > 0)
					{
						vehicleDetectedSinceGate1Open = true;
					}
					if (timeSinceGate1Open > 15f || (vehicleDetectedSinceGate1Open && ImmediateVehicleDetector.vehicles.Count == 0))
					{
						SetGate1Open(o: false);
					}
				}
				else
				{
					timeSinceGate1Open = 0f;
					vehicleDetectedSinceGate1Open = false;
				}
				if (SyncAccessor__003CGate2Open_003Ek__BackingField)
				{
					timeSinceGate2Open += Time.deltaTime;
					if (ImmediateVehicleDetector.vehicles.Count > 0)
					{
						vehicleDetectedSinceGate2Open = true;
					}
					if (timeSinceGate2Open > 15f || (vehicleDetectedSinceGate2Open && ImmediateVehicleDetector.vehicles.Count == 0))
					{
						SetGate2Open(o: false);
					}
				}
				else
				{
					timeSinceGate2Open = 0f;
					vehicleDetectedSinceGate2Open = false;
				}
			}
			else
			{
				timeSinceGate1Open = 0f;
				vehicleDetectedSinceGate1Open = false;
				timeSinceGate2Open = 0f;
				vehicleDetectedSinceGate2Open = false;
			}
		}

		protected virtual void ApplyState()
		{
			appliedState = ActivationState;
			if (ActivationState == ECheckpointState.Disabled)
			{
				container.SetActive(value: false);
			}
			else
			{
				container.SetActive(value: true);
			}
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void Enable(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_Enable_328543758(conn);
				RpcLogic___Enable_328543758(conn);
			}
			else
			{
				RpcWriter___Target_Enable_328543758(conn);
			}
		}

		[ObserversRpc(RunLocally = true)]
		public void Disable()
		{
			RpcWriter___Observers_Disable_2166136261();
			RpcLogic___Disable_2166136261();
		}

		public void SetGate1Open(bool o)
		{
			Gate1Open = o;
		}

		public void SetGate2Open(bool o)
		{
			Gate2Open = o;
		}

		private void ResetTrafficCones()
		{
			if (trafficConeOriginalTransforms.Count != 0)
			{
				for (int i = 0; i < TrafficCones.Length; i++)
				{
					TrafficCones[i].transform.position = trafficConeOriginalTransforms[TrafficCones[i]].Item1;
					TrafficCones[i].transform.rotation = trafficConeOriginalTransforms[TrafficCones[i]].Item2;
				}
			}
		}

		public void PlayerDetected(Player player)
		{
			if (onPlayerWalkThrough != null)
			{
				onPlayerWalkThrough.Invoke(player);
			}
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CGate2Open_003Ek__BackingField = new SyncVar<bool>(this, 1u, WritePermission.ServerOnly, ReadPermission.Observers, 0.25f, Channel.Reliable, Gate2Open);
				syncVar____003CGate1Open_003Ek__BackingField = new SyncVar<bool>(this, 0u, WritePermission.ServerOnly, ReadPermission.Observers, 0.25f, Channel.Reliable, Gate1Open);
				RegisterObserversRpc(0u, RpcReader___Observers_Enable_328543758);
				RegisterTargetRpc(1u, RpcReader___Target_Enable_328543758);
				RegisterObserversRpc(2u, RpcReader___Observers_Disable_2166136261);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EPolice_002ERoadCheckpoint);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CGate2Open_003Ek__BackingField.SetRegistered();
				syncVar____003CGate1Open_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_Enable_328543758(NetworkConnection conn)
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
				SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___Enable_328543758(NetworkConnection conn)
		{
			ResetTrafficCones();
			ActivationState = ECheckpointState.Enabled;
		}

		private void RpcReader___Observers_Enable_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Enable_328543758(null);
			}
		}

		private void RpcWriter___Target_Enable_328543758(NetworkConnection conn)
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
				SendTargetRpc(1u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_Enable_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___Enable_328543758(base.LocalConnection);
			}
		}

		private void RpcWriter___Observers_Disable_2166136261()
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
				SendObserversRpc(2u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___Disable_2166136261()
		{
			ActivationState = ECheckpointState.Disabled;
			if (InstanceFinder.IsServer)
			{
				for (int i = 0; i < AssignedNPCs.Count; i++)
				{
					(AssignedNPCs[i] as PoliceOfficer).UnassignFromCheckpoint();
				}
			}
		}

		private void RpcReader___Observers_Disable_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Disable_2166136261();
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EPolice_002ERoadCheckpoint(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
			case 1:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CGate2Open_003Ek__BackingField.SetValue(syncVar____003CGate2Open_003Ek__BackingField.GetValue(calledByUser: true), true);
					return true;
				}
				bool value2 = PooledReader0.ReadBoolean();
                        syncVar____003CGate2Open_003Ek__BackingField.SetValue(value2, Boolean2);
				return true;
			}
			case 0:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CGate1Open_003Ek__BackingField.SetValue(syncVar____003CGate1Open_003Ek__BackingField.GetValue(calledByUser: true), true);
					return true;
				}
				bool value = PooledReader0.ReadBoolean();
                syncVar____003CGate1Open_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			default:
				return false;
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EPolice_002ERoadCheckpoint_Assembly_002DCSharp_002Edll()
		{
			if (EnabledOnStart)
			{
				ActivationState = ECheckpointState.Enabled;
			}
			ApplyState();
			if (trafficConeOriginalTransforms.Count == 0)
			{
				for (int i = 0; i < TrafficCones.Length; i++)
				{
					trafficConeOriginalTransforms.Add(TrafficCones[i], new Tuple<Vector3, Quaternion>(TrafficCones[i].transform.position, TrafficCones[i].transform.rotation));
				}
			}
		}
	}
}
