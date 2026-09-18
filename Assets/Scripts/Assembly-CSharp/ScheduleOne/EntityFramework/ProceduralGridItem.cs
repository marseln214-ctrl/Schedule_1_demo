using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.EntityFramework
{
	public class ProceduralGridItem : BuildableItem
	{
		public class FootprintTileMatch
		{
			public FootprintTile footprint;

			public ProceduralTile matchedTile;
		}

		[Header("Grid item data")]
		public List<CoordinateFootprintTilePair> CoordinateFootprintTilePairs = new List<CoordinateFootprintTilePair>();

		public ProceduralTile.EProceduralTileType ProceduralTileType;

		[SyncVar]
		[HideInInspector]
		public int Rotation;

		[SyncVar]
		[HideInInspector]
		public List<CoordinateProceduralTilePair> footprintTileMatches = new List<CoordinateProceduralTilePair>();

		public SyncVar<int> syncVar___Rotation;

		public SyncVar<List<CoordinateProceduralTilePair>> syncVar___footprintTileMatches;

		private bool NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted;

		public int FootprintXSize => CoordinateFootprintTilePairs.OrderByDescending((CoordinateFootprintTilePair c) => c.coord.x).FirstOrDefault().coord.x + 1;

		public int FootprintYSize => CoordinateFootprintTilePairs.OrderByDescending((CoordinateFootprintTilePair c) => c.coord.y).FirstOrDefault().coord.y + 1;

		public int SyncAccessor_Rotation
		{
			get
			{
				return Rotation;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					Rotation = value;
				}
				if (Application.isPlaying)
				{
					syncVar___Rotation.SetValue(value, value != 0);
				}
			}
		}

		public List<CoordinateProceduralTilePair> SyncAccessor_footprintTileMatches
		{
			get
			{
				return footprintTileMatches;
			}
			set
			{
				if (value != null || !base.IsServerInitialized)
				{
					footprintTileMatches = value;
				}
				if (Application.isPlaying)
				{
					syncVar___footprintTileMatches.SetValue(value, value != null);
				}
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EEntityFramework_002EProceduralGridItem_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void OnStartClient()
		{
			base.OnStartClient();
			if (base.Initialized && base.LocallyBuilt)
			{
				StartCoroutine(WaitForDataSend());
			}
			IEnumerator WaitForDataSend()
			{
				yield return new WaitUntil(() => base.NetworkObject.IsSpawned);
				SendProceduralGridItemData(base.ItemInstance, SyncAccessor_Rotation, SyncAccessor_footprintTileMatches, base.GUID.ToString());
			}
		}

		protected override void SendInitToClient(NetworkConnection conn)
		{
			InitializeProceduralGridItem(conn, base.ItemInstance, SyncAccessor_Rotation, SyncAccessor_footprintTileMatches, base.GUID.ToString());
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendProceduralGridItemData(ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
		{
			RpcWriter___Server_SendProceduralGridItemData_638911643(instance, _rotation, _footprintTileMatches, GUID);
		}

		[TargetRpc]
		[ObserversRpc(RunLocally = true)]
		public virtual void InitializeProceduralGridItem(NetworkConnection conn, ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_InitializeProceduralGridItem_3164718044(conn, instance, _rotation, _footprintTileMatches, GUID);
				RpcLogic___InitializeProceduralGridItem_3164718044(conn, instance, _rotation, _footprintTileMatches, GUID);
			}
			else
			{
				RpcWriter___Target_InitializeProceduralGridItem_3164718044(conn, instance, _rotation, _footprintTileMatches, GUID);
			}
		}

		public virtual void InitializeProceduralGridItem(ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
		{
			if (_footprintTileMatches.Count == 0)
			{
				Console.LogError(base.gameObject.name + " initialized with zero footprint tile matches!");
				return;
			}
			SetProceduralGridData(_rotation, _footprintTileMatches);
			NetworkObject tileParent = _footprintTileMatches[0].tileParent;
			if (tileParent == null)
			{
				Console.LogError("Base object is null for " + base.gameObject.name);
				return;
			}
			ScheduleOne.Property.Property property = GetProperty(tileParent.transform);
			if (property == null)
			{
				Console.LogError("Failed to find property from base " + tileParent.gameObject.name);
			}
			else
			{
				base.InitializeBuildableItem(instance, GUID, property.PropertyCode);
			}
		}

		protected virtual void SetProceduralGridData(int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches)
		{
			syncVar___Rotation.SetValue(_rotation, true);
			syncVar___footprintTileMatches.SetValue(_footprintTileMatches, true);
			for (int i = 0; i < SyncAccessor_footprintTileMatches.Count; i++)
			{
				_footprintTileMatches[i].tile.AddOccupant(GetFootprintTile(SyncAccessor_footprintTileMatches[i].coord), this);
			}
			if (base.NetworkObject.IsSpawned)
			{
				base.transform.SetParent(SyncAccessor_footprintTileMatches[0].tile.ParentBuildableItem.transform.parent);
				RefreshTransform();
			}
			else
			{
				StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				yield return new WaitUntil(() => base.NetworkObject.IsSpawned);
				base.transform.SetParent(SyncAccessor_footprintTileMatches[0].tile.ParentBuildableItem.transform.parent);
				RefreshTransform();
			}
		}

		private void RefreshTransform()
		{
			ProceduralTile tile = SyncAccessor_footprintTileMatches[0].tile;
			base.transform.forward = tile.transform.forward;
			base.transform.Rotate(tile.transform.up, SyncAccessor_Rotation);
			base.transform.position = tile.transform.position - (GetFootprintTile(SyncAccessor_footprintTileMatches[0].coord).transform.position - base.transform.position);
		}

		private void ClearPositionData()
		{
			for (int i = 0; i < SyncAccessor_footprintTileMatches.Count; i++)
			{
				SyncAccessor_footprintTileMatches[i].tile.RemoveOccupant(GetFootprintTile(SyncAccessor_footprintTileMatches[i].coord), this);
			}
		}

		public override void DestroyItem(bool callOnServer = true)
		{
			ClearPositionData();
			base.DestroyItem(callOnServer);
		}

		protected override ScheduleOne.Property.Property GetProperty(Transform searchTransform = null)
		{
			if (searchTransform != null && searchTransform.GetComponent<GridItem>() != null)
			{
				return searchTransform.GetComponent<GridItem>().ParentProperty;
			}
			return base.GetProperty(searchTransform);
		}

		public virtual void CalculateFootprintTileIntersections()
		{
			for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
			{
				CoordinateFootprintTilePairs[i].footprintTile.tileDetector.CheckIntersections();
			}
		}

		public void SetFootprintTileVisiblity(bool visible)
		{
			for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
			{
				CoordinateFootprintTilePairs[i].footprintTile.tileAppearance.SetVisible(visible);
			}
		}

		public FootprintTile GetFootprintTile(Coordinate coord)
		{
			for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
			{
				if (CoordinateFootprintTilePairs[i].coord.Equals(coord))
				{
					return CoordinateFootprintTilePairs[i].footprintTile;
				}
			}
			return null;
		}

		public override string GetSaveString()
		{
			FootprintMatchData[] array = new FootprintMatchData[SyncAccessor_footprintTileMatches.Count];
			for (int i = 0; i < SyncAccessor_footprintTileMatches.Count; i++)
			{
				string tileOwnerGUID = ((IGUIDRegisterable)SyncAccessor_footprintTileMatches[i].tileParent.GetComponent<BuildableItem>()).GUID.ToString();
				int tileIndex = SyncAccessor_footprintTileMatches[i].tileIndex;
				Vector2 footprintCoordinate = new Vector2(SyncAccessor_footprintTileMatches[i].coord.x, SyncAccessor_footprintTileMatches[i].coord.y);
				array[i] = new FootprintMatchData(tileOwnerGUID, tileIndex, footprintCoordinate);
			}
			return new ProceduralGridItemData(base.GUID, base.ItemInstance, 50, SyncAccessor_Rotation, array).GetJson();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar___footprintTileMatches = new SyncVar<List<CoordinateProceduralTilePair>>(this, 1u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, footprintTileMatches);
				syncVar___Rotation = new SyncVar<int>(this, 0u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, Rotation);
				RegisterServerRpc(5u, RpcReader___Server_SendProceduralGridItemData_638911643);
				RegisterTargetRpc(6u, RpcReader___Target_InitializeProceduralGridItem_3164718044);
				RegisterObserversRpc(7u, RpcReader___Observers_InitializeProceduralGridItem_3164718044);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EEntityFramework_002EProceduralGridItem);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar___footprintTileMatches.SetRegistered();
				syncVar___Rotation.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendProceduralGridItemData_638911643(ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
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
				writer.WriteItemInstance(instance);
				writer.WriteInt32(_rotation);
				GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerated(writer, _footprintTileMatches);
				writer.WriteString(GUID);
				SendServerRpc(5u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendProceduralGridItemData_638911643(ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
		{
			InitializeProceduralGridItem(null, instance, _rotation, _footprintTileMatches, GUID);
		}

		private void RpcReader___Server_SendProceduralGridItemData_638911643(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			ItemInstance instance = PooledReader0.ReadItemInstance();
			int rotation = PooledReader0.ReadInt32();
			List<CoordinateProceduralTilePair> list = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds(PooledReader0);
			string gUID = PooledReader0.ReadString();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendProceduralGridItemData_638911643(instance, rotation, list, gUID);
			}
		}

		private void RpcWriter___Target_InitializeProceduralGridItem_3164718044(NetworkConnection conn, ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
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
				writer.WriteItemInstance(instance);
				writer.WriteInt32(_rotation);
				GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerated(writer, _footprintTileMatches);
				writer.WriteString(GUID);
				SendTargetRpc(6u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		public virtual void RpcLogic___InitializeProceduralGridItem_3164718044(NetworkConnection conn, ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
		{
			InitializeProceduralGridItem(instance, _rotation, _footprintTileMatches, GUID);
		}

		private void RpcReader___Target_InitializeProceduralGridItem_3164718044(PooledReader PooledReader0, Channel channel)
		{
			ItemInstance instance = PooledReader0.ReadItemInstance();
			int rotation = PooledReader0.ReadInt32();
			List<CoordinateProceduralTilePair> list = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds(PooledReader0);
			string gUID = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___InitializeProceduralGridItem_3164718044(base.LocalConnection, instance, rotation, list, gUID);
			}
		}

		private void RpcWriter___Observers_InitializeProceduralGridItem_3164718044(NetworkConnection conn, ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
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
				writer.WriteItemInstance(instance);
				writer.WriteInt32(_rotation);
				GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerated(writer, _footprintTileMatches);
				writer.WriteString(GUID);
				SendObserversRpc(7u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcReader___Observers_InitializeProceduralGridItem_3164718044(PooledReader PooledReader0, Channel channel)
		{
			ItemInstance instance = PooledReader0.ReadItemInstance();
			int rotation = PooledReader0.ReadInt32();
			List<CoordinateProceduralTilePair> list = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds(PooledReader0);
			string gUID = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___InitializeProceduralGridItem_3164718044(null, instance, rotation, list, gUID);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EEntityFramework_002EProceduralGridItem(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
			case 1:
			{
				if (PooledReader0 == null)
				{
					syncVar___footprintTileMatches.SetValue(syncVar___footprintTileMatches.GetValue(calledByUser: true), true);
					return true;
				}
				List<CoordinateProceduralTilePair> value2 = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds(PooledReader0);
                        syncVar___footprintTileMatches.SetValue(value2, Boolean2);
				return true;
			}
			case 0:
			{
				if (PooledReader0 == null)
				{
					syncVar___Rotation.SetValue(syncVar___Rotation.GetValue(calledByUser: true), true);
					return true;
				}
				int value = PooledReader0.ReadInt32();
                        syncVar___Rotation.SetValue(value, Boolean2);
				return true;
			}
			default:
				return false;
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EEntityFramework_002EProceduralGridItem_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			SetFootprintTileVisiblity(visible: false);
		}
	}
}
