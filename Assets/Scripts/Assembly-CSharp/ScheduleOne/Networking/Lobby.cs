using System;
using System.Linq;
using System.Text;
using FishNet.Managing;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.UI;
using ScheduleOne.UI.MainMenu;
using Steamworks;
using UnityEngine;

namespace ScheduleOne.Networking
{
	public class Lobby : PersistentSingleton<Lobby>
	{
		public const bool ENABLED = true;

		public const int PLAYER_LIMIT = 4;

		public const string JOIN_READY = "ready";

		public const string HOST_LOADING = "host_loading";

		public NetworkManager NetworkManager;

		public CSteamID[] Players = new CSteamID[4];

		public Action onLobbyChange;

		private Callback<LobbyCreated_t> LobbyCreatedCallback;

		private Callback<LobbyEnter_t> LobbyEnteredCallback;

		private Callback<LobbyChatUpdate_t> ChatUpdateCallback;

		private Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequestedCallback;

		private Callback<LobbyChatMsg_t> LobbyChatMessageCallback;

		public bool IsHost
		{
			get
			{
				if (IsInLobby)
				{
					if (Players.Length != 0)
					{
						return Players[0] == LocalPlayerID;
					}
					return false;
				}
				return true;
			}
		}

		public ulong LobbyID { get; private set; }

		public CSteamID LobbySteamID => new CSteamID(LobbyID);

		public bool IsInLobby => LobbyID != 0;

		public int PlayerCount
		{
			get
			{
				if (!IsInLobby)
				{
					return 1;
				}
				return Players.Count((CSteamID p) => p != CSteamID.Nil);
			}
		}

		public CSteamID LocalPlayerID { get; private set; } = CSteamID.Nil;


		protected override void Awake()
		{
			base.Awake();
			_ = Destroyed;
		}

