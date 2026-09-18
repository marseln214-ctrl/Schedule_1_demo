using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.AvatarFramework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Property;
using ScheduleOne.Quests;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Employees
{
	public class EmployeeManager : NetworkSingleton<EmployeeManager>
	{
		[Serializable]
		public class EmployeeAppearance
		{
			public AvatarSettings Settings;

			public Sprite Mugshot;
		}

		public const float MALE_EMPLOYEE_CHANCE = 0.67f;

		public List<Employee> AllEmployees = new List<Employee>();

		public Quest_Employees[] EmployeeQuests;

		[Header("Prefabs")]
		public Botanist BotanistPrefab;

		public Packager PackagerPrefab;

		public Chemist ChemistPrefab;

		public Cleaner CleanerPrefab;

		[Header("Appearances")]
		public List<EmployeeAppearance> MaleAppearances;

		public List<EmployeeAppearance> FemaleAppearances;

		[Header("Voices")]
		public VODatabase[] MaleVoices;

		public VODatabase[] FemaleVoices;

		[Header("Names")]
		public string[] MaleFirstNames;

		public string[] FemaleFirstNames;

		public string[] LastNames;

		private List<string> takenNames = new List<string>();

		private List<int> takenMaleAppearances = new List<int>();

		private List<int> takenFemaleAppearances = new List<int>();

		private bool NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted;

		public void CreateNewEmployee(ScheduleOne.Property.Property property, EEmployeeType type)
		{
			bool male = 0.67f > UnityEngine.Random.Range(0f, 1f);
			GenerateRandomName(male, out var firstName, out var lastName);
			string id = firstName.ToLower() + "_" + lastName.ToLower();
			GetRandomAppearance(male, out var index, out var _);
			string guid = GUIDManager.GenerateUniqueGUID().ToString();
			CreateEmployee(property, type, firstName, lastName, id, male, index, property.NPCSpawnPoint.position, property.NPCSpawnPoint.rotation, guid);
		}

		[ServerRpc(RequireOwnership = false)]
		public void CreateEmployee(ScheduleOne.Property.Property property, EEmployeeType type, string firstName, string lastName, string id, bool male, int appearanceIndex, Vector3 position, Quaternion rotation, string guid = "")
		{
			RpcWriter___Server_CreateEmployee_311954683(property, type, firstName, lastName, id, male, appearanceIndex, position, rotation, guid);
		}

		public Employee CreateEmployee_Server(ScheduleOne.Property.Property property, EEmployeeType type, string firstName, string lastName, string id, bool male, int appearanceIndex, Vector3 position, Quaternion rotation, string guid)
		{
			if (property.Employees.Count >= property.EmployeeCapacity)
			{
				Console.LogError("Property " + property.PropertyCode + " is at capacity.");
				return null;
			}
			return null;
		}

		public void RegisterName(string name)
		{
			takenNames.Add(name);
		}

		public void RegisterAppearance(bool male, int index)
		{
			if (male)
			{
				takenMaleAppearances.Add(index);
			}
			else
			{
				takenFemaleAppearances.Add(index);
			}
		}

		public void GenerateRandomName(bool male, out string firstName, out string lastName)
		{
			do
			{
				if (male)
				{
					firstName = MaleFirstNames[UnityEngine.Random.Range(0, MaleFirstNames.Length)].ToString();
				}
				else
				{
					firstName = FemaleFirstNames[UnityEngine.Random.Range(0, FemaleFirstNames.Length)].ToString();
				}
				lastName = LastNames[UnityEngine.Random.Range(0, LastNames.Length)].ToString();
			}
			while (takenNames.Contains(firstName + " " + lastName));
		}

		public EmployeeAppearance GetAppearance(bool male, int index)
		{
			if (!male)
			{
				return FemaleAppearances[index];
			}
			return MaleAppearances[index];
		}

		public VODatabase GetVoice(bool male, int index)
		{
			if (!male)
			{
				return FemaleVoices[index % FemaleVoices.Length];
			}
			return MaleVoices[index % MaleVoices.Length];
		}

		public void GetRandomAppearance(bool male, out int index, out AvatarSettings settings)
		{
			List<EmployeeAppearance> list = (male ? MaleAppearances : FemaleAppearances);
			List<int> list2 = (male ? takenMaleAppearances : takenFemaleAppearances);
			index = UnityEngine.Random.Range(0, list.Count);
			settings = list[index].Settings;
			if (list2.Count >= list.Count)
			{
				return;
			}
			while (list2.Contains(index))
			{
				index++;
				if (index >= list.Count)
				{
					index = 0;
				}
			}
			settings = list[index].Settings;
		}

		public Employee GetEmployeePrefab(EEmployeeType type)
		{
			switch (type)
			{
			case EEmployeeType.Botanist:
				return BotanistPrefab;
			case EEmployeeType.Packager:
				return PackagerPrefab;
			case EEmployeeType.Chemist:
				return ChemistPrefab;
			case EEmployeeType.Cleaner:
				return CleanerPrefab;
			default:
				Console.LogError("Employee type not found: " + type);
				return null;
			}
		}

		public List<Employee> GetEmployeesByType(EEmployeeType type)
		{
			List<Employee> list = new List<Employee>();
			foreach (Employee allEmployee in AllEmployees)
			{
				if (allEmployee.EmployeeType == type)
				{
					list.Add(allEmployee);
				}
			}
			return list;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(0u, RpcReader___Server_CreateEmployee_311954683);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_CreateEmployee_311954683(ScheduleOne.Property.Property property, EEmployeeType type, string firstName, string lastName, string id, bool male, int appearanceIndex, Vector3 position, Quaternion rotation, string guid = "")
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
				return;
			}
			Channel channel = Channel.Reliable;
			PooledWriter writer = WriterPool.GetWriter();
			GeneratedWriters___Internal.Write___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerated(writer, property);
			GeneratedWriters___Internal.Write___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerated(writer, type);
			writer.WriteString(firstName);
			writer.WriteString(lastName);
			writer.WriteString(id);
			writer.WriteBoolean(male);
			writer.WriteInt32(appearanceIndex);
			writer.WriteVector3(position);
			writer.WriteQuaternion(rotation);
			writer.WriteString(guid);
			SendServerRpc(0u, writer, channel, DataOrderType.Default);
			writer.Store();
		}

		public void RpcLogic___CreateEmployee_311954683(ScheduleOne.Property.Property property, EEmployeeType type, string firstName, string lastName, string id, bool male, int appearanceIndex, Vector3 position, Quaternion rotation, string guid = "")
		{
			CreateEmployee_Server(property, type, firstName, lastName, id, male, appearanceIndex, position, rotation, guid);
		}

		private void RpcReader___Server_CreateEmployee_311954683(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			ScheduleOne.Property.Property property = GeneratedReaders___Internal.Read___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerateds(PooledReader0);
			EEmployeeType type = GeneratedReaders___Internal.Read___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerateds(PooledReader0);
			string firstName = PooledReader0.ReadString();
			string lastName = PooledReader0.ReadString();
			string id = PooledReader0.ReadString();
			bool male = PooledReader0.ReadBoolean();
			int appearanceIndex = PooledReader0.ReadInt32();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			string guid = PooledReader0.ReadString();
			if (base.IsServerInitialized)
			{
				RpcLogic___CreateEmployee_311954683(property, type, firstName, lastName, id, male, appearanceIndex, position, rotation, guid);
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
