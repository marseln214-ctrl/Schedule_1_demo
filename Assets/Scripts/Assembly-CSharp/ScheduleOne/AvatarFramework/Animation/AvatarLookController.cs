using System.Collections.Generic;
using System.Linq;
using RootMotion.FinalIK;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Animation
{
	public class AvatarLookController : MonoBehaviour
	{
		public const float LookAtPlayerRange = 4f;

		public const float EyeContractRange = 10f;

		public const float AimIKRange = 20f;

		public bool DEBUG;

		[Header("References")]
		public AimIK Aim;

		public Transform HeadBone;

		public Transform LookForwardTarget;

		public Transform LookOrigin;

		public EyeController Eyes;

		[Header("Optional NPC reference")]
		public NPC NPC;

		[Header("Settings")]
		public bool AutoLookAtPlayer = true;

		public float LookLerpSpeed = 1f;

		public float AimIKWeight = 0.6f;

		public float BodyRotationSpeed = 1f;

		private Avatar avatar;

		private Vector3 lookAtPos = Vector3.zero;

		private Transform lookAtTarget;

		private Vector3 lastFrameOffset = Vector3.zero;

		private bool overrideLookAt;

		private Vector3 overriddenLookTarget = Vector3.zero;

		private int overrideLookPriority;

		private bool overrideRotateBody;

		private Vector3 lastFrameLookOriginPos;

		private Vector3 lastFrameLookOriginForward;

		public Transform ForceLookTarget;

		public bool ForceLookRotateBody;

		private float defaultIKWeight = 0.6f;

		private Player nearestPlayer;

		private float nearestPlayerDist;

		private void Awake()
		{
			avatar = GetComponent<Avatar>();
			avatar.onRagdollChange.AddListener(RagdollChange);
			defaultIKWeight = Aim.solver.GetIKPositionWeight();
			lookAtTarget = new GameObject("LookAtTarget (" + base.gameObject.name + ")").transform;
			lookAtTarget.SetParent(GameObject.Find("_Temp")?.transform);
			LookForward();
			lookAtTarget.transform.position = lookAtPos;
			lastFrameOffset = LookOrigin.InverseTransformPoint(lookAtTarget.position);
			NPC = GetComponentInParent<NPC>();
			InvokeRepeating("UpdateNearestPlayer", 0f, 0.5f);
		}

		private void UpdateShit()
		{
			if (ForceLookTarget != null && CanLookAt(ForceLookTarget.position))
			{
				OverrideLookTarget(ForceLookTarget.position, 100, ForceLookRotateBody);
				return;
			}
			if (AutoLookAtPlayer && Player.Local != null && Player.Local.Paranoid)
			{
				OverrideLookTarget(Player.Local.MimicCamera.position, 200);
				Aim.enabled = nearestPlayerDist < 20f * QualitySettings.lodBias;
				Aim.solver.clampWeight = Mathf.MoveTowards(Aim.solver.clampWeight, AimIKWeight, Time.deltaTime * 2f);
				return;
			}
			if (nearestPlayer != null && CanLookAt(nearestPlayer.EyePosition.position) && AutoLookAtPlayer && (NPC == null || NPC.awareness.VisionCone.GetPlayerVisibility(nearestPlayer) > NPC.awareness.VisionCone.MinVisionDelta))
			{
				Vector3 position = nearestPlayer.EyePosition.position;
				if (nearestPlayer.IsOwner)
				{
					position = nearestPlayer.MimicCamera.position;
				}
				if (nearestPlayerDist < 4f)
				{
					lookAtPos = position;
				}
				else if (nearestPlayerDist < 10f && Vector3.Angle(position - HeadBone.position, HeadBone.forward) < 45f)
				{
					Transform mimicCamera = nearestPlayer.MimicCamera;
					if (Vector3.Angle(mimicCamera.forward, (HeadBone.position - mimicCamera.position).normalized) < 15f)
					{
						lookAtPos = position;
					}
					else
					{
						LookForward();
					}
				}
				else
				{
					LookForward();
				}
			}
			else
			{
				LookForward();
			}
			if (Aim != null)
			{
				if (avatar.Ragdolled || avatar.Anim.StandUpAnimationPlaying)
				{
					Aim.solver.clampWeight = 0f;
					Aim.enabled = false;
				}
				else
				{
					Aim.enabled = nearestPlayerDist < 20f * QualitySettings.lodBias;
					Aim.solver.clampWeight = Mathf.MoveTowards(Aim.solver.clampWeight, AimIKWeight, Time.deltaTime * 2f);
				}
			}
		}

		private void UpdateNearestPlayer()
		{
			List<Player> list = new List<Player>();
			foreach (Player player in Player.PlayerList)
			{
				if (player.Avatar.LookController == this)
				{
					list.Add(player);
				}
			}
			nearestPlayer = Player.GetClosestPlayer(base.transform.position, out nearestPlayerDist, list);
		}

		private void LateUpdate()
		{
			if (nearestPlayerDist > 30f * QualitySettings.lodBias)
			{
				if (Aim != null)
				{
					Aim.enabled = false;
				}
				return;
			}
			UpdateShit();
			if (overrideLookAt)
			{
				lookAtPos = overriddenLookTarget;
			}
			if (!avatar.Ragdolled)
			{
				if (overrideLookAt && overrideRotateBody)
				{
					Vector3 to = lookAtPos - base.transform.position;
					to.y = 0f;
					to.Normalize();
					float y = Vector3.SignedAngle(base.transform.parent.forward, to, Vector3.up);
					avatar.transform.localRotation = Quaternion.Lerp(avatar.transform.localRotation, Quaternion.Euler(0f, y, 0f), Time.deltaTime * BodyRotationSpeed);
				}
				else if (avatar.transform.parent != null)
				{
					avatar.transform.localRotation = Quaternion.Lerp(avatar.transform.localRotation, Quaternion.identity, Time.deltaTime * BodyRotationSpeed);
				}
			}
			LerpTargetTransform();
			Eyes.LookAt(lookAtPos);
			overrideLookAt = false;
			overriddenLookTarget = Vector3.zero;
			overrideLookPriority = 0;
			overrideRotateBody = false;
			lastFrameLookOriginPos = LookOrigin.position;
			lastFrameLookOriginForward = LookOrigin.forward;
		}

		public void OverrideLookTarget(Vector3 targetPosition, int priority, bool rotateBody = false)
		{
			if (!overrideLookAt || priority >= overrideLookPriority)
			{
				overrideLookAt = true;
				overriddenLookTarget = targetPosition;
				overrideLookPriority = priority;
				overrideRotateBody = rotateBody;
			}
		}

		private void LookForward()
		{
			LookForwardTarget.position = HeadBone.position + base.transform.forward * 1f;
			lookAtPos = LookForwardTarget.position;
		}

		private void LerpTargetTransform()
		{
			lookAtTarget.position = LookOrigin.TransformPoint(lastFrameOffset);
			Vector3 normalized = (lookAtTarget.position - LookOrigin.position).normalized;
			Vector3 normalized2 = (lookAtPos - LookOrigin.position).normalized;
			Vector3 vector = Vector3.Lerp(normalized, normalized2, Time.deltaTime * LookLerpSpeed);
			lookAtTarget.position = LookOrigin.position + vector;
			if (Aim != null)
			{
				Aim.solver.target = lookAtTarget;
			}
			lastFrameOffset = LookOrigin.InverseTransformPoint(lookAtTarget.position);
		}

		private Player GetNearestPlayer()
		{
			List<Player> playerList = Player.PlayerList;
			if (playerList.Count <= 0)
			{
				return null;
			}
			return playerList.OrderBy((Player p) => Vector3.Distance(p.transform.position, base.transform.position)).First();
		}

		private bool CanLookAt(Vector3 position)
		{
			return Vector3.Angle(lastFrameLookOriginForward, position - lastFrameLookOriginPos) < 90f;
		}

		protected void RagdollChange(bool oldValue, bool ragdoll, bool playStandUpAnim)
		{
		}

		public void OverrideIKWeight(float weight)
		{
			Aim.solver.SetIKPositionWeight(weight);
		}

		public void ResetIKWeight()
		{
			Aim.solver.SetIKPositionWeight(defaultIKWeight);
		}
	}
}
