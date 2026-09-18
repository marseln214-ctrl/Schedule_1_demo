using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.StationFramework;
using UnityEngine;

namespace ScheduleOne.Management
{
	public class ConfigurationReplicator : NetworkBehaviour
	{
		public EntityConfiguration Configuration;

		private bool NetworkInitialize___EarlyScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted;

		public void ReplicateField(ConfigField field, NetworkConnection conn = null)
		{
			int num = Configuration.Fields.IndexOf(field);
			if (num == -1)
			{
				Console.LogError("Failed to find field in configuration");
			}
			else if (field is ItemField)
			{
				ItemField itemField = (ItemField)field;
				SendItemField(num, (itemField.SelectedItem != null) ? itemField.SelectedItem.name : string.Empty);
			}
			else if (field is NPCField)
			{
				NPCField nPCField = (NPCField)field;
				SendNPCField(num, (nPCField.SelectedNPC != null) ? nPCField.SelectedNPC.NetworkObject : null);
			}
			else if (field is ObjectField)
			{
				ObjectField objectField = (ObjectField)field;
				NetworkObject obj = null;
				if (objectField.SelectedObject != null)
				{
					obj = objectField.SelectedObject.NetworkObject;
				}
				SendObjectField(num, obj);
			}
			else if (field is ObjectListField)
			{
				ObjectListField objectListField = (ObjectListField)field;
				List<NetworkObject> list = new List<NetworkObject>();
				for (int i = 0; i < objectListField.SelectedObjects.Count; i++)
				{
					list.Add(objectListField.SelectedObjects[i].NetworkObject);
				}
				SendObjectListField(num, list);
			}
			else if (field is StationRecipeField)
			{
				StationRecipeField stationRecipeField = (StationRecipeField)field;
				int recipeIndex = -1;
				if (stationRecipeField.SelectedRecipe != null)
				{
					recipeIndex = stationRecipeField.Options.IndexOf(stationRecipeField.SelectedRecipe);
				}
				SendRecipeField(num, recipeIndex);
			}
			else if (field is NumberField)
			{
				NumberField numberField = (NumberField)field;
				SendNumberField(num, numberField.Value);
			}
			else
			{
				Console.LogError("Failed to find replication method for " + field.GetType());
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendItemField(int fieldIndex, string value)
		{
			RpcWriter___Server_SendItemField_2801973956(fieldIndex, value);
		}

		[ObserversRpc]
		private void ReceiveItemField(int fieldIndex, string value)
		{
			RpcWriter___Observers_ReceiveItemField_2801973956(fieldIndex, value);
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendNPCField(int fieldIndex, NetworkObject npcObject)
		{
			RpcWriter___Server_SendNPCField_1687693739(fieldIndex, npcObject);
		}

		[ObserversRpc]
		private void ReceiveNPCField(int fieldIndex, NetworkObject npcObject)
		{
			RpcWriter___Observers_ReceiveNPCField_1687693739(fieldIndex, npcObject);
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendObjectField(int fieldIndex, NetworkObject obj)
		{
			RpcWriter___Server_SendObjectField_1687693739(fieldIndex, obj);
		}

		[ObserversRpc]
		private void ReceiveObjectField(int fieldIndex, NetworkObject obj)
		{
			RpcWriter___Observers_ReceiveObjectField_1687693739(fieldIndex, obj);
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendObjectListField(int fieldIndex, List<NetworkObject> objects)
		{
			RpcWriter___Server_SendObjectListField_690244341(fieldIndex, objects);
		}

		[ObserversRpc]
		private void ReceiveObjectListField(int fieldIndex, List<NetworkObject> objects)
		{
			RpcWriter___Observers_ReceiveObjectListField_690244341(fieldIndex, objects);
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendRecipeField(int fieldIndex, int recipeIndex)
		{
			RpcWriter___Server_SendRecipeField_1692629761(fieldIndex, recipeIndex);
		}

		[ObserversRpc]
		private void ReceiveRecipeField(int fieldIndex, int recipeIndex)
		{
			RpcWriter___Observers_ReceiveRecipeField_1692629761(fieldIndex, recipeIndex);
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendNumberField(int fieldIndex, float value)
		{
			RpcWriter___Server_SendNumberField_1293284375(fieldIndex, value);
		}

		[ObserversRpc]
		private void ReceiveNumberField(int fieldIndex, float value)
		{
			RpcWriter___Observers_ReceiveNumberField_1293284375(fieldIndex, value);
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SendItemField_2801973956);
				RegisterObserversRpc(1u, RpcReader___Observers_ReceiveItemField_2801973956);
				RegisterServerRpc(2u, RpcReader___Server_SendNPCField_1687693739);
				RegisterObserversRpc(3u, RpcReader___Observers_ReceiveNPCField_1687693739);
				RegisterServerRpc(4u, RpcReader___Server_SendObjectField_1687693739);
				RegisterObserversRpc(5u, RpcReader___Observers_ReceiveObjectField_1687693739);
				RegisterServerRpc(6u, RpcReader___Server_SendObjectListField_690244341);
				RegisterObserversRpc(7u, RpcReader___Observers_ReceiveObjectListField_690244341);
				RegisterServerRpc(8u, RpcReader___Server_SendRecipeField_1692629761);
				RegisterObserversRpc(9u, RpcReader___Observers_ReceiveRecipeField_1692629761);
				RegisterServerRpc(10u, RpcReader___Server_SendNumberField_1293284375);
				RegisterObserversRpc(11u, RpcReader___Observers_ReceiveNumberField_1293284375);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendItemField_2801973956(int fieldIndex, string value)
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
				writer.WriteInt32(fieldIndex);
				writer.WriteString(value);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendItemField_2801973956(int fieldIndex, string value)
		{
			ReceiveItemField(fieldIndex, value);
		}

		private void RpcReader___Server_SendItemField_2801973956(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			string value = PooledReader0.ReadString();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendItemField_2801973956(fieldIndex, value);
			}
		}

		private void RpcWriter___Observers_ReceiveItemField_2801973956(int fieldIndex, string value)
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
				writer.WriteInt32(fieldIndex);
				writer.WriteString(value);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveItemField_2801973956(int fieldIndex, string value)
		{
			ItemField obj = Configuration.Fields[fieldIndex] as ItemField;
			ItemDefinition item = null;
			if (value != string.Empty)
			{
				item = Registry.GetItem(value);
			}
			obj.SetItem(item, network: false);
		}

		private void RpcReader___Observers_ReceiveItemField_2801973956(PooledReader PooledReader0, Channel channel)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			string value = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveItemField_2801973956(fieldIndex, value);
			}
		}

		private void RpcWriter___Server_SendNPCField_1687693739(int fieldIndex, NetworkObject npcObject)
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
				writer.WriteInt32(fieldIndex);
				writer.WriteNetworkObject(npcObject);
				SendServerRpc(2u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendNPCField_1687693739(int fieldIndex, NetworkObject npcObject)
		{
			ReceiveNPCField(fieldIndex, npcObject);
		}

		private void RpcReader___Server_SendNPCField_1687693739(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			NetworkObject npcObject = PooledReader0.ReadNetworkObject();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendNPCField_1687693739(fieldIndex, npcObject);
			}
		}

		private void RpcWriter___Observers_ReceiveNPCField_1687693739(int fieldIndex, NetworkObject npcObject)
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
				writer.WriteInt32(fieldIndex);
				writer.WriteNetworkObject(npcObject);
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveNPCField_1687693739(int fieldIndex, NetworkObject npcObject)
		{
			NPCField obj = Configuration.Fields[fieldIndex] as NPCField;
			NPC npc = null;
			if (npcObject != null)
			{
				npc = npcObject.GetComponent<NPC>();
			}
			obj.SetNPC(npc, network: false);
		}

		private void RpcReader___Observers_ReceiveNPCField_1687693739(PooledReader PooledReader0, Channel channel)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			NetworkObject npcObject = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveNPCField_1687693739(fieldIndex, npcObject);
			}
		}

		private void RpcWriter___Server_SendObjectField_1687693739(int fieldIndex, NetworkObject obj)
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
				writer.WriteInt32(fieldIndex);
				writer.WriteNetworkObject(obj);
				SendServerRpc(4u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendObjectField_1687693739(int fieldIndex, NetworkObject obj)
		{
			ReceiveObjectField(fieldIndex, obj);
		}

		private void RpcReader___Server_SendObjectField_1687693739(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			NetworkObject obj = PooledReader0.ReadNetworkObject();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendObjectField_1687693739(fieldIndex, obj);
			}
		}

		private void RpcWriter___Observers_ReceiveObjectField_1687693739(int fieldIndex, NetworkObject obj)
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
				writer.WriteInt32(fieldIndex);
				writer.WriteNetworkObject(obj);
				SendObserversRpc(5u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveObjectField_1687693739(int fieldIndex, NetworkObject obj)
		{
			ObjectField obj2 = Configuration.Fields[fieldIndex] as ObjectField;
			BuildableItem obj3 = null;
			if (obj != null)
			{
				obj3 = obj.GetComponent<BuildableItem>();
			}
			obj2.SetObject(obj3, network: false);
		}

		private void RpcReader___Observers_ReceiveObjectField_1687693739(PooledReader PooledReader0, Channel channel)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			NetworkObject obj = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveObjectField_1687693739(fieldIndex, obj);
			}
		}

		private void RpcWriter___Server_SendObjectListField_690244341(int fieldIndex, List<NetworkObject> objects)
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
				writer.WriteInt32(fieldIndex);
				GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerated(writer, objects);
				SendServerRpc(6u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendObjectListField_690244341(int fieldIndex, List<NetworkObject> objects)
		{
			ReceiveObjectListField(fieldIndex, objects);
		}

		private void RpcReader___Server_SendObjectListField_690244341(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			List<NetworkObject> objects = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsServerInitialized)
			{
				RpcLogic___SendObjectListField_690244341(fieldIndex, objects);
			}
		}

		private void RpcWriter___Observers_ReceiveObjectListField_690244341(int fieldIndex, List<NetworkObject> objects)
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
				writer.WriteInt32(fieldIndex);
				GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerated(writer, objects);
				SendObserversRpc(7u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveObjectListField_690244341(int fieldIndex, List<NetworkObject> objects)
		{
			ObjectListField objectListField = Configuration.Fields[fieldIndex] as ObjectListField;
			List<BuildableItem> list = new List<BuildableItem>();
			for (int i = 0; i < objects.Count; i++)
			{
				list.Add(objects[i].GetComponent<BuildableItem>());
			}
			objectListField.SetList(list, network: false);
		}

		private void RpcReader___Observers_ReceiveObjectListField_690244341(PooledReader PooledReader0, Channel channel)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			List<NetworkObject> objects = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveObjectListField_690244341(fieldIndex, objects);
			}
		}

