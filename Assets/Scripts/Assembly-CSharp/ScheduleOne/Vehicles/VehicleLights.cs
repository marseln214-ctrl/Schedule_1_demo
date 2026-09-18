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
using ScheduleOne.Vehicles.AI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Vehicles
{
	public class VehicleLights : NetworkBehaviour
	{
		public LandVehicle vehicle;

		[Header("Headlights")]
		public bool hasHeadLights;

		public MeshRenderer[] headLightMeshes;

		public OptimizedLight[] headLightSources;

		public Material headlightMat_On;

		public Material headLightMat_Off;

		[CompilerGenerated]
		[SyncVar(Channel = Channel.Unreliable, SendRate = 0.25f, WritePermissions = WritePermission.ClientUnsynchronized)]
		public bool _003CheadLightsOn_003Ek__BackingField;

		protected bool headLightsApplied;

		[Header("Brake lights")]
		public bool hasBrakeLights;

		public MeshRenderer[] brakeLightMeshes;

		public Light[] brakeLightSources;

		public Material brakeLightMat_On;

		public Material brakeLightMat_Off;

		public Material brakeLightMat_Ambient;

		protected bool brakeLightsOn;

		protected bool brakeLightsApplied = true;

		[Header("Reverse lights")]
		public bool hasReverseLights;

		public MeshRenderer[] reverseLightMeshes;

		public Light[] reverseLightSources;

		public Material reverseLightMat_On;

		public Material reverseLightMat_Off;

		protected bool reverseLightsOn;

		protected bool reverseLightsApplied = true;

		public UnityEvent onHeadlightsOn;

		public UnityEvent onHeadlightsOff;

		private List<bool> brakesAppliedHistory = new List<bool>();

		private VehicleAgent agent;

		public SyncVar<bool> syncVar____003CheadLightsOn_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted;

		public bool headLightsOn
		{
			[CompilerGenerated]
			get
			{
				return _003CheadLightsOn_003Ek__BackingField;
			}
			[CompilerGenerated]
			[ServerRpc(RunLocally = true, RequireOwnership = false)]
			set
			{
				_003CheadLightsOn_003Ek__BackingField = value;

                RpcWriter___Server_set_headLightsOn_1140765316(value);
				RpcLogic___set_headLightsOn_1140765316(value);
			}
		}

		public bool SyncAccessor__003CheadLightsOn_003Ek__BackingField
		{
			get
			{
				return headLightsOn;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					headLightsOn = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CheadLightsOn_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EVehicles_002EVehicleLights_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected virtual void Update()
		{
			if (!vehicle.localPlayerIsDriver || !hasHeadLights || !GameInput.GetButtonDown(GameInput.ButtonCode.ToggleLights))
			{
				return;
			}
			headLightsOn = !SyncAccessor__003CheadLightsOn_003Ek__BackingField;
			if (SyncAccessor__003CheadLightsOn_003Ek__BackingField)
			{
				if (onHeadlightsOn != null)
				{
					onHeadlightsOn.Invoke();
				}
			}
			else if (onHeadlightsOff != null)
			{
				onHeadlightsOff.Invoke();
			}
		}

		protected virtual void FixedUpdate()
		{
			reverseLightsOn = vehicle.isReversing;
			if (agent == null || !agent.AutoDriving)
			{
				brakeLightsOn = vehicle.brakesApplied;
				return;
			}
			brakesAppliedHistory.Add(vehicle.brakesApplied);
			if (brakesAppliedHistory.Count > 60)
			{
				brakesAppliedHistory.RemoveAt(0);
			}
			int num = 0;
			for (int i = 0; i < brakesAppliedHistory.Count; i++)
			{
				if (brakesAppliedHistory[i])
				{
					num++;
				}
			}
			brakeLightsOn = (float)num / (float)brakesAppliedHistory.Count > 0.2f;
		}

		protected virtual void LateUpdate()
		{
			if (hasHeadLights && SyncAccessor__003CheadLightsOn_003Ek__BackingField != headLightsApplied)
			{
				if (SyncAccessor__003CheadLightsOn_003Ek__BackingField)
				{
					headLightsApplied = true;
					for (int i = 0; i < headLightMeshes.Length; i++)
					{
						headLightMeshes[i].material = headlightMat_On;
					}
					for (int j = 0; j < headLightSources.Length; j++)
					{
						headLightSources[j].Enabled = true;
					}
				}
				else
				{
					headLightsApplied = false;
					for (int k = 0; k < headLightMeshes.Length; k++)
					{
						headLightMeshes[k].material = headLightMat_Off;
					}
					for (int l = 0; l < headLightSources.Length; l++)
					{
						headLightSources[l].Enabled = false;
					}
				}
			}
			if (hasBrakeLights && brakeLightsOn != brakeLightsApplied)
			{
				if (brakeLightsOn)
				{
					brakeLightsApplied = true;
					for (int m = 0; m < brakeLightMeshes.Length; m++)
					{
						brakeLightMeshes[m].material = brakeLightMat_On;
					}
					if (vehicle.localPlayerIsInVehicle)
					{
						for (int n = 0; n < brakeLightSources.Length; n++)
						{
							brakeLightSources[n].enabled = true;
						}
					}
				}
				else
				{
					brakeLightsApplied = false;
					for (int num = 0; num < brakeLightMeshes.Length; num++)
					{
						brakeLightMeshes[num].material = brakeLightMat_Off;
					}
					for (int num2 = 0; num2 < brakeLightSources.Length; num2++)
					{
						brakeLightSources[num2].enabled = false;
					}
				}
			}
			if (!hasReverseLights || reverseLightsOn == reverseLightsApplied)
			{
				return;
			}
			if (reverseLightsOn)
			{
				reverseLightsApplied = true;
				for (int num3 = 0; num3 < reverseLightMeshes.Length; num3++)
				{
					reverseLightMeshes[num3].material = reverseLightMat_On;
				}
				if (vehicle.localPlayerIsInVehicle)
				{
					for (int num4 = 0; num4 < reverseLightSources.Length; num4++)
					{
						reverseLightSources[num4].enabled = true;
					}
				}
			}
			else
			{
				reverseLightsApplied = false;
				for (int num5 = 0; num5 < reverseLightMeshes.Length; num5++)
				{
					reverseLightMeshes[num5].material = reverseLightMat_Off;
				}
				for (int num6 = 0; num6 < reverseLightSources.Length; num6++)
				{
					reverseLightSources[num6].enabled = false;
				}
			}
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CheadLightsOn_003Ek__BackingField = new SyncVar<bool>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, 0.25f, Channel.Unreliable, headLightsOn);
				RegisterServerRpc(0u, RpcReader___Server_set_headLightsOn_1140765316);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EVehicles_002EVehicleLights);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CheadLightsOn_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_set_headLightsOn_1140765316(bool value)
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
				writer.WriteBoolean(value);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		[SpecialName]
		public void RpcLogic___set_headLightsOn_1140765316(bool value)
		{
            syncVar____003CheadLightsOn_003Ek__BackingField.SetValue(value, true);
		}

		private void RpcReader___Server_set_headLightsOn_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			bool value = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___set_headLightsOn_1140765316(value);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EVehicles_002EVehicleLights(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 0)
			{
				if (PooledReader0 == null)
				{
					//this.sync___set_value__003CheadLightsOn_003Ek__BackingField(syncVar____003CheadLightsOn_003Ek__BackingField.GetValue(calledByUser: true), asServer: true);
                    syncVar____003CheadLightsOn_003Ek__BackingField.SetValue(syncVar____003CheadLightsOn_003Ek__BackingField.GetValue(calledByUser: true), true);
                    return true;
				}
				bool value = PooledReader0.ReadBoolean();
                syncVar____003CheadLightsOn_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			return false;
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EVehicles_002EVehicleLights_Assembly_002DCSharp_002Edll()
		{
			agent = GetComponent<VehicleAgent>();
		}
	}
}
