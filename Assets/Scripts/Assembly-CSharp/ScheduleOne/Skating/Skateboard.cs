using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Skating
{
	public class Skateboard : NetworkBehaviour
	{
		public const float GroundedRaycastDistance = 0.1f;

		public const float JumpCooldown = 0.3f;

		public const float JumpForceMin = 0.5f;

		public const float JumpForceBuildTime = 0.5f;

		public const float PushCooldown = 1f;

		public const float PushStaminaConsumption = 12.5f;

		public const float PitchLimit = 60f;

		public const float RollLimit = 20f;

		[Header("Info - Readonly")]
		public float CurrentSpeed_Kmh;

		[CompilerGenerated]
		[SyncVar(Channel = Channel.Unreliable)]
		public float _003CJumpBuildAmount_003Ek__BackingField;

		[Header("References")]
		public Rigidbody Rb;

		public Transform CoM;

		public Transform[] HoverPoints;

		public Transform FrontAxlePosition;

		public Transform RearAxlePosition;

		public Transform PlayerContainer;

		public SkateboardAnimation Animation;

		public SmoothedVelocityCalculator VelocityCalculator;

		public AverageAcceleration Accelerometer;

		public Skateboard_Equippable Equippable;

		[Header("Turn Settings")]
		public float TurnForce = 1f;

		public float TurnChangeRate = 2f;

		public float TurnReturnToRestRate = 1f;

		public float TurnSpeedBoost = 1f;

		public AnimationCurve TurnForceMap;

		[Header("Settings")]
		public float Gravity = 10f;

		public float BrakeForce = 1f;

		public float ReverseTopSpeed_Kmh = 5f;

		public LayerMask GroundDetectionMask;

		public Collider[] MainColliders;

		public float RotationClampForce = 1f;

		[Header("Friction Settings")]
		public bool FrictionEnabled = true;

		public AnimationCurve LongitudinalFrictionCurve;

		public float LongitudinalFrictionMultiplier = 1f;

		public float LateralFrictionForceMultiplier = 1f;

		[Header("Jump Settings")]
		public float JumpForce = 1f;

		public float JumpDuration_Min = 0.2f;

		public float JumpDuration_Max = 0.5f;

		public AnimationCurve FrontAxleJumpCurve;

		public AnimationCurve RearAxleJumpCurve;

		public AnimationCurve JumpForwardForceCurve;

		public float JumpForwardBoost = 1f;

		[Header("Hover Settings")]
		public float HoverForce = 1f;

		public float HoverRayLength = 0.1f;

		public float HoverHeight = 0.05f;

		public float Hover_P = 1f;

		public float Hover_I = 1f;

		public float Hover_D = 1f;

		[Header("Pushing Setings")]
		[Tooltip("Top speed in m/s")]
		public float TopSpeed_Kmh = 10f;

		public float PushForceMultiplier = 1f;

		public AnimationCurve PushForceMultiplierMap;

		public float PushForceDuration = 0.4f;

		public float PushDelay = 0.35f;

		public AnimationCurve PushForceCurve;

		[Header("Air Movement")]
		public bool AirMovementEnabled = true;

		public float AirMovementForce = 1f;

		public float AirMovementJumpReductionDuration = 0.25f;

		public AnimationCurve AirMovementJumpReductionCurve;

		[Header("Events")]
		public UnityEvent OnPushStart;

		public UnityEvent<float> OnJump;

		public UnityEvent OnLand;

		private int horizontalInput;

		private bool jumpReleased;

		private float timeSinceLastJump;

		private float timeGrounded;

		private float timeAirborne = 0.21f;

		private float jumpHeldTime;

		private float frontAxleForce;

		private float rearAxleForce;

		private float jumpForwardForce;

		private List<PID> hoverPIDs = new List<PID>();

		private bool pushQueued;

		private bool isPushing;

		private float thisFramePushForce;

		private float timeSincePushStart = 2f;

		private bool braking;

		public SyncVar<float> syncVar____003CJumpBuildAmount_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002ESkating_002ESkateboardAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ESkating_002ESkateboardAssembly_002DCSharp_002Edll_Excuted;

		public float CurrentSteerInput { get; protected set; }

		public bool IsPushing => isPushing;

		public float TimeSincePushStart => timeSincePushStart;

		public bool isGrounded => timeGrounded > 0f;

		public float AirTime => timeAirborne;

		public float JumpBuildAmount
		{
			[CompilerGenerated]
			get
			{
				return _003CJumpBuildAmount_003Ek__BackingField;
			}
			[CompilerGenerated]
			[ServerRpc]
			set
			{
				_003CJumpBuildAmount_003Ek__BackingField = value;

                RpcWriter___Server_set_JumpBuildAmount_431000436(value);
			}
		}

		public float TopSpeed_Ms => TopSpeed_Kmh / 3.6f;

		public float SyncAccessor__003CJumpBuildAmount_003Ek__BackingField
		{
			get
			{
				return JumpBuildAmount;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					JumpBuildAmount = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CJumpBuildAmount_003Ek__BackingField.SetValue(value, value != 0);
				}
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ESkating_002ESkateboard_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public void Update()
		{
			GetInput();
			if (base.IsOwner)
			{
				Rb.interpolation = RigidbodyInterpolation.Interpolate;
			}
		}

		private void GetInput()
		{
			if (!base.IsOwner)
			{
				return;
			}
			if (Player.Local.IsTased)
			{
				if (Random.Range(0f, 1f) > 0.9f)
				{
					horizontalInput = Random.Range(-1, 2);
				}
			}
			else
			{
				horizontalInput = 0;
				if (GameInput.GetButton(GameInput.ButtonCode.Left))
				{
					if (Player.Local.Disoriented)
					{
						horizontalInput++;
					}
					else
					{
						horizontalInput--;
					}
				}
				if (GameInput.GetButton(GameInput.ButtonCode.Right))
				{
					if (Player.Local.Disoriented)
					{
						horizontalInput--;
					}
					else
					{
						horizontalInput++;
					}
				}
			}
			jumpReleased = false;
			if (GameInput.GetButton(GameInput.ButtonCode.Jump))
			{
				jumpHeldTime += Time.deltaTime;
			}
			else if (jumpHeldTime > 0f)
			{
				jumpReleased = true;
			}
			RpcWriter___Server_set_JumpBuildAmount_431000436(Mathf.Clamp01(jumpHeldTime / 0.5f));
			braking = GameInput.GetButton(GameInput.ButtonCode.Backward);
			if (GameInput.GetButton(GameInput.ButtonCode.Forward) && !isPushing && timeGrounded > 0.1f && !braking && timeSincePushStart >= 1f && jumpHeldTime == 0f && PlayerSingleton<PlayerMovement>.Instance.CurrentStaminaReserve >= 12.5f && !Player.Local.IsTased)
			{
				pushQueued = true;
				PlayerSingleton<PlayerMovement>.Instance.ChangeStamina(-12.5f);
			}
		}

		private void FixedUpdate()
		{
			if (!base.IsOwner)
			{
				CurrentSpeed_Kmh = VelocityCalculator.Velocity.magnitude * 3.6f;
				CheckGrounded();
				return;
			}
			ApplyInput();
			ApplyLateralFriction();
			UpdateHover();
			CheckGrounded();
			CheckJump();
			ApplyGravity();
		}

		private void LateUpdate()
		{
			if (base.IsOwner)
			{
				ClampRotation();
			}
		}

		private void ApplyInput()
		{
			Vector3 vector = base.transform.InverseTransformVector(Rb.velocity);
			bool flag = false;
			if (horizontalInput == 1)
			{
				CurrentSteerInput = Mathf.MoveTowards(CurrentSteerInput, 1f, Time.fixedDeltaTime * TurnChangeRate);
				flag = true;
			}
			else if (horizontalInput == -1)
			{
				CurrentSteerInput = Mathf.MoveTowards(CurrentSteerInput, -1f, Time.fixedDeltaTime * TurnChangeRate);
				flag = true;
			}
			else
			{
				CurrentSteerInput = Mathf.MoveTowards(CurrentSteerInput, 0f, Time.fixedDeltaTime * TurnReturnToRestRate);
			}
			float num = CurrentSteerInput * TurnForce * TurnForceMap.Evaluate(Mathf.Clamp01(Mathf.Abs(CurrentSpeed_Kmh / TopSpeed_Kmh)));
			if (vector.z < 0f)
			{
				num *= -1f;
			}
			Rb.AddTorque(base.transform.up * num, ForceMode.Acceleration);
			if (flag)
			{
				Rb.AddForce(base.transform.forward * Mathf.Abs(CurrentSteerInput) * TurnSpeedBoost, ForceMode.Acceleration);
			}
			timeSincePushStart += Time.deltaTime;
			if (pushQueued)
			{
				Push();
			}
			if (isPushing)
			{
				float num2 = PushForceMultiplierMap.Evaluate(Mathf.Clamp01(Rb.velocity.magnitude / TopSpeed_Ms));
				Rb.AddForce(base.transform.forward * thisFramePushForce * PushForceMultiplier * num2, ForceMode.Acceleration);
			}
			if (timeGrounded == 0f && AirMovementEnabled)
			{
				float num3 = 1f;
				if (timeAirborne < AirMovementJumpReductionDuration)
				{
					num3 = AirMovementJumpReductionCurve.Evaluate(timeAirborne / AirMovementJumpReductionDuration);
				}
				Rb.AddTorque(base.transform.right * GameInput.MotionAxis.y * AirMovementForce * num3, ForceMode.Acceleration);
			}
			if (braking)
			{
				float num4 = 1f;
				if (vector.z < 0f)
				{
					float num5 = Mathf.Clamp01(vector.z / (0f - ReverseTopSpeed_Kmh));
					num4 = 1f - num5;
				}
				Rb.AddForce(-base.transform.forward * BrakeForce * num4, ForceMode.Acceleration);
			}
			float magnitude = Rb.velocity.magnitude;
			CurrentSpeed_Kmh = magnitude * 3.6f;
		}

		private void ApplyLateralFriction()
		{
			if (FrictionEnabled)
			{
				Vector3 vector = base.transform.InverseTransformVector(Rb.velocity);
				Vector3 zero = Vector3.zero;
				float num = vector.x * LateralFrictionForceMultiplier;
				zero += -base.transform.right * num;
				float num2 = LongitudinalFrictionCurve.Evaluate(Mathf.Clamp01(vector.z) / TopSpeed_Ms);
				float num3 = vector.z * num2;
				Vector3 vector2 = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up);
				zero += -vector2 * num3;
				Rb.AddForce(zero, ForceMode.Acceleration);
				Vector3 velocity = Rb.velocity;
				float surfaceSmoothness = GetSurfaceSmoothness();
				Rb.AddForce(-velocity * (1f - surfaceSmoothness), ForceMode.Acceleration);
			}
		}

		private void UpdateHover()
		{
			List<float> list = new List<float>();
			for (int i = 0; i < HoverPoints.Length; i++)
			{
				if (Physics.Raycast(HoverPoints[i].position, -HoverPoints[i].up, out var hitInfo, HoverRayLength, GroundDetectionMask))
				{
					list.Add(hitInfo.distance);
					Debug.DrawLine(HoverPoints[i].position, hitInfo.point, Color.red);
					PID pID = hoverPIDs[i];
					pID.pFactor = Hover_P;
					pID.iFactor = Hover_I;
					pID.dFactor = Hover_D;
					float num = pID.Update(HoverHeight, hitInfo.distance, Time.fixedDeltaTime);
					num *= HoverForce;
					num = Mathf.Max(num, 0f);
					Rb.AddForceAtPosition(HoverPoints[i].up * num, HoverPoints[i].position, ForceMode.Acceleration);
				}
				else
				{
					list.Add(HoverRayLength);
					Debug.DrawRay(HoverPoints[i].position, -HoverPoints[i].up * HoverRayLength, Color.blue);
					hoverPIDs[i].Update(HoverHeight, HoverRayLength, Time.fixedDeltaTime);
				}
			}
		}

		private void ApplyGravity()
		{
			Rb.AddForce(Vector3.down * Gravity * Mathf.Sqrt(PlayerMovement.GravityMultiplier), ForceMode.Acceleration);
		}

		private void CheckGrounded()
		{
			if (IsGrounded())
			{
				timeGrounded += Time.fixedDeltaTime;
				if (timeGrounded > 0.05f)
				{
					if (timeAirborne > 0.2f && OnLand != null)
					{
						OnLand.Invoke();
					}
					timeAirborne = 0f;
				}
			}
			else
			{
				timeAirborne += Time.fixedDeltaTime;
				timeGrounded = 0f;
			}
		}

		private void CheckJump()
		{
			timeSinceLastJump += Time.fixedDeltaTime;
			if (frontAxleForce > 0f)
			{
				Rb.AddForceAtPosition(Vector3.up * frontAxleForce, FrontAxlePosition.position, ForceMode.Acceleration);
			}
			if (frontAxleForce > 0f)
			{
				Rb.AddForceAtPosition(Vector3.up * rearAxleForce, RearAxlePosition.position, ForceMode.Acceleration);
			}
			if (jumpForwardForce > 0f)
			{
				Rb.AddForce(Vector3.ProjectOnPlane(base.transform.forward, Vector3.up) * jumpForwardForce, ForceMode.Acceleration);
			}
			if (jumpReleased)
			{
				if (timeGrounded > 0.3f)
				{
					Jump();
				}
				jumpHeldTime = 0f;
			}
		}

		[ServerRpc(RunLocally = true)]
		private void SendJump(float jumpHeldTime)
		{
			RpcWriter___Server_SendJump_431000436(jumpHeldTime);
			RpcLogic___SendJump_431000436(jumpHeldTime);
		}

		[ObserversRpc(RunLocally = true)]
		private void ReceiveJump(float _jumpHeldTime)
		{
			RpcWriter___Observers_ReceiveJump_431000436(_jumpHeldTime);
			RpcLogic___ReceiveJump_431000436(_jumpHeldTime);
		}

		private void Jump()
		{
			SendJump(jumpHeldTime);
			float t = Mathf.Clamp01(jumpHeldTime / 0.5f);
			float JumpDuration = Mathf.Lerp(JumpDuration_Min, JumpDuration_Max, t);
			StartCoroutine(Jump());
			IEnumerator Jump()
			{
				for (float i = 0f; i < JumpDuration; i += Time.deltaTime)
				{
					if (timeGrounded > 0.2f)
					{
						Debug.LogError("Breaking jump");
						break;
					}
					frontAxleForce = FrontAxleJumpCurve.Evaluate(i / JumpDuration) * JumpForce;
					rearAxleForce = RearAxleJumpCurve.Evaluate(i / JumpDuration) * JumpForce;
					jumpForwardForce = JumpForwardForceCurve.Evaluate(i / JumpDuration) * JumpForwardBoost * (1f - Mathf.Clamp01(CurrentSpeed_Kmh / TopSpeed_Kmh));
					yield return new WaitForEndOfFrame();
				}
				frontAxleForce = 0f;
				rearAxleForce = 0f;
			}
		}

		private void Push()
		{
			pushQueued = false;
			isPushing = true;
			timeSincePushStart = 0f;
			if (OnPushStart != null)
			{
				OnPushStart.Invoke();
			}
			StartCoroutine(Push());
			IEnumerator Push()
			{
				yield return new WaitForSeconds(PushDelay);
				for (float i = 0f; i < PushForceDuration; i += Time.deltaTime)
				{
					if (braking)
					{
						break;
					}
					if (timeGrounded == 0f)
					{
						break;
					}
					thisFramePushForce = PushForceCurve.Evaluate(i / PushForceDuration);
					yield return new WaitForEndOfFrame();
				}
				isPushing = false;
				thisFramePushForce = 0f;
			}
		}

		public bool IsGrounded()
		{
			RaycastHit hit;
			return IsGrounded(out hit);
		}

		public bool IsGrounded(out RaycastHit hit)
		{
			if (Physics.Raycast(FrontAxlePosition.position + base.transform.up * 0.01f, -base.transform.up, out hit, 0.1f, GroundDetectionMask))
			{
				return true;
			}
			if (Physics.Raycast(RearAxlePosition.position + base.transform.up * 0.01f, -base.transform.up, out hit, 0.1f, GroundDetectionMask))
			{
				return true;
			}
			return false;
		}

		public void SetVelocity(Vector3 velocity)
		{
			Rb.isKinematic = false;
			Rb.velocity = velocity;
		}

		private void ClampRotation()
		{
			Vector3 normalized = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up).normalized;
			Vector3 normalized2 = Vector3.ProjectOnPlane(base.transform.right, Vector3.up).normalized;
			float num = Vector3.SignedAngle(base.transform.forward, normalized, base.transform.right);
			float num2 = Vector3.SignedAngle(normalized2, base.transform.right, base.transform.forward);
			if (Mathf.Abs(num) > 60f)
			{
				Rb.AddTorque(base.transform.right * num * RotationClampForce, ForceMode.Acceleration);
			}
			if (Mathf.Abs(num2) > 20f)
			{
				Rb.AddTorque(base.transform.forward * (0f - num2) * RotationClampForce, ForceMode.Acceleration);
			}
		}

		public float GetSurfaceSmoothness()
		{
			if (!IsGrounded(out var hit))
			{
				return 1f;
			}
			if (hit.collider.gameObject.tag == "Terrain")
			{
				return 0.4f;
			}
			return 1f;
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ESkating_002ESkateboardAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ESkating_002ESkateboardAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CJumpBuildAmount_003Ek__BackingField = new SyncVar<float>(this, 0u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Unreliable, JumpBuildAmount);
				RegisterServerRpc(0u, RpcReader___Server_set_JumpBuildAmount_431000436);
				RegisterServerRpc(1u, RpcReader___Server_SendJump_431000436);
				RegisterObserversRpc(2u, RpcReader___Observers_ReceiveJump_431000436);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002ESkating_002ESkateboard);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ESkating_002ESkateboardAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ESkating_002ESkateboardAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CJumpBuildAmount_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_set_JumpBuildAmount_431000436(float value)
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
			else if (!base.IsOwner)
			{
				NetworkManager networkManager2 = base.NetworkManager;
				if ((object)networkManager2 == null)
				{
					networkManager2 = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager2 != null)
				{
					networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because you are not the owner of this object. .");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteSingle(value);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		[SpecialName]
		public void RpcLogic___set_JumpBuildAmount_431000436(float value)
		{
			syncVar____003CJumpBuildAmount_003Ek__BackingField.SetValue(value, true);
		}

		private void RpcReader___Server_set_JumpBuildAmount_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			float value = PooledReader0.ReadSingle();
			if (base.IsServerInitialized && OwnerMatches(conn))
			{
				RpcLogic___set_JumpBuildAmount_431000436(value);
			}
		}

		private void RpcWriter___Server_SendJump_431000436(float jumpHeldTime)
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
			else if (!base.IsOwner)
			{
				NetworkManager networkManager2 = base.NetworkManager;
				if ((object)networkManager2 == null)
				{
					networkManager2 = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager2 != null)
				{
					networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because you are not the owner of this object. .");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteSingle(jumpHeldTime);
				SendServerRpc(1u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendJump_431000436(float jumpHeldTime)
		{
			ReceiveJump(jumpHeldTime);
		}

		private void RpcReader___Server_SendJump_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			float num = PooledReader0.ReadSingle();
			if (base.IsServerInitialized && OwnerMatches(conn) && !conn.IsLocalClient)
			{
				RpcLogic___SendJump_431000436(num);
			}
		}

		private void RpcWriter___Observers_ReceiveJump_431000436(float _jumpHeldTime)
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
				writer.WriteSingle(_jumpHeldTime);
				SendObserversRpc(2u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveJump_431000436(float _jumpHeldTime)
		{
			if (!(timeSinceLastJump < 0.3f))
			{
				timeSinceLastJump = 0f;
				timeGrounded = 0f;
				float arg = Mathf.Clamp01(_jumpHeldTime / 0.5f);
				if (OnJump != null)
				{
					OnJump.Invoke(arg);
				}
			}
		}

		private void RpcReader___Observers_ReceiveJump_431000436(PooledReader PooledReader0, Channel channel)
		{
			float num = PooledReader0.ReadSingle();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ReceiveJump_431000436(num);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002ESkating_002ESkateboard(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 0)
			{
				if (PooledReader0 == null)
				{
					syncVar____003CJumpBuildAmount_003Ek__BackingField.SetValue(syncVar____003CJumpBuildAmount_003Ek__BackingField.GetValue(calledByUser: true), true);
					return true;
				}
				float value = PooledReader0.ReadSingle();
				syncVar____003CJumpBuildAmount_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			return false;
		}

		private void Awake_UserLogic_ScheduleOne_002ESkating_002ESkateboard_Assembly_002DCSharp_002Edll()
		{
			Rb.centerOfMass = Rb.transform.InverseTransformPoint(CoM.position);
			Rb.useGravity = false;
			Rb.automaticInertiaTensor = false;
			for (int i = 0; i < HoverPoints.Length; i++)
			{
				PID item = new PID(Hover_P, Hover_I, Hover_D);
				hoverPIDs.Add(item);
			}
		}
	}
}
