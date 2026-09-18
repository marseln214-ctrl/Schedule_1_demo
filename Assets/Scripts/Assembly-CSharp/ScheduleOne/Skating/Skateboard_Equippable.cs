using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Skating
{
	public class Skateboard_Equippable : Equippable_Viewmodel
	{
		public const float ModelLerpSpeed = 8f;

		public const float SurfaceSampleDistance = 0.4f;

		public const float SurfaceSampleRayLength = 0.7f;

		public const float BoardSpawnUpwardsShift = 0.1f;

		public const float BoardSpawnAngleLimit = 30f;

		public const float MountTime = 0.33f;

		public const float BoardMomentumTransfer = 1.2f;

		public const float DismountAngle = 80f;

		public Skateboard SkateboardPrefab;

		public bool blockDismount;

		[Header("References")]
		public Transform ModelContainer;

		public Transform ModelPosition_Raised;

		public Transform ModelPosition_Lowered;

		private float mountTime;

		public bool IsRiding { get; private set; }

		public Skateboard ActiveSkateboard { get; private set; }

		public override void Equip(ItemInstance item)
		{
			base.Equip(item);
			GameInput.RegisterExitListener(Exit);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("heldskateboard");
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && action.exitType == ExitType.Escape && IsRiding)
			{
				action.used = true;
				Dismount();
			}
		}

		protected override void Update()
		{
			base.Update();
			if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && !blockDismount && !Singleton<PauseMenu>.Instance.IsPaused)
			{
				if (IsRiding)
				{
					if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
					{
						Dismount();
					}
				}
				else if (CanMountHere() && !PlayerSingleton<PlayerMovement>.Instance.isCrouched && (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) || mountTime > 0f))
				{
					mountTime += Time.deltaTime;
					Singleton<HUD>.Instance.ShowRadialIndicator(mountTime / 0.33f);
					if (mountTime >= 0.33f)
					{
						Mount();
					}
				}
				else
				{
					mountTime = 0f;
				}
			}
			else
			{
				mountTime = 0f;
			}
			if (IsRiding && Vector3.Angle(ActiveSkateboard.transform.up, Vector3.up) > 80f)
			{
				Dismount();
			}
			UpdateModel();
		}

		private void UpdateModel()
		{
			Vector3 b = (IsRiding ? ModelPosition_Lowered.localPosition : ModelPosition_Raised.localPosition);
			ModelContainer.localPosition = Vector3.Lerp(ModelContainer.localPosition, b, Time.deltaTime * 8f);
		}

		public override void Unequip()
		{
			base.Unequip();
			GameInput.DeregisterExitListener(Exit);
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			if (IsRiding)
			{
				Dismount();
			}
		}

		public void Mount()
		{
			IsRiding = true;
			mountTime = 0f;
			ActiveSkateboard = Object.Instantiate(SkateboardPrefab.gameObject, null).GetComponent<Skateboard>();
			ActiveSkateboard.Equippable = this;
			Pose skateboardSpawnPose = GetSkateboardSpawnPose();
			ActiveSkateboard.transform.position = skateboardSpawnPose.position;
			ActiveSkateboard.transform.rotation = skateboardSpawnPose.rotation;
			Player.Local.Spawn(ActiveSkateboard.NetworkObject, Player.Local.Connection);
			Vector3 velocity = Player.Local.VelocityCalculator.Velocity;
			ActiveSkateboard.SetVelocity(velocity * 1.2f);
			Player.Local.MountSkateboard(ActiveSkateboard);
			Player.Local.Avatar.SetEquippable(string.Empty);
		}

		public void Dismount()
		{
			IsRiding = false;
			mountTime = 0f;
			Vector3 velocity = ActiveSkateboard.Rb.velocity;
			float num = 50f;
			float time = 0.7f * Mathf.Clamp01(velocity.magnitude / 9f);
			Vector3 normalized = Vector3.ProjectOnPlane(velocity, Vector3.up).normalized;
			PlayerSingleton<PlayerMovement>.Instance.SetResidualVelocity(normalized, velocity.magnitude * num, time);
			Player.Local.DismountSkateboard();
			Player.Local.Despawn(ActiveSkateboard.NetworkObject);
			Object.Destroy(ActiveSkateboard.gameObject);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("heldskateboard");
			ActiveSkateboard = null;
		}

		private bool CanMountHere()
		{
			if (Vector3.Angle(GetSkateboardSpawnPose().rotation * Vector3.up, Vector3.up) > 30f)
			{
				return false;
			}
			return true;
		}

		private Pose GetSkateboardSpawnPose()
		{
			Vector3 vector = Player.Local.PlayerBasePosition + Player.Local.transform.forward * 0.4f + Vector3.up * 0.4f;
			Vector3 vector2 = Player.Local.PlayerBasePosition - Player.Local.transform.forward * 0.4f + Vector3.up * 0.4f;
			Debug.DrawRay(vector, Vector3.down * 0.7f, Color.cyan, 10f);
			Debug.DrawRay(vector2, Vector3.down * 0.7f, Color.cyan, 10f);
			if (!Physics.Raycast(vector, Vector3.down, out var hitInfo, 0.7f, SkateboardPrefab.GroundDetectionMask, QueryTriggerInteraction.Ignore))
			{
				hitInfo.point = vector + Vector3.down * 0.7f;
			}
			if (!Physics.Raycast(vector2, Vector3.down, out var hitInfo2, 0.7f, SkateboardPrefab.GroundDetectionMask, QueryTriggerInteraction.Ignore))
			{
				hitInfo2.point = vector2 + Vector3.down * 0.7f;
			}
			Vector3 position = (hitInfo.point + hitInfo2.point) / 2f + Vector3.up * 0.1f;
			Vector3 normalized = (hitInfo.point - hitInfo2.point).normalized;
			Vector3 normalized2 = Vector3.Cross(Vector3.up, normalized).normalized;
			Vector3 normalized3 = Vector3.Cross(normalized, normalized2).normalized;
			Quaternion quaternion = Quaternion.LookRotation(normalized, normalized3);
			Pose result = default(Pose);
			result.position = position;
			result.rotation = quaternion;
			return result;
		}
	}
}
