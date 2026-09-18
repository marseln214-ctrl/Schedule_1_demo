using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.Compass;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.TV
{
	public class TVInterface : MonoBehaviour
	{
		public const float OPEN_TIME = 0.15f;

		public const float FOV = 60f;

		public List<Player> Players = new List<Player>();

		[Header("References")]
		public Canvas Canvas;

		public Transform CameraPosition;

		public TVHomeScreen HomeScreen;

		public TextMeshPro TimeLabel;

		public TextMeshPro Daylabel;

		public UnityEvent<Player> onPlayerAdded = new UnityEvent<Player>();

		public UnityEvent<Player> onPlayerRemoved = new UnityEvent<Player>();

		public bool IsOpen { get; private set; }

		public void Awake()
		{
			Canvas.enabled = false;
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		public void Start()
		{
			GameInput.RegisterExitListener(Exit, 2);
			MinPass();
		}

		private void OnDestroy()
		{
			if (NetworkSingleton<TimeManager>.InstanceExists)
			{
				TimeManager instance = NetworkSingleton<TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			}
		}

		private void MinPass()
		{
			TimeLabel.text = TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime);
			Daylabel.text = NetworkSingleton<TimeManager>.Instance.CurrentDay.ToString();
		}

		public void Open()
		{
			if (!IsOpen)
			{
				IsOpen = true;
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
				PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0.15f);
				PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.15f);
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
				PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
				PlayerSingleton<PlayerMovement>.Instance.canMove = false;
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
				Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
				Singleton<CompassManager>.Instance.SetVisible(visible: false);
				AddPlayer(Player.Local);
				Canvas.enabled = true;
				TimeLabel.gameObject.SetActive(value: false);
				HomeScreen.Open();
			}
		}

		public void Close()
		{
			if (IsOpen)
			{
				IsOpen = false;
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.15f);
				PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.15f);
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				PlayerSingleton<PlayerMovement>.Instance.canMove = true;
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				RemovePlayer(Player.Local);
				Canvas.enabled = false;
				TimeLabel.gameObject.SetActive(value: true);
				Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
				Singleton<CompassManager>.Instance.SetVisible(visible: true);
			}
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && IsOpen)
			{
				action.used = true;
				Close();
			}
		}

		public bool CanOpen()
		{
			return !IsOpen;
		}

		public void AddPlayer(Player player)
		{
			if (!Players.Contains(player))
			{
				Players.Add(player);
				if (onPlayerAdded != null)
				{
					onPlayerAdded.Invoke(player);
				}
			}
		}

		public void RemovePlayer(Player player)
		{
			if (Players.Contains(player))
			{
				Players.Remove(player);
				if (onPlayerRemoved != null)
				{
					onPlayerRemoved.Invoke(player);
				}
			}
		}
	}
}
