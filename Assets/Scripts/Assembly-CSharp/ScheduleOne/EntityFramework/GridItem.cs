using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.EntityFramework
{
	public class GridItem : BuildableItem
	{
		public enum EGridType
		{
			All = 0,
			IndoorOnly = 1,
			OutdoorOnly = 2
		}

		[Header("Grid item data")]
		public List<CoordinateFootprintTilePair> CoordinateFootprintTilePairs = new List<CoordinateFootprintTilePair>();

		public EGridType GridType;

		public Guid OwnerGridGUID;

		public Vector2 OriginCoordinate;

		public int Rotation;

		public List<CoordinatePair> CoordinatePairs = new List<CoordinatePair>();

		private bool NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted;

		public FootprintTile OriginFootprint => CoordinateFootprintTilePairs[0].footprintTile;

		public int FootprintX => CoordinateFootprintTilePairs.OrderByDescending((CoordinateFootprintTilePair c) => c.coord.x).FirstOrDefault().coord.x + 1;

		public int FootprintY => CoordinateFootprintTilePairs.OrderByDescending((CoordinateFootprintTilePair c) => c.coord.y).FirstOrDefault().coord.y + 1;

		public ScheduleOne.Tiles.Grid OwnerGrid { get; protected set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EEntityFramework_002EGridItem_Assembly_002DCSharp_002Edll();
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
				SendGridItemData(base.ItemInstance, OwnerGridGUID.ToString(), OriginCoordinate, Rotation, base.GUID.ToString());
			}
		}

		protected override void SendInitToClient(NetworkConnection conn)
		{
			InitializeGridItem(conn, base.ItemInstance, OwnerGridGUID.ToString(), OriginCoordinate, Rotation, base.GUID.ToString());
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendGridItemData(ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
		{
			RpcWriter___Server_SendGridItemData_2821640832(instance, gridGUID, originCoordinate, rotation, GUID);
		}

		[TargetRpc]
		[ObserversRpc(RunLocally = true)]
		public virtual void InitializeGridItem(NetworkConnection conn, ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_InitializeGridItem_1883577149(conn, instance, gridGUID, originCoordinate, rotation, GUID);
				RpcLogic___InitializeGridItem_1883577149(conn, instance, gridGUID, originCoordinate, rotation, GUID);
			}
			else
			{
				RpcWriter___Target_InitializeGridItem_1883577149(conn, instance, gridGUID, originCoordinate, rotation, GUID);
			}
		}

		public virtual void InitializeGridItem(ItemInstance instance, ScheduleOne.Tiles.Grid grid, Vector2 originCoordinate, int rotation, string GUID)
		{
			if (!base.Initialized)
			{
				base.InitializeBuildableItem(instance, GUID, GetProperty(grid.transform).PropertyCode);
				SetGridData(grid.GUID, originCoordinate, rotation);
			}
		}

		protected void SetGridData(Guid gridGUID, Vector2 originCoordinate, int rotation)
		{
            ScheduleOne.Tiles.Grid @object = GUIDManager.GetObject<ScheduleOne.Tiles.Grid>(gridGUID);
			if (@object == null)
			{
				Console.LogError("InitializeConstructable_GridBased: grid is null");
				DestroyItem();
				return;
			}
			OwnerGridGUID = gridGUID;
			OwnerGrid = @object;
			OriginCoordinate = originCoordinate;
			Rotation = rotation;
			ProcessGridData();
		}

		private void ProcessGridData()
		{
			OwnerGrid = GUIDManager.GetObject<ScheduleOne.Tiles.Grid>(OwnerGridGUID);
			if (OwnerGrid == null)
			{
				Console.LogWarning("GridItem OwnerGrid is null");
				return;
			}
			base.ParentProperty = GetProperty(OwnerGrid.transform);
			if (base.NetworkObject.IsSpawned)
			{
				base.transform.SetParent(OwnerGrid.Container);
			}
			else
			{
				StartCoroutine(Routine());
			}
			List<CoordinatePair> list = Coordinate.BuildCoordinateMatches(new Coordinate(OriginCoordinate), FootprintX, FootprintY, Rotation);
			for (int i = 0; i < list.Count; i++)
			{
				if (OwnerGrid.GetTile(list[i].coord2) == null)
				{
					Console.LogError("ReceiveData: grid does not contain tile at " + list[i].coord2);
					DestroyItem();
					return;
				}
			}
			ClearPositionData();
			CoordinatePairs.AddRange(list);
			RefreshTransform();
			for (int j = 0; j < CoordinatePairs.Count; j++)
			{
				OwnerGrid.GetTile(CoordinatePairs[j].coord2).AddOccupant(this, GetFootprintTile(CoordinatePairs[j].coord1));
				GetFootprintTile(CoordinatePairs[j].coord1).Initialize(OwnerGrid.GetTile(CoordinatePairs[j].coord2));
			}
			IEnumerator Routine()
			{
				yield return new WaitUntil(() => base.NetworkObject.IsSpawned);
				base.transform.SetParent(OwnerGrid.Container);
			}
		}

		private void RefreshTransform()
		{
			base.transform.rotation = OwnerGrid.transform.rotation * (Quaternion.Inverse(BuildPoint.transform.rotation) * base.transform.rotation);
			base.transform.Rotate(BuildPoint.up, Rotation);
			base.transform.position = OwnerGrid.GetTile(CoordinatePairs[0].coord2).transform.position - (OriginFootprint.transform.position - base.transform.position);
		}

		private void ClearPositionData()
		{
			if (OwnerGrid != null)
			{
				for (int i = 0; i < CoordinatePairs.Count; i++)
				{
					OwnerGrid.GetTile(CoordinatePairs[i].coord2).RemoveOccupant(this, GetFootprintTile(CoordinatePairs[i].coord1));
				}
			}
			CoordinatePairs.Clear();
		}

		public override void DestroyItem(bool callOnServer = true)
		{
			ClearPositionData();
			base.DestroyItem(callOnServer);
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

		public Tile GetParentTileAtFootprintCoordinate(Coordinate footprintCoord)
		{
			return OwnerGrid.GetTile(CoordinatePairs.Find((CoordinatePair x) => x.coord1 == footprintCoord).coord2);
		}

		public virtual bool CanShareTileWith(List<GridItem> obstacles)
		{
			for (int i = 0; i < obstacles.Count; i++)
			{
				if (!(obstacles[i] is FloorRack))
				{
					return false;
				}
			}
			return true;
		}

		public override string GetSaveString()
		{
			return new GridItemData(base.GUID, base.ItemInstance, 0, OwnerGrid, OriginCoordinate, Rotation).GetJson();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(5u, RpcReader___Server_SendGridItemData_2821640832);
				RegisterTargetRpc(6u, RpcReader___Target_InitializeGridItem_1883577149);
				RegisterObserversRpc(7u, RpcReader___Observers_InitializeGridItem_1883577149);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendGridItemData_2821640832(ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
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
				writer.WriteString(gridGUID);
				writer.WriteVector2(originCoordinate);
				writer.WriteInt32(rotation);
				writer.WriteString(GUID);
				SendServerRpc(5u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendGridItemData_2821640832(ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
		{
			InitializeGridItem(null, instance, gridGUID, originCoordinate, rotation, GUID);
		}

		private void RpcReader___Server_SendGridItemData_2821640832(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			ItemInstance instance = PooledReader0.ReadItemInstance();
			string gridGUID = PooledReader0.ReadString();
			Vector2 originCoordinate = PooledReader0.ReadVector2();
			int rotation = PooledReader0.ReadInt32();
			string gUID = PooledReader0.ReadString();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendGridItemData_2821640832(instance, gridGUID, originCoordinate, rotation, gUID);
			}
		}

		private void RpcWriter___Target_InitializeGridItem_1883577149(NetworkConnection conn, ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
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
				writer.WriteString(gridGUID);
				writer.WriteVector2(originCoordinate);
				writer.WriteInt32(rotation);
				writer.WriteString(GUID);
				SendTargetRpc(6u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		public virtual void RpcLogic___InitializeGridItem_1883577149(NetworkConnection conn, ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
		{
			InitializeGridItem(instance, GUIDManager.GetObject<ScheduleOne.Tiles.Grid>(new Guid(gridGUID)), originCoordinate, rotation, GUID);
		}

		private void RpcReader___Target_InitializeGridItem_1883577149(PooledReader PooledReader0, Channel channel)
		{
			ItemInstance instance = PooledReader0.ReadItemInstance();
			string gridGUID = PooledReader0.ReadString();
			Vector2 originCoordinate = PooledReader0.ReadVector2();
			int rotation = PooledReader0.ReadInt32();
			string gUID = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___InitializeGridItem_1883577149(base.LocalConnection, instance, gridGUID, originCoordinate, rotation, gUID);
			}
		}

		private void RpcWriter___Observers_InitializeGridItem_1883577149(NetworkConnection conn, ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
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
				writer.WriteString(gridGUID);
				writer.WriteVector2(originCoordinate);
				writer.WriteInt32(rotation);
				writer.WriteString(GUID);
				SendObserversRpc(7u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcReader___Observers_InitializeGridItem_1883577149(PooledReader PooledReader0, Channel channel)
		{
			ItemInstance instance = PooledReader0.ReadItemInstance();
			string gridGUID = PooledReader0.ReadString();
			Vector2 originCoordinate = PooledReader0.ReadVector2();
			int rotation = PooledReader0.ReadInt32();
			string gUID = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___InitializeGridItem_1883577149(null, instance, gridGUID, originCoordinate, rotation, gUID);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EEntityFramework_002EGridItem_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			BoundingCollider.isTrigger = true;
			BoundingCollider.gameObject.layer = LayerMask.NameToLayer("Invisible");
			SetFootprintTileVisiblity(visible: false);
		}
	}
}
