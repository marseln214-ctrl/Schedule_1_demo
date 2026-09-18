using System;
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
	public class ColorFeature : Feature
	{
		[Serializable]
		public class NamedColor
		{
			public string colorName;

			public Color color;

			public float price = 100f;
		}

		[Serializable]
		public class SecondaryPaintTarget
		{
			public List<MeshRenderer> colorTargets = new List<MeshRenderer>();

			public float sChange;

			public float vChange;
		}

		[Header("References")]
		[SerializeField]
		protected List<MeshRenderer> colorTargets = new List<MeshRenderer>();

		[SerializeField]
		protected List<SecondaryPaintTarget> secondaryTargets = new List<SecondaryPaintTarget>();

		[Header("Color settings")]
		public List<NamedColor> colors = new List<NamedColor>();

		public int defaultColorIndex;

		[SyncVar]
		public int ownedColorIndex;

		public SyncVar<int> syncVar___ownedColorIndex;

		private bool NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EColorFeatureAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EColorFeatureAssembly_002DCSharp_002Edll_Excuted;

		public int SyncAccessor_ownedColorIndex
		{
			get
			{
				return ownedColorIndex;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					ownedColorIndex = value;
				}
				if (Application.isPlaying)
				{
					syncVar___ownedColorIndex.SetValue(value, value != 0);
				}
			}
		}

		public override FI_Base CreateInterface(Transform parent)
		{
			FI_ColorPicker obj = base.CreateInterface(parent) as FI_ColorPicker;
			obj.onSelectionChanged.AddListener(ApplyColor);
			obj.onSelectionPurchased.AddListener(BuyColor);
			return obj;
		}

		public override void Default()
		{
			BuyColor(colors[defaultColorIndex]);
		}

		private void ApplyColor(NamedColor color)
		{
			for (int i = 0; i < colorTargets.Count; i++)
			{
				colorTargets[i].material.color = color.color;
			}
			foreach (SecondaryPaintTarget secondaryTarget in secondaryTargets)
			{
				for (int j = 0; j < secondaryTarget.colorTargets.Count; j++)
				{
					secondaryTarget.colorTargets[j].material.color = ModifyColor(color.color, secondaryTarget.sChange, secondaryTarget.vChange);
				}
			}
		}

		public static Color ModifyColor(Color original, float sChange, float vChange)
		{
			Color.RGBToHSV(original, out var H, out var S, out var V);
			S = Mathf.Clamp(S + sChange / 100f, 0f, 1f);
			V = Mathf.Clamp(V + vChange / 100f, 0f, 1f);
			return Color.HSVToRGB(H, S, V);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		protected virtual void SetData(int colorIndex)
		{
			RpcWriter___Server_SetData_3316948804(colorIndex);
			RpcLogic___SetData_3316948804(colorIndex);
		}

		private void ReceiveData()
		{
			ApplyColor(colors[SyncAccessor_ownedColorIndex]);
		}

		private void BuyColor(NamedColor color)
		{
			SetData(colors.IndexOf(color));
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EColorFeatureAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EColorFeatureAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar___ownedColorIndex = new SyncVar<int>(this, 0u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, ownedColorIndex);
				RegisterServerRpc(0u, RpcReader___Server_SetData_3316948804);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EConstruction_002EFeatures_002EColorFeature);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EColorFeatureAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EColorFeatureAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar___ownedColorIndex.SetRegistered();
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
				ApplyColor(colors[colorIndex]);
			}
			else
			{
                syncVar___ownedColorIndex.SetValue(colorIndex, true);
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

		public virtual bool ReadSyncVar___ScheduleOne_002EConstruction_002EFeatures_002EColorFeature(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 0)
			{
				if (PooledReader0 == null)
				{
                    syncVar___ownedColorIndex.SetValue(syncVar___ownedColorIndex.GetValue(calledByUser: true), true);
					return true;
				}
				int value = PooledReader0.ReadInt32();
                syncVar___ownedColorIndex.SetValue(value, Boolean2);
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