		private void RpcWriter___Server_SendRecipeField_1692629761(int fieldIndex, int recipeIndex)
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
				writer.WriteInt32(fieldIndex);
				writer.WriteInt32(recipeIndex);
				SendServerRpc(8u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendRecipeField_1692629761(int fieldIndex, int recipeIndex)
		{
			ReceiveRecipeField(fieldIndex, recipeIndex);
		}

		private void RpcReader___Server_SendRecipeField_1692629761(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			int recipeIndex = PooledReader0.ReadInt32();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendRecipeField_1692629761(fieldIndex, recipeIndex);
			}
		}

		private void RpcWriter___Observers_ReceiveRecipeField_1692629761(int fieldIndex, int recipeIndex)
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
				writer.WriteInt32(fieldIndex);
				writer.WriteInt32(recipeIndex);
				SendObserversRpc(9u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveRecipeField_1692629761(int fieldIndex, int recipeIndex)
		{
			StationRecipeField stationRecipeField = Configuration.Fields[fieldIndex] as StationRecipeField;
			StationRecipe recipe = null;
			if (recipeIndex != -1)
			{
				recipe = stationRecipeField.Options[recipeIndex];
			}
			stationRecipeField.SetRecipe(recipe, network: false);
		}

		private void RpcReader___Observers_ReceiveRecipeField_1692629761(PooledReader PooledReader0, Channel channel)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			int recipeIndex = PooledReader0.ReadInt32();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveRecipeField_1692629761(fieldIndex, recipeIndex);
			}
		}

		private void RpcWriter___Server_SendNumberField_1293284375(int fieldIndex, float value)
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
				writer.WriteInt32(fieldIndex);
				writer.WriteSingle(value);
				SendServerRpc(10u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendNumberField_1293284375(int fieldIndex, float value)
		{
			ReceiveNumberField(fieldIndex, value);
		}

		private void RpcReader___Server_SendNumberField_1293284375(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			float value = PooledReader0.ReadSingle();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendNumberField_1293284375(fieldIndex, value);
			}
		}

		private void RpcWriter___Observers_ReceiveNumberField_1293284375(int fieldIndex, float value)
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
				writer.WriteInt32(fieldIndex);
				writer.WriteSingle(value);
				SendObserversRpc(11u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveNumberField_1293284375(int fieldIndex, float value)
		{
			(Configuration.Fields[fieldIndex] as NumberField).SetValue(value, network: false);
		}

		private void RpcReader___Observers_ReceiveNumberField_1293284375(PooledReader PooledReader0, Channel channel)
		{
			int fieldIndex = PooledReader0.ReadInt32();
			float value = PooledReader0.ReadSingle();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveNumberField_1293284375(fieldIndex, value);
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}
	}
}
