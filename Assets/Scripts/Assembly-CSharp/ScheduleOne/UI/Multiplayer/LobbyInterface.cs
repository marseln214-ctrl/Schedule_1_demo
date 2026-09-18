using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Networking;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Multiplayer
{
	public class LobbyInterface : PersistentSingleton<LobbyInterface>
	{
		[Header("References")]
		public Lobby Lobby;

		public Canvas Canvas;

		public TextMeshProUGUI LobbyTitle;

		public RectTransform[] PlayerSlots;

		public Button InviteButton;

		public Button LeaveButton;

		public GameObject InviteHint;

		protected override void Awake()
		{
			base.Awake();
			InviteButton.onClick.AddListener(InviteClicked);
			LeaveButton.onClick.AddListener(LeaveClicked);
			Lobby lobby = Lobby;
			lobby.onLobbyChange = (Action)Delegate.Combine(lobby.onLobbyChange, (Action)delegate
			{
				UpdateButtons();
				UpdatePlayers();
				LobbyTitle.text = "Lobby (" + Lobby.PlayerCount + "/" + 4 + ")";
			});
		}

		protected override void Start()
		{
			base.Start();
			UpdateButtons();
			UpdatePlayers();
			if (PlayerPrefs.GetInt("InviteHintShown", 0) == 0)
			{
				InviteHint.SetActive(value: true);
			}
			else
			{
				InviteHint.SetActive(value: false);
			}
		}

		private void LateUpdate()
		{
			if (Singleton<PauseMenu>.InstanceExists)
			{
				Canvas.enabled = Singleton<PauseMenu>.Instance.IsPaused && Lobby.IsInLobby && !GameManager.IS_TUTORIAL;
				if (Canvas.enabled)
				{
					LeaveButton.gameObject.SetActive(value: false);
				}
			}
			else
			{
				Canvas.enabled = true;
				LeaveButton.gameObject.SetActive(!Lobby.IsHost);
			}
		}

		public void SetVisible(bool visible)
		{
			Canvas.enabled = visible;
		}

		public void LeaveClicked()
		{
			Lobby.LeaveLobby();
		}

		public void InviteClicked()
		{
			PlayerPrefs.SetInt("InviteHintShown", 1);
			InviteHint.SetActive(value: false);
			Lobby.TryOpenInviteInterface();
		}

		private void UpdateButtons()
		{
			InviteButton.gameObject.SetActive(Lobby.IsHost && Lobby.PlayerCount < 4);
			LeaveButton.gameObject.SetActive(!Lobby.IsHost);
		}

		private void UpdatePlayers()
		{
			if (Lobby.IsInLobby)
			{
				for (int i = 0; i < PlayerSlots.Length; i++)
				{
					if (Lobby.Players[i] != CSteamID.Nil)
					{
						SetPlayer(i, Lobby.Players[i]);
					}
					else
					{
						ClearPlayer(i);
					}
				}
			}
			else
			{
				SetPlayer(0, Lobby.LocalPlayerID);
				for (int j = 1; j < PlayerSlots.Length; j++)
				{
					ClearPlayer(j);
				}
			}
		}

		public void SetPlayer(int index, CSteamID player)
		{
			Lobby.Players[index] = player;
			PlayerSlots[index].Find("Frame/Avatar").GetComponent<RawImage>().texture = GetAvatar(player);
			PlayerSlots[index].gameObject.SetActive(value: true);
		}

		public void ClearPlayer(int index)
		{
			Lobby.Players[index] = CSteamID.Nil;
			PlayerSlots[index].gameObject.SetActive(value: false);
		}

		private Texture2D GetAvatar(CSteamID user)
		{
			if (!SteamManager.Initialized)
			{
				Debug.LogWarning("Steamworks not initialized");
				return new Texture2D(0, 0);
			}
			int mediumFriendAvatar = SteamFriends.GetMediumFriendAvatar(user);
			if (SteamUtils.GetImageSize(mediumFriendAvatar, out var pnWidth, out var pnHeight) && pnWidth != 0 && pnHeight != 0)
			{
				byte[] array = new byte[pnWidth * pnHeight * 4];
				Texture2D texture2D = new Texture2D((int)pnWidth, (int)pnHeight, TextureFormat.RGBA32, mipChain: false, linear: false);
				if (SteamUtils.GetImageRGBA(mediumFriendAvatar, array, (int)(pnWidth * pnHeight * 4)))
				{
					texture2D.LoadRawTextureData(array);
					texture2D.Apply();
				}
				return texture2D;
			}
			Debug.LogWarning("Couldn't get avatar.");
			return new Texture2D(0, 0);
		}
	}
}
