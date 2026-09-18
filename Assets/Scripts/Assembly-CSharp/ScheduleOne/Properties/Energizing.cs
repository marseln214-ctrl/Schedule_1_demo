using ScheduleOne.AvatarFramework;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Properties
{
	[CreateAssetMenu(fileName = "Energizing", menuName = "Properties/Energizing Property")]
	public class Energizing : Property
	{
		public const float SPEED_MULTIPLIER = 1.15f;

		public override void ApplyToNPC(NPC npc)
		{
			npc.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
			{
				bottomLidOpen = 0.6f,
				topLidOpen = 0.7f
			});
			npc.Avatar.Eyes.ForceBlink();
			npc.Movement.SpeedController.SpeedMultiplier = 1.15f;
		}

		public override void ApplyToPlayer(Player player)
		{
			player.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
			{
				bottomLidOpen = 0.6f,
				topLidOpen = 0.7f
			});
			player.Avatar.Eyes.ForceBlink();
			if (player.IsOwner)
			{
				PlayerSingleton<PlayerMovement>.Instance.MoveSpeedMultiplier = 1.15f;
				PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.AddOverride(5f, Tier, "energizing");
				PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.VolumeController.AddOverride(0.3f, Tier, "energizing");
				PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.PitchController.AddOverride(1.4f, Tier, "energizing");
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
				PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.RemoveOverride("energizing");
				PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.VolumeController.RemoveOverride("energizing");
				PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.PitchController.RemoveOverride("energizing");
			}
		}
	}
}
