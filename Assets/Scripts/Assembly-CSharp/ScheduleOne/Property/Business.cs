using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Money;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Property
{
	public class Business : Property, ISaveable
	{
		public static List<Business> Businesses = new List<Business>();

		public static List<Business> UnownedBusinesses = new List<Business>();

		public static List<Business> OwnedBusinesses = new List<Business>();

		[Header("Settings")]
		public float LaunderCapacity = 1000f;

		public List<LaunderingOperation> LaunderingOperations = new List<LaunderingOperation>();

		public static Action<LaunderingOperation> onOperationStarted;

		public static Action<LaunderingOperation> onOperationFinished;

		private BusinessLoader loader = new BusinessLoader();

		private bool NetworkInitialize___EarlyScheduleOne_002EProperty_002EBusinessAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EProperty_002EBusinessAssembly_002DCSharp_002Edll_Excuted;

		public float currentLaunderTotal => LaunderingOperations.Sum((LaunderingOperation x) => x.amount);

		public float appliedLaunderLimit => LaunderCapacity - currentLaunderTotal;

		public new string SaveFileName => "Business";

		public new Loader Loader => loader;

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EProperty_002EBusiness_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Start()
		{
			base.Start();
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		protected override void OnDestroy()
		{
			Businesses.Remove(this);
			UnownedBusinesses.Remove(this);
			OwnedBusinesses.Remove(this);
			base.OnDestroy();
		}

		protected override void GetNetworth(MoneyManager.FloatContainer container)
		{
			base.GetNetworth(container);
			container.ChangeValue(currentLaunderTotal);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			for (int i = 0; i < LaunderingOperations.Count; i++)
			{
				ReceiveLaunderingOperation(connection, LaunderingOperations[i].amount, LaunderingOperations[i].minutesSinceStarted);
			}
		}

		protected virtual void MinPass()
		{
			for (int i = 0; i < LaunderingOperations.Count; i++)
			{
				LaunderingOperations[i].minutesSinceStarted++;
				if (LaunderingOperations[i].minutesSinceStarted >= LaunderingOperations[i].completionTime_Minutes)
				{
					CompleteOperation(LaunderingOperations[i]);
					i--;
				}
			}
		}

		public override string GetSaveString()
		{
			bool[] array = new bool[Switches.Count];
			for (int i = 0; i < Switches.Count; i++)
			{
				array[i] = Switches[i].isOn;
			}
			LaunderOperationData[] array2 = new LaunderOperationData[LaunderingOperations.Count];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = new LaunderOperationData(LaunderingOperations[j].amount, LaunderingOperations[j].minutesSinceStarted);
			}
			bool[] array3 = new bool[Toggleables.Count];
			for (int k = 0; k < Toggleables.Count; k++)
			{
				array3[k] = Toggleables[k].IsActivated;
			}
			return new BusinessData(propertyCode, base.IsOwned, array, array2, array3).GetJson();
		}

		public virtual void Load(BusinessData businessData, string containerPath)
		{
			if (businessData.IsOwned)
			{
				SetOwned();
			}
			for (int i = 0; i < businessData.LaunderingOperations.Length; i++)
			{
				StartLaunderingOperation(businessData.LaunderingOperations[i].Amount, businessData.LaunderingOperations[i].MinutesSinceStarted);
			}
		}

		protected override void RecieveOwned()
		{
			base.RecieveOwned();
			UnownedBusinesses.Remove(this);
			OwnedBusinesses.Add(this);
		}

		[ServerRpc(RequireOwnership = false)]
		public void StartLaunderingOperation(float amount, int minutesSinceStarted = 0)
		{
			RpcWriter___Server_StartLaunderingOperation_1481775633(amount, minutesSinceStarted);
		}

		[TargetRpc]
		[ObserversRpc]
		private void ReceiveLaunderingOperation(NetworkConnection conn, float amount, int minutesSinceStarted = 0)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_ReceiveLaunderingOperation_1001022388(conn, amount, minutesSinceStarted);
			}
			else
			{
				RpcWriter___Target_ReceiveLaunderingOperation_1001022388(conn, amount, minutesSinceStarted);
			}
		}

		protected void CompleteOperation(LaunderingOperation op)
		{
			if (InstanceFinder.IsServer)
			{
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Money laundering (" + propertyName + ")", op.amount, 1f, string.Empty);
				float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("LaunderingOperationsCompleted");
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("LaunderingOperationsCompleted", (value + 1f).ToString());
			}
			Singleton<NotificationsManager>.Instance.SendNotification(propertyName, "<color=#16F01C>" + MoneyManager.FormatAmount(op.amount) + "</color> Laundered", NetworkSingleton<MoneyManager>.Instance.LaunderingNotificationIcon);
			LaunderingOperations.Remove(op);
			base.HasChanged = true;
			if (onOperationFinished != null)
			{
				onOperationFinished(op);
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EProperty_002EBusinessAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EProperty_002EBusinessAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(5u, RpcReader___Server_StartLaunderingOperation_1481775633);
				RegisterTargetRpc(6u, RpcReader___Target_ReceiveLaunderingOperation_1001022388);
				RegisterObserversRpc(7u, RpcReader___Observers_ReceiveLaunderingOperation_1001022388);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EProperty_002EBusinessAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EProperty_002EBusinessAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_StartLaunderingOperation_1481775633(float amount, int minutesSinceStarted = 0)
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
				writer.WriteSingle(amount);
				writer.WriteInt32(minutesSinceStarted);
				SendServerRpc(5u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___StartLaunderingOperation_1481775633(float amount, int minutesSinceStarted = 0)
		{
			ReceiveLaunderingOperation(null, amount, minutesSinceStarted);
		}

		private void RpcReader___Server_StartLaunderingOperation_1481775633(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			float amount = PooledReader0.ReadSingle();
			int minutesSinceStarted = PooledReader0.ReadInt32();
			if (base.IsServerInitialized)
			{
				RpcLogic___StartLaunderingOperation_1481775633(amount, minutesSinceStarted);
			}
		}

		private void RpcWriter___Target_ReceiveLaunderingOperation_1001022388(NetworkConnection conn, float amount, int minutesSinceStarted = 0)
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
				writer.WriteSingle(amount);
				writer.WriteInt32(minutesSinceStarted);
				SendTargetRpc(6u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveLaunderingOperation_1001022388(NetworkConnection conn, float amount, int minutesSinceStarted = 0)
		{
			LaunderingOperation launderingOperation = new LaunderingOperation(this, amount, minutesSinceStarted);
			LaunderingOperations.Add(launderingOperation);
			base.HasChanged = true;
			if (onOperationStarted != null)
			{
				onOperationStarted(launderingOperation);
			}
		}

		private void RpcReader___Target_ReceiveLaunderingOperation_1001022388(PooledReader PooledReader0, Channel channel)
		{
			float amount = PooledReader0.ReadSingle();
			int minutesSinceStarted = PooledReader0.ReadInt32();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveLaunderingOperation_1001022388(base.LocalConnection, amount, minutesSinceStarted);
			}
		}

		private void RpcWriter___Observers_ReceiveLaunderingOperation_1001022388(NetworkConnection conn, float amount, int minutesSinceStarted = 0)
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
				writer.WriteSingle(amount);
				writer.WriteInt32(minutesSinceStarted);
				SendObserversRpc(7u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcReader___Observers_ReceiveLaunderingOperation_1001022388(PooledReader PooledReader0, Channel channel)
		{
			float amount = PooledReader0.ReadSingle();
			int minutesSinceStarted = PooledReader0.ReadInt32();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveLaunderingOperation_1001022388(null, amount, minutesSinceStarted);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EProperty_002EBusiness_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			Businesses.Add(this);
			UnownedBusinesses.Remove(this);
			UnownedBusinesses.Add(this);
		}
	}
}
