using System;
using System.Collections.Generic;
using System.Linq;
using EPOOutline;
using FishNet;
using FishNet.Component.Ownership;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Building;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.EntityFramework
{
	[RequireComponent(typeof(PredictedSpawn))]
	public class BuildableItem : NetworkBehaviour, IGUIDRegisterable, ISaveable
	{
		public enum EOutlineColor
		{
			White = 0,
			Blue = 1,
			LightBlue = 2
		}

		[HideInInspector]
		public bool isGhost;

		[Header("Build Settings")]
		[SerializeField]
		protected GameObject buildHandler;

		public float HoldDistance = 2.5f;

		public Transform BuildPoint;

		public Transform MidAirCenterPoint;

		public BoxCollider BoundingCollider;

		[Header("Outline settings")]
		[SerializeField]
		protected List<GameObject> OutlineRenderers = new List<GameObject>();

		[SerializeField]
		protected bool IncludeOutlineRendererChildren = true;

		protected Outlinable OutlineEffect;

		[Header("Buildable Events")]
		public UnityEvent onInitialized;

		public UnityEvent onDestroyed;

		private bool NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted;

		public ItemInstance ItemInstance { get; protected set; }

		public ScheduleOne.Property.Property ParentProperty { get; protected set; }

		public bool IsDestroyed { get; protected set; }

		public bool Initialized { get; protected set; }

		public Guid GUID { get; protected set; }

		public GameObject BuildHandler => buildHandler;

		public bool LocallyBuilt { get; protected set; }

		public string SaveFolderName => ItemInstance.ID + "_" + GUID.ToString().Substring(0, 6);

		public string SaveFileName => "Data";

		public Loader Loader => null;

		public bool ShouldSaveUnderFolder => true;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		public void SetLocallyBuilt()
		{
			LocallyBuilt = true;
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EEntityFramework_002EBuildableItem_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected virtual void Start()
		{
			if (!isGhost)
			{
				InitializeSaveable();
				if (GUID == Guid.Empty)
				{
					GUID = GUIDManager.GenerateUniqueGUID();
					GUIDManager.RegisterObject(this);
				}
				ActivateDuringBuild[] componentsInChildren = base.transform.GetComponentsInChildren<ActivateDuringBuild>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].gameObject.SetActive(value: false);
				}
			}
		}

		protected virtual ScheduleOne.Property.Property GetProperty(Transform searchTransform = null)
		{
			if (searchTransform == null)
			{
				searchTransform = base.transform;
			}
			PropertyContentsContainer componentInParent = searchTransform.GetComponentInParent<PropertyContentsContainer>();
			if (componentInParent != null)
			{
				return componentInParent.Property;
			}
			return searchTransform.GetComponentInParent<ScheduleOne.Property.Property>();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (!connection.IsLocalClient && Initialized)
			{
				SendInitToClient(connection);
			}
		}

		protected virtual void SendInitToClient(NetworkConnection conn)
		{
			Console.Log("Sending BuildableItem init to client");
			ReceiveBuildableItemData(conn, ItemInstance, GUID.ToString(), ParentProperty.PropertyCode);
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendBuildableItemData(ItemInstance instance, string GUID, string parentPropertyCode)
		{
			RpcWriter___Server_SendBuildableItemData_3537728543(instance, GUID, parentPropertyCode);
		}

		[ObserversRpc]
		[TargetRpc]
		public void ReceiveBuildableItemData(NetworkConnection conn, ItemInstance instance, string GUID, string parentPropertyCode)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_ReceiveBuildableItemData_3859851844(conn, instance, GUID, parentPropertyCode);
			}
			else
			{
				RpcWriter___Target_ReceiveBuildableItemData_3859851844(conn, instance, GUID, parentPropertyCode);
			}
		}

		public virtual void InitializeBuildableItem(ItemInstance instance, string GUID, string parentPropertyCode)
		{
			if (Initialized)
			{
				return;
			}
			if (instance == null)
			{
				Console.LogError("InitializeBuildItem: passed null instance");
			}
			if (instance.Quantity != 1)
			{
				Console.LogWarning("BuiltadlbeItem initialized with quantity '" + instance.Quantity + "'! This should be 1.");
			}
			Initialized = true;
			ItemInstance = instance;
			SetGUID(new Guid(GUID));
			ParentProperty = ScheduleOne.Property.Property.Properties.FirstOrDefault((ScheduleOne.Property.Property p) => p.PropertyCode == parentPropertyCode);
			if (ParentProperty == null)
			{
				ParentProperty = Business.Businesses.FirstOrDefault((Business b) => b.PropertyCode == parentPropertyCode);
			}
			if (ParentProperty != null)
			{
				ParentProperty.BuildableItems.Add(this);
			}
			else
			{
				Console.LogError("BuildableItem '" + base.gameObject.name + "' does not have a parent Property!");
			}
			ActivateDuringBuild[] componentsInChildren = base.transform.GetComponentsInChildren<ActivateDuringBuild>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.SetActive(value: false);
			}
			if (onInitialized != null)
			{
				onInitialized.Invoke();
			}
		}

		public bool CanBePickedUp(out string reason)
		{
			if (PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(ItemInstance))
			{
				return CanBeDestroyed(out reason);
			}
			reason = "Item won't fit in inventory";
			return false;
		}

		public virtual bool CanBeDestroyed(out string reason)
		{
			reason = string.Empty;
			return true;
		}

		public virtual void PickupItem()
		{
			string reason = string.Empty;
			if (!CanBePickedUp(out reason))
			{
				Console.LogWarning("Item can not be picked up!");
				return;
			}
			PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(ItemInstance);
			DestroyItem();
		}

		public virtual void DestroyItem(bool callOnServer = true)
		{
			if (!IsDestroyed)
			{
				IsDestroyed = true;
				if (callOnServer)
				{
					Destroy_Networked();
				}
				if (ParentProperty != null)
				{
					ParentProperty.BuildableItems.Remove(this);
				}
				if (onDestroyed != null)
				{
					onDestroyed.Invoke();
				}
				base.gameObject.SetActive(value: false);
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void Destroy_Networked()
		{
			RpcWriter___Server_Destroy_Networked_2166136261();
		}

		[ObserversRpc]
		private void DestroyItemWrapper()
		{
			RpcWriter___Observers_DestroyItemWrapper_2166136261();
		}

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
		}

		public static Color32 GetColorFromOutlineColorEnum(EOutlineColor col)
		{
			return col switch
			{
				EOutlineColor.White => Color.white, 
				EOutlineColor.Blue => new Color32(0, 200, byte.MaxValue, byte.MaxValue), 
				EOutlineColor.LightBlue => new Color32(120, 225, byte.MaxValue, byte.MaxValue), 
				_ => Color.white, 
			};
		}

		public virtual void ShowOutline(Color color)
		{
			if (OutlineEffect == null)
			{
				OutlineEffect = base.gameObject.AddComponent<Outlinable>();
				OutlineEffect.OutlineParameters.BlurShift = 0f;
				OutlineEffect.OutlineParameters.DilateShift = 0.5f;
				OutlineEffect.OutlineParameters.FillPass.Shader = Resources.Load<Shader>("Easy performant outline/Shaders/Fills/ColorFill");
				foreach (GameObject outlineRenderer in OutlineRenderers)
				{
					MeshRenderer[] array = new MeshRenderer[0];
					array = ((!IncludeOutlineRendererChildren) ? new MeshRenderer[1] { outlineRenderer.GetComponent<MeshRenderer>() } : outlineRenderer.GetComponentsInChildren<MeshRenderer>());
					for (int i = 0; i < array.Length; i++)
					{
						OutlineTarget target = new OutlineTarget(array[i]);
						OutlineEffect.TryAddTarget(target);
					}
				}
			}
			OutlineEffect.OutlineParameters.Color = color;
			Color32 color2 = color;
			color2.a = 9;
			OutlineEffect.OutlineParameters.FillPass.SetColor("_PublicColor", color2);
			OutlineEffect.enabled = true;
		}

		public void ShowOutline(EOutlineColor color)
		{
			ShowOutline(GetColorFromOutlineColorEnum(color));
		}

		public virtual void HideOutline()
		{
			if (OutlineEffect != null)
			{
				OutlineEffect.enabled = false;
			}
		}

		public Vector3 GetFurthestPointFromBoundingCollider(Vector3 pos)
		{
			Vector3[] array = new Vector3[8];
			BoxCollider boundingCollider = BoundingCollider;
			array[0] = BoundingCollider.transform.TransformPoint(boundingCollider.center + new Vector3(boundingCollider.size.x, 0f - boundingCollider.size.y, boundingCollider.size.z) * 0.5f);
			array[1] = BoundingCollider.transform.TransformPoint(boundingCollider.center + new Vector3(0f - boundingCollider.size.x, 0f - boundingCollider.size.y, boundingCollider.size.z) * 0.5f);
			array[2] = BoundingCollider.transform.TransformPoint(boundingCollider.center + new Vector3(0f - boundingCollider.size.x, 0f - boundingCollider.size.y, 0f - boundingCollider.size.z) * 0.5f);
			array[3] = BoundingCollider.transform.TransformPoint(boundingCollider.center + new Vector3(boundingCollider.size.x, 0f - boundingCollider.size.y, 0f - boundingCollider.size.z) * 0.5f);
			array[4] = BoundingCollider.transform.TransformPoint(boundingCollider.center + new Vector3(boundingCollider.size.x, boundingCollider.size.y, boundingCollider.size.z) * 0.5f);
			array[5] = BoundingCollider.transform.TransformPoint(boundingCollider.center + new Vector3(0f - boundingCollider.size.x, boundingCollider.size.y, boundingCollider.size.z) * 0.5f);
			array[6] = BoundingCollider.transform.TransformPoint(boundingCollider.center + new Vector3(0f - boundingCollider.size.x, boundingCollider.size.y, 0f - boundingCollider.size.z) * 0.5f);
			array[7] = BoundingCollider.transform.TransformPoint(boundingCollider.center + new Vector3(boundingCollider.size.x, boundingCollider.size.y, 0f - boundingCollider.size.z) * 0.5f);
			List<Vector3> list = new List<Vector3>();
			Vector3[] array2 = array;
			foreach (Vector3 vector in array2)
			{
				if (list.Count == 0)
				{
					list.Add(vector);
				}
				else if (Vector3.Distance(pos, vector) > Vector3.Distance(pos, list[0]))
				{
					list.Clear();
					list.Add(vector);
				}
				else if (Mathf.Abs(Vector3.Distance(pos, vector) - Vector3.Distance(pos, list[0])) < 1E-06f)
				{
					list.Add(vector);
				}
			}
			Vector3 zero = Vector3.zero;
			for (int j = 0; j < list.Count; j++)
			{
				zero += list[j];
			}
			return zero / list.Count;
		}

		public bool GetPenetration(out float x, out float z, out float y)
		{
			Vector3 vector = BoundingCollider.transform.TransformPoint(BoundingCollider.center);
			float num = BoundingCollider.size.x / 2f;
			float num2 = 0f;
			x = 0f;
			z = 0f;
			y = 0f;
			Vector3 vector2 = vector - base.transform.right * num;
			if (HasLoS_IgnoreBuildables(vector2) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(vector2, base.transform.right, BoundingCollider.size.x / 2f + num - num2, out var hit, 1 << LayerMask.NameToLayer("Default"), includeTriggers: false, num2, 45f) && Vector3.Angle(base.transform.right, -hit.normal) < 5f)
			{
				x = BoundingCollider.size.x - Vector3.Distance(vector2, hit.point);
				Debug.DrawLine(vector - base.transform.right * num, hit.point, Color.green);
			}
			vector2 = vector + base.transform.right * num;
			if (HasLoS_IgnoreBuildables(vector2) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(vector2, -base.transform.right, BoundingCollider.size.x / 2f + num - num2, out hit, 1 << LayerMask.NameToLayer("Default"), includeTriggers: false, num2, 45f) && Vector3.Angle(-base.transform.right, -hit.normal) < 5f)
			{
				float num3 = 0f - (BoundingCollider.size.x - Vector3.Distance(vector2, hit.point));
				x = num3;
				Debug.DrawLine(vector + base.transform.right * num, hit.point, Color.red);
			}
			num = BoundingCollider.size.z / 2f;
			vector2 = vector - base.transform.forward * num;
			if (HasLoS_IgnoreBuildables(vector2) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(vector2, base.transform.forward, BoundingCollider.size.z / 2f + num - num2, out hit, 1 << LayerMask.NameToLayer("Default"), includeTriggers: false, num2, 45f) && Vector3.Angle(base.transform.forward, -hit.normal) < 5f)
			{
				z = BoundingCollider.size.z - Vector3.Distance(vector2, hit.point);
				Debug.DrawLine(vector - base.transform.forward * num, hit.point, Color.cyan);
			}
			vector2 = vector + base.transform.forward * num;
			if (HasLoS_IgnoreBuildables(vector2) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(vector2, -base.transform.forward, BoundingCollider.size.z / 2f + num - num2, out hit, 1 << LayerMask.NameToLayer("Default"), includeTriggers: false, num2, 45f) && Vector3.Angle(-base.transform.forward, -hit.normal) < 5f)
			{
				float num4 = 0f - (BoundingCollider.size.z - Vector3.Distance(vector2, hit.point));
				z = num4;
				Debug.DrawLine(vector + base.transform.forward * num, hit.point, Color.yellow);
			}
			num = BoundingCollider.size.y / 2f;
			vector2 = vector - base.transform.up * num;
			if (HasLoS_IgnoreBuildables(vector2) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(vector2, base.transform.up, BoundingCollider.size.y / 2f + num - num2, out hit, 1 << LayerMask.NameToLayer("Default"), includeTriggers: false, num2, 45f) && Vector3.Angle(base.transform.forward, -hit.normal) < 5f)
			{
				y = BoundingCollider.size.y - Vector3.Distance(vector2, hit.point);
				Debug.DrawLine(vector - base.transform.up * num, hit.point, Color.cyan);
			}
			vector2 = vector + base.transform.up * num;
			if (HasLoS_IgnoreBuildables(vector2) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(vector2, -base.transform.up, BoundingCollider.size.y / 2f + num - num2, out hit, 1 << LayerMask.NameToLayer("Default"), includeTriggers: false, num2, 45f) && Vector3.Angle(-base.transform.up, -hit.normal) < 5f)
			{
				float num5 = 0f - (BoundingCollider.size.y - Vector3.Distance(vector2, hit.point));
				y = num5;
				Debug.DrawLine(vector + base.transform.up * num, hit.point, Color.yellow);
			}
			if (x != 0f || z != 0f || y != 0f)
			{
				return true;
			}
			return false;
		}

		private bool HasLoS_IgnoreBuildables(Vector3 point)
		{
			if (PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(PlayerSingleton<PlayerCamera>.Instance.transform.position, point - PlayerSingleton<PlayerCamera>.Instance.transform.position, Vector3.Distance(point, PlayerSingleton<PlayerCamera>.Instance.transform.position) - 0.01f, out var _, 1 << LayerMask.NameToLayer("Default")))
			{
				return false;
			}
			return true;
		}

		public virtual string GetSaveString()
		{
			return new BuildableItemData(GUID, ItemInstance, 0).GetJson();
		}

		public virtual List<string> WriteData(string parentFolderPath)
		{
			return new List<string>();
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SendBuildableItemData_3537728543);
				RegisterObserversRpc(1u, RpcReader___Observers_ReceiveBuildableItemData_3859851844);
				RegisterTargetRpc(2u, RpcReader___Target_ReceiveBuildableItemData_3859851844);
				RegisterServerRpc(3u, RpcReader___Server_Destroy_Networked_2166136261);
				RegisterObserversRpc(4u, RpcReader___Observers_DestroyItemWrapper_2166136261);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendBuildableItemData_3537728543(ItemInstance instance, string GUID, string parentPropertyCode)
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
				writer.WriteString(GUID);
				writer.WriteString(parentPropertyCode);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendBuildableItemData_3537728543(ItemInstance instance, string GUID, string parentPropertyCode)
		{
			ReceiveBuildableItemData(null, instance, GUID, parentPropertyCode);
		}

		private void RpcReader___Server_SendBuildableItemData_3537728543(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			ItemInstance instance = PooledReader0.ReadItemInstance();
			string gUID = PooledReader0.ReadString();
			string parentPropertyCode = PooledReader0.ReadString();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendBuildableItemData_3537728543(instance, gUID, parentPropertyCode);
			}
		}

		private void RpcWriter___Observers_ReceiveBuildableItemData_3859851844(NetworkConnection conn, ItemInstance instance, string GUID, string parentPropertyCode)
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
				writer.WriteString(GUID);
				writer.WriteString(parentPropertyCode);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___ReceiveBuildableItemData_3859851844(NetworkConnection conn, ItemInstance instance, string GUID, string parentPropertyCode)
		{
			InitializeBuildableItem(instance, GUID, parentPropertyCode);
		}

		private void RpcReader___Observers_ReceiveBuildableItemData_3859851844(PooledReader PooledReader0, Channel channel)
		{
			ItemInstance instance = PooledReader0.ReadItemInstance();
			string gUID = PooledReader0.ReadString();
			string parentPropertyCode = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveBuildableItemData_3859851844(null, instance, gUID, parentPropertyCode);
			}
		}

		private void RpcWriter___Target_ReceiveBuildableItemData_3859851844(NetworkConnection conn, ItemInstance instance, string GUID, string parentPropertyCode)
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
				writer.WriteString(GUID);
				writer.WriteString(parentPropertyCode);
				SendTargetRpc(2u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_ReceiveBuildableItemData_3859851844(PooledReader PooledReader0, Channel channel)
		{
			ItemInstance instance = PooledReader0.ReadItemInstance();
			string gUID = PooledReader0.ReadString();
			string parentPropertyCode = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveBuildableItemData_3859851844(base.LocalConnection, instance, gUID, parentPropertyCode);
			}
		}

		private void RpcWriter___Server_Destroy_Networked_2166136261()
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
				SendServerRpc(3u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___Destroy_Networked_2166136261()
		{
			DestroyItemWrapper();
			Despawn(DespawnType.Destroy);
		}

		private void RpcReader___Server_Destroy_Networked_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized)
			{
				RpcLogic___Destroy_Networked_2166136261();
			}
		}

		private void RpcWriter___Observers_DestroyItemWrapper_2166136261()
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
				SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___DestroyItemWrapper_2166136261()
		{
			DestroyItem(callOnServer: false);
		}

		private void RpcReader___Observers_DestroyItemWrapper_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___DestroyItemWrapper_2166136261();
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EEntityFramework_002EBuildableItem_Assembly_002DCSharp_002Edll()
		{
		}
	}
}
