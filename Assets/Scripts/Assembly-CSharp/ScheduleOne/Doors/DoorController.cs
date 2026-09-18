using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Doors
{
	public class DoorController : NetworkBehaviour
	{
		public const float DISTANT_PLAYER_THRESHOLD = 40f;

		public EDoorAccess PlayerAccess;

		public bool AutoOpenForPlayer;

		[Header("References")]
		[SerializeField]
		protected InteractableObject[] InteriorIntObjs;

		[SerializeField]
		protected InteractableObject[] ExteriorIntObjs;

		[Tooltip("Used to block player from entering when the door is open for an NPC, but player isn't permitted access.")]
		[SerializeField]
		protected BoxCollider PlayerBlocker;

		[Header("Animation")]
		[SerializeField]
		protected Animation InteriorDoorHandleAnimation;

		[SerializeField]
		protected Animation ExteriorDoorHandleAnimation;

		[Header("Settings")]
		[SerializeField]
		protected bool AutoCloseOnSleep = true;

		[SerializeField]
		protected bool AutoCloseOnDistantPlayer = true;

		[Header("NPC Access")]
		[SerializeField]
		protected bool OpenableByNPCs = true;

		[Tooltip("How many seconds to wait after NPC passes through to return to original state")]
		[SerializeField]
		protected float ReturnToOriginalTime = 0.5f;

		public UnityEvent<EDoorSide> onDoorOpened;

		public UnityEvent onDoorClosed;

		private EDoorSide lastOpenSide = EDoorSide.Exterior;

		private bool autoOpenedForPlayer;

		private bool NetworkInitialize___EarlyScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted;

		public bool IsOpen { get; protected set; }

		public bool openedByNPC { get; protected set; }

		public float timeSinceNPCSensed { get; protected set; } = float.MaxValue;


		public bool playerDetectedSinceOpened { get; protected set; }

		public float timeSincePlayerSensed { get; protected set; } = float.MaxValue;


		public float timeInCurrentState { get; protected set; }

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EDoors_002EDoorController_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected virtual void Start()
		{
			if (!AutoCloseOnSleep)
			{
				return;
			}
			ScheduleOne.GameTime.TimeManager.onSleepStart = (Action)Delegate.Combine(ScheduleOne.GameTime.TimeManager.onSleepStart, (Action)delegate
			{
				if (IsOpen)
				{
					SetIsOpen(open: false, EDoorSide.Interior);
				}
			});
		}

		protected virtual void Update()
		{
			timeSinceNPCSensed += Time.deltaTime;
			timeSincePlayerSensed += Time.deltaTime;
			timeInCurrentState += Time.deltaTime;
			if (InstanceFinder.IsServer && IsOpen && ((openedByNPC && timeSinceNPCSensed > ReturnToOriginalTime) || (autoOpenedForPlayer && timeSincePlayerSensed > ReturnToOriginalTime)))
			{
				openedByNPC = false;
				autoOpenedForPlayer = false;
				PlayerBlocker.enabled = false;
				SetIsOpen_Server(open: false, EDoorSide.Interior, openedForPlayer: false);
			}
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (IsOpen)
			{
				SetIsOpen(connection, open: true, lastOpenSide);
			}
		}

		public virtual void InteriorHandleHovered()
		{
			InteractableObject[] interiorIntObjs;
			if (CanPlayerAccess(EDoorSide.Interior, out var reason))
			{
				interiorIntObjs = InteriorIntObjs;
				foreach (InteractableObject obj in interiorIntObjs)
				{
					obj.SetMessage(IsOpen ? "Close" : "Open");
					obj.SetInteractableState(InteractableObject.EInteractableState.Default);
				}
				return;
			}
			interiorIntObjs = InteriorIntObjs;
			foreach (InteractableObject interactableObject in interiorIntObjs)
			{
				if (reason != string.Empty)
				{
					interactableObject.SetMessage(reason);
					interactableObject.SetInteractableState(InteractableObject.EInteractableState.Invalid);
				}
				else
				{
					interactableObject.SetInteractableState(InteractableObject.EInteractableState.Disabled);
				}
			}
		}

		public virtual void InteriorHandleInteracted()
		{
			if (CanPlayerAccess(EDoorSide.Interior))
			{
				if (!IsOpen && InteriorDoorHandleAnimation != null)
				{
					InteriorDoorHandleAnimation.Play();
				}
				SetIsOpen_Server(!IsOpen, EDoorSide.Interior, openedForPlayer: false);
			}
		}

		public virtual void ExteriorHandleHovered()
		{
			InteractableObject[] exteriorIntObjs;
			if (CanPlayerAccess(EDoorSide.Exterior, out var reason))
			{
				exteriorIntObjs = ExteriorIntObjs;
				foreach (InteractableObject obj in exteriorIntObjs)
				{
					obj.SetMessage(IsOpen ? "Close" : "Open");
					obj.SetInteractableState(InteractableObject.EInteractableState.Default);
				}
				return;
			}
			exteriorIntObjs = ExteriorIntObjs;
			foreach (InteractableObject interactableObject in exteriorIntObjs)
			{
				if (reason != string.Empty)
				{
					interactableObject.SetMessage(reason);
					interactableObject.SetInteractableState(InteractableObject.EInteractableState.Invalid);
				}
				else
				{
					interactableObject.SetInteractableState(InteractableObject.EInteractableState.Disabled);
				}
			}
		}

		public virtual void ExteriorHandleInteracted()
		{
			if (CanPlayerAccess(EDoorSide.Exterior))
			{
				if (!IsOpen && ExteriorDoorHandleAnimation != null)
				{
					ExteriorDoorHandleAnimation.Play();
				}
				SetIsOpen_Server(!IsOpen, EDoorSide.Exterior, openedForPlayer: false);
			}
		}

		public bool CanPlayerAccess(EDoorSide side)
		{
			string reason;
			return CanPlayerAccess(side, out reason);
		}

		protected virtual bool CanPlayerAccess(EDoorSide side, out string reason)
		{
			reason = string.Empty;
			switch (side)
			{
			case EDoorSide.Interior:
				if (PlayerAccess != 0)
				{
					return PlayerAccess == EDoorAccess.ExitOnly;
				}
				return true;
			case EDoorSide.Exterior:
				if (PlayerAccess != 0)
				{
					return PlayerAccess == EDoorAccess.EnterOnly;
				}
				return true;
			default:
				return false;
			}
		}

		public virtual void NPCVicinityDetected(EDoorSide side)
		{
			if (InstanceFinder.IsServer)
			{
				timeSinceNPCSensed = 0f;
				if (OpenableByNPCs && PlayerAccess != 0)
				{
					PlayerBlocker.enabled = true;
				}
				if (!IsOpen && OpenableByNPCs)
				{
					openedByNPC = true;
					SetIsOpen_Server(open: true, side, openedForPlayer: false);
				}
			}
		}

		public virtual void PlayerVicinityDetected(EDoorSide side)
		{
			timeSincePlayerSensed = 0f;
			if (IsOpen)
			{
				playerDetectedSinceOpened = true;
			}
			if (!IsOpen && AutoOpenForPlayer && CanPlayerAccess(side))
			{
				autoOpenedForPlayer = true;
				SetIsOpen_Server(open: true, side, openedForPlayer: true);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SetIsOpen_Server(bool open, EDoorSide accessSide, bool openedForPlayer)
		{
			RpcWriter___Server_SetIsOpen_Server_1319291243(open, accessSide, openedForPlayer);
			RpcLogic___SetIsOpen_Server_1319291243(open, accessSide, openedForPlayer);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void SetIsOpen(NetworkConnection conn, bool open, EDoorSide openSide)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetIsOpen_3381113727(conn, open, openSide);
				RpcLogic___SetIsOpen_3381113727(conn, open, openSide);
			}
			else
			{
				RpcWriter___Target_SetIsOpen_3381113727(conn, open, openSide);
			}
		}

		public virtual void SetIsOpen(bool open, EDoorSide openSide)
		{
			if (IsOpen != open)
			{
				timeInCurrentState = 0f;
			}
			IsOpen = open;
			if (IsOpen)
			{
				playerDetectedSinceOpened = false;
			}
			lastOpenSide = openSide;
			if (IsOpen)
			{
				onDoorOpened.Invoke(openSide);
			}
			else
			{
				onDoorClosed.Invoke();
			}
		}

		protected virtual void CheckAutoCloseForDistantPlayer()
		{
			if (InstanceFinder.IsServer && IsOpen && !(timeSinceNPCSensed < ReturnToOriginalTime) && !(timeSincePlayerSensed < ReturnToOriginalTime))
			{
				Player.GetClosestPlayer(base.transform.position, out var distance);
				if (distance > 40f)
				{
					SetIsOpen_Server(open: false, EDoorSide.Interior, openedForPlayer: false);
				}
			}
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SetIsOpen_Server_1319291243);
				RegisterObserversRpc(1u, RpcReader___Observers_SetIsOpen_3381113727);
				RegisterTargetRpc(2u, RpcReader___Target_SetIsOpen_3381113727);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SetIsOpen_Server_1319291243(bool open, EDoorSide accessSide, bool openedForPlayer)
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
				writer.WriteBoolean(open);
				GeneratedWriters___Internal.Write___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerated(writer, accessSide);
				writer.WriteBoolean(openedForPlayer);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetIsOpen_Server_1319291243(bool open, EDoorSide accessSide, bool openedForPlayer)
		{
			autoOpenedForPlayer = openedForPlayer;
			if (openedForPlayer)
			{
				timeSincePlayerSensed = 0f;
			}
			SetIsOpen(null, open, accessSide);
		}

		private void RpcReader___Server_SetIsOpen_Server_1319291243(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			bool open = PooledReader0.ReadBoolean();
			EDoorSide accessSide = GeneratedReaders___Internal.Read___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerateds(PooledReader0);
			bool openedForPlayer = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetIsOpen_Server_1319291243(open, accessSide, openedForPlayer);
			}
		}

		private void RpcWriter___Observers_SetIsOpen_3381113727(NetworkConnection conn, bool open, EDoorSide openSide)
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
				writer.WriteBoolean(open);
				GeneratedWriters___Internal.Write___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerated(writer, openSide);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetIsOpen_3381113727(NetworkConnection conn, bool open, EDoorSide openSide)
		{
			SetIsOpen(open, openSide);
		}

		private void RpcReader___Observers_SetIsOpen_3381113727(PooledReader PooledReader0, Channel channel)
		{
			bool open = PooledReader0.ReadBoolean();
			EDoorSide openSide = GeneratedReaders___Internal.Read___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetIsOpen_3381113727(null, open, openSide);
			}
		}

		private void RpcWriter___Target_SetIsOpen_3381113727(NetworkConnection conn, bool open, EDoorSide openSide)
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
				writer.WriteBoolean(open);
				GeneratedWriters___Internal.Write___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerated(writer, openSide);
				SendTargetRpc(2u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetIsOpen_3381113727(PooledReader PooledReader0, Channel channel)
		{
			bool open = PooledReader0.ReadBoolean();
			EDoorSide openSide = GeneratedReaders___Internal.Read___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___SetIsOpen_3381113727(base.LocalConnection, open, openSide);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EDoors_002EDoorController_Assembly_002DCSharp_002Edll()
		{
			PlayerBlocker.enabled = false;
			InteractableObject[] interiorIntObjs = InteriorIntObjs;
			foreach (InteractableObject obj in interiorIntObjs)
			{
				obj.onHovered.AddListener(InteriorHandleHovered);
				obj.onInteractStart.AddListener(InteriorHandleInteracted);
				obj.SetMessage(IsOpen ? "Close" : "Open");
			}
			interiorIntObjs = ExteriorIntObjs;
			foreach (InteractableObject obj2 in interiorIntObjs)
			{
				obj2.onHovered.AddListener(ExteriorHandleHovered);
				obj2.onInteractStart.AddListener(ExteriorHandleInteracted);
				obj2.SetMessage(IsOpen ? "Close" : "Open");
			}
			if (base.gameObject.isStatic)
			{
				Console.LogError("DoorController is static! Doors should not be static!", base.gameObject);
			}
			if (AutoCloseOnDistantPlayer)
			{
				InvokeRepeating("CheckAutoCloseForDistantPlayer", 2f, 2f);
			}
		}
	}
}
