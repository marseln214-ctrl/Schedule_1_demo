using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.WorldspacePopup;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Vision
{
	public class VisionCone : NetworkBehaviour
	{
		public enum EEventLevel
		{
			Start = 0,
			Half = 1,
			Full = 2,
			Zero = 3
		}

		[Serializable]
		public class StateContainer
		{
			public PlayerVisualState.EVisualState state;

			public bool Enabled;

			public float RequiredNoticeTime = 0.5f;
		}

		public class PlayerSightData
		{
			public Player Player;

			public float VisionDelta;

			public float TimeVisible;
		}

		public delegate void EventStateChange(VisionEventReceipt _event);

		public const float VISION_UPDATE_INTERVAL = 0.05f;

		public static float UniversalAttentivenessScale = 1f;

		public static float UniversalMemoryScale = 1f;

		[Header("Frustrum Settings")]
		public float HorizontalFOV = 90f;

		public float VerticalFOV = 30f;

		public float Range = 40f;

		public float MinorWidth = 3f;

		public float MinorHeight = 1.5f;

		public Transform VisionOrigin;

		public bool DEBUG_FRUSTRUM;

		[Header("Vision Settings")]
		public bool VisionEnabled = true;

		public AnimationCurve VisionFalloff;

		public LayerMask VisibilityBlockingLayers;

		[Range(0f, 2f)]
		public float RangeMultiplier = 1f;

		[Header("Interest settings")]
		public List<StateContainer> StatesOfInterest = new List<StateContainer>();

		[Header("Notice Settings")]
		public float MinVisionDelta = 0.1f;

		public float Attentiveness = 1f;

		public float Memory = 1f;

		[Header("Worldspace Icons")]
		public bool WorldspaceIconsEnabled = true;

		public WorldspacePopup QuestionMarkPopup;

		public WorldspacePopup ExclamationPointPopup;

		public AudioSourceController ExclamationSound;

		public EventStateChange onVisionEventStarted;

		public EventStateChange onVisionEventHalf;

		public EventStateChange onVisionEventFull;

		public EventStateChange onVisionEventExpired;

		public Dictionary<Player, Dictionary<PlayerVisualState.EVisualState, StateContainer>> StateSettings = new Dictionary<Player, Dictionary<PlayerVisualState.EVisualState, StateContainer>>();

		protected List<VisionEvent> activeVisionEvents = new List<VisionEvent>();

		protected Dictionary<Player, PlayerSightData> playerSightDatas = new Dictionary<Player, PlayerSightData>();

		protected NPC npc;

		private bool NetworkInitialize___EarlyScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted;

		protected float effectiveRange => Range * RangeMultiplier;

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EVision_002EVisionCone_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		private void PlayerSpawned(Player plr)
		{
			Dictionary<PlayerVisualState.EVisualState, StateContainer> dictionary = new Dictionary<PlayerVisualState.EVisualState, StateContainer>();
			for (int i = 0; i < StatesOfInterest.Count; i++)
			{
				dictionary.Add(StatesOfInterest[i].state, StatesOfInterest[i]);
			}
			StateSettings.Add(plr, dictionary);
		}

		private void OnDisable()
		{
			while (activeVisionEvents.Count > 0)
			{
				activeVisionEvents[0].EndEvent();
			}
			playerSightDatas.Clear();
		}

		protected virtual void Update()
		{
			if (DEBUG_FRUSTRUM)
			{
				GetFrustumVertices();
			}
		}

		protected virtual void FixedUpdate()
		{
			if (VisionEnabled)
			{
				return;
			}
			foreach (VisionEvent activeVisionEvent in activeVisionEvents)
			{
				activeVisionEvent.EndEvent();
			}
		}

		protected virtual void VisionUpdate()
		{
			if (base.enabled && VisionEnabled)
			{
				UpdateVision(0.05f);
				UpdateEvents(0.05f);
			}
		}

		protected virtual void UpdateEvents(float tickTime)
		{
			foreach (Player key in playerSightDatas.Keys)
			{
				if (key != Player.Local || !key.Health.IsAlive || key.IsArrested)
				{
					continue;
				}
				foreach (PlayerVisualState.VisualState visualState in key.VisualState.visualStates)
				{
					if (!StateSettings[key].ContainsKey(visualState.state) || !StateSettings[key][visualState.state].Enabled)
					{
						continue;
					}
					StateContainer stateContainer = StateSettings[key][visualState.state];
					if (GetEvent(key, visualState) == null)
					{
						VisionEvent visionEvent = new VisionEvent(this, key, visualState, stateContainer.RequiredNoticeTime);
						visionEvent.UpdateEvent(playerSightDatas[key].VisionDelta, tickTime);
						activeVisionEvents.Add(visionEvent);
						if (onVisionEventStarted != null)
						{
							VisionEventReceipt @event = new VisionEventReceipt(key.NetworkObject, visualState.state);
							onVisionEventStarted(@event);
						}
					}
				}
			}
			List<VisionEvent> list = new List<VisionEvent>();
			list.AddRange(activeVisionEvents);
			foreach (VisionEvent item in list)
			{
				if (!StateSettings[item.Target].ContainsKey(item.State.state) || !StateSettings[item.Target][item.State.state].Enabled)
				{
					item.EndEvent();
				}
			}
			List<VisionEvent> list2 = activeVisionEvents.FindAll((VisionEvent x) => x.Target == Player.Local);
			float num = 0f;
			for (int i = 0; i < list2.Count; i++)
			{
				if (playerSightDatas.ContainsKey(Player.Local))
				{
					list2[i].UpdateEvent(playerSightDatas[Player.Local].VisionDelta, tickTime);
				}
				else
				{
					list2[i].UpdateEvent(0f, tickTime);
				}
				if (list2[i].NormalizedNoticeLevel > num)
				{
					num = list2[i].NormalizedNoticeLevel;
				}
			}
			if (num > 0f && WorldspaceIconsEnabled)
			{
				QuestionMarkPopup.enabled = true;
				QuestionMarkPopup.CurrentFillLevel = num;
			}
			else
			{
				QuestionMarkPopup.enabled = false;
			}
		}

		protected virtual void UpdateVision(float tickTime)
		{
			List<Player> list = new List<Player>();
			for (int i = 0; i < Player.PlayerList.Count; i++)
			{
				Player player = Player.PlayerList[i];
				if (npc != null && !npc.IsConscious)
				{
					return;
				}
				if (!IsPointWithinSight(player.Avatar.CenterPoint, ignoreLoS: true))
				{
					continue;
				}
				float num = player.Visibility.CalculateExposureToPoint(VisionOrigin.position, effectiveRange, npc);
				if (player.CurrentVehicle != null && IsPointWithinSight(player.CurrentVehicle.transform.position, ignoreLoS: false, player.CurrentVehicle.GetComponent<LandVehicle>()))
				{
					num = 1f;
				}
				if (!(num > 0f))
				{
					continue;
				}
				float num2 = num * VisionFalloff.Evaluate(Mathf.Clamp01(Vector3.Distance(VisionOrigin.position, player.Avatar.CenterPoint) / effectiveRange)) * player.Visibility.CurrentVisibility / 100f;
				if (num2 > MinVisionDelta)
				{
					list.Add(player);
					if (IsPlayerVisible(player, out var data))
					{
						data.TimeVisible += tickTime;
						data.VisionDelta = num2;
						continue;
					}
					data = new PlayerSightData();
					data.Player = player;
					data.TimeVisible = 0f;
					data.VisionDelta = num2;
					playerSightDatas.Add(player, data);
				}
			}
			foreach (Player item in new List<Player>(playerSightDatas.Keys))
			{
				if (!list.Contains(item))
				{
					playerSightDatas.Remove(item);
				}
			}
		}

		public virtual void EventReachedZero(VisionEvent _event)
		{
			activeVisionEvents.Remove(_event);
			VisionEventReceipt receipt = new VisionEventReceipt(_event.Target.NetworkObject, _event.State.state);
			SendEventReceipt(receipt, EEventLevel.Zero);
		}

		public virtual void EventHalfNoticed(VisionEvent _event)
		{
			VisionEventReceipt receipt = new VisionEventReceipt(_event.Target.NetworkObject, _event.State.state);
			SendEventReceipt(receipt, EEventLevel.Half);
		}

		public virtual void EventFullyNoticed(VisionEvent _event)
		{
			activeVisionEvents.Remove(_event);
			if (WorldspaceIconsEnabled && _event.Target.Owner.IsLocalClient)
			{
				ExclamationPointPopup.Popup();
				ExclamationSound.Play();
			}
			VisionEventReceipt receipt = new VisionEventReceipt(_event.Target.NetworkObject, _event.State.state);
			SendEventReceipt(receipt, EEventLevel.Full);
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public void SendEventReceipt(VisionEventReceipt receipt, EEventLevel level)
		{
			RpcWriter___Server_SendEventReceipt_3486014028(receipt, level);
			RpcLogic___SendEventReceipt_3486014028(receipt, level);
		}

		[ObserversRpc(RunLocally = true, ExcludeOwner = true)]
		public virtual void ReceiveEventReceipt(VisionEventReceipt receipt, EEventLevel level)
		{
			RpcWriter___Observers_ReceiveEventReceipt_3486014028(receipt, level);
			RpcLogic___ReceiveEventReceipt_3486014028(receipt, level);
		}

		public virtual bool IsPointWithinSight(Vector3 point, bool ignoreLoS = false, LandVehicle vehicleToIgnore = null)
		{
			if (Vector3.Distance(point, VisionOrigin.position) > effectiveRange)
			{
				return false;
			}
			if (Vector3.SignedAngle(VisionOrigin.forward, (point - VisionOrigin.position).normalized, VisionOrigin.up) > 90f)
			{
				return false;
			}
			if (Vector3.SignedAngle(VisionOrigin.forward, (point - VisionOrigin.position).normalized, VisionOrigin.right) > 90f)
			{
				return false;
			}
			Plane[] frustumPlanes = GetFrustumPlanes();
			for (int i = 0; i < 6; i++)
			{
				if (frustumPlanes[i].GetDistanceToPoint(point) > 0f)
				{
					return false;
				}
			}
			if (!ignoreLoS && Physics.Raycast(VisionOrigin.position, point - VisionOrigin.position, out var hitInfo, Vector3.Distance(point, VisionOrigin.position), VisibilityBlockingLayers))
			{
				if (vehicleToIgnore != null && hitInfo.collider.GetComponentInParent<LandVehicle>() == vehicleToIgnore)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public VisionEvent GetEvent(Player target, PlayerVisualState.VisualState state)
		{
			return activeVisionEvents.Find((VisionEvent x) => x.Target == target && x.State == state);
		}

		public bool IsPlayerVisible(Player player)
		{
			if (playerSightDatas.ContainsKey(player))
			{
				return playerSightDatas[player].VisionDelta > MinVisionDelta;
			}
			return false;
		}

		public float GetPlayerVisibility(Player player)
		{
			if (playerSightDatas.ContainsKey(player))
			{
				return playerSightDatas[player].VisionDelta;
			}
			return 0f;
		}

		public bool IsPlayerVisible(Player player, out PlayerSightData data)
		{
			if (playerSightDatas.ContainsKey(player))
			{
				data = playerSightDatas[player];
				return true;
			}
			data = null;
			return false;
		}

		public virtual void SetGeneralCrimeResponseActive(Player player, bool active)
		{
			StateSettings[player][PlayerVisualState.EVisualState.PettyCrime].Enabled = active;
		}

		private void OnDie()
		{
			ClearEvents();
		}

		public void ClearEvents()
		{
			ExclamationPointPopup.enabled = false;
			QuestionMarkPopup.enabled = false;
			VisionEvent[] array = activeVisionEvents.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].EndEvent();
			}
		}

		private Vector3[] GetFrustumVertices()
		{
			Vector3 position = VisionOrigin.position;
			Quaternion rotation = VisionOrigin.rotation;
			float z = 0f;
			float z2 = effectiveRange;
			float minorWidth = MinorWidth;
			float minorHeight = MinorHeight;
			float num = minorWidth + 2f * effectiveRange * Mathf.Tan(HorizontalFOV * (MathF.PI / 180f) / 2f);
			float num2 = minorHeight + 2f * effectiveRange * Mathf.Tan(VerticalFOV * (MathF.PI / 180f) / 2f);
			Vector3[] array = new Vector3[8];
			Vector3 vector = position + rotation * new Vector3((0f - minorWidth) / 2f, minorHeight / 2f, z);
			Vector3 vector2 = position + rotation * new Vector3(minorWidth / 2f, minorHeight / 2f, z);
			Vector3 vector3 = position + rotation * new Vector3((0f - minorWidth) / 2f, (0f - minorHeight) / 2f, z);
			Vector3 vector4 = position + rotation * new Vector3(minorWidth / 2f, (0f - minorHeight) / 2f, z);
			Vector3 vector5 = position + rotation * new Vector3((0f - num) / 2f, num2 / 2f, z2);
			Vector3 vector6 = position + rotation * new Vector3(num / 2f, num2 / 2f, z2);
			Vector3 vector7 = position + rotation * new Vector3((0f - num) / 2f, (0f - num2) / 2f, z2);
			Vector3 vector8 = position + rotation * new Vector3(num / 2f, (0f - num2) / 2f, z2);
			array[0] = vector;
			array[1] = vector2;
			array[2] = vector3;
			array[3] = vector4;
			array[4] = vector5;
			array[5] = vector6;
			array[6] = vector7;
			array[7] = vector8;
			Debug.DrawLine(vector, vector5, Color.red);
			Debug.DrawLine(vector2, vector6, Color.green);
			Debug.DrawLine(vector3, vector7, Color.blue);
			Debug.DrawLine(vector4, vector8, Color.magenta);
			return array;
		}

		private Plane[] GetFrustumPlanes()
		{
			Vector3 position = VisionOrigin.position;
			Quaternion rotation = VisionOrigin.rotation;
			float z = 0f;
			float z2 = effectiveRange;
			float minorWidth = MinorWidth;
			float minorHeight = MinorHeight;
			float num = minorWidth + 2f * effectiveRange * Mathf.Tan(HorizontalFOV * (MathF.PI / 180f) / 2f);
			float num2 = minorHeight + 2f * effectiveRange * Mathf.Tan(VerticalFOV * (MathF.PI / 180f) / 2f);
			Plane[] array = new Plane[6];
			Vector3 vector = position + rotation * new Vector3((0f - minorWidth) / 2f, minorHeight / 2f, z);
			Vector3 vector2 = position + rotation * new Vector3(minorWidth / 2f, minorHeight / 2f, z);
			Vector3 vector3 = position + rotation * new Vector3((0f - minorWidth) / 2f, (0f - minorHeight) / 2f, z);
			Vector3 vector4 = position + rotation * new Vector3(minorWidth / 2f, (0f - minorHeight) / 2f, z);
			Vector3 vector5 = position + rotation * new Vector3((0f - num) / 2f, num2 / 2f, z2);
			Vector3 vector6 = position + rotation * new Vector3(num / 2f, num2 / 2f, z2);
			Vector3 c = position + rotation * new Vector3((0f - num) / 2f, (0f - num2) / 2f, z2);
			Vector3 c2 = position + rotation * new Vector3(num / 2f, (0f - num2) / 2f, z2);
			array[0] = new Plane(vector2, vector, vector5);
			array[1] = new Plane(vector3, vector4, c2);
			array[2] = new Plane(vector, vector3, c);
			array[3] = new Plane(vector4, vector2, vector6);
			array[4] = new Plane(vector, vector2, vector4);
			array[5] = new Plane(vector6, vector5, c);
			return array;
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SendEventReceipt_3486014028);
				RegisterObserversRpc(1u, RpcReader___Observers_ReceiveEventReceipt_3486014028);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendEventReceipt_3486014028(VisionEventReceipt receipt, EEventLevel level)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerated(writer, receipt);
				GeneratedWriters___Internal.Write___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerated(writer, level);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendEventReceipt_3486014028(VisionEventReceipt receipt, EEventLevel level)
		{
			ReceiveEventReceipt(receipt, level);
		}

		private void RpcReader___Server_SendEventReceipt_3486014028(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			VisionEventReceipt receipt = GeneratedReaders___Internal.Read___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerateds(PooledReader0);
			EEventLevel level = GeneratedReaders___Internal.Read___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendEventReceipt_3486014028(receipt, level);
			}
		}

		private void RpcWriter___Observers_ReceiveEventReceipt_3486014028(VisionEventReceipt receipt, EEventLevel level)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerated(writer, receipt);
				GeneratedWriters___Internal.Write___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerated(writer, level);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: true);
				writer.Store();
			}
		}

		public virtual void RpcLogic___ReceiveEventReceipt_3486014028(VisionEventReceipt receipt, EEventLevel level)
		{
			switch (level)
			{
			case EEventLevel.Start:
				if (onVisionEventStarted != null)
				{
					onVisionEventStarted(receipt);
				}
				break;
			case EEventLevel.Half:
				if (onVisionEventHalf != null)
				{
					onVisionEventHalf(receipt);
				}
				break;
			case EEventLevel.Full:
				if (onVisionEventFull != null)
				{
					onVisionEventFull(receipt);
				}
				break;
			case EEventLevel.Zero:
				if (onVisionEventExpired != null)
				{
					onVisionEventExpired(receipt);
				}
				break;
			}
		}

		private void RpcReader___Observers_ReceiveEventReceipt_3486014028(PooledReader PooledReader0, Channel channel)
		{
			VisionEventReceipt receipt = GeneratedReaders___Internal.Read___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerateds(PooledReader0);
			EEventLevel level = GeneratedReaders___Internal.Read___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ReceiveEventReceipt_3486014028(receipt, level);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EVision_002EVisionCone_Assembly_002DCSharp_002Edll()
		{
			if (VisionOrigin == null)
			{
				VisionOrigin = base.transform;
			}
			npc = GetComponentInParent<NPC>();
			for (int i = 0; i < Player.PlayerList.Count; i++)
			{
				PlayerSpawned(Player.PlayerList[i]);
			}
			Player.onPlayerSpawned = (Action<Player>)Delegate.Combine(Player.onPlayerSpawned, new Action<Player>(PlayerSpawned));
			if (npc != null)
			{
				npc.Health.onDie.AddListener(OnDie);
				npc.Health.onKnockedOut.AddListener(OnDie);
			}
			InvokeRepeating("VisionUpdate", UnityEngine.Random.Range(0f, 0.05f), 0.05f);
		}
	}
}
