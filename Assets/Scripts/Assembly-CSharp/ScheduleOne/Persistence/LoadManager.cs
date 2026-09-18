using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FishNet;
using FishNet.Component.Scenes;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Yak;
using FishySteamworks;
using Pathfinding;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Networking;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.ItemLoaders;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Quests;
using ScheduleOne.UI;
using ScheduleOne.UI.MainMenu;
using ScheduleOne.UI.Phone;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Persistence
{
	public class LoadManager : PersistentSingleton<LoadManager>
	{
		public enum ELoadStatus
		{
			None = 0,
			LoadingScene = 1,
			Initializing = 2,
			LoadingData = 3,
			SpawningPlayer = 4,
			WaitingForHost = 5
		}

		public const int LOADS_PER_FRAME = 50;

		public const bool DEBUG = false;

		public static List<string> LoadHistory = new List<string>();

		public static SaveInfo[] SaveGames = new SaveInfo[5];

		public static SaveInfo LastPlayedGame = null;

		private List<LoadRequest> loadRequests = new List<LoadRequest>();

		public List<ItemLoader> ItemLoaders = new List<ItemLoader>();

		public List<BuildableItemLoader> ObjectLoaders = new List<BuildableItemLoader>();

		public List<NPCLoader> NPCLoaders = new List<NPCLoader>();

		public UnityEvent onPreSceneChange;

		public UnityEvent onPreLoad;

		public UnityEvent onLoadComplete;

		public UnityEvent onSaveInfoLoaded;

		public string DefaultTutorialSaveFolder => System.IO.Path.Combine(Application.streamingAssetsPath, "DefaultTutorialSave");

		public bool IsGameLoaded { get; protected set; }

		public bool IsLoading { get; protected set; }

		public float TimeSinceGameLoaded { get; protected set; }

		public bool DebugMode { get; protected set; }

		public ELoadStatus LoadStatus { get; protected set; }

		public string LoadedGameFolderPath { get; protected set; } = string.Empty;


		public SaveInfo ActiveSaveInfo { get; private set; }

		public SaveInfo StoredSaveInfo { get; private set; }

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
			InitializeItemLoaders();
			InitializeObjectLoaders();
			InitializeNPCLoaders();
			RefreshSaveInfo();
			if (SceneManager.GetActiveScene().name == "Main" || SceneManager.GetActiveScene().name == "Tutorial")
			{
				DebugMode = true;
				IsGameLoaded = true;
				LoadedGameFolderPath = System.IO.Path.Combine(Singleton<SaveManager>.Instance.SaveContainerFolderPath, "DevSave");
				if (!Directory.Exists(LoadedGameFolderPath))
				{
					Directory.CreateDirectory(LoadedGameFolderPath);
				}
			}
		}

		private void InitializeItemLoaders()
		{
			new ItemLoader();
			new WateringCanLoader();
			new CashLoader();
			new QualityItemLoader();
			new ProductItemLoader();
			new WeedLoader();
			new MethLoader();
			new CocaineLoader();
			new IntegerItemLoader();
			new TrashGrabberLoader();
			new ClothingLoader();
		}

		private void InitializeObjectLoaders()
		{
			new BuildableItemLoader();
			new GridItemLoader();
			new ProceduralGridItemLoader();
			new ToggleableItemLoader();
			new PotLoader();
			new PackagingStationLoader();
			new StorageRackLoader();
			new ChemistryStationLoader();
			new LabOvenLoader();
			new BrickPressLoader();
			new MixingStationLoader();
			new CauldronLoader();
			new TrashContainerLoader();
			new SoilPourerLoader();
		}

		private void InitializeNPCLoaders()
		{
			new NPCLoader();
			new EmployeeLoader();
			new PackagerLoader();
			new BotanistLoader();
			new ChemistLoader();
			new CleanerLoader();
		}

		public void Update()
		{
			if (IsGameLoaded && LoadedGameFolderPath != string.Empty && Input.GetKeyDown(KeyCode.F6) && (Application.isEditor || Debug.isDebugBuild))
			{
				NetworkManager networkManager = UnityEngine.Object.FindObjectOfType<NetworkManager>();
				networkManager.ClientManager.StopConnection();
				networkManager.ServerManager.StopConnection(sendDisconnectMessage: false);
				StartGame(new SaveInfo(LoadedGameFolderPath, -1, "Test Org", DateTime.Now, DateTime.Now, 0f, Application.version, new MetaData(null, null, string.Empty, string.Empty, playTutorial: false)), allowLoadStacking: true);
			}
			if (IsGameLoaded && LoadStatus == ELoadStatus.None)
			{
				TimeSinceGameLoaded += Time.deltaTime;
			}
		}

		public void QueueLoadRequest(LoadRequest request)
		{
			loadRequests.Add(request);
		}

		public void DequeueLoadRequest(LoadRequest request)
		{
			loadRequests.Remove(request);
		}

		public ItemLoader GetItemLoader(string itemType)
		{
			ItemLoader itemLoader = ItemLoaders.Find((ItemLoader loader) => loader.ItemType == itemType);
			if (itemLoader == null)
			{
				Console.LogError("No item loader found for data type: " + itemType);
				return null;
			}
			return itemLoader;
		}

		public BuildableItemLoader GetObjectLoader(string objectType)
		{
			BuildableItemLoader buildableItemLoader = ObjectLoaders.Find((BuildableItemLoader loader) => loader.ItemType == objectType);
			if (buildableItemLoader == null)
			{
				Console.LogError("No object loader found for data type: " + objectType);
				return null;
			}
			return buildableItemLoader;
		}

		public NPCLoader GetNPCLoader(string npcType)
		{
			NPCLoader nPCLoader = NPCLoaders.Find((NPCLoader loader) => loader.NPCType == npcType);
			if (nPCLoader == null)
			{
				Console.LogError("No NPC loader found for NPC type: " + npcType);
				return null;
			}
			return nPCLoader;
		}

		public string GetLoadStatusText()
		{
			return LoadStatus switch
			{
				ELoadStatus.LoadingScene => "Loading world...", 
				ELoadStatus.Initializing => "Initializing...", 
				ELoadStatus.SpawningPlayer => "Spawning player...", 
				ELoadStatus.LoadingData => "Loading data...", 
				ELoadStatus.WaitingForHost => "Waiting for host to finish loading...", 
				_ => string.Empty, 
			};
		}

		public void StartGame(SaveInfo info, bool allowLoadStacking = false)
		{
			if (IsGameLoaded && !allowLoadStacking)
			{
				Console.LogWarning("Game already loaded, cannot start another");
				return;
			}
			if (info == null)
			{
				Console.LogWarning("Save info is null, cannot start game");
				return;
			}
			string savePath = info.SavePath;
			if (!Directory.Exists(savePath))
			{
				Console.LogWarning("Save game does not exist at " + savePath);
				return;
			}
			Singleton<MusicPlayer>.Instance.StopAndDisableTracks();
			Console.Log("Starting game!");
			ActiveSaveInfo = info;
			IsLoading = true;
			TimeSinceGameLoaded = 0f;
			LoadedGameFolderPath = info.SavePath;
			LoadHistory.Add("Loading game: " + ActiveSaveInfo.OrganisationName);
			StartCoroutine(LoadRoutine());
			IEnumerator Load()
			{
				Console.Log("Load start!");
				foreach (IBaseSaveable baseSaveable in Singleton<SaveManager>.Instance.BaseSaveables)
				{
					new LoadRequest(System.IO.Path.Combine(LoadedGameFolderPath, baseSaveable.SaveFolderName), baseSaveable.Loader);
				}
				while (loadRequests.Count > 0)
				{
					for (int i = 0; i < 50; i++)
					{
						if (loadRequests.Count <= 0)
						{
							break;
						}
						LoadRequest loadRequest = loadRequests[0];
						try
						{
							loadRequest.Complete();
						}
						catch (Exception ex)
						{
							Console.LogError("LOAD ERROR for load request: " + loadRequest.Path + " : " + ex.Message + "\nSite: " + ex.TargetSite);
							if (loadRequests.FirstOrDefault() == loadRequest)
							{
								loadRequests.RemoveAt(0);
							}
						}
					}
					yield return new WaitForEndOfFrame();
				}
				yield return new WaitForEndOfFrame();
				yield return new WaitForEndOfFrame();
				if (onLoadComplete != null)
				{
					onLoadComplete.Invoke();
				}
			}
			IEnumerator LoadRoutine()
			{
				if (Singleton<Lobby>.Instance.IsInLobby && Singleton<Lobby>.Instance.IsHost)
				{
					Console.Log("Sending host loading message to lobby");
					Singleton<Lobby>.Instance.SetLobbyData("host_loading", "true");
					Singleton<Lobby>.Instance.SendLobbyMessage("host_loading");
				}
				LoadStatus = ELoadStatus.LoadingScene;
				Singleton<LoadingScreen>.Instance.Open();
				yield return new WaitForSecondsRealtime(1.25f);
				if (InstanceFinder.IsServer)
				{
					InstanceFinder.NetworkManager.ServerManager.StopConnection(sendDisconnectMessage: false);
				}
				if (InstanceFinder.IsClient)
				{
					InstanceFinder.NetworkManager.ClientManager.StopConnection();
				}
				if (onPreSceneChange != null)
				{
					onPreSceneChange.Invoke();
				}
				CleanUp();
				string sceneName = "Main";
				_ = info.MetaData.PlayTutorial;
				StoredSaveInfo = null;
				if (InstanceFinder.NetworkManager != null)
				{
					InstanceFinder.NetworkManager.gameObject.GetComponent<DefaultScene>().SetOnlineScene("Main");
				}
				AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
				while (!asyncLoad.isDone)
				{
					yield return new WaitForEndOfFrame();
				}
				Console.Log("Scene loaded: " + SceneManager.GetActiveScene().name);
				LoadStatus = ELoadStatus.Initializing;
				if (onPreLoad != null)
				{
					onPreLoad.Invoke();
				}
				Console.Log("Starting server");
				global::FishySteamworks.FishySteamworks fishy;
				ushort port;
				if (Singleton<Lobby>.Instance.IsInLobby && Singleton<Lobby>.Instance.IsHost)
				{
					fishy = InstanceFinder.TransportManager.GetTransport<Multipass>().GetTransport<global::FishySteamworks.FishySteamworks>();
					fishy.SetClientAddress(Singleton<Lobby>.Instance.LocalPlayerID.ToString());
					port = fishy.GetPort();
					fishy.OnServerConnectionState += Done;
					fishy.StartConnection(server: true);
				}
				else
				{
					Yak yak = InstanceFinder.TransportManager.GetTransport<Multipass>().GetTransport<Yak>();
					yak.SetPort(38465);
					yak.StartConnection(server: true);
					yield return new WaitUntil(() => InstanceFinder.IsServer);
					Console.Log("Server initialized");
					InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport(yak);
					yak.SetClientAddress("localhost");
					yak.StartConnection(server: false);
				}
				yield return new WaitUntil(() => InstanceFinder.NetworkManager.IsClient);
				Console.Log("Network initialized");
				LoadStatus = ELoadStatus.SpawningPlayer;
				yield return new WaitUntil(() => Player.Local != null);
				Console.Log("Local player spawned");
				LoadStatus = ELoadStatus.LoadingData;
				StartCoroutine(Load());
				yield return new WaitForSeconds(1f);
				LoadStatus = ELoadStatus.None;
				Console.Log("Game loaded");
				Singleton<LoadingScreen>.Instance.Close();
				IsLoading = false;
				IsGameLoaded = true;
				if (Singleton<Lobby>.Instance.IsInLobby && Singleton<Lobby>.Instance.IsHost)
				{
					Console.Log("Sending join ready message to lobby");
					Singleton<Lobby>.Instance.SetLobbyData("ready", "true");
					Singleton<Lobby>.Instance.SendLobbyMessage("ready");
					Singleton<Lobby>.Instance.SetLobbyData("host_loading", "false");
				}
				void Done(ServerConnectionStateArgs args)
				{
					Console.Log("Server connection state: " + args.ConnectionState.ToString() + " and transport index: " + args.TransportIndex);
					if (args.ConnectionState == LocalConnectionState.Started)
					{
						Console.Log("Server intialized");
						fishy.OnServerConnectionState -= Done;
						Console.Log("Starting FishySteamworks client connection: " + fishy.LocalUserSteamID);
						InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport<global::FishySteamworks.FishySteamworks>();
						InstanceFinder.NetworkManager.ClientManager.StartConnection(fishy.LocalUserSteamID.ToString(), port);
					}
				}
			}
		}

		public void LoadAsClient(string steamId64)
		{
			bool waitForExit = false;
			if (IsGameLoaded)
			{
				Console.LogWarning("Game already loaded, exiting");
				waitForExit = true;
				ExitToMenu();
			}
			StartCoroutine(LoadRoutine());
			IEnumerator LoadRoutine()
			{
				if (waitForExit)
				{
					yield return new WaitUntil(() => !IsLoading && SceneManager.GetActiveScene().name == "Menu");
				}
				Console.Log("Joining as client to: " + steamId64);
				LoadHistory.Add("Loading as client to: " + steamId64);
				ActiveSaveInfo = null;
				IsLoading = true;
				TimeSinceGameLoaded = 0f;
				LoadedGameFolderPath = string.Empty;
				LoadStatus = ELoadStatus.LoadingScene;
				Singleton<LoadingScreen>.Instance.Open();
				if (onPreSceneChange != null)
				{
					onPreSceneChange.Invoke();
				}
				CleanUp();
				InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport<global::FishySteamworks.FishySteamworks>();
				InstanceFinder.ClientManager.StartConnection(steamId64);
				Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
				yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Main");
				Console.Log("Scene loaded: " + SceneManager.GetActiveScene().name);
				if (onPreLoad != null)
				{
					onPreLoad.Invoke();
				}
				LoadStatus = ELoadStatus.SpawningPlayer;
				yield return new WaitUntil(() => Player.Local != null);
				Console.Log("Local player spawned");
				LoadStatus = ELoadStatus.LoadingData;
				yield return new WaitUntil(() => Player.Local.playerDataRetrieveReturned);
				Console.Log("Player data retrieved");
				LoadStatus = ELoadStatus.Initializing;
				yield return new WaitForSeconds(2f);
				if (onLoadComplete != null)
				{
					onLoadComplete.Invoke();
				}
				LoadStatus = ELoadStatus.None;
				Console.Log("Game loaded as client");
				Singleton<LoadingScreen>.Instance.Close();
				IsLoading = false;
				IsGameLoaded = true;
			}
			static void PlayerSpawned()
			{
				Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
				Console.Log("Local player spawned");
			}
		}

		public void SetWaitingForHostLoad()
		{
			IsLoading = true;
			LoadStatus = ELoadStatus.WaitingForHost;
		}

		public void LoadLastSave()
		{
			if (ActiveSaveInfo == null)
			{
				Console.LogWarning("No active save info, cannot load last save");
			}
			else
			{
				StartGame(ActiveSaveInfo, allowLoadStacking: true);
			}
		}

		private void CleanUp()
		{
			GUIDManager.Clear();
			Quest.Quests.Clear();
			Quest.ActiveQuests.Clear();
			NodeLink.validNodeLinks.Clear();
			Player.onLocalPlayerSpawned = null;
			Phone.ActiveApp = null;
			NavMeshUtility.ClearCache();
		}

		public void ExitToMenu(SaveInfo autoLoadSave = null, MainMenuPopup.Data mainMenuPopup = null)
		{
			if (!IsGameLoaded)
			{
				Console.LogWarning("Game not loaded, cannot exit to menu");
				return;
			}
			Console.Log("Exiting to menu");
			LoadHistory.Add("Exiting to menu");
			if (Player.Local != null && InstanceFinder.IsServer)
			{
				Player.Local.HostExitedGame();
			}
			if (Singleton<Lobby>.InstanceExists && Singleton<Lobby>.Instance.IsInLobby)
			{
				Singleton<Lobby>.Instance.LeaveLobby();
			}
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			IsGameLoaded = false;
			ActiveSaveInfo = null;
			IsLoading = true;
			Time.timeScale = 1f;
			Singleton<MusicPlayer>.Instance.StopAndDisableTracks();
			StartCoroutine(Load());
			IEnumerator Load()
			{
				Singleton<LoadingScreen>.Instance.Open();
				if (!InstanceFinder.IsServer)
				{
					Console.Log("Requesting server to save player data");
					Player.Local.RequestSavePlayer();
					float maxWait = 3f;
					float timeOnWaitStart = Time.realtimeSinceStartup;
					yield return new WaitUntil(() => Player.Local.playerSaveRequestReturned || Time.realtimeSinceStartup - timeOnWaitStart > maxWait);
					Console.Log("Player data saved");
				}
				yield return new WaitForSecondsRealtime(1.25f);
				if (onPreSceneChange != null)
				{
					onPreSceneChange.Invoke();
				}
				InstanceFinder.NetworkManager.ServerManager.StopConnection(sendDisconnectMessage: true);
				InstanceFinder.NetworkManager.ClientManager.StopConnection();
				AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Menu");
				while (!asyncLoad.isDone)
				{
					yield return new WaitForEndOfFrame();
				}
				if (autoLoadSave != null)
				{
					IsLoading = false;
					StartGame(autoLoadSave);
				}
				else
				{
					RefreshSaveInfo();
					yield return new WaitForSeconds(0.5f);
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					if (mainMenuPopup != null)
					{
						Singleton<MainMenuPopup>.Instance.Open(mainMenuPopup);
					}
					Singleton<LoadingScreen>.Instance.Close();
					IsLoading = false;
				}
			}
		}

		public void RefreshSaveInfo()
		{
			for (int i = 0; i < 5; i++)
			{
				SaveGames[i] = null;
				string text = System.IO.Path.Combine(Singleton<SaveManager>.Instance.SaveContainerFolderPath, "SaveGame_" + (i + 1));
				if (!Directory.Exists(text))
				{
					continue;
				}
				string path = System.IO.Path.Combine(text, "Metadata.json");
				MetaData metaData = null;
				if (File.Exists(path))
				{
					string text2 = string.Empty;
					try
					{
						text2 = File.ReadAllText(path);
					}
					catch (Exception ex)
					{
						Console.LogError("Error reading save metadata: " + ex.Message);
					}
					if (!string.IsNullOrEmpty(text2))
					{
						try
						{
							metaData = JsonUtility.FromJson<MetaData>(text2);
						}
						catch (Exception ex2)
						{
							metaData = null;
							Console.LogError("Error parsing save metadata: " + ex2.Message);
						}
					}
					else
					{
						Console.LogWarning("Metadata is empty");
					}
				}
				string path2 = System.IO.Path.Combine(text, "Game.json");
				GameData gameData = null;
				if (File.Exists(path2))
				{
					string text3 = string.Empty;
					try
					{
						text3 = File.ReadAllText(path2);
					}
					catch (Exception ex3)
					{
						Console.LogError("Error reading save game data: " + ex3.Message);
					}
					if (!string.IsNullOrEmpty(text3))
					{
						try
						{
							gameData = JsonUtility.FromJson<GameData>(text3);
						}
						catch (Exception ex4)
						{
							gameData = null;
							Console.LogError("Error parsing save game data: " + ex4.Message);
						}
					}
					else
					{
						Console.LogWarning("Game data is empty");
					}
				}
				float networth = 0f;
				string path3 = System.IO.Path.Combine(text, "Money.json");
				MoneyData moneyData = null;
				if (File.Exists(path3))
				{
					string text4 = string.Empty;
					try
					{
						text4 = File.ReadAllText(path3);
					}
					catch (Exception ex5)
					{
						Console.LogError("Error reading save money data: " + ex5.Message);
					}
					if (!string.IsNullOrEmpty(text4))
					{
						try
						{
							moneyData = JsonUtility.FromJson<MoneyData>(text4);
						}
						catch (Exception ex6)
						{
							moneyData = null;
							Console.LogError("Error parsing save money data: " + ex6.Message);
						}
					}
					else
					{
						Console.LogWarning("Money data is empty");
					}
					if (moneyData != null)
					{
						networth = moneyData.Networth;
					}
				}
				if (metaData == null)
				{
					Console.LogWarning("Failed to load metadata. Setting default");
					metaData = new MetaData(new DateTimeData(DateTime.Now), new DateTimeData(DateTime.Now), Application.version, Application.version, playTutorial: false);
					try
					{
						File.WriteAllText(path, metaData.GetJson());
					}
					catch (Exception)
					{
					}
				}
				if (gameData == null)
				{
					Console.LogWarning("Failed to load game data. Setting default");
					gameData = new GameData("Unknown", UnityEngine.Random.Range(0, int.MaxValue));
					try
					{
						File.WriteAllText(path2, gameData.GetJson());
					}
					catch (Exception)
					{
					}
				}
				SaveInfo saveInfo = new SaveInfo(text, i + 1, gameData.OrganisationName, metaData.CreationDate.GetDateTime(), metaData.LastPlayedDate.GetDateTime(), networth, metaData.LastSaveVersion, metaData);
				SaveGames[i] = saveInfo;
			}
			LastPlayedGame = null;
			for (int j = 0; j < SaveGames.Length; j++)
			{
				if (SaveGames[j] != null && (LastPlayedGame == null || SaveGames[j].DateLastPlayed > LastPlayedGame.DateLastPlayed))
				{
					LastPlayedGame = SaveGames[j];
				}
			}
			if (onSaveInfoLoaded != null)
			{
				onSaveInfoLoaded.Invoke();
			}
		}
	}
}
