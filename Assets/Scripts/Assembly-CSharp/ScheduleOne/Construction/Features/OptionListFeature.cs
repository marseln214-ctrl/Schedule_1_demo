using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.UI.Construction.Features;
using UnityEngine;

namespace ScheduleOne.Construction.Features
{
	public abstract class OptionListFeature : Feature
	{
		[Header("Option list feature settings")]
		public int defaultOptionIndex;

		[SyncVar]
		public int ownedOptionIndex;

		public SyncVar<int> syncVar___ownedOptionIndex;

		private bool NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EOptionListFeatureAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EOptionListFeatureAssembly_002DCSharp_002Edll_Excuted;

		public int SyncAccessor_ownedOptionIndex
		{
			get
			{
				return ownedOptionIndex;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					ownedOptionIndex = value;
				}
				if (Application.isPlaying)
				{
					syncVar___ownedOptionIndex.SetValue(value, value != 0);
				}
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EConstruction_002EFeatures_002EOptionListFeature_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override FI_Base CreateInterface(Transform parent)
		{
			FI_OptionList component = Object.Instantiate(featureInterfacePrefab, parent).GetComponent<FI_OptionList>();
			component.Initialize(this, GetOptions());
			component.onSelectionChanged.AddListener(SelectOption);
			component.onSelectionPurchased.AddListener(PurchaseOption);
			return component;
		}

		public override void Default()
		{
			PurchaseOption(defaultOptionIndex);
		}

		protected abstract List<FI_OptionList.Option> GetOptions();

		public virtual void SelectOption(int optionIndex)
		{
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		protected virtual void SetData(int colorIndex)
		{
			RpcWriter___Server_SetData_3316948804(colorIndex);
			RpcLogic___SetData_3316948804(colorIndex);
		}

		private void ReceiveData()
		{
			SelectOption(SyncAccessor_ownedOptionIndex);
		}

		public virtual void PurchaseOption(int optionIndex)
		{
			SetData(optionIndex);
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EOptionListFeatureAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EOptionListFeatureAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar___ownedOptionIndex = new SyncVar<int>(this, 0u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, ownedOptionIndex);
				RegisterServerRpc(0u, RpcReader___Server_SetData_3316948804);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EConstruction_002EFeatures_002EOptionListFeature);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EOptionListFeatureAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EOptionListFeatureAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar___ownedOptionIndex.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SetData_3316948804(int colorIndex)
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
				writer.WriteInt32(colorIndex);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___SetData_3316948804(int colorIndex)
		{
			if (!base.IsSpawned)
			{
				SelectOption(colorIndex);
			}
			else
			{
                syncVar___ownedOptionIndex.SetValue(colorIndex, true);
			}
		}

		private void RpcReader___Server_SetData_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int colorIndex = PooledReader0.ReadInt32();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetData_3316948804(colorIndex);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EConstruction_002EFeatures_002EOptionListFeature(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 0)
			{
				if (PooledReader0 == null)
				{
                    syncVar___ownedOptionIndex.SetValue(syncVar___ownedOptionIndex.GetValue(calledByUser: true), true);
					return true;
				}
				int value = PooledReader0.ReadInt32();
                syncVar___ownedOptionIndex.SetValue(value, Boolean2);
				return true;
			}
			return false;
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EConstruction_002EFeatures_002EOptionListFeature_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
		}
	}
}
