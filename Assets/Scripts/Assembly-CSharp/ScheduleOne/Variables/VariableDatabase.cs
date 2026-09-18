using System;
using System.Collections.Generic;
using System.IO;
using EasyButtons;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Variables
{
	public class VariableDatabase : NetworkSingleton<VariableDatabase>, IBaseSaveable, ISaveable
	{
		public enum EVariableType
		{
			Bool = 0,
			Number = 1
		}

		public List<BaseVariable> VariableList = new List<BaseVariable>();

		public Dictionary<string, BaseVariable> VariableDict = new Dictionary<string, BaseVariable>();

		private List<string> playerVariables = new List<string>();

		public VariableCreator[] Creators;

		public StorableItemDefinition[] ItemsToTrackAcquire;

		private VariablesLoader loader = new VariablesLoader();

		private bool NetworkInitialize___EarlyScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted;

		public string SaveFolderName => "Variables";

		public string SaveFileName => "Variables";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => true;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EVariables_002EVariableDatabase_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		private void CreateVariables()
		{
			for (int i = 0; i < Creators.Length; i++)
			{
				if (Creators[i].Mode == EVariableMode.Player)
				{
					playerVariables.Add(Creators[i].Name.ToLower());
				}
				else
				{
					CreateVariable(Creators[i].Name, Creators[i].Type, Creators[i].InitialValue, Creators[i].Persistent, EVariableMode.Global, null);
				}
			}
			SetVariableValue("IsDemo", true.ToString());
		}

		public void CreatePlayerVariables(Player owner)
		{
			for (int i = 0; i < Creators.Length; i++)
			{
				if (Creators[i].Mode == EVariableMode.Player)
				{
					CreateVariable(Creators[i].Name, Creators[i].Type, Creators[i].InitialValue, Creators[i].Persistent, EVariableMode.Player, owner, EVariableReplicationMode.Local);
				}
			}
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (connection.IsHost)
			{
				return;
			}
			for (int i = 0; i < VariableList.Count; i++)
			{
				if (VariableList[i].ReplicationMode != 0)
				{
					VariableList[i].ReplicateValue(connection);
				}
			}
		}

		public void CreateVariable(string name, EVariableType type, string initialValue, bool persistent, EVariableMode mode, Player owner, EVariableReplicationMode replicationMode = EVariableReplicationMode.Networked)
		{
			switch (type)
			{
			case EVariableType.Bool:
				new BoolVariable(name, replicationMode, persistent, mode, owner, initialValue == "true");
				break;
			case EVariableType.Number:
			{
				float result;
				float value = (float.TryParse(initialValue, out result) ? result : 0f);
				new NumberVariable(name, replicationMode, persistent, mode, owner, value);
				break;
			}
			}
		}

		public void AddVariable(BaseVariable variable)
		{
			if (VariableDict.ContainsKey(variable.Name))
			{
				Console.LogError("Variable with name " + variable.Name + " already exists in the database.");
				return;
			}
			VariableList.Add(variable);
			VariableDict.Add(variable.Name.ToLower(), variable);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendValue(NetworkConnection conn, string variableName, string value)
		{
			RpcWriter___Server_SendValue_3895153758(conn, variableName, value);
			RpcLogic___SendValue_3895153758(conn, variableName, value);
		}

		[ObserversRpc]
		[TargetRpc]
		public void ReceiveValue(NetworkConnection conn, string variableName, string value)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_ReceiveValue_3895153758(conn, variableName, value);
			}
			else
			{
				RpcWriter___Target_ReceiveValue_3895153758(conn, variableName, value);
			}
		}

		public void SetVariableValue(string variableName, string value, bool network = true)
		{
			variableName = variableName.ToLower();
			if (playerVariables.Contains(variableName))
			{
				Player.Local.SetVariableValue(variableName, value, network);
			}
			else if (VariableDict.ContainsKey(variableName))
			{
				VariableDict[variableName].SetValue(value, network);
			}
			else
			{
				Console.LogWarning("Failed to find variable with name: " + variableName);
			}
		}

		public BaseVariable GetVariable(string variableName)
		{
			variableName = variableName.ToLower();
			if (playerVariables.Contains(variableName))
			{
				return Player.Local.GetVariable(variableName);
			}
			if (VariableDict.ContainsKey(variableName))
			{
				return VariableDict[variableName];
			}
			Console.LogWarning("Failed to find variable with name: " + variableName);
			return null;
		}

		public T GetValue<T>(string variableName)
		{
			variableName = variableName.ToLower();
			if (playerVariables.Contains(variableName))
			{
				return Player.Local.GetValue<T>(variableName);
			}
			if (VariableDict.ContainsKey(variableName))
			{
				return (T)VariableDict[variableName].GetValue();
			}
			Console.LogError("Variable with name " + variableName + " does not exist in the database.");
			return default(T);
		}

		[Button]
		public void PrintAllVariables()
		{
			for (int i = 0; i < VariableList.Count; i++)
			{
				PrintVariableValue(VariableList[i].Name);
			}
		}

		public void PrintVariableValue(string variableName)
		{
			variableName = variableName.ToLower();
			if (VariableDict.ContainsKey(variableName))
			{
				Console.Log("Value of " + variableName + ": " + VariableDict[variableName].GetValue());
			}
			else
			{
				Console.LogError("Variable with name " + variableName + " does not exist in the database.");
			}
		}

		public void NotifyItemAcquired(string id, int quantity)
		{
			if (VariableDict.ContainsKey(id + "_acquired"))
			{
				float value = GetValue<float>(id + "_acquired");
				SetVariableValue(id + "_acquired", (value + (float)quantity).ToString());
			}
		}

		public virtual string GetSaveString()
		{
			return string.Empty;
		}

		public virtual List<string> WriteData(string parentFolderPath)
		{
			List<string> list = new List<string>();
			string containerFolder = ((ISaveable)this).GetContainerFolder(parentFolderPath);
			for (int i = 0; i < VariableList.Count; i++)
			{
				if (VariableList[i] != null && VariableList[i].Persistent && VariableList[i].VariableMode != EVariableMode.Player)
				{
					string json = new VariableData(VariableList[i].Name, VariableList[i].GetValue().ToString()).GetJson();
					string text = SaveManager.MakeFileSafe(VariableList[i].Name) + ".json";
					list.Add(text);
					string text2 = Path.Combine(containerFolder, text);
					try
					{
						File.WriteAllText(text2, json);
					}
					catch (Exception ex)
					{
						Console.LogWarning("Failed to write variable file: " + text2 + " - " + ex.Message);
					}
				}
			}
			return list;
		}

		public void Load(VariableData data)
		{
			if (playerVariables.Contains(data.Name.ToLower()))
			{
				Console.Log("Player variable: " + data.Name + " loaded from database. Redirecting to player.");
				Player.Local.SetVariableValue(data.Name, data.Value, network: false);
				return;
			}
			BaseVariable variable = GetVariable(data.Name);
			if (variable == null)
			{
				Console.LogWarning("Failed to find variable with name: " + data.Name);
			}
			else
			{
				variable.SetValue(data.Value);
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(0u, RpcReader___Server_SendValue_3895153758);
				RegisterObserversRpc(1u, RpcReader___Observers_ReceiveValue_3895153758);
				RegisterTargetRpc(2u, RpcReader___Target_ReceiveValue_3895153758);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendValue_3895153758(NetworkConnection conn, string variableName, string value)
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
				writer.WriteNetworkConnection(conn);
				writer.WriteString(variableName);
				writer.WriteString(value);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendValue_3895153758(NetworkConnection conn, string variableName, string value)
		{
			ReceiveValue(conn, variableName, value);
		}

		private void RpcReader___Server_SendValue_3895153758(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkConnection conn2 = PooledReader0.ReadNetworkConnection();
			string variableName = PooledReader0.ReadString();
			string value = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendValue_3895153758(conn2, variableName, value);
			}
		}

		private void RpcWriter___Observers_ReceiveValue_3895153758(NetworkConnection conn, string variableName, string value)
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
				writer.WriteString(variableName);
				writer.WriteString(value);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___ReceiveValue_3895153758(NetworkConnection conn, string variableName, string value)
		{
			variableName = variableName.ToLower();
			if (VariableDict.ContainsKey(variableName))
			{
				VariableDict[variableName].SetValue(value, replicate: false);
			}
		}

		private void RpcReader___Observers_ReceiveValue_3895153758(PooledReader PooledReader0, Channel channel)
		{
			string variableName = PooledReader0.ReadString();
			string value = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveValue_3895153758(null, variableName, value);
			}
		}

		private void RpcWriter___Target_ReceiveValue_3895153758(NetworkConnection conn, string variableName, string value)
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
				writer.WriteString(variableName);
				writer.WriteString(value);
				SendTargetRpc(2u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_ReceiveValue_3895153758(PooledReader PooledReader0, Channel channel)
		{
			string variableName = PooledReader0.ReadString();
			string value = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveValue_3895153758(base.LocalConnection, variableName, value);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EVariables_002EVariableDatabase_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			List<VariableCreator> list = new List<VariableCreator>(Creators);
			for (int i = 0; i < ItemsToTrackAcquire.Length; i++)
			{
				VariableCreator variableCreator = new VariableCreator();
				variableCreator.InitialValue = "0";
				variableCreator.Mode = EVariableMode.Global;
				variableCreator.Type = EVariableType.Number;
				variableCreator.Persistent = true;
				variableCreator.Name = ItemsToTrackAcquire[i].ID + "_acquired";
				list.Add(variableCreator);
			}
			Creators = list.ToArray();
			CreateVariables();
			InitializeSaveable();
		}
	}
}
