using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Property.Utilities.Power;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.ConstructableScripts
{
	public class Constructable_GridBased : Constructable
	{
		[Header("Grid Based Constructable References")]
		public Transform buildPoint;

		public List<CoordinateFootprintTilePair> CoordinateFootprintTilePairs = new List<CoordinateFootprintTilePair>();

		public Transform ContentContainer;

		public ScheduleOne.Tiles.Grid[] Grids;

		public List<GameObject> roofObjectsForVisibility = new List<GameObject>();

		[Header("Power")]
		[SerializeField]
		protected bool AlwaysPowered;

		[SerializeField]
		protected PowerNode powerNode;

		[HideInInspector]
		public bool isGhost;

		protected bool dataChangedThisFrame;

		[SyncVar]
		public Guid OwnerGridGUID;

		[SyncVar]
		public Vector2 OriginCoordinate;

		[SyncVar]
		public float Rotation;

		public List<CoordinatePair> coordinatePairs = new List<CoordinatePair>();

		private Dictionary<GameObject, LayerMask> originalRoofLayers = new Dictionary<GameObject, LayerMask>();

		protected bool roofVisible = true;

		public SyncVar<Guid> syncVar___OwnerGridGUID;

		public SyncVar<Vector2> syncVar___OriginCoordinate;

		public SyncVar<float> syncVar___Rotation;

		private bool NetworkInitialize___EarlyScheduleOne_002EConstructableScripts_002EConstructable_GridBasedAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EConstructableScripts_002EConstructable_GridBasedAssembly_002DCSharp_002Edll_Excuted;

		public FootprintTile OriginFootprint => CoordinateFootprintTilePairs[0].footprintTile;

		public int FootprintX => CoordinateFootprintTilePairs[CoordinateFootprintTilePairs.Count - 1].coord.x + 1;

		public int FootprintY => CoordinateFootprintTilePairs[CoordinateFootprintTilePairs.Count - 1].coord.y + 1;

		public bool hasWaterSupply => true;

		public PowerNode PowerNode => powerNode;

		public bool isPowered
		{
			get
			{
				if (!AlwaysPowered)
				{
					return powerNode.isConnectedToPower;
				}
				return true;
			}
		}

		public ScheduleOne.Tiles.Grid OwnerGrid { get; protected set; }

		public Guid SyncAccessor_OwnerGridGUID
		{
			get
			{
				return OwnerGridGUID;
			}
			set
			{
				if (value != Guid.Empty || !base.IsServerInitialized)
				{
					OwnerGridGUID = value;
				}
				if (Application.isPlaying)
				{
					syncVar___OwnerGridGUID.SetValue(value, value != Guid.Empty);
				}
			}
		}

		public Vector2 SyncAccessor_OriginCoordinate
		{
			get
			{
				return OriginCoordinate;
			}
			set
			{
				if (value != Vector2.zero || !base.IsServerInitialized)
				{
					OriginCoordinate = value;
				}
				if (Application.isPlaying)
				{
					syncVar___OriginCoordinate.SetValue(value, value != Vector2.zero);
				}
			}
		}

		public float SyncAccessor_Rotation
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

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EConstructableScripts_002EConstructable_GridBased_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			Console.Log("On start server");
			GenerateGridGUIDs();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			Console.Log("On spawn server");
			if (!connection.IsLocalClient)
			{
				Console.Log("Sending thingys");
				SetGridGUIDs(connection, GetGridGUIDs());
			}
		}

		public override void OnStartNetwork()
		{
			base.OnStartNetwork();
			Console.Log("OnStartNetwork");
			ReceiveData();
		}

		public virtual void InitializeConstructable_GridBased(ScheduleOne.Tiles.Grid grid, Vector2 originCoordinate, float rotation)
		{
			SetData(grid.GUID, originCoordinate, rotation);
		}

		private void ReceiveData()
		{
			if (base.IsStatic)
			{
				return;
			}
			Console.Log("Constructable received data");
			OwnerGrid = GUIDManager.GetObject<ScheduleOne.Tiles.Grid>(SyncAccessor_OwnerGridGUID);
			bool flag = false;
			if (base.NetworkObject.IsSpawned)
			{
				SetParent(OwnerGrid.Container);
				flag = true;
			}
			List<CoordinatePair> list = Coordinate.BuildCoordinateMatches(new Coordinate(SyncAccessor_OriginCoordinate), FootprintX, FootprintY, SyncAccessor_Rotation);
			for (int i = 0; i < list.Count; i++)
			{
				if (OwnerGrid.GetTile(list[i].coord2) == null)
				{
					Console.LogError("InitializeConstructable_GridBased: grid does not contain tile at " + list[i].coord2);
					DestroyConstructable();
					return;
				}
			}
			ClearPositionData();
			coordinatePairs.AddRange(list);
			RefreshTransform();
			for (int j = 0; j < coordinatePairs.Count; j++)
			{
				OwnerGrid.GetTile(coordinatePairs[j].coord2).AddOccupant(this, GetFootprintTile(coordinatePairs[j].coord1));
			}
			if (!flag)
			{
				StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				yield return new WaitUntil(() => base.NetworkObject.IsSpawned);
				SetParent(OwnerGrid.Container);
			}
		}

		private void SetParent(Transform parent)
		{
			base.transform.SetParent(parent);
			ContentContainer.SetParent(parent);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		protected virtual void SetData(Guid gridGUID, Vector2 originCoordinate, float rotation)
		{
			RpcWriter___Server_SetData_810381718(gridGUID, originCoordinate, rotation);
			RpcLogic___SetData_810381718(gridGUID, originCoordinate, rotation);
		}

		public virtual void RepositionConstructable(Guid gridGUID, Vector2 originCoordinate, float rotation)
		{
			SetData(gridGUID, originCoordinate, rotation);
		}

		private void RefreshTransform()
		{
			base.transform.rotation = OwnerGrid.transform.rotation * (Quaternion.Inverse(buildPoint.transform.rotation) * base.transform.rotation);
			base.transform.Rotate(buildPoint.up, SyncAccessor_Rotation);
			base.transform.position = OwnerGrid.GetTile(coordinatePairs[0].coord2).transform.position - (OriginFootprint.transform.position - base.transform.position);
			ContentContainer.transform.position = base.transform.position;
			ContentContainer.transform.rotation = base.transform.rotation;
		}

		private void ClearPositionData()
		{
			for (int i = 0; i < coordinatePairs.Count; i++)
			{
				OwnerGrid.GetTile(coordinatePairs[i].coord2).RemoveOccupant(this, GetFootprintTile(coordinatePairs[i].coord1));
			}
			coordinatePairs.Clear();
		}

		public override void DestroyConstructable(bool callOnServer = true)
		{
            ScheduleOne.Tiles.Grid[] componentsInChildren = base.gameObject.GetComponentsInChildren<ScheduleOne.Tiles.Grid>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].DestroyGrid();
			}
			for (int j = 0; j < coordinatePairs.Count; j++)
			{
				OwnerGrid.GetTile(coordinatePairs[j].coord2).RemoveOccupant(this, GetFootprintTile(coordinatePairs[j].coord1));
			}
			base.DestroyConstructable(callOnServer);
		}

		private void GenerateGridGUIDs()
		{
			for (int i = 0; i < Grids.Length; i++)
			{
				((IGUIDRegisterable)Grids[i]).SetGUID(GUIDManager.GenerateUniqueGUID());
				Console.LogError("Generated GRID GUID: " + Grids[i].GUID.ToString());
			}
			Console.Log("Sending GRID GUIDs");
			SetGridGUIDs(null, GetGridGUIDs());
		}

		private string[] GetGridGUIDs()
		{
			string[] array = new string[Grids.Length];
			for (int i = 0; i < Grids.Length; i++)
			{
				array[i] = Grids[i].GUID.ToString();
			}
			return array;
		}

		[ObserversRpc]
		[TargetRpc]
		protected void SetGridGUIDs(NetworkConnection target, string[] guids)
		{
			if ((object)target == null)
			{
				RpcWriter___Observers_SetGridGUIDs_2890081366(target, guids);
			}
			else
			{
				RpcWriter___Target_SetGridGUIDs_2890081366(target, guids);
			}
		}

		public override void SetInvisible()
		{
			base.SetInvisible();
			if (PowerNode != null)
			{
				for (int i = 0; i < PowerNode.connections.Count; i++)
				{
					PowerNode.connections[i].SetVisible(v: false);
				}
			}
		}

		public override void RestoreVisibility()
		{
			base.RestoreVisibility();
			if (PowerNode != null)
			{
				for (int i = 0; i < PowerNode.connections.Count; i++)
				{
					PowerNode.connections[i].SetVisible(v: true);
				}
			}
		}

		public virtual void SetRoofVisible(bool vis)
		{
			if (roofVisible == vis)
			{
				return;
			}
			roofVisible = vis;
			if (roofVisible)
			{
				foreach (GameObject item in roofObjectsForVisibility)
				{
					if (originalRoofLayers.ContainsKey(item))
					{
						item.layer = originalRoofLayers[item];
					}
					else
					{
						item.layer = LayerMask.NameToLayer("Default");
					}
				}
				return;
			}
			foreach (GameObject item2 in roofObjectsForVisibility)
			{
				if (item2.gameObject.layer != LayerMask.NameToLayer("Default"))
				{
					if (originalRoofLayers.ContainsKey(item2))
					{
						originalRoofLayers[item2] = item2.layer;
					}
					else
					{
						originalRoofLayers.Add(item2, item2.layer);
					}
				}
				item2.layer = LayerMask.NameToLayer("Invisible");
			}
		}

		public void CalculateFootprintTileIntersections()
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

		public List<FootprintTile> GetFootprintTiles()
		{
			List<FootprintTile> list = new List<FootprintTile>();
			for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
			{
				list.Add(CoordinateFootprintTilePairs[i].footprintTile);
			}
			return list;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EConstructableScripts_002EConstructable_GridBasedAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EConstructableScripts_002EConstructable_GridBasedAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar___Rotation = new SyncVar<float>(this, 2u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, Rotation);
				syncVar___OriginCoordinate = new SyncVar<Vector2>(this, 1u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, OriginCoordinate);
				syncVar___OwnerGridGUID = new SyncVar<Guid>(this, 0u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, OwnerGridGUID);
				RegisterServerRpc(2u, RpcReader___Server_SetData_810381718);
				RegisterObserversRpc(3u, RpcReader___Observers_SetGridGUIDs_2890081366);
				RegisterTargetRpc(4u, RpcReader___Target_SetGridGUIDs_2890081366);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EConstructableScripts_002EConstructable_GridBased);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EConstructableScripts_002EConstructable_GridBasedAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EConstructableScripts_002EConstructable_GridBasedAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar___Rotation.SetRegistered();
				syncVar___OriginCoordinate.SetRegistered();
				syncVar___OwnerGridGUID.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SetData_810381718(Guid gridGUID, Vector2 originCoordinate, float rotation)
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
				writer.WriteGuidAllocated(gridGUID);
				writer.WriteVector2(originCoordinate);
				writer.WriteSingle(rotation);
				SendServerRpc(2u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___SetData_810381718(Guid gridGUID, Vector2 originCoordinate, float rotation)
		{
			Console.Log("SetData");
            ScheduleOne.Tiles.Grid @object = GUIDManager.GetObject<ScheduleOne.Tiles.Grid>(gridGUID);
			if (@object == null)
			{
				Console.LogError("InitializeConstructable_GridBased: grid is null");
				DestroyConstructable();
				return;
			}
			syncVar___OwnerGridGUID.SetValue(gridGUID, true);
			OwnerGrid = @object;
			syncVar___OriginCoordinate.SetValue(originCoordinate, true);
			syncVar___Rotation.SetValue(rotation, true);
		}

		private void RpcReader___Server_SetData_810381718(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			Guid gridGUID = PooledReader0.ReadGuid();
			Vector2 originCoordinate = PooledReader0.ReadVector2();
			float rotation = PooledReader0.ReadSingle();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetData_810381718(gridGUID, originCoordinate, rotation);
			}
		}

		private void RpcWriter___Observers_SetGridGUIDs_2890081366(NetworkConnection target, string[] guids)
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
				GeneratedWriters___Internal.Write___System_002EString_005B_005DFishNet_002ESerializing_002EGenerated(writer, guids);
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		protected void RpcLogic___SetGridGUIDs_2890081366(NetworkConnection target, string[] guids)
		{
			Console.Log("Setting GRID GUIDs");
			for (int i = 0; i < guids.Length; i++)
			{
				((IGUIDRegisterable)Grids[i]).SetGUID(new Guid(guids[i]));
			}
		}

		private void RpcReader___Observers_SetGridGUIDs_2890081366(PooledReader PooledReader0, Channel channel)
		{
			string[] guids = GeneratedReaders___Internal.Read___System_002EString_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___SetGridGUIDs_2890081366(null, guids);
			}
		}

		private void RpcWriter___Target_SetGridGUIDs_2890081366(NetworkConnection target, string[] guids)
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
				GeneratedWriters___Internal.Write___System_002EString_005B_005DFishNet_002ESerializing_002EGenerated(writer, guids);
				SendTargetRpc(4u, writer, channel, DataOrderType.Default, target, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetGridGUIDs_2890081366(PooledReader PooledReader0, Channel channel)
		{
			string[] guids = GeneratedReaders___Internal.Read___System_002EString_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___SetGridGUIDs_2890081366(base.LocalConnection, guids);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EConstructableScripts_002EConstructable_GridBased(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
			case 2:
			{
				if (PooledReader0 == null)
				{
					syncVar___Rotation.SetValue(syncVar___Rotation.GetValue(calledByUser: true), true);
					return true;
				}
				float value3 = PooledReader0.ReadSingle();
                syncVar___Rotation.SetValue(value3, Boolean2);
				return true;
			}
			case 1:
			{
				if (PooledReader0 == null)
				{
					syncVar___OriginCoordinate.SetValue(syncVar___OriginCoordinate.GetValue(calledByUser: true), true);
					return true;
				}
				Vector2 value2 = PooledReader0.ReadVector2();
				syncVar___OriginCoordinate.SetValue(value2, Boolean2);
				return true;
			}
			case 0:
			{
				if (PooledReader0 == null)
				{
					syncVar___OwnerGridGUID.SetValue(syncVar___OwnerGridGUID.GetValue(calledByUser: true), true);
					return true;
				}
				Guid value = PooledReader0.ReadGuid();
                syncVar___OwnerGridGUID.SetValue(value, Boolean2);
				return true;
			}
			default:
				return false;
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EConstructableScripts_002EConstructable_GridBased_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			_ = base.IsStatic;
			if (Grids.Length != GetComponentsInChildren<ScheduleOne.Tiles.Grid>().Length)
			{
				Console.LogWarning(base.gameObject.name + ": Grids array length does not match number of child grids! (Grids array length: " + Grids.Length + ", child grids: " + GetComponentsInChildren<ScheduleOne.Tiles.Grid>().Length + ")");
			}
		}
	}
}
