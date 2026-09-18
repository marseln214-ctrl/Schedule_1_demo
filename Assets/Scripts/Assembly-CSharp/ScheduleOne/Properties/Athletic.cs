using ScheduleOne.AvatarFramework;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "Athletic", menuName = "Properties/Athletic Property")]
	public class Athletic : Property
	{
		public const float SPEED_MULTIPLIER = 1.3f;

		[ColorUsage(true, true)]
		[SerializeField]
		public Color TintColor = Color.white;

		public override void ApplyToNPC(NPC npc)
		{
			npc.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
			{
				bottomLidOpen = 0.7f,
				topLidOpen = 0.8f
			});
			npc.Avatar.Eyes.ForceBlink();
			npc.Movement.SpeedController.SpeedMultiplier = 1.3f;
		}

		public override void ApplyToPlayer(Player player)
		{
			player.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
			{
				bottomLidOpen = 0.7f,
				topLidOpen = 0.8f
			});
			player.Avatar.Eyes.ForceBlink();
			if (player.IsOwner)
			{
				PlayerSingleton<PlayerMovement>.Instance.MoveSpeedMultiplier = 1.3f;
				PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.AddOverride(10f, Tier, "athletic");
				PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.VolumeController.AddOverride(0.5f, Tier, "athletic");
				PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.PitchController.AddOverride(1.7f, Tier, "athletic");
				Singleton<PostProcessingManager>.Instance.ColorFilterController.AddOverride(TintColor, Tier, "athletic");
			}
		}

		public override void ClearFromNPC(NPC npc)
		{
			npc.Avatar.Eyes.ResetEyeLids();
			npc.Avatar.Eyes.ForceBlink();
			npc.Movement.SpeedController.SpeedMultiplier = 1f;
		}

		public override void ClearFromPlayer(Player player)
		{
			player.Avatar.Eyes.ResetEyeLids();
			player.Avatar.Eyes.ForceBlink();
			if (player.IsOwner)
			{
				PlayerSingleton<PlayerMovement>.Instance.MoveSpeedMultiplier = 1f;
				PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.RemoveOverride("athletic");
				PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.VolumeController.RemoveOverride("athletic");
				PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.PitchController.RemoveOverride("athletic");
				Singleton<PostProcessingManager>.Instance.ColorFilterController.RemoveOverride("athletic");
			}
		}
	}
}