		protected override void Start()
		{
			base.Start();
			if (Destroyed)
			{
				return;
			}
			if (!SteamManager.Initialized)
			{
				Debug.LogError("Steamworks not initialized");
				return;
			}
			LocalPlayerID = SteamUser.GetSteamID();
			InitializeCallbacks();
			string launchLobby = GetLaunchLobby();
			if (launchLobby == null || !(launchLobby != string.Empty) || !SteamManager.Initialized)
			{
				return;
			}
			try
			{
				SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse(launchLobby)));
			}
			catch
			{
				Console.LogWarning("There is an issue with launch commands.");
			}
		}

		private void InitializeCallbacks()
		{
			LobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
			LobbyEnteredCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
			ChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(PlayerEnterOrLeave);
			GameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(LobbyJoinRequested);
			LobbyChatMessageCallback = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);
		}

		public void TryOpenInviteInterface()
		{
			if (!IsInLobby)
			{
				Console.Log("Not currently in a lobby, creating one...");
				CreateLobby();
			}
			if (SteamMatchmaking.GetNumLobbyMembers(LobbySteamID) >= 4)
			{
				Debug.LogWarning("Lobby already at max capacity!");
			}
			else
			{
				SteamFriends.ActivateGameOverlayInviteDialog(LobbySteamID);
			}
		}

		public void LeaveLobby()
		{
			if (IsInLobby)
			{
				SteamMatchmaking.LeaveLobby(LobbySteamID);
				Console.Log("Leaving lobby: " + LobbyID);
			}
			LobbyID = 0uL;
			UpdateLobbyMembers();
			if (onLobbyChange != null)
			{
				onLobbyChange();
			}
		}

		private void CreateLobby()
		{
			SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
		}

		private string GetLaunchLobby()
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (commandLineArgs[i].ToLower() == "+connect_lobby" && commandLineArgs.Length > i + 1)
				{
					return commandLineArgs[i + 1];
				}
			}
			return string.Empty;
		}

		private void UpdateLobbyMembers()
		{
			for (int i = 0; i < Players.Length; i++)
			{
				Players[i] = CSteamID.Nil;
			}
			int num = (IsInLobby ? SteamMatchmaking.GetNumLobbyMembers(LobbySteamID) : 0);
			for (int j = 0; j < num; j++)
			{
				Players[j] = SteamMatchmaking.GetLobbyMemberByIndex(LobbySteamID, j);
			}
		}

		public void JoinAsClient(string steamId64)
		{
			Singleton<LoadManager>.Instance.LoadAsClient(steamId64);
		}

		public void SendLobbyMessage(string message)
		{
			if (!IsInLobby)
			{
				Console.LogWarning("Not in a lobby, cannot send message.");
				return;
			}
			byte[] bytes = Encoding.ASCII.GetBytes(message);
			SteamMatchmaking.SendLobbyChatMsg(LobbySteamID, bytes, bytes.Length);
		}

		public void SetLobbyData(string key, string value)
		{
			if (!IsInLobby)
			{
				Console.LogWarning("Not in a lobby, cannot set data.");
			}
			else
			{
				SteamMatchmaking.SetLobbyData(LobbySteamID, key, value);
			}
		}

		private void OnLobbyCreated(LobbyCreated_t result)
		{
			if (result.m_eResult == EResult.k_EResultOK)
			{
				Console.Log("Lobby created: " + result.m_ulSteamIDLobby);
			}
			else
			{
				Console.LogWarning("Lobby creation failed: " + result.m_eResult);
			}
			LobbyID = result.m_ulSteamIDLobby;
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "owner", SteamUser.GetSteamID().ToString());
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "version", Application.version);
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "host_loading", "false");
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "ready", "false");
			UpdateLobbyMembers();
			if (onLobbyChange != null)
			{
				onLobbyChange();
			}
		}

		private void OnLobbyEntered(LobbyEnter_t result)
		{
			string lobbyData = SteamMatchmaking.GetLobbyData(new CSteamID(result.m_ulSteamIDLobby), "version");
			Console.Log("Lobby version: " + lobbyData + ", client version: " + Application.version);
			if (lobbyData != Application.version)
			{
				Console.LogWarning("Lobby version mismatch, cannot join.");
				if (Singleton<MainMenuPopup>.InstanceExists)
				{
					Singleton<MainMenuPopup>.Instance.Open("Version Mismatch", "Host version: " + lobbyData + "\nYour version: " + Application.version, isBad: true);
				}
				LeaveLobby();
				return;
			}
			Console.Log("Entered lobby: " + result.m_ulSteamIDLobby);
			LobbyID = result.m_ulSteamIDLobby;
			UpdateLobbyMembers();
			if (onLobbyChange != null)
			{
				onLobbyChange();
			}
			if (SteamMatchmaking.GetLobbyData(LobbySteamID, "ready") == "true" && !IsHost)
			{
				JoinAsClient(SteamMatchmaking.GetLobbyOwner(LobbySteamID).m_SteamID.ToString());
			}
			if (SteamMatchmaking.GetLobbyData(LobbySteamID, "host_loading") == "true" && !IsHost)
			{
				Singleton<LoadManager>.Instance.SetWaitingForHostLoad();
				Singleton<LoadingScreen>.Instance.Open();
			}
		}

		private void PlayerEnterOrLeave(LobbyChatUpdate_t result)
		{
			Console.Log("Player join/leave: " + SteamFriends.GetFriendPersonaName(new CSteamID(result.m_ulSteamIDUserChanged)));
			UpdateLobbyMembers();
			if (result.m_ulSteamIDMakingChange == LobbySteamID.m_SteamID && result.m_ulSteamIDUserChanged != LocalPlayerID.m_SteamID)
			{
				Console.Log("Lobby owner left, leaving lobby.");
				LeaveLobby();
			}
			if (onLobbyChange != null)
			{
				onLobbyChange();
			}
		}

		private void LobbyJoinRequested(GameLobbyJoinRequested_t result)
		{
			CSteamID steamIDLobby = result.m_steamIDLobby;
			Console.Log("Join requested: " + steamIDLobby.ToString());
			if (LobbyID != 0L)
			{
				LeaveLobby();
			}
			SteamMatchmaking.JoinLobby(result.m_steamIDLobby);
		}

		private void OnLobbyChatMessage(LobbyChatMsg_t result)
		{
			byte[] array = new byte[128];
			int cubData = 128;
			SteamMatchmaking.GetLobbyChatEntry(new CSteamID(LobbyID), (int)result.m_iChatID, out var pSteamIDUser, array, cubData, out var _);
			string @string = Encoding.ASCII.GetString(array);
			@string = @string.TrimEnd(new char[1]);
			Console.Log("Lobby chat message received: " + @string);
			if (@string == "ready" && !IsHost)
			{
				JoinAsClient(pSteamIDUser.m_SteamID.ToString());
			}
			if (@string == "host_loading" && !IsHost)
			{
				Singleton<LoadManager>.Instance.SetWaitingForHostLoad();
				Singleton<LoadingScreen>.Instance.Open();
			}
		}
	}
}
