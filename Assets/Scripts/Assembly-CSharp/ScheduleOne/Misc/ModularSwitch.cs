using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Misc
{
	public class ModularSwitch : NetworkBehaviour
	{
		public delegate void ButtonChange(bool isOn);

		public bool isOn;

		[Header("References")]
		[SerializeField]
		protected InteractableObject intObj;

		[SerializeField]
		protected Transform button;

		public AudioSourceController OnAudio;

		public AudioSourceController OffAudio;

		[Header("Settings")]
		[SerializeField]
		protected List<ModularSwitch> SwitchesToSyncWith = new List<ModularSwitch>();

		public ButtonChange onToggled;

		public UnityEvent switchedOn;

		public UnityEvent switchedOff;

		private bool NetworkInitialize___EarlyScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted;

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EMisc_002EModularSwitch_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			SetIsOn(connection, isOn);
		}

		protected virtual void LateUpdate()
		{
			if (isOn)
			{
				button.localEulerAngles = new Vector3(-7f, 0f, 0f);
			}
			else
			{
				button.localEulerAngles = new Vector3(7f, 0f, 0f);
			}
		}

		public void Hovered()
		{
			if (isOn)
			{
				intObj.SetMessage("Switch off");
			}
			else
			{
				intObj.SetMessage("Switch on");
			}
		}

		public void Interacted()
		{
			if (isOn)
			{
				SendIsOn(isOn: false);
			}
			else
			{
				SendIsOn(isOn: true);
			}
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		private void SendIsOn(bool isOn)
		{
			RpcWriter___Server_SendIsOn_1140765316(isOn);
			RpcLogic___SendIsOn_1140765316(isOn);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void SetIsOn(NetworkConnection conn, bool isOn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetIsOn_214505783(conn, isOn);
				RpcLogic___SetIsOn_214505783(conn, isOn);
			}
			else
			{
				RpcWriter___Target_SetIsOn_214505783(conn, isOn);
			}
		}

		public void SwitchOn()
		{
			if (!isOn)
			{
				isOn = true;
				if (switchedOn != null)
				{
					switchedOn.Invoke();
				}
				if (onToggled != null)
				{
					onToggled(isOn);
				}
				for (int i = 0; i < SwitchesToSyncWith.Count; i++)
				{
					SwitchesToSyncWith[i].SwitchOn();
				}
				OnAudio.Play();
			}
		}

		public void SwitchOff()
		{
			if (isOn)
			{
				isOn = false;
				if (switchedOff != null)
				{
					switchedOff.Invoke();
				}
				if (onToggled != null)
				{
					onToggled(isOn);
				}
				for (int i = 0; i < SwitchesToSyncWith.Count; i++)
				{
					SwitchesToSyncWith[i].SwitchOff();
				}
				OffAudio.Play();
			}
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SendIsOn_1140765316);
				RegisterObserversRpc(1u, RpcReader___Observers_SetIsOn_214505783);
				RegisterTargetRpc(2u, RpcReader___Target_SetIsOn_214505783);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendIsOn_1140765316(bool isOn)
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
				writer.WriteBoolean(isOn);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendIsOn_1140765316(bool isOn)
		{
			SetIsOn(null, isOn);
		}

		private void RpcReader___Server_SendIsOn_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			bool flag = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendIsOn_1140765316(flag);
			}
		}

		private void RpcWriter___Observers_SetIsOn_214505783(NetworkConnection conn, bool isOn)
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
				writer.WriteBoolean(isOn);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetIsOn_214505783(NetworkConnection conn, bool isOn)
		{
			if (isOn)
			{
				SwitchOn();
			}
			else
			{
				SwitchOff();
			}
		}

		private void RpcReader___Observers_SetIsOn_214505783(PooledReader PooledReader0, Channel channel)
		{
			bool flag = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetIsOn_214505783(null, flag);
			}
		}

		private void RpcWriter___Target_SetIsOn_214505783(NetworkConnection conn, bool isOn)
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
				writer.WriteBoolean(isOn);
				SendTargetRpc(2u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetIsOn_214505783(PooledReader PooledReader0, Channel channel)
		{
			bool flag = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetIsOn_214505783(base.LocalConnection, flag);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EMisc_002EModularSwitch_Assembly_002DCSharp_002Edll()
		{
			for (int i = 0; i < SwitchesToSyncWith.Count; i++)
			{
				if (!SwitchesToSyncWith[i].SwitchesToSyncWith.Contains(this))
				{
					SwitchesToSyncWith[i].SwitchesToSyncWith.Add(this);
				}
			}
		}
	}
}
