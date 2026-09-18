using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Skating;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework.Animation
{
	public class AvatarAnimation : MonoBehaviour
	{
		public enum EFlinchType
		{
			Light = 0,
			Heavy = 1
		}

		public enum EFlinchDirection
		{
			Forward = 0,
			Backward = 1,
			Left = 2,
			Right = 3
		}

		public const float AnimationRangeSqr = 2500f;

		public const float RunningAnimationSpeed = 8f;

		public const float MaxBoneOffset = 0.01f;

		public const float MaxBoneOffsetSqr = 0.0001f;

		public static Vector3 SITTING_OFFSET = new Vector3(0f, -0.825f, 0f);

		public const float SEAT_TIME = 0.5f;

		public bool DEBUG_MODE;

		private int framesActive;

		[Header("References")]
		public Animator animator;

		public Transform HipBone;

		public Transform[] Bones;

		protected Avatar avatar;

		public Transform LeftHandContainer;

		public Transform RightHandContainer;

		public Transform RightHandAlignmentPoint;

		public Transform LeftHandAlignmentPoint;

		public AvatarIKController IKController;

		[Header("Settings")]
		public LayerMask GroundingMask;

		public string StandUpFromBackClipName;

		public string StandUpFromFrontClipName;

		public UnityEvent onStandupStart;

		public UnityEvent onStandupDone;

		public UnityEvent onHeavyFlinch;

		private BoneTransform[] standingBoneTransforms;

		private BoneTransform[] standUpFromBackBoneTransforms;

		private BoneTransform[] standUpFromFrontBoneTransforms;

		private BoneTransform[] ragdollBoneTransforms;

		private Coroutine standUpRoutine;

		private Coroutine seatRoutine;

		private Skateboard activeSkateboard;

		private bool animationEnabled = true;

		public bool IsCrouched { get; protected set; }

		public bool IsSeated => CurrentSeat != null;

		public float TimeSinceSitEnd { get; protected set; } = 1000f;


		public AvatarSeat CurrentSeat { get; protected set; }

		public bool StandUpAnimationPlaying { get; protected set; }

		protected virtual void Awake()
		{
			animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			avatar = GetComponent<Avatar>();
			avatar.onRagdollChange.AddListener(RagdollChange);
			standUpFromBackBoneTransforms = new BoneTransform[Bones.Length];
			standUpFromFrontBoneTransforms = new BoneTransform[Bones.Length];
			ragdollBoneTransforms = new BoneTransform[Bones.Length];
			standingBoneTransforms = new BoneTransform[Bones.Length];
			for (int i = 0; i < Bones.Length; i++)
			{
				standUpFromBackBoneTransforms[i] = new BoneTransform();
				standUpFromFrontBoneTransforms[i] = new BoneTransform();
				ragdollBoneTransforms[i] = new BoneTransform();
				standingBoneTransforms[i] = new BoneTransform();
			}
			PopulateBoneTransforms(standingBoneTransforms);
		}

		protected virtual void Start()
		{
			PopulateAnimationStartBoneTransforms(StandUpFromFrontClipName, standUpFromFrontBoneTransforms);
			PopulateAnimationStartBoneTransforms(StandUpFromBackClipName, standUpFromBackBoneTransforms);
			if (NetworkSingleton<TimeManager>.InstanceExists)
			{
				TimeManager instance = NetworkSingleton<TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
				TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
				instance2.onMinutePass = (Action)Delegate.Combine(instance2.onMinutePass, new Action(MinPass));
			}
			Player componentInParent = GetComponentInParent<Player>();
			if (componentInParent != null)
			{
				componentInParent.onSkateboardMounted = (Action<Skateboard>)Delegate.Combine(componentInParent.onSkateboardMounted, new Action<Skateboard>(SkateboardMounted));
				componentInParent.onSkateboardDismounted = (Action)Delegate.Combine(componentInParent.onSkateboardDismounted, new Action(SkateboardDismounted));
			}
			framesActive = 0;
		}

		private void OnDestroy()
		{
			if (NetworkSingleton<TimeManager>.InstanceExists)
			{
				TimeManager instance = NetworkSingleton<TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			}
		}

		private void OnEnable()
		{
			framesActive = 0;
			CheckBoneOffsets();
		}

		private void Update()
		{
			if (IsSeated)
			{
				TimeSinceSitEnd = 0f;
			}
			else
			{
				TimeSinceSitEnd += Time.deltaTime;
			}
			if (seatRoutine == null && CurrentSeat != null)
			{
				base.transform.position = CurrentSeat.SittingPoint.position + SITTING_OFFSET * base.transform.localScale.y;
				base.transform.rotation = CurrentSeat.SittingPoint.rotation;
			}
			UpdateAnimationActive();
			if (base.gameObject.activeInHierarchy)
			{
				framesActive++;
			}
		}

		private void MinPass()
		{
			if (Time.timeSinceLevelLoad > 3f && animator.cullingMode != AnimatorCullingMode.CullCompletely)
			{
				animator.cullingMode = AnimatorCullingMode.CullCompletely;
			}
			CheckBoneOffsets();
		}

		private void CheckBoneOffsets()
		{
			if (animator.enabled)
			{
				return;
			}
			for (int i = 0; i < Bones.Length; i++)
			{
				if (!(Bones[i] == HipBone) && Bones[i].localPosition.sqrMagnitude > 0.0001f)
				{
					Console.LogWarning("Avatar bone offset exceeded maximum, resetting to idle pose.");
					UpdateAnimationActive(forceWriteIdle: true);
					break;
				}
			}
		}

		private void UpdateAnimationActive(bool forceWriteIdle = false)
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				return;
			}
			bool flag = Vector3.SqrMagnitude(PlayerSingleton<PlayerCamera>.Instance.transform.position - base.transform.position) < 2500f * QualitySettings.lodBias;
			if (Time.timeSinceLevelLoad < 3f)
			{
				flag = true;
			}
			if (!flag && (animator.enabled || forceWriteIdle))
			{
				for (int i = 0; i < Bones.Length; i++)
				{
					Bones[i].localPosition = standingBoneTransforms[i].Position;
					Bones[i].localRotation = standingBoneTransforms[i].Rotation;
				}
			}
			animator.enabled = animationEnabled && flag;
		}

		public void SetDirection(float dir)
		{
			animator.SetFloat("Direction", dir);
		}

		public void SetStrafe(float strafe)
		{
			animator.SetFloat("Strafe", strafe);
		}

		public void SetTimeAirborne(float airbone)
		{
			animator.SetFloat("TimeAirborne", airbone);
		}

		public void SetCrouched(bool crouched)
		{
			IsCrouched = crouched;
			animator.SetBool("isCrouched", crouched);
		}

		public void SetGrounded(bool grounded)
		{
			animator.SetBool("isGrounded", grounded);
		}

		public void Jump()
		{
			animator.SetTrigger("Jump");
		}

		public void SetAnimationEnabled(bool enabled)
		{
			animationEnabled = enabled;
			UpdateAnimationActive();
		}

		public void Flinch(Vector3 forceDirection, EFlinchType flinchType)
		{
			Vector3 vector = base.transform.InverseTransformDirection(forceDirection);
			EFlinchDirection eFlinchDirection = EFlinchDirection.Forward;
			eFlinchDirection = ((Mathf.Abs(vector.z) > Mathf.Abs(vector.x)) ? ((!(vector.z > 0f)) ? EFlinchDirection.Backward : EFlinchDirection.Forward) : ((!(vector.x > 0f)) ? EFlinchDirection.Left : EFlinchDirection.Right));
			if (flinchType == EFlinchType.Light)
			{
				switch (eFlinchDirection)
				{
				case EFlinchDirection.Forward:
					animator.SetTrigger("Flinch_Forward");
					break;
				case EFlinchDirection.Backward:
					animator.SetTrigger("Flinch_Backward");
					break;
				case EFlinchDirection.Left:
					animator.SetTrigger("Flinch_Left");
					break;
				case EFlinchDirection.Right:
					animator.SetTrigger("Flinch_Right");
					break;
				}
				return;
			}
			switch (eFlinchDirection)
			{
			case EFlinchDirection.Forward:
				animator.SetTrigger("Flinch_Heavy_Forward");
				break;
			case EFlinchDirection.Backward:
				animator.SetTrigger("Flinch_Heavy_Backward");
				break;
			case EFlinchDirection.Left:
				animator.SetTrigger("Flinch_Heavy_Left");
				break;
			case EFlinchDirection.Right:
				animator.SetTrigger("Flinch_Heavy_Right");
				break;
			}
			if (onHeavyFlinch != null)
			{
				onHeavyFlinch.Invoke();
			}
		}

		public void PlayStandUpAnimation()
		{
			StandUpAnimationPlaying = true;
			if (onStandupStart != null)
			{
				onStandupStart.Invoke();
			}
			PopulateBoneTransforms(ragdollBoneTransforms);
			bool standUpFromBack = ShouldGetUpFromBack();
			PopulateAnimationStartBoneTransforms(StandUpFromFrontClipName, standUpFromFrontBoneTransforms);
			PopulateAnimationStartBoneTransforms(StandUpFromBackClipName, standUpFromBackBoneTransforms);
			BoneTransform[] finalBoneTransforms = (standUpFromBack ? standUpFromBackBoneTransforms : standUpFromFrontBoneTransforms);
			if (standUpRoutine != null)
			{
				StopCoroutine(standUpRoutine);
			}
			standUpRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(StandUpRoutine());
			IEnumerator StandUpRoutine()
			{
				float time = 0.3f;
				for (int j = 0; j < Bones.Length; j++)
				{
					Rigidbody component = null;
					if (Bones[j].TryGetComponent<Rigidbody>(out component))
					{
						component.interpolation = RigidbodyInterpolation.None;
					}
				}
				for (float i = 0f; i < time; i += Time.deltaTime)
				{
					for (int k = 0; k < Bones.Length; k++)
					{
						Bones[k].localPosition = Vector3.Lerp(ragdollBoneTransforms[k].Position, finalBoneTransforms[k].Position, i / time);
						Bones[k].localRotation = Quaternion.Lerp(ragdollBoneTransforms[k].Rotation, finalBoneTransforms[k].Rotation, i / time);
					}
					yield return new WaitForEndOfFrame();
				}
				for (int l = 0; l < Bones.Length; l++)
				{
					Bones[l].localPosition = finalBoneTransforms[l].Position;
					Bones[l].localRotation = finalBoneTransforms[l].Rotation;
				}
				SetAnimationEnabled(enabled: true);
				if (animator.enabled)
				{
					string trigger = (standUpFromBack ? "StandUp_Back" : "StandUp_Front");
					animator.SetTrigger(trigger);
				}
				for (int m = 0; m < Bones.Length; m++)
				{
					Rigidbody component2 = null;
					if (Bones[m].TryGetComponent<Rigidbody>(out component2))
					{
						component2.interpolation = RigidbodyInterpolation.Interpolate;
					}
				}
				yield return new WaitForSecondsRealtime(1.5f);
				if (onStandupDone != null)
				{
					onStandupDone.Invoke();
				}
				standUpRoutine = null;
				StandUpAnimationPlaying = false;
			}
		}

		protected void RagdollChange(bool oldValue, bool ragdoll, bool playStandUpAnim)
		{
			bool num = oldValue && !ragdoll && playStandUpAnim;
			if (ragdoll && IsSeated)
			{
				if (CurrentSeat != null)
				{
					CurrentSeat.SetOccupant(null);
					CurrentSeat = null;
				}
				animator.SetBool("Sitting", value: false);
				GetComponentInParent<NPCMovement>().SpeedController.RemoveSpeedControl("seated");
			}
			if (ragdoll && standUpRoutine != null)
			{
				StopCoroutine(standUpRoutine);
			}
			if (oldValue && !ragdoll)
			{
				AlignPositionToHips();
			}
			if (!num)
			{
				SetAnimationEnabled(!ragdoll);
			}
			else
			{
				PlayStandUpAnimation();
			}
		}

		private void AlignPositionToHips()
		{
			Vector3 position = HipBone.position;
			Quaternion rotation = HipBone.rotation;
			base.transform.position = HipBone.position;
			if (Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, 10f, GroundingMask))
			{
				base.transform.position = new Vector3(base.transform.position.x, hitInfo.point.y, base.transform.position.z);
			}
			base.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(ShouldGetUpFromBack() ? (-HipBone.up) : HipBone.up, Vector3.up), Vector3.up);
			HipBone.position = position;
			HipBone.rotation = rotation;
		}

		private bool ShouldGetUpFromBack()
		{
			return Vector3.Angle(HipBone.forward, Vector3.up) < 90f;
		}

		private void PopulateBoneTransforms(BoneTransform[] boneTransforms)
		{
			for (int i = 0; i < Bones.Length; i++)
			{
				boneTransforms[i].Position = Bones[i].localPosition;
				boneTransforms[i].Rotation = Bones[i].localRotation;
			}
		}

		private List<Pose> GetBoneTransforms()
		{
			List<Pose> list = new List<Pose>();
			for (int i = 0; i < Bones.Length; i++)
			{
				list.Add(new Pose(Bones[i].localPosition, Bones[i].localRotation));
			}
			return list;
		}

		private void PopulateAnimationStartBoneTransforms(string clipName, BoneTransform[] boneTransforms)
		{
			Vector3 position = animator.transform.position;
			Quaternion rotation = animator.transform.rotation;
			if (animator.runtimeAnimatorController == null)
			{
				return;
			}
			AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
			foreach (AnimationClip animationClip in animationClips)
			{
				if (animationClip.name == clipName)
				{
					animationClip.SampleAnimation(animator.gameObject, 0f);
					PopulateBoneTransforms(boneTransforms);
					break;
				}
			}
			animator.transform.position = position;
			animator.transform.rotation = rotation;
		}

		public void SetTrigger(string trigger)
		{
			if (!string.IsNullOrEmpty(trigger))
			{
				animator.SetTrigger(trigger);
				UpdateAnimationActive(forceWriteIdle: true);
			}
		}

		public void ResetTrigger(string trigger)
		{
			animator.ResetTrigger(trigger);
		}

		public void SetBool(string id, bool value)
		{
			animator.SetBool(id, value);
			UpdateAnimationActive(forceWriteIdle: true);
		}

		public void SetSeat(AvatarSeat seat)
		{
			Vector3 startPos;
			Quaternion startRot;
			Vector3 endPos;
			Quaternion endRot;
			if (!(seat == CurrentSeat))
			{
				if (CurrentSeat != null)
				{
					CurrentSeat.SetOccupant(null);
				}
				CurrentSeat = seat;
				if (CurrentSeat != null)
				{
					CurrentSeat.SetOccupant(GetComponentInParent<NPC>());
				}
				animator.SetBool("Sitting", IsSeated);
				startPos = base.transform.position;
				startRot = base.transform.rotation;
				if (seatRoutine != null)
				{
					Singleton<CoroutineService>.Instance.StopCoroutine(seatRoutine);
				}
				if (CurrentSeat != null)
				{
					endPos = CurrentSeat.SittingPoint.position + SITTING_OFFSET * base.transform.localScale.y;
					endRot = CurrentSeat.SittingPoint.rotation;
					seatRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Lerp(resetLocalCoordinates: false));
					GetComponentInParent<NPCMovement>().SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("seated", 100, -1f));
				}
				else
				{
					endPos = base.transform.parent.position;
					endRot = base.transform.parent.rotation;
					seatRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Lerp(resetLocalCoordinates: true));
				}
			}
			IEnumerator Lerp(bool resetLocalCoordinates)
			{
				for (float i = 0f; i < 0.5f; i += Time.deltaTime)
				{
					base.transform.position = Vector3.Lerp(startPos, endPos, i / 0.5f);
					base.transform.rotation = Quaternion.Lerp(startRot, endRot, i / 0.5f);
					yield return new WaitForEndOfFrame();
				}
				base.transform.position = endPos;
				base.transform.rotation = endRot;
				if (resetLocalCoordinates)
				{
					NPCMovement componentInParent = GetComponentInParent<NPCMovement>();
					componentInParent.transform.position = endPos;
					componentInParent.transform.rotation = endRot;
					base.transform.localPosition = Vector3.zero;
					base.transform.localRotation = Quaternion.identity;
					componentInParent.SpeedController.RemoveSpeedControl("seated");
				}
				seatRoutine = null;
			}
		}

		public void SkateboardMounted(Skateboard board)
		{
			IKController.BodyIK.solvers.pelvis.target = board.Animation.PelvisAlignment.Transform;
			IKController.BodyIK.solvers.spine.target = board.Animation.SpineAlignment.Transform;
			IKController.BodyIK.solvers.leftFoot.target = board.Animation.LeftFootAlignment.Transform;
			IKController.BodyIK.solvers.rightFoot.target = board.Animation.RightFootAlignment.Transform;
			IKController.BodyIK.solvers.leftHand.target = board.Animation.LeftHandAlignment.Transform;
			IKController.BodyIK.solvers.rightHand.target = board.Animation.RightHandAlignment.Transform;
			IKController.BodyIK.solvers.rightFoot.SetBendPlaneToCurrent();
			IKController.BodyIK.solvers.leftFoot.SetBendPlaneToCurrent();
			IKController.OverrideLegBendTargets(board.Animation.LeftLegBendTarget.Transform, board.Animation.RightLegBendTarget.Transform);
			IKController.SetIKActive(active: true);
			avatar.SetEquippable(string.Empty);
			avatar.LookController.ForceLookTarget = board.Animation.AvatarFaceTarget;
			avatar.LookController.ForceLookRotateBody = true;
			SetBool("SkateIdle", value: true);
			activeSkateboard = board;
			activeSkateboard.OnPushStart.AddListener(SkateboardPush);
		}

		public void SkateboardDismounted()
		{
			IKController.ResetLegBendTargets();
			IKController.SetIKActive(active: false);
			avatar.LookController.ForceLookTarget = null;
			avatar.LookController.ForceLookRotateBody = false;
			SetBool("SkateIdle", value: false);
			activeSkateboard.OnPushStart.RemoveListener(SkateboardPush);
			activeSkateboard = null;
		}

		private void SkateboardPush()
		{
			SetTrigger("SkatePush");
		}
	}
}
