using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.Noise;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Vision;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class PursuitBehaviour : Behaviour
	{
		private enum EPursuitAction
		{
			None = 0,
			Move = 1,
			Shoot = 2,
			MoveAndShoot = 3
		}

		public const float ARREST_RANGE = 2.5f;

		public const float ARREST_TIME = 1.75f;

		public const float EXTRA_VISIBILITY_TIME = 2f;

		public const float MOVE_SPEED_INVESTIGATING = 0.35f;

		public const float MOVE_SPEED_ARRESTING = 0.6f;

		public const float MOVE_SPEED_CHASE = 0.8f;

		public const float MOVE_SPEED_SHOOTING = 0.15f;

		public const float SEARCH_RADIUS_MIN = 25f;

		public const float SEARCH_RADIUS_MAX = 80f;

		public const float ARREST_MAX_DISTANCE = 15f;

		public const int LEAVE_ARREST_CIRCLE_LIMIT = 3;

		public const float CONSECUTIVE_MISS_ACCURACY_BOOST = 0.1f;

		[Header("Settings")]
		public float ArrestCircle_MaxVisibleDistance = 5f;

		public float ArrestCircle_MaxOpacity = 0.25f;

		[SyncVar(SendRate = 0.25f, WritePermissions = WritePermission.ClientUnsynchronized)]
		public bool isTargetVisible;

		protected bool isTargetStrictlyVisible;

		protected bool arrestingEnabled;

		protected float timeSinceLastSighting = 10000f;

		protected float currentPursuitLevelDuration;

		protected float timeWithinArrestRange;

		protected float playerSightedDuration;

		protected float distanceOnPursuitStart;

		private Coroutine searchRoutine;

		private Coroutine rangedWeaponRoutine;

		private Vector3 currentSearchDestination = Vector3.zero;

		private bool hasSearchDestination;

		private PoliceOfficer officer;

		private bool targetWasDrivingOnPursuitStart;

		private bool wasInArrestCircleLastFrame;

		private int leaveArrestCircleCount;

		private AvatarRangedWeapon rangedWeapon;

		private int consecutiveMissedShots;

		private float nextAngryVO;

		public SyncVar<bool> syncVar___isTargetVisible;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public Player TargetPlayer { get; protected set; }

		public bool IsSearching { get; protected set; }

		public bool SyncAccessor_isTargetVisible
		{
			get
			{
				return isTargetVisible;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					isTargetVisible = value;
				}
				if (Application.isPlaying)
				{
					syncVar___isTargetVisible.SetValue(value, value);
				}
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviour_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		private void OnDestroy()
		{
			PoliceOfficer.OnPoliceVisionEvent = (Action<VisionEventReceipt>)Delegate.Remove(PoliceOfficer.OnPoliceVisionEvent, new Action<VisionEventReceipt>(ProcessThirdPartyVisionEvent));
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (base.Active && TargetPlayer != null)
			{
				AssignTarget(connection, TargetPlayer.NetworkObject);
			}
		}

		[ObserversRpc(RunLocally = true)]
		public virtual void AssignTarget(NetworkConnection conn, NetworkObject target)
		{
			RpcWriter___Observers_AssignTarget_1824087381(conn, target);
			RpcLogic___AssignTarget_1824087381(conn, target);
		}

		protected override void Begin()
		{
			base.Begin();
			CheckPlayerVisibility();
			syncVar___isTargetVisible.SetValue(true,  true);
			nextAngryVO = Time.time + UnityEngine.Random.Range(5f, 15f);
			officer.ProxCircle.SetRadius(2.5f);
			officer.Avatar.EmotionManager.AddEmotionOverride((UnityEngine.Random.Range(0f, 1f) > 0.5f) ? "Angry" : "Annoyed", "pursuit");
			officer.Movement.SetStance(NPCMovement.EStance.Stanced);
			officer.Movement.SetAgentType(NPCMovement.EAgentType.IgnoreCosts);
		}

		protected override void Resume()
		{
			base.Resume();
			CheckPlayerVisibility();
			syncVar___isTargetVisible.SetValue(true,  true);
			nextAngryVO = Time.time + UnityEngine.Random.Range(5f, 15f);
			officer.ProxCircle.SetRadius(2.5f);
			officer.Avatar.EmotionManager.AddEmotionOverride((UnityEngine.Random.Range(0f, 1f) > 0.5f) ? "Angry" : "Annoyed", "pursuit");
			officer.Movement.SetStance(NPCMovement.EStance.Stanced);
			officer.Movement.SetAgentType(NPCMovement.EAgentType.IgnoreCosts);
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			UpdateLookAt();
			UpdateArrest();
			UpdateArrestCircle();
			if (Time.time > nextAngryVO)
			{
				EVOLineType lineType = ((UnityEngine.Random.Range(0, 2) == 0) ? EVOLineType.Angry : EVOLineType.Command);
				base.Npc.PlayVO(lineType);
				nextAngryVO = Time.time + UnityEngine.Random.Range(5f, 15f);
			}
			SetWorldspaceIconsActive(timeSinceLastSighting < 3f || timeSinceLastSighting > 10f);
			if (InstanceFinder.IsServer)
			{
				if (!IsTargetValid())
				{
					Disable_Networked(null);
					return;
				}
				if (rangedWeaponRoutine != null && TargetPlayer.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.NonLethal && TargetPlayer.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.Lethal)
				{
					StopRangedWeaponRoutine();
				}
			}
			if (!IsTargetValid())
			{
				return;
			}
			if (IsSearching)
			{
				if (SyncAccessor_isTargetVisible || TargetPlayer.CrimeData.TimeSinceSighted < 1f)
				{
					StopSearching();
				}
			}
			else
			{
				switch (TargetPlayer.CrimeData.CurrentPursuitLevel)
				{
				case PlayerCrimeData.EPursuitLevel.None:
					if (InstanceFinder.IsServer)
					{
						End_Networked(null);
					}
					break;
				case PlayerCrimeData.EPursuitLevel.Investigating:
					UpdateInvestigatingBehaviour();
					break;
				case PlayerCrimeData.EPursuitLevel.Arresting:
					UpdateArrestBehaviour();
					break;
				case PlayerCrimeData.EPursuitLevel.NonLethal:
					UpdateNonLethalBehaviour();
					break;
				case PlayerCrimeData.EPursuitLevel.Lethal:
					UpdateLethalBehaviour();
					break;
				}
			}
			UpdateEquippable();
		}

		private bool IsTargetValid()
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
			if (TargetPlayer.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
			{
				return false;
			}
			return true;
		}

		protected virtual void FixedUpdate()
		{
			if (base.Active)
			{
				CheckPlayerVisibility();
				currentPursuitLevelDuration += Time.fixedDeltaTime;
			}
		}

		protected virtual void UpdateInvestigatingBehaviour()
		{
			arrestingEnabled = false;
			if (InstanceFinder.IsServer && !base.Npc.Movement.SpeedController.DoesSpeedControlExist("investigating"))
			{
				base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("investigating", 50, 0.35f));
			}
			if (!(Vector3.Distance(base.transform.position, TargetPlayer.Avatar.CenterPoint) >= 2.5f))
			{
				return;
			}
			if (isTargetStrictlyVisible && SyncAccessor_isTargetVisible)
			{
				TargetPlayer.CrimeData.Escalate();
			}
			else
			{
				if (!InstanceFinder.IsServer)
				{
					return;
				}
				if (!base.Npc.Movement.IsMoving && Vector3.Distance(TargetPlayer.CrimeData.LastKnownPosition, base.transform.position) < 2.5f)
				{
					StartSearching();
				}
				else if (!base.Npc.Movement.IsMoving || Vector3.Distance(TargetPlayer.CrimeData.LastKnownPosition, base.Npc.Movement.CurrentDestination) > 2.5f)
				{
					if (base.Npc.Movement.CanGetTo(TargetPlayer.CrimeData.LastKnownPosition, 2.5f))
					{
						base.Npc.Movement.SetDestination(TargetPlayer.CrimeData.LastKnownPosition);
					}
					else
					{
						StartSearching();
					}
				}
			}
		}

		protected virtual void UpdateArrestBehaviour()
		{
			arrestingEnabled = true;
			if (InstanceFinder.IsServer)
			{
				if (!base.Npc.Movement.SpeedController.DoesSpeedControlExist("arresting"))
				{
					base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("arresting", 50, 0.6f));
				}
				if (Vector3.Distance(base.transform.position, TargetPlayer.Avatar.CenterPoint) >= 2.5f)
				{
					if (SyncAccessor_isTargetVisible)
					{
						bool flag = false;
						if (!base.Npc.Movement.IsMoving)
						{
							flag = true;
						}
						if (Vector3.Distance(TargetPlayer.Avatar.CenterPoint, base.Npc.Movement.CurrentDestination) > 2.5f)
						{
							flag = true;
						}
						if (flag)
						{
							base.Npc.Movement.SetDestination(GetNewArrestDestination());
						}
					}
					else
					{
						if (!base.Npc.Movement.IsMoving && Vector3.Distance(TargetPlayer.CrimeData.LastKnownPosition, base.transform.position) < 2.5f)
						{
							StartSearching();
							return;
						}
						if (!base.Npc.Movement.IsMoving || Vector3.Distance(TargetPlayer.CrimeData.LastKnownPosition, base.Npc.Movement.CurrentDestination) > 2.5f)
						{
							if (!base.Npc.Movement.CanGetTo(TargetPlayer.CrimeData.LastKnownPosition, 2.5f))
							{
								StartSearching();
								return;
							}
							base.Npc.Movement.SetDestination(TargetPlayer.CrimeData.LastKnownPosition);
						}
					}
				}
			}
			if (Vector3.Distance(base.transform.position, TargetPlayer.Avatar.CenterPoint) > Mathf.Max(15f, distanceOnPursuitStart + 5f) && timeSinceLastSighting < 1f)
			{
				Debug.Log("Target too far! Escalating");
				TargetPlayer.CrimeData.Escalate();
			}
			if (TargetPlayer.CurrentVehicle != null && !targetWasDrivingOnPursuitStart && timeSinceLastSighting < 1f)
			{
				Debug.Log("Target got in vehicle! Escalating");
				TargetPlayer.CrimeData.Escalate();
			}
			if (leaveArrestCircleCount >= 3 && timeSinceLastSighting < 1f)
			{
				Debug.Log("Left arrest circle too many times! Escalating");
				TargetPlayer.CrimeData.Escalate();
			}
		}

		private void UpdateArrest()
		{
			if (TargetPlayer == null)
			{
				return;
			}
			if (Vector3.Distance(base.transform.position, TargetPlayer.Avatar.CenterPoint) < 2.5f && arrestingEnabled && SyncAccessor_isTargetVisible)
			{
				timeWithinArrestRange += Time.deltaTime;
				if (timeWithinArrestRange > 0.5f)
				{
					wasInArrestCircleLastFrame = true;
				}
			}
			else
			{
				if (wasInArrestCircleLastFrame)
				{
					leaveArrestCircleCount++;
					wasInArrestCircleLastFrame = false;
				}
				timeWithinArrestRange = Mathf.Clamp(timeWithinArrestRange - Time.deltaTime, 0f, float.MaxValue);
			}
			if (TargetPlayer.IsOwner && timeWithinArrestRange / 1.75f > TargetPlayer.CrimeData.CurrentArrestProgress)
			{
				TargetPlayer.CrimeData.SetArrestProgress(timeWithinArrestRange / 1.75f);
			}
		}

		private Vector3 GetNewArrestDestination()
		{
			return TargetPlayer.Avatar.CenterPoint + (base.transform.position - TargetPlayer.Avatar.CenterPoint).normalized * 0.75f;
		}

		private void ClearSpeedControls()
		{
			if (base.Npc.Movement.SpeedController.DoesSpeedControlExist("investigating"))
			{
				base.Npc.Movement.SpeedController.RemoveSpeedControl("investigating");
			}
			if (base.Npc.Movement.SpeedController.DoesSpeedControlExist("arresting"))
			{
				base.Npc.Movement.SpeedController.RemoveSpeedControl("arresting");
			}
			if (base.Npc.Movement.SpeedController.DoesSpeedControlExist("chasing"))
			{
				base.Npc.Movement.SpeedController.RemoveSpeedControl("chasing");
			}
			if (base.Npc.Movement.SpeedController.DoesSpeedControlExist("shooting"))
			{
				base.Npc.Movement.SpeedController.RemoveSpeedControl("shooting");
			}
		}

		protected virtual void UpdateNonLethalBehaviour()
		{
			arrestingEnabled = true;
			if (InstanceFinder.IsServer && rangedWeaponRoutine == null)
			{
				rangedWeaponRoutine = StartCoroutine(RangedWeaponRoutine());
			}
		}

		protected virtual void UpdateLethalBehaviour()
		{
			arrestingEnabled = true;
			if (InstanceFinder.IsServer && rangedWeaponRoutine == null)
			{
				rangedWeaponRoutine = StartCoroutine(RangedWeaponRoutine());
			}
		}

		private IEnumerator RangedWeaponRoutine()
		{
			EPursuitAction currentAction = EPursuitAction.None;
			float currentActionDuration = 0f;
			float currentActionTime = 0f;
			while (true)
			{
				if (rangedWeapon == null)
				{
					yield return new WaitForEndOfFrame();
					continue;
				}
				float num = Vector3.Distance(base.transform.position, TargetPlayer.Avatar.CenterPoint);
				if (SyncAccessor_isTargetVisible && num > rangedWeapon.MinUseRange && num < rangedWeapon.MaxUseRange)
				{
					currentActionDuration += Time.deltaTime;
					if (currentActionDuration > currentActionTime)
					{
						currentAction = EPursuitAction.None;
					}
					if (currentAction == EPursuitAction.None)
					{
						currentActionDuration = 0f;
						EPursuitAction ePursuitAction = ((!rangedWeapon.CanShootWhileMoving) ? ((UnityEngine.Random.Range(0, 2) == 0) ? EPursuitAction.Move : EPursuitAction.Shoot) : ((UnityEngine.Random.Range(0, 3) == 0) ? EPursuitAction.Move : ((!((double)num < (double)rangedWeapon.MaxUseRange * 0.5)) ? EPursuitAction.MoveAndShoot : EPursuitAction.Shoot)));
						if (TargetPlayer.CrimeData.timeSinceLastShot < 2f)
						{
							ePursuitAction = EPursuitAction.Move;
						}
						SetWeaponRaised(ePursuitAction == EPursuitAction.Shoot || ePursuitAction == EPursuitAction.MoveAndShoot);
						consecutiveMissedShots = 0;
						ClearSpeedControls();
						switch (ePursuitAction)
						{
						case EPursuitAction.Move:
							base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("chasing", 60, 0.8f));
							break;
						case EPursuitAction.MoveAndShoot:
							base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("shooting", 60, 0.15f));
							break;
						}
						currentActionTime = UnityEngine.Random.Range(3f, 6f);
						currentAction = ePursuitAction;
					}
					switch (currentAction)
					{
					case EPursuitAction.Move:
						if (arrestingEnabled)
						{
							if (!base.Npc.Movement.IsMoving && Vector3.Distance(base.Npc.Movement.transform.position, TargetPlayer.Avatar.CenterPoint) < 2.5f)
							{
								currentAction = EPursuitAction.None;
							}
							else if (!base.Npc.Movement.IsMoving || Vector3.Distance(base.Npc.Movement.CurrentDestination, TargetPlayer.Avatar.CenterPoint) > 2.5f)
							{
								base.Npc.Movement.SetDestination(TargetPlayer.Avatar.CenterPoint);
							}
						}
						else if (!base.Npc.Movement.IsMoving && Vector3.Distance(base.Npc.Movement.transform.position, TargetPlayer.Avatar.CenterPoint) < rangedWeapon.MaxUseRange)
						{
							currentAction = EPursuitAction.None;
						}
						else if (!base.Npc.Movement.IsMoving || Vector3.Distance(base.Npc.Movement.CurrentDestination, TargetPlayer.Avatar.CenterPoint) > rangedWeapon.MaxUseRange)
						{
							float randomRadius2 = Mathf.Max(Mathf.Min(rangedWeapon.MaxUseRange * 0.6f, num), rangedWeapon.MinUseRange * 2f);
							base.Npc.Movement.SetDestination(GetRandomReachablePointNear(TargetPlayer.Avatar.CenterPoint, randomRadius2, rangedWeapon.MinUseRange));
						}
						break;
					case EPursuitAction.Shoot:
						if (base.Npc.Movement.IsMoving)
						{
							base.Npc.Movement.Stop();
						}
						if (Vector3.Distance(base.transform.position, TargetPlayer.Avatar.CenterPoint) > rangedWeapon.MaxUseRange)
						{
							currentAction = EPursuitAction.None;
						}
						if (CanShoot() && Shoot())
						{
							currentAction = EPursuitAction.None;
						}
						break;
					case EPursuitAction.MoveAndShoot:
						if (arrestingEnabled)
						{
							if (!base.Npc.Movement.IsMoving || Vector3.Distance(base.Npc.Movement.CurrentDestination, TargetPlayer.Avatar.CenterPoint) > 2.5f)
							{
								base.Npc.Movement.SetDestination(TargetPlayer.Avatar.CenterPoint);
							}
						}
						else if (!base.Npc.Movement.IsMoving || Vector3.Distance(base.Npc.Movement.CurrentDestination, TargetPlayer.Avatar.CenterPoint) > rangedWeapon.MaxUseRange)
						{
							float randomRadius = Mathf.Max(Mathf.Min(rangedWeapon.MaxUseRange * 0.6f, num), rangedWeapon.MinUseRange * 2f);
							base.Npc.Movement.SetDestination(GetRandomReachablePointNear(TargetPlayer.Avatar.CenterPoint, randomRadius, rangedWeapon.MinUseRange));
						}
						if (CanShoot() && Shoot())
						{
							currentAction = EPursuitAction.None;
						}
						break;
					}
				}
				else
				{
					ClearSpeedControls();
					base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("chasing", 60, 0.8f));
					SetWeaponRaised(raised: false);
					currentAction = EPursuitAction.Move;
					if (SyncAccessor_isTargetVisible)
					{
						if (arrestingEnabled)
						{
							if (!base.Npc.Movement.IsMoving || Vector3.Distance(base.Npc.Movement.CurrentDestination, TargetPlayer.Avatar.CenterPoint) > 2.5f)
							{
								base.Npc.Movement.SetDestination(TargetPlayer.Avatar.CenterPoint);
							}
						}
						else if (!base.Npc.Movement.IsMoving || Vector3.Distance(base.Npc.Movement.CurrentDestination, TargetPlayer.Avatar.CenterPoint) > rangedWeapon.MaxUseRange)
						{
							float randomRadius3 = Mathf.Max(Mathf.Min(rangedWeapon.MaxUseRange * 0.6f, num), rangedWeapon.MinUseRange * 2f);
							base.Npc.Movement.SetDestination(GetRandomReachablePointNear(TargetPlayer.Avatar.CenterPoint, randomRadius3, rangedWeapon.MinUseRange));
						}
					}
					else
					{
						if (!base.Npc.Movement.IsMoving && Vector3.Distance(TargetPlayer.CrimeData.LastKnownPosition, base.transform.position) < 2.5f)
						{
							StartSearching();
							StopRangedWeaponRoutine();
							yield break;
						}
						if (!base.Npc.Movement.IsMoving || Vector3.Distance(TargetPlayer.CrimeData.LastKnownPosition, base.Npc.Movement.CurrentDestination) > 2.5f)
						{
							if (!base.Npc.Movement.CanGetTo(TargetPlayer.CrimeData.LastKnownPosition, 2.5f))
							{
								break;
							}
							base.Npc.Movement.SetDestination(TargetPlayer.CrimeData.LastKnownPosition);
						}
					}
				}
				yield return new WaitForEndOfFrame();
			}
			StartSearching();
			StopRangedWeaponRoutine();
		}

		private bool CanShoot()
		{
			if (base.Npc.IsInVehicle)
			{
				return false;
			}
			if (base.Npc.Avatar.Ragdolled)
			{
				return false;
			}
			if (base.Npc.Avatar.Anim.StandUpAnimationPlaying)
			{
				return false;
			}
			if (!isTargetStrictlyVisible)
			{
				return false;
			}
			return rangedWeapon.CanShoot();
		}

		private bool Shoot()
		{
			bool flag = false;
			float num = Mathf.Lerp(rangedWeapon.HitChange_MinRange, rangedWeapon.HitChange_MaxRange, Mathf.Clamp01(Vector3.Distance(base.transform.position, TargetPlayer.Avatar.CenterPoint) / rangedWeapon.MaxUseRange));
			num *= TargetPlayer.CrimeData.GetShotAccuracyMultiplier();
			num *= 1f + 0.1f * (float)consecutiveMissedShots;
			if (UnityEngine.Random.Range(0f, 1f) < num)
			{
				flag = true;
			}
			Vector3 vector = TargetPlayer.Avatar.CenterPoint;
			bool flag2 = false;
			if (flag && rangedWeapon.IsPlayerInLoS(TargetPlayer))
			{
				flag2 = true;
			}
			else
			{
				vector += UnityEngine.Random.insideUnitSphere * 4f;
				Vector3 normalized = (vector - rangedWeapon.MuzzlePoint.position).normalized;
				vector = ((!Physics.Raycast(rangedWeapon.MuzzlePoint.position, normalized, out var hitInfo, rangedWeapon.MaxUseRange, LayerMask.GetMask(AvatarRangedWeapon.RaycastLayers))) ? (rangedWeapon.MuzzlePoint.position + normalized * rangedWeapon.MaxUseRange) : hitInfo.point);
			}
			if (flag2)
			{
				consecutiveMissedShots = 0;
				if (rangedWeapon is Taser)
				{
					TargetPlayer.Taze();
				}
				if (rangedWeapon.Damage > 0f && TargetPlayer.Health.CanTakeDamage)
				{
					TargetPlayer.Health.TakeDamage(rangedWeapon.Damage);
				}
				if (rangedWeapon is Handgun)
				{
					NoiseUtility.EmitNoise(rangedWeapon.MuzzlePoint.position, ENoiseType.Gunshot, 25f, base.Npc.gameObject);
				}
				TargetPlayer.CrimeData.ResetShotAccuracy();
			}
			else
			{
				consecutiveMissedShots++;
			}
			base.Npc.SendEquippableMessage_Networked_Vector(null, "Shoot", vector);
			return flag2;
		}

		private void SetWeaponRaised(bool raised)
		{
			if (rangedWeapon.IsRaised != raised)
			{
				if (raised)
				{
					base.Npc.SendEquippableMessage_Networked(null, "Raise");
				}
				else
				{
					base.Npc.SendEquippableMessage_Networked(null, "Lower");
				}
			}
		}

		private void StopRangedWeaponRoutine()
		{
			if (rangedWeaponRoutine != null)
			{
				StopCoroutine(rangedWeaponRoutine);
				rangedWeaponRoutine = null;
			}
		}

		protected virtual void UpdateLookAt()
		{
			if (SyncAccessor_isTargetVisible && TargetPlayer != null)
			{
				base.Npc.Avatar.LookController.OverrideLookTarget(TargetPlayer.EyePosition.position, 10, rotateBody: true);
			}
		}

		protected virtual void UpdateEquippable()
		{
			if (!base.Active || !InstanceFinder.IsServer)
			{
				return;
			}
			rangedWeapon = null;
			string text = string.Empty;
			if (TargetPlayer.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.Arresting)
			{
				text = officer.BatonPrefab.AssetPath;
				officer.belt.SetBatonVisible(vis: false);
			}
			else
			{
				officer.belt.SetBatonVisible(vis: true);
			}
			if (TargetPlayer.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.NonLethal)
			{
				text = officer.TaserPrefab.AssetPath;
				officer.belt.SetTaserVisible(vis: false);
			}
			else
			{
				officer.belt.SetTaserVisible(vis: true);
			}
			if (TargetPlayer.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.Lethal)
			{
				text = officer.GunPrefab.AssetPath;
				officer.belt.SetGunVisible(vis: false);
			}
			else
			{
				officer.belt.SetGunVisible(vis: true);
			}
			if (text != string.Empty)
			{
				if (base.Npc.Avatar.CurrentEquippable == null || base.Npc.Avatar.CurrentEquippable.AssetPath != text)
				{
					base.Npc.SetEquippable_Networked(null, text);
				}
				if (base.Npc.Avatar.CurrentEquippable is AvatarRangedWeapon)
				{
					rangedWeapon = base.Npc.Avatar.CurrentEquippable as AvatarRangedWeapon;
				}
			}
			else if (base.Npc.Avatar.CurrentEquippable != null)
			{
				base.Npc.SetEquippable_Networked(null, string.Empty);
			}
		}

		public override void Disable()
		{
			base.Disable();
			TargetPlayer = null;
			End();
		}

		protected override void Pause()
		{
			base.Pause();
			Stop();
		}

		protected override void End()
		{
			base.End();
			Stop();
		}

		private void Stop()
		{
			ClearSpeedControls();
			SetArrestCircleAlpha(0f);
			StopSearching();
			StopRangedWeaponRoutine();
			ClearEquippables();
			officer.Movement.SetAgentType(NPCMovement.EAgentType.Humanoid);
			officer.Movement.SetStance(NPCMovement.EStance.None);
			officer.Avatar.EmotionManager.RemoveEmotionOverride("pursuit");
			arrestingEnabled = false;
			timeSinceLastSighting = 10000f;
			currentPursuitLevelDuration = 0f;
			timeWithinArrestRange = 0f;
			rangedWeapon = null;
			if (TargetPlayer != null)
			{
				base.Npc.awareness.VisionCone.StateSettings[TargetPlayer][PlayerVisualState.EVisualState.Visible].Enabled = false;
			}
		}

		private void ClearEquippables()
		{
			base.Npc.SetEquippable_Networked(null, string.Empty);
			if (officer.belt != null)
			{
				officer.belt.SetBatonVisible(vis: true);
				officer.belt.SetTaserVisible(vis: true);
				officer.belt.SetGunVisible(vis: true);
			}
		}

		protected void CheckPlayerVisibility()
		{
			if (TargetPlayer == null)
			{
				return;
			}
			if (IsPlayerVisible())
			{
				playerSightedDuration += Time.fixedDeltaTime;
				syncVar___isTargetVisible.SetValue( true,  true);
				isTargetStrictlyVisible = true;
			}
			else
			{
				playerSightedDuration = 0f;
				timeSinceLastSighting += Time.fixedDeltaTime;
				syncVar___isTargetVisible.SetValue( false,  true);
				isTargetStrictlyVisible = false;
				if (timeSinceLastSighting < 2f)
				{
					TargetPlayer.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: false);
					syncVar___isTargetVisible.SetValue( true,  true);
				}
			}
			if (SyncAccessor_isTargetVisible)
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
			if (base.Active && visionEventReceipt.TargetPlayer == TargetPlayer.NetworkObject && visionEventReceipt.State == PlayerVisualState.EVisualState.SearchedFor)
			{
				syncVar___isTargetVisible.SetValue( true,  true);
				isTargetStrictlyVisible = true;
			}
		}

		private void ProcessThirdPartyVisionEvent(VisionEventReceipt visionEventReceipt)
		{
			if (base.Active && visionEventReceipt.TargetPlayer == TargetPlayer.NetworkObject && visionEventReceipt.State == PlayerVisualState.EVisualState.SearchedFor)
			{
				syncVar___isTargetVisible.SetValue( true,  true);
				isTargetStrictlyVisible = true;
			}
		}

		protected virtual void UpdateArrestCircle()
		{
			if (TargetPlayer == null || !arrestingEnabled || TargetPlayer != Player.Local)
			{
				SetArrestCircleAlpha(0f);
				return;
			}
			if (TargetPlayer.CrimeData.NearestOfficer != base.Npc)
			{
				SetArrestCircleAlpha(0f);
				return;
			}
			if (!SyncAccessor_isTargetVisible)
			{
				SetArrestCircleAlpha(0f);
				return;
			}
			float num = Vector3.Distance(TargetPlayer.Avatar.CenterPoint, base.transform.position);
			if (num < 2.5f)
			{
				SetArrestCircleAlpha(ArrestCircle_MaxOpacity);
				SetArrestCircleColor(new Color32(byte.MaxValue, 50, 50, byte.MaxValue));
			}
			else if (num < ArrestCircle_MaxVisibleDistance)
			{
				float arrestCircleAlpha = Mathf.Lerp(ArrestCircle_MaxOpacity, 0f, (num - 2.5f) / (ArrestCircle_MaxVisibleDistance - 2.5f));
				SetArrestCircleAlpha(arrestCircleAlpha);
				SetArrestCircleColor(Color.white);
			}
			else
			{
				SetArrestCircleAlpha(0f);
			}
		}

		public void ResetArrestProgress()
		{
			timeWithinArrestRange = 0f;
		}

		private void SetArrestCircleAlpha(float alpha)
		{
			officer.ProxCircle.SetAlpha(alpha);
		}

		private void SetArrestCircleColor(Color col)
		{
			officer.ProxCircle.SetColor(col);
		}

		private void StartSearching()
		{
			if (InstanceFinder.IsServer)
			{
				IsSearching = true;
				searchRoutine = StartCoroutine(SearchRoutine());
			}
		}

		private void StopSearching()
		{
			if (InstanceFinder.IsServer)
			{
				IsSearching = false;
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
					if (Vector3.Distance(base.transform.position, currentSearchDestination) < 2.5f)
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
			float a = Mathf.Lerp(25f, 80f, Mathf.Clamp(timeSinceLastSighting / TargetPlayer.CrimeData.GetSearchTime(), 0f, 1f));
			a = Mathf.Min(a, Vector3.Distance(base.transform.position, TargetPlayer.Avatar.CenterPoint));
			return GetRandomReachablePointNear(TargetPlayer.Avatar.CenterPoint, a);
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
				if (base.Npc.Movement.CanGetTo(hit.position, 2.5f) && Vector3.Distance(point, hit.position) > minDistance)
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

		private void SetWorldspaceIconsActive(bool active)
		{
			base.Npc.awareness.VisionCone.WorldspaceIconsEnabled = active;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar___isTargetVisible = new SyncVar<bool>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, 0.25f, Channel.Reliable, isTargetVisible);
				RegisterObserversRpc(15u, RpcReader___Observers_AssignTarget_1824087381);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviour);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar___isTargetVisible.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_AssignTarget_1824087381(NetworkConnection conn, NetworkObject target)
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

		public virtual void RpcLogic___AssignTarget_1824087381(NetworkConnection conn, NetworkObject target)
		{
			TargetPlayer = target.GetComponent<Player>();
			playerSightedDuration = 0f;
			timeSinceLastSighting = 0f;
			timeWithinArrestRange = 0f;
			leaveArrestCircleCount = 0;
			wasInArrestCircleLastFrame = false;
			distanceOnPursuitStart = Vector3.Distance(base.transform.position, TargetPlayer.Avatar.CenterPoint);
			targetWasDrivingOnPursuitStart = TargetPlayer.CurrentVehicle != null;
		}

		private void RpcReader___Observers_AssignTarget_1824087381(PooledReader PooledReader0, Channel channel)
		{
			NetworkConnection conn = PooledReader0.ReadNetworkConnection();
			NetworkObject target = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___AssignTarget_1824087381(conn, target);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviour(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 0)
			{
				if (PooledReader0 == null)
				{
					syncVar___isTargetVisible.SetValue(syncVar___isTargetVisible.GetValue(calledByUser: true),  true);
					return true;
				}
				bool value = PooledReader0.ReadBoolean();
				syncVar___isTargetVisible.SetValue(value, Boolean2);
				return true;
			}
			return false;
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviour_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			officer = base.Npc as PoliceOfficer;
			VisionCone visionCone = officer.awareness.VisionCone;
			visionCone.onVisionEventFull = (VisionCone.EventStateChange)Delegate.Combine(visionCone.onVisionEventFull, new VisionCone.EventStateChange(ProcessVisionEvent));
			PoliceOfficer.OnPoliceVisionEvent = (Action<VisionEventReceipt>)Delegate.Combine(PoliceOfficer.OnPoliceVisionEvent, new Action<VisionEventReceipt>(ProcessThirdPartyVisionEvent));
		}
	}
}
