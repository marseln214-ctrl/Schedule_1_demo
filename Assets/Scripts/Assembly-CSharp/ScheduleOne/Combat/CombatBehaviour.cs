using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vision;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Combat
{
	public class CombatBehaviour : ScheduleOne.NPCs.Behaviour.Behaviour
	{
		public const float EXTRA_VISIBILITY_TIME = 2.5f;

		public const float SEARCH_RADIUS_MIN = 25f;

		public const float SEARCH_RADIUS_MAX = 60f;

		public const float SEARCH_SPEED = 0.4f;

		public const float CONSECUTIVE_MISS_ACCURACY_BOOST = 0.1f;

		public const float REACHED_DESTINATION_DISTANCE = 2f;

		public bool TEST_MODE;

		[Header("General Setttings")]
		public float GiveUpRange = 20f;

		public float GiveUpTime = 30f;

		public int GiveUpAfterSuccessfulHits;

		public bool PlayAngryVO = true;

		[Header("Movement settings")]
		[Range(0f, 1f)]
		public float DefaultMovementSpeed = 0.6f;

		[Header("Weapon settings")]
		public AvatarWeapon DefaultWeapon;

		public AvatarMeleeWeapon VirtualPunchWeapon;

		[Header("Search settings")]
		public float DefaultSearchTime = 30f;

		protected bool overrideTargetDistance;

		protected float targetDistanceOverride;

		protected bool isTargetRecentlyVisible;

		protected bool isTargetImmediatelyVisible;

		protected float timeSinceLastSighting = 10000f;

		protected float playerSightedDuration;

		protected Vector3 lastKnownTargetPosition = Vector3.zero;

		protected AvatarWeapon currentWeapon;

		protected int successfulHits;

		protected int consecutiveMissedShots;

		protected Coroutine rangedWeaponRoutine;

		protected Coroutine searchRoutine;

		protected Vector3 currentSearchDestination = Vector3.zero;

		protected bool hasSearchDestination;

		private float nextAngryVO;

		private bool NetworkInitialize___EarlyScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public Player TargetPlayer { get; protected set; }

		public bool IsSearching { get; protected set; }

		public float TimeSinceTargetReacquired { get; protected set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ECombat_002ECombatBehaviour_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (base.Active && TargetPlayer != null)
			{
				SetTarget(connection, TargetPlayer.NetworkObject);
			}
		}

		[ObserversRpc(RunLocally = true)]
		public virtual void SetTarget(NetworkConnection conn, NetworkObject target)
		{
			RpcWriter___Observers_SetTarget_1824087381(conn, target);
			RpcLogic___SetTarget_1824087381(conn, target);
		}

		protected override void Begin()
		{
			base.Begin();
			base.Npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "post combat", 120f, 1);
			StartCombat();
		}

		protected override void Resume()
		{
			base.Resume();
			StartCombat();
		}

		protected override void Pause()
		{
			base.Pause();
			EndCombat();
		}

		protected override void End()
		{
			base.End();
			EndCombat();
		}

		public override void Disable()
		{
			base.Disable();
			TargetPlayer = null;
			End();
		}

		protected virtual void StartCombat()
		{
			CheckPlayerVisibility();
			isTargetRecentlyVisible = true;
			SetMovementSpeed(DefaultMovementSpeed);
			base.Npc.Movement.SetStance(NPCMovement.EStance.Stanced);
			base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.IgnoreCosts);
			base.Npc.Avatar.EmotionManager.AddEmotionOverride("Angry", "combat", 0f, 3);
			if (InstanceFinder.IsServer && DefaultWeapon != null)
			{
				SetWeapon(DefaultWeapon.AssetPath);
			}
			nextAngryVO = Time.time + UnityEngine.Random.Range(5f, 15f);
			successfulHits = 0;
		}

		protected void EndCombat()
		{
			StopSearching();
			if (InstanceFinder.IsServer && currentWeapon != null)
			{
				ClearWeapon();
			}
			base.Npc.Movement.SpeedController.RemoveSpeedControl("combat");
			base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.Humanoid);
			base.Npc.Movement.SetStance(NPCMovement.EStance.None);
			base.Npc.Avatar.EmotionManager.RemoveEmotionOverride("combat");
			if (TargetPlayer != null)
			{
				base.Npc.awareness.VisionCone.StateSettings[TargetPlayer][PlayerVisualState.EVisualState.Visible].Enabled = false;
			}
			timeSinceLastSighting = 10000f;
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (InstanceFinder.IsServer && !IsTargetValid())
			{
				Disable_Networked(null);
				return;
			}
			UpdateLookAt();
			if (Time.time > nextAngryVO && PlayAngryVO)
			{
				EVOLineType lineType = ((UnityEngine.Random.Range(0, 2) == 0) ? EVOLineType.Angry : EVOLineType.Command);
				base.Npc.PlayVO(lineType);
				nextAngryVO = Time.time + UnityEngine.Random.Range(5f, 15f);
			}
			if (isTargetRecentlyVisible)
			{
				lastKnownTargetPosition = TargetPlayer.Avatar.CenterPoint;
			}
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			TimeSinceTargetReacquired += Time.deltaTime;
			if (GiveUpTime > 0f && TimeSinceTargetReacquired > GiveUpTime)
			{
				Disable_Networked(null);
				return;
			}
			if (InstanceFinder.IsServer)
			{
				if (IsSearching)
				{
					return;
				}
				_ = base.Npc.Avatar.CenterPoint;
				if (base.Npc.Movement.IsMoving)
				{
					_ = base.Npc.Movement.CurrentDestination;
				}
				if (isTargetRecentlyVisible)
				{
					if (IsTargetInRange(base.Npc.Avatar.CenterPoint) && isTargetImmediatelyVisible)
					{
						if (ReadyToAttack())
						{
							Attack();
						}
					}
					else
					{
						RepositionToTargetRange(lastKnownTargetPosition);
					}
				}
				else if (base.Npc.Movement.IsMoving)
				{
					if (Vector3.Distance(base.Npc.Movement.CurrentDestination, lastKnownTargetPosition) > 2f)
					{
						base.Npc.Movement.SetDestination(lastKnownTargetPosition);
					}
				}
				else if (Vector3.Distance(base.transform.position, lastKnownTargetPosition) < 2f)
				{
					StartSearching();
				}
				else
				{
					base.Npc.Movement.SetDestination(lastKnownTargetPosition);
				}
			}
			if (IsSearching && isTargetImmediatelyVisible)
			{
				StopSearching();
			}
		}

		protected virtual void Update()
		{
			if (TEST_MODE && Input.GetKeyDown(KeyCode.R))
			{
				SetTarget(null, Player.Local.NetworkObject);
				Enable_Networked(null);
			}
		}

		protected virtual void FixedUpdate()
		{
			if (base.Active)
			{
				CheckPlayerVisibility();
				UpdateTimeout();
			}
		}

		protected void UpdateTimeout()
		{
			if (InstanceFinder.IsServer && timeSinceLastSighting > GetSearchTime())
			{
				Disable_Networked(null);
			}
		}

		protected virtual void UpdateLookAt()
		{
			if (isTargetImmediatelyVisible && TargetPlayer != null)
			{
				base.Npc.Avatar.LookController.OverrideLookTarget(TargetPlayer.EyePosition.position, 10, rotateBody: true);
			}
		}

		protected void SetMovementSpeed(float speed)
		{
			base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("combat", 5, speed));
		}

		[ObserversRpc(RunLocally = true)]
		protected virtual void SetWeapon(string weaponPath)
		{
			RpcWriter___Observers_SetWeapon_3615296227(weaponPath);
			RpcLogic___SetWeapon_3615296227(weaponPath);
		}

		[ObserversRpc(RunLocally = true)]
		protected void ClearWeapon()
		{
			RpcWriter___Observers_ClearWeapon_2166136261();
			RpcLogic___ClearWeapon_2166136261();
		}

		protected virtual bool ReadyToAttack(bool checkTarget = true)
		{
			if (TimeSinceTargetReacquired < 0.5f && checkTarget)
			{
				return false;
			}
			if (currentWeapon != null)
			{
				return currentWeapon.IsReadyToAttack();
			}
			return VirtualPunchWeapon.IsReadyToAttack();
		}

		[ObserversRpc(RunLocally = true)]
		protected virtual void Attack()
		{
			RpcWriter___Observers_Attack_2166136261();
			RpcLogic___Attack_2166136261();
		}

		protected void SucessfulHit()
		{
			successfulHits++;
			if (GiveUpAfterSuccessfulHits > 0 && successfulHits >= GiveUpAfterSuccessfulHits)
			{
				Disable_Networked(null);
			}
		}

		protected void CheckPlayerVisibility()
		{
			if (TargetPlayer == null)
			{
				return;
			}
			base.Npc.awareness.VisionCone.StateSettings[TargetPlayer][PlayerVisualState.EVisualState.Visible].Enabled = !isTargetRecentlyVisible;
			if (IsPlayerVisible())
			{
				playerSightedDuration += Time.fixedDeltaTime;
				isTargetImmediatelyVisible = true;
			}
			else
			{
				playerSightedDuration = 0f;
				timeSinceLastSighting += Time.fixedDeltaTime;
				isTargetImmediatelyVisible = false;
				if (timeSinceLastSighting < 2.5f)
				{
					TargetPlayer.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: false);
					isTargetRecentlyVisible = true;
				}
				else
				{
					isTargetRecentlyVisible = false;
				}
			}
			if (isTargetRecentlyVisible)
			{
				MarkPlayerVisible();
			}
		}

		public void MarkPlayerVisible()
		{
			if (IsPlayerVisible())
			{
				TargetPlayer.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: true);
				timeSinceLastSighting = 0f;
			}
			else
			{
				TargetPlayer.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: false);
			}
		}

		protected bool IsPlayerVisible()
		{
			return base.Npc.awareness.VisionCone.IsPlayerVisible(TargetPlayer);
		}

		private void ProcessVisionEvent(VisionEventReceipt visionEventReceipt)
		{
			if (base.Active && visionEventReceipt.TargetPlayer == TargetPlayer.NetworkObject)
			{
				if (!isTargetRecentlyVisible)
				{
					TimeSinceTargetReacquired = 0f;
				}
				isTargetRecentlyVisible = true;
				isTargetImmediatelyVisible = true;
				if (PlayAngryVO)
				{
					base.Npc.PlayVO(EVOLineType.Angry);
					nextAngryVO = Time.time + UnityEngine.Random.Range(5f, 15f);
				}
			}
		}

		protected virtual float GetSearchTime()
		{
			return DefaultSearchTime;
		}

		private void StartSearching()
		{
			if (InstanceFinder.IsServer)
			{
				IsSearching = true;
				base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("searching", 6, 0.4f));
				searchRoutine = StartCoroutine(SearchRoutine());
			}
		}

		private void StopSearching()
		{
			if (InstanceFinder.IsServer)
			{
				IsSearching = false;
				base.Npc.Movement.SpeedController.RemoveSpeedControl("searching");
				hasSearchDestination = false;
				if (searchRoutine != null)
				{
					StopCoroutine(searchRoutine);
				}
			}
		}

		private IEnumerator SearchRoutine()
		{
			while (IsSearching)
			{
				if (!hasSearchDestination)
				{
					currentSearchDestination = GetNextSearchLocation();
					base.Npc.Movement.SetDestination(currentSearchDestination);
					hasSearchDestination = true;
				}
				while (true)
				{
					if (!base.Npc.Movement.IsMoving && base.Npc.Movement.CanMove())
					{
						base.Npc.Movement.SetDestination(currentSearchDestination);
					}
					if (Vector3.Distance(base.transform.position, currentSearchDestination) < 2f)
					{
						break;
					}
					yield return new WaitForSeconds(1f);
				}
				hasSearchDestination = false;
				yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 6f));
			}
			searchRoutine = null;
			StopSearching();
		}

		private Vector3 GetNextSearchLocation()
		{
			float a = Mathf.Lerp(25f, 60f, Mathf.Clamp(timeSinceLastSighting / TargetPlayer.CrimeData.GetSearchTime(), 0f, 1f));
			a = Mathf.Min(a, Vector3.Distance(base.transform.position, TargetPlayer.Avatar.CenterPoint));
			return GetRandomReachablePointNear(TargetPlayer.Avatar.CenterPoint, a);
		}

		protected bool IsTargetValid()
		{
			if (TargetPlayer == null)
			{
				return false;
			}
			if (TargetPlayer.IsArrested)
			{
				return false;
			}
			if (TargetPlayer.IsUnconscious)
			{
				return false;
			}
			if (!TargetPlayer.Health.IsAlive)
			{
				return false;
			}
			if (TargetPlayer.CrimeData.BodySearchPending)
			{
				return false;
			}
			if (Vector3.Distance(base.transform.position, TargetPlayer.Avatar.CenterPoint) > GiveUpRange)
			{
				return false;
			}
			return true;
		}

		private void RepositionToTargetRange(Vector3 origin)
		{
			if (!(TargetPlayer == null))
			{
				Vector3 randomReachablePointNear = GetRandomReachablePointNear(origin, GetMaxTargetDistance(), GetMinTargetDistance());
				base.Npc.Movement.SetDestination(randomReachablePointNear);
			}
		}

		private Vector3 GetRandomReachablePointNear(Vector3 point, float randomRadius, float minDistance = 0f)
		{
			bool flag = false;
			Vector3 result = point;
			int num = 0;
			while (!flag)
			{
				Vector3 vector = UnityEngine.Random.insideUnitSphere * randomRadius;
				NavMeshUtility.SamplePosition(point + vector, out var hit, randomRadius, base.Npc.Movement.Agent.areaMask);
				if (base.Npc.Movement.CanGetTo(hit.position, 2f) && Vector3.Distance(point, hit.position) > minDistance)
				{
					flag = true;
					result = hit.position;
					break;
				}
				num++;
				if (num > 20)
				{
					Console.LogError("Failed to find search destination");
					break;
				}
			}
			return result;
		}

		protected float GetMinTargetDistance()
		{
			if (overrideTargetDistance)
			{
				return targetDistanceOverride;
			}
			if (currentWeapon != null)
			{
				return currentWeapon.MinUseRange;
			}
			return 0f;
		}

		protected float GetMaxTargetDistance()
		{
			if (overrideTargetDistance)
			{
				return targetDistanceOverride;
			}
			if (currentWeapon != null)
			{
				return currentWeapon.MaxUseRange;
			}
			return 1.5f;
		}

		protected bool IsTargetInRange(Vector3 origin = default(Vector3))
		{
			if (origin == default(Vector3))
			{
				origin = base.transform.position;
			}
			float num = Vector3.Distance(origin, TargetPlayer.Avatar.CenterPoint);
			if (num > GetMinTargetDistance())
			{
				return num < GetMaxTargetDistance();
			}
			return false;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_SetTarget_1824087381);
				RegisterObserversRpc(16u, RpcReader___Observers_SetWeapon_3615296227);
				RegisterObserversRpc(17u, RpcReader___Observers_ClearWeapon_2166136261);
				RegisterObserversRpc(18u, RpcReader___Observers_Attack_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_SetTarget_1824087381(NetworkConnection conn, NetworkObject target)
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
				writer.WriteNetworkConnection(conn);
				writer.WriteNetworkObject(target);
				SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public virtual void RpcLogic___SetTarget_1824087381(NetworkConnection conn, NetworkObject target)
		{
			TargetPlayer = target.GetComponent<Player>();
			playerSightedDuration = 0f;
			timeSinceLastSighting = 0f;
			TimeSinceTargetReacquired = 0f;
		}

		private void RpcReader___Observers_SetTarget_1824087381(PooledReader PooledReader0, Channel channel)
		{
			NetworkConnection conn = PooledReader0.ReadNetworkConnection();
			NetworkObject target = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetTarget_1824087381(conn, target);
			}
		}

		private void RpcWriter___Observers_SetWeapon_3615296227(string weaponPath)
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
				writer.WriteString(weaponPath);
				SendObserversRpc(16u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___SetWeapon_3615296227(string weaponPath)
		{
			if (currentWeapon != null)
			{
				if (weaponPath == currentWeapon.AssetPath)
				{
					return;
				}
				ClearWeapon();
			}
			if (!(weaponPath == string.Empty))
			{
				VirtualPunchWeapon.onSuccessfulHit.RemoveListener(SucessfulHit);
				currentWeapon = base.Npc.SetEquippable_Return(weaponPath) as AvatarWeapon;
				currentWeapon.onSuccessfulHit.AddListener(SucessfulHit);
				if (currentWeapon == null)
				{
					Console.LogError("Failed to equip weapon");
				}
			}
		}

		private void RpcReader___Observers_SetWeapon_3615296227(PooledReader PooledReader0, Channel channel)
		{
			string weaponPath = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetWeapon_3615296227(weaponPath);
			}
		}

		private void RpcWriter___Observers_ClearWeapon_2166136261()
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
				SendObserversRpc(17u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		protected void RpcLogic___ClearWeapon_2166136261()
		{
			if (!(currentWeapon == null))
			{
				currentWeapon.onSuccessfulHit.RemoveListener(SucessfulHit);
				base.Npc.SetEquippable_Networked(null, string.Empty);
				currentWeapon = null;
				VirtualPunchWeapon.onSuccessfulHit.AddListener(SucessfulHit);
			}
		}

		private void RpcReader___Observers_ClearWeapon_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ClearWeapon_2166136261();
			}
		}

		private void RpcWriter___Observers_Attack_2166136261()
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
				SendObserversRpc(18u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___Attack_2166136261()
		{
			if (ReadyToAttack(checkTarget: false))
			{
				if (currentWeapon != null)
				{
					currentWeapon.Attack();
				}
				else
				{
					VirtualPunchWeapon.Attack();
				}
			}
		}

		private void RpcReader___Observers_Attack_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Attack_2166136261();
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ECombat_002ECombatBehaviour_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			VisionCone visionCone = base.Npc.awareness.VisionCone;
			visionCone.onVisionEventFull = (VisionCone.EventStateChange)Delegate.Combine(visionCone.onVisionEventFull, new VisionCone.EventStateChange(ProcessVisionEvent));
			VirtualPunchWeapon.Equip(base.Npc.Avatar);
		}
	}
}
