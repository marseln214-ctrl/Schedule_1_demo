using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;

namespace ScheduleOne.Vehicles
{
	public class Forklift : LandVehicle
	{
		[Header("Forklift References")]
		[SerializeField]
		protected Transform steeringWheel;

		[SerializeField]
		protected Rigidbody forkRb;

		[SerializeField]
		protected ConfigurableJoint joint;

		[Header("Forklift settings")]
		[SerializeField]
		protected float steeringWheelAngleMultiplier = 2f;

		[SerializeField]
		protected float lift_MinY;

		[SerializeField]
		protected float lift_MaxY;

		[SerializeField]
		protected float liftMoveRate = 0.5f;

		[CompilerGenerated]
		[SyncVar(Channel = Channel.Unreliable)]
		public float _003CtargetForkHeight_003Ek__BackingField;

		private float lastFrameTargetForkHeight;

		[CompilerGenerated]
		[SyncVar(SendRate = 0.04f, Channel = Channel.Unreliable)]
		public float _003CactualForkHeight_003Ek__BackingField;

		public SyncVar<float> syncVar____003CtargetForkHeight_003Ek__BackingField;

		public SyncVar<float> syncVar____003CactualForkHeight_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EVehicles_002EForkliftAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EVehicles_002EForkliftAssembly_002DCSharp_002Edll_Excuted;

		public float targetForkHeight
		{
			[CompilerGenerated]
			get
			{
				return _003CtargetForkHeight_003Ek__BackingField;
			}
			[CompilerGenerated]
			[ServerRpc(RunLocally = true)]
			protected set
			{
				_003CtargetForkHeight_003Ek__BackingField = value;

                RpcWriter___Server_set_targetForkHeight_431000436(value);
				RpcLogic___set_targetForkHeight_431000436(value);
			}
		}

		public float actualForkHeight
		{
			[CompilerGenerated]
			get
			{
				return _003CactualForkHeight_003Ek__BackingField;
			}
			[CompilerGenerated]
			[ServerRpc(RunLocally = true)]
			protected set
			{
				_003CactualForkHeight_003Ek__BackingField = value;

                RpcWriter___Server_set_actualForkHeight_431000436(value);
				RpcLogic___set_actualForkHeight_431000436(value);
			}
		}

		public float SyncAccessor__003CtargetForkHeight_003Ek__BackingField
		{
			get
			{
				return targetForkHeight;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					targetForkHeight = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CtargetForkHeight_003Ek__BackingField.SetValue(value, value != 0);
				}
			}
		}

		public float SyncAccessor__003CactualForkHeight_003Ek__BackingField
		{
			get
			{
				return actualForkHeight;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					actualForkHeight = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CactualForkHeight_003Ek__BackingField.SetValue(value, value != 0);
				}
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EVehicles_002EForklift_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Update()
		{
			base.Update();
			if (base.localPlayerIsDriver)
			{
				targetForkHeight = lastFrameTargetForkHeight;
				int num = 0;
				if (Input.GetKey(KeyCode.UpArrow))
				{
					num++;
				}
				if (Input.GetKey(KeyCode.DownArrow))
				{
					num--;
				}
				targetForkHeight = Mathf.Clamp(SyncAccessor__003CtargetForkHeight_003Ek__BackingField + (float)num * Time.deltaTime * liftMoveRate, 0f, 1f);
			}
			lastFrameTargetForkHeight = SyncAccessor__003CtargetForkHeight_003Ek__BackingField;
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.IsOwner || (base.OwnerId == -1 && InstanceFinder.IsHost))
			{
				forkRb.isKinematic = false;
				joint.targetPosition = new Vector3(0f, Mathf.Lerp(lift_MinY, lift_MaxY, SyncAccessor__003CtargetForkHeight_003Ek__BackingField), 0f);
				Vector3 vector = forkRb.transform.position - base.transform.TransformPoint(joint.connectedAnchor);
				vector = base.transform.InverseTransformVector(vector);
				actualForkHeight = 1f - Mathf.InverseLerp(lift_MinY, lift_MaxY, vector.y);
			}
		}

