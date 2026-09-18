using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.Interaction;
using ScheduleOne.Management;
using ScheduleOne.Map;
using ScheduleOne.Misc;
using ScheduleOne.Money;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.UI.Management;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Property
{
	public class Property : NetworkBehaviour, ISaveable
	{
		public delegate void PropertyChange(Property property);

		public static List<Property> Properties = new List<Property>();

		public static List<Property> UnownedProperties = new List<Property>();

		public static List<Property> OwnedProperties = new List<Property>();

		public static PropertyChange onPropertyAcquired;

		public UnityEvent onThisPropertyAcquired;

		[Header("Settings")]
		[SerializeField]
		protected string propertyName = "Property Name";

		public bool AvailableInDemo = true;

		[SerializeField]
		protected string propertyCode = "propertycode";

		public float Price = 1f;

		public float DefaultRotation;

		public int EmployeeCapacity = 10;

		public bool OwnedByDefault;

		public bool DEBUG_SET_OWNED;

		public string IsOwnedVariable = string.Empty;

		[Header("References")]
		public PropertyContentsContainer Container;

		public Transform EmployeeContainer;

		public Transform SpawnPoint;

		public Transform InteriorSpawnPoint;

		public GameObject ForSaleSign;

		public GameObject BoundingBox;

		public POI PoI;

		public Transform ListingPoster;

		public Transform NPCSpawnPoint;

		public Transform[] EmployeeIdlePoints;

		public List<ModularSwitch> Switches;

		public List<InteractableToggleable> Toggleables;

		public PropertyDisposalArea DisposalArea;

		[HideInInspector]
		public List<BuildableItem> BuildableItems = new List<BuildableItem>();

		public List<IConfigurable> Configurables = new List<IConfigurable>();

		private PropertyLoader loader = new PropertyLoader();

		private List<string> savedObjectPaths = new List<string>();

		private List<string> savedEmployeePaths = new List<string>();

		private bool NetworkInitialize___EarlyScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted;

		public bool IsOwned { get; protected set; }

		public List<Employee> Employees { get; protected set; } = new List<Employee>();


		public RectTransform WorldspaceUIContainer { get; protected set; }

		public string PropertyName => propertyName;

		public string PropertyCode => propertyCode;

		public int ParkingSpaces => 0;

		public string SaveFolderName => propertyName;

		public string SaveFileName => "Property";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => true;

		public List<string> LocalExtraFiles { get; set; } = new List<string> { "Safe" };


		public List<string> LocalExtraFolders { get; set; } = new List<string> { "Objects", "Employees" };


		public bool HasChanged { get; set; }

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EProperty_002EProperty_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		protected virtual void Start()
		{
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Combine(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			for (int i = 0; i < Toggleables.Count; i++)
			{
				if (Toggleables[i].IsActivated)
				{
					SetToggleableState(connection, i, Toggleables[i].IsActivated);
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (NetworkSingleton<MoneyManager>.InstanceExists)
			{
				MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
				instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Remove(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
			}
			Properties.Remove(this);
			UnownedProperties.Remove(this);
			OwnedProperties.Remove(this);
		}

		protected virtual void GetNetworth(MoneyManager.FloatContainer container)
		{
			if (IsOwned)
			{
				container.ChangeValue(Price);
			}
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			if ((Application.isEditor || Debug.isDebugBuild) && DEBUG_SET_OWNED)
			{
				SetOwned_Server();
			}
			else if (OwnedByDefault)
			{
				SetOwned_Server();
			}
			if (base.NetworkObject.GetInitializeOrder() == 0)
			{
				Console.LogError("Property " + PropertyName + " has an initialize order of 0. This will cause issues.");
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		protected void SetOwned_Server()
		{
			RpcWriter___Server_SetOwned_Server_2166136261();
			RpcLogic___SetOwned_Server_2166136261();
		}

		[ObserversRpc(RunLocally = true, BufferLast = true)]
		private void ReceiveOwned_Networked()
		{
			RpcWriter___Observers_ReceiveOwned_Networked_2166136261();
			RpcLogic___ReceiveOwned_Networked_2166136261();
		}

		protected virtual void RecieveOwned()
		{
			if (!IsOwned)
			{
				IsOwned = true;
				HasChanged = true;
				if (IsOwnedVariable != string.Empty && NetworkSingleton<VariableDatabase>.InstanceExists && InstanceFinder.IsServer)
				{
					NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(IsOwnedVariable, "true");
				}
				if (UnownedProperties.Contains(this))
				{
					UnownedProperties.Remove(this);
					OwnedProperties.Add(this);
				}
				if (onPropertyAcquired != null)
				{
					onPropertyAcquired(this);
				}
				if (onThisPropertyAcquired != null)
				{
					onThisPropertyAcquired.Invoke();
				}
				ForSaleSign.gameObject.SetActive(value: false);
				if (ListingPoster != null)
				{
					ListingPoster.gameObject.SetActive(value: false);
				}
				PoI.gameObject.SetActive(value: true);
				PoI.SetMainText(propertyName + " (Owned)");
				StartCoroutine(Wait());
			}
			IEnumerator Wait()
			{
				yield return new WaitUntil(() => PoI.UISetup);
				PoI.IconContainer.Find("Unowned").gameObject.SetActive(value: false);
				PoI.IconContainer.Find("Owned").gameObject.SetActive(value: true);
			}
		}

		public virtual bool ShouldSave()
		{
			if (!IsOwned)
			{
				return Container.transform.childCount > 0;
			}
			return true;
		}

		public void SetOwned()
		{
			SetOwned_Server();
		}

		public void SetBoundsVisible(bool vis)
		{
		}

		public int RegisterEmployee(Employee emp)
		{
			Employees.Add(emp);
			return Employees.IndexOf(emp);
		}

		private void ToggleableActioned(InteractableToggleable toggleable)
		{
			HasChanged = true;
			SendToggleableState(Toggleables.IndexOf(toggleable), toggleable.IsActivated);
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendToggleableState(int index, bool state)
		{
			RpcWriter___Server_SendToggleableState_3658436649(index, state);
		}

		[ObserversRpc]
		[TargetRpc]
		public void SetToggleableState(NetworkConnection conn, int index, bool state)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetToggleableState_338960014(conn, index, state);
			}
			else
			{
				RpcWriter___Target_SetToggleableState_338960014(conn, index, state);
			}
		}

		public virtual string GetSaveString()
		{
			bool[] array = new bool[Switches.Count];
			for (int i = 0; i < Switches.Count; i++)
			{
				if (!(Switches[i] == null))
				{
					array[i] = Switches[i].isOn;
				}
			}
			bool[] array2 = new bool[Toggleables.Count];
			for (int j = 0; j < Toggleables.Count; j++)
			{
				if (!(Toggleables[j] == null))
				{
					array2[j] = Toggleables[j].IsActivated;
				}
			}
			return new PropertyData(propertyCode, IsOwned, array, array2).GetJson();
		}

		public virtual List<string> WriteData(string parentFolderPath)
		{
			List<string> result = new List<string>();
			savedObjectPaths.Clear();
			savedEmployeePaths.Clear();
			string parentFolderPath2 = ((ISaveable)this).WriteFolder(parentFolderPath, "Objects");
			foreach (BuildableItem buildableItem in BuildableItems)
			{
				try
				{
					new SaveRequest(buildableItem, parentFolderPath2);
					savedObjectPaths.Add(buildableItem.SaveFolderName);
				}
				catch (Exception ex)
				{
					Console.LogError("Error saving object: " + ex.Message);
					SaveManager.ReportSaveError();
				}
			}
			string parentFolderPath3 = ((ISaveable)this).WriteFolder(parentFolderPath, "Employees");
			foreach (Employee employee in Employees)
			{
				try
				{
					new SaveRequest(employee, parentFolderPath3);
					savedEmployeePaths.Add(employee.SaveFolderName);
				}
				catch (Exception ex2)
				{
					Console.LogError("Error saving employees: " + ex2.Message);
					SaveManager.ReportSaveError();
				}
			}
			return result;
		}

		public virtual void DeleteUnapprovedFiles(string parentFolderPath)
		{
			string path = ((ISaveable)this).WriteFolder(parentFolderPath, "Objects");
			string path2 = ((ISaveable)this).WriteFolder(parentFolderPath, "Employees");
			string[] directories = Directory.GetDirectories(path);
			for (int i = 0; i < directories.Length; i++)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(directories[i]);
				if (!savedObjectPaths.Contains(directoryInfo.Name))
				{
					Directory.Delete(directories[i], recursive: true);
				}
			}
			directories = Directory.GetDirectories(path2);
			for (int j = 0; j < directories.Length; j++)
			{
				DirectoryInfo directoryInfo2 = new DirectoryInfo(directories[j]);
				if (!savedEmployeePaths.Contains(directoryInfo2.Name))
				{
					Directory.Delete(directories[j], recursive: true);
				}
			}
		}

		public virtual void Load(PropertyData propertyData, string containerPath)
		{
			if (propertyData.IsOwned)
			{
				SetOwned();
			}
			for (int i = 0; i < propertyData.SwitchStates.Length && i < Switches.Count; i++)
			{
				if (propertyData.SwitchStates[i] && Switches.Count > i)
				{
					Switches[i].SwitchOn();
				}
			}
			if (propertyData.ToggleableStates == null)
			{
				return;
			}
			for (int j = 0; j < propertyData.ToggleableStates.Length && j < Toggleables.Count; j++)
			{
				if (propertyData.ToggleableStates[j] && Toggleables.Count > j)
				{
					Toggleables[j].Toggle();
				}
			}
		}

		public bool DoBoundsContainPoint(Vector3 point)
		{
			Collider[] componentsInChildren = BoundingBox.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].bounds.Contains(point))
				{
					return true;
				}
			}
			return false;
		}

		public List<Bed> GetUnassignedBeds()
		{
			return (from x in Container.GetComponentsInChildren<Bed>()
				where x.AssignedEmployee == null
				select x).ToList();
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SetOwned_Server_2166136261);
				RegisterObserversRpc(1u, RpcReader___Observers_ReceiveOwned_Networked_2166136261);
				RegisterServerRpc(2u, RpcReader___Server_SendToggleableState_3658436649);
				RegisterObserversRpc(3u, RpcReader___Observers_SetToggleableState_338960014);
				RegisterTargetRpc(4u, RpcReader___Target_SetToggleableState_338960014);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SetOwned_Server_2166136261()
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
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		protected void RpcLogic___SetOwned_Server_2166136261()
		{
			ReceiveOwned_Networked();
		}

		private void RpcReader___Server_SetOwned_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetOwned_Server_2166136261();
			}
		}

		private void RpcWriter___Observers_ReceiveOwned_Networked_2166136261()
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
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: true, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveOwned_Networked_2166136261()
		{
			RecieveOwned();
		}

		private void RpcReader___Observers_ReceiveOwned_Networked_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ReceiveOwned_Networked_2166136261();
			}
		}

		private void RpcWriter___Server_SendToggleableState_3658436649(int index, bool state)
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
				writer.WriteInt32(index);
				writer.WriteBoolean(state);
				SendServerRpc(2u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendToggleableState_3658436649(int index, bool state)
		{
			SetToggleableState(null, index, state);
		}

		private void RpcReader___Server_SendToggleableState_3658436649(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int index = PooledReader0.ReadInt32();
			bool state = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendToggleableState_3658436649(index, state);
			}
		}

		private void RpcWriter___Observers_SetToggleableState_338960014(NetworkConnection conn, int index, bool state)
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
				writer.WriteInt32(index);
				writer.WriteBoolean(state);
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetToggleableState_338960014(NetworkConnection conn, int index, bool state)
		{
			Toggleables[index].SetState(state);
		}

		private void RpcReader___Observers_SetToggleableState_338960014(PooledReader PooledReader0, Channel channel)
		{
			int index = PooledReader0.ReadInt32();
			bool state = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetToggleableState_338960014(null, index, state);
			}
		}

		private void RpcWriter___Target_SetToggleableState_338960014(NetworkConnection conn, int index, bool state)
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
				writer.WriteInt32(index);
				writer.WriteBoolean(state);
				SendTargetRpc(4u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetToggleableState_338960014(PooledReader PooledReader0, Channel channel)
		{
			int index = PooledReader0.ReadInt32();
			bool state = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetToggleableState_338960014(base.LocalConnection, index, state);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EProperty_002EProperty_Assembly_002DCSharp_002Edll()
		{
			if (!(this is Business))
			{
				Properties.Add(this);
				UnownedProperties.Remove(this);
				UnownedProperties.Add(this);
			}
			Container.Property = this;
			PoI.SetMainText(propertyName + " (Unowned)");
			SetBoundsVisible(vis: false);
			ForSaleSign.transform.Find("Name").GetComponent<TextMeshPro>().text = propertyName;
			ForSaleSign.transform.Find("Price").GetComponent<TextMeshPro>().text = MoneyManager.FormatAmount(Price);
			Collider[] componentsInChildren = BoundingBox.GetComponentsInChildren<Collider>();
			foreach (Collider obj in componentsInChildren)
			{
				obj.isTrigger = true;
				obj.gameObject.layer = LayerMask.NameToLayer("Invisible");
			}
			if (DisposalArea == null)
			{
				Console.LogWarning("Property " + PropertyName + " has no disposal area.");
			}
			if (EmployeeIdlePoints.Length < EmployeeCapacity)
			{
				Debug.LogWarning("Property " + PropertyName + " has less idle points than employee capacity.");
			}
			if (!GameManager.IS_TUTORIAL)
			{
				WorldspaceUIContainer = new GameObject(propertyName + " Worldspace UI Container").AddComponent<RectTransform>();
				WorldspaceUIContainer.SetParent(Singleton<ManagementWorldspaceCanvas>.Instance.Canvas.transform);
				WorldspaceUIContainer.gameObject.SetActive(value: false);
			}
			if (ListingPoster != null)
			{
				ListingPoster.Find("Title").GetComponent<TextMeshPro>().text = propertyName;
				ListingPoster.Find("Price").GetComponent<TextMeshPro>().text = MoneyManager.FormatAmount(Price);
				ListingPoster.Find("Parking/Text").GetComponent<TextMeshPro>().text = ParkingSpaces.ToString();
				ListingPoster.Find("Employee/Text").GetComponent<TextMeshPro>().text = EmployeeCapacity.ToString();
			}
			PoI.gameObject.SetActive(value: false);
			foreach (ModularSwitch @switch in Switches)
			{
				if (!(@switch == null))
				{
					@switch.onToggled = (ModularSwitch.ButtonChange)Delegate.Combine(@switch.onToggled, (ModularSwitch.ButtonChange)delegate
					{
						HasChanged = true;
					});
				}
			}
			foreach (InteractableToggleable toggleable2 in Toggleables)
			{
				if (!(toggleable2 == null))
				{
					InteractableToggleable toggleable1 = toggleable2;
					toggleable2.onToggle.AddListener(delegate
					{
						ToggleableActioned(toggleable1);
					});
				}
			}
			InitializeSaveable();
		}
	}
}