		protected new virtual void LateUpdate()
		{
			if (!base.localPlayerIsDriver && (!InstanceFinder.IsHost || base.CurrentPlayerOccupancy > 0))
			{
				forkRb.isKinematic = true;
				forkRb.transform.position = base.transform.TransformPoint(joint.connectedAnchor + new Vector3(0f, 0f - Mathf.Lerp(lift_MinY, lift_MaxY, SyncAccessor__003CactualForkHeight_003Ek__BackingField), 0f));
				forkRb.transform.rotation = base.transform.rotation;
			}
			steeringWheel.localEulerAngles = new Vector3(0f, base.SyncAccessor_currentSteerAngle * steeringWheelAngleMultiplier, 0f);
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EVehicles_002EForkliftAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EVehicles_002EForkliftAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar____003CactualForkHeight_003Ek__BackingField = new SyncVar<float>(this, 4u, WritePermission.ServerOnly, ReadPermission.Observers, 0.04f, Channel.Unreliable, actualForkHeight);
				syncVar____003CtargetForkHeight_003Ek__BackingField = new SyncVar<float>(this, 3u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Unreliable, targetForkHeight);
				RegisterServerRpc(14u, RpcReader___Server_set_targetForkHeight_431000436);
				RegisterServerRpc(15u, RpcReader___Server_set_actualForkHeight_431000436);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EVehicles_002EForklift);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EVehicles_002EForkliftAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EVehicles_002EForkliftAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar____003CactualForkHeight_003Ek__BackingField.SetRegistered();
				syncVar____003CtargetForkHeight_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_set_targetForkHeight_431000436(float value)
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
			else if (!base.IsOwner)
			{
				NetworkManager networkManager2 = base.NetworkManager;
				if ((object)networkManager2 == null)
				{
					networkManager2 = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager2 != null)
				{
					networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because you are not the owner of this object. .");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteSingle(value);
				SendServerRpc(14u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		[SpecialName]
		protected void RpcLogic___set_targetForkHeight_431000436(float value)
		{
			syncVar____003CtargetForkHeight_003Ek__BackingField.SetValue(value,  true);
		}

		private void RpcReader___Server_set_targetForkHeight_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			float value = PooledReader0.ReadSingle();
			if (base.IsServerInitialized && OwnerMatches(conn) && !conn.IsLocalClient)
			{
				RpcLogic___set_targetForkHeight_431000436(value);
			}
		}

		private void RpcWriter___Server_set_actualForkHeight_431000436(float value)
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
			else if (!base.IsOwner)
			{
				NetworkManager networkManager2 = base.NetworkManager;
				if ((object)networkManager2 == null)
				{
					networkManager2 = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager2 != null)
				{
					networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because you are not the owner of this object. .");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteSingle(value);
				SendServerRpc(15u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		[SpecialName]
		protected void RpcLogic___set_actualForkHeight_431000436(float value)
		{
			syncVar____003CactualForkHeight_003Ek__BackingField.SetValue(value,  true);
		}

		private void RpcReader___Server_set_actualForkHeight_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			float value = PooledReader0.ReadSingle();
			if (base.IsServerInitialized && OwnerMatches(conn) && !conn.IsLocalClient)
			{
				RpcLogic___set_actualForkHeight_431000436(value);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EVehicles_002EForklift(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
			case 4:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CactualForkHeight_003Ek__BackingField.SetValue(syncVar____003CactualForkHeight_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				float value2 = PooledReader0.ReadSingle();
				syncVar____003CactualForkHeight_003Ek__BackingField.SetValue(value2, Boolean2);
				return true;
			}
			case 3:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CtargetForkHeight_003Ek__BackingField.SetValue(syncVar____003CtargetForkHeight_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				float value = PooledReader0.ReadSingle();
				syncVar____003CtargetForkHeight_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			default:
				return false;
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EVehicles_002EForklift_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			Vector3 position = forkRb.transform.position;
			Quaternion rotation = forkRb.transform.rotation;
			forkRb.transform.SetParent(null);
			forkRb.transform.position = position;
			forkRb.transform.rotation = rotation;
		}
	}
}
