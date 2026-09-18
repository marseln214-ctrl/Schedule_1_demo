using FishNet;
using ScheduleOne.Combat;
using ScheduleOne.Law;
using ScheduleOne.NPCs.Responses;
using ScheduleOne.Noise;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using ScheduleOne.VoiceOver;

namespace ScheduleOne.Police
{
	public class NPCResponses_Police : NPCResponses
	{
		private PoliceOfficer officer;

		protected override void Awake()
		{
			base.Awake();
			officer = base.npc as PoliceOfficer;
		}

		public override void HitByCar(LandVehicle vehicle)
		{
			base.HitByCar(vehicle);
			base.npc.PlayVO(EVOLineType.Angry);
			if (vehicle.DriverPlayer != null && vehicle.DriverPlayer.IsOwner)
			{
				vehicle.DriverPlayer.CrimeData.AddCrime(new VehicularAssault());
				if (vehicle.DriverPlayer.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
				{
					vehicle.DriverPlayer.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.NonLethal);
				}
				else
				{
					vehicle.DriverPlayer.CrimeData.Escalate();
				}
			}
		}

		public override void NoticedDrugDeal(Player player)
		{
			base.NoticedDrugDeal(player);
			base.npc.PlayVO(EVOLineType.Command);
			if (player.IsOwner)
			{
				player.CrimeData.AddCrime(new DrugTrafficking());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.NetworkObject);
			}
		}

		public override void NoticedPettyCrime(Player player)
		{
			base.NoticedPettyCrime(player);
			base.npc.PlayVO(EVOLineType.Command);
			if (player.IsOwner)
			{
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.NetworkObject);
			}
		}

		public override void NoticedVandalism(Player player)
		{
			base.NoticedVandalism(player);
			base.npc.PlayVO(EVOLineType.Command);
			if (player.IsOwner)
			{
				player.CrimeData.AddCrime(new Vandalism());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.NetworkObject);
			}
		}

		public override void SawPickpocketing(Player player)
		{
			base.SawPickpocketing(player);
			base.npc.PlayVO(EVOLineType.Command);
			if (player.IsOwner)
			{
				player.CrimeData.AddCrime(new Theft());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.NetworkObject);
			}
		}

		public override void NoticePlayerBrandishingWeapon(Player player)
		{
			base.NoticePlayerBrandishingWeapon(player);
			base.npc.PlayVO(EVOLineType.Command);
			if (player.IsOwner)
			{
				player.CrimeData.AddCrime(new BrandishingWeapon());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.NonLethal);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.NetworkObject);
			}
		}

		public override void NoticePlayerDischargingWeapon(Player player)
		{
			base.NoticePlayerDischargingWeapon(player);
			base.npc.PlayVO(EVOLineType.Command);
			if (player.IsOwner)
			{
				player.CrimeData.AddCrime(new DischargeFirearm());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.NonLethal);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.NetworkObject);
			}
		}

		public override void NoticedWantedPlayer(Player player)
		{
			base.NoticedWantedPlayer(player);
			base.npc.PlayVO(EVOLineType.Command);
			if (player.IsOwner)
			{
				player.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: true);
				if (base.npc.CurrentVehicle != null)
				{
					(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.NetworkObject, includeColleagues: false);
					(base.npc as PoliceOfficer).BeginVehiclePursuit_Networked(player.NetworkObject, base.npc.CurrentVehicle.NetworkObject, beginAsSighted: true);
				}
				else
				{
					(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.NetworkObject);
				}
			}
		}

		public override void NoticedSuspiciousPlayer(Player player)
		{
			base.NoticedSuspiciousPlayer(player);
			if (player.IsOwner)
			{
				(base.npc as PoliceOfficer).BeginBodySearch_Networked(player.NetworkObject);
			}
		}

		public override void NoticedViolatingCurfew(Player player)
		{
			base.NoticedViolatingCurfew(player);
			base.npc.PlayVO(EVOLineType.Command);
			if (player.IsOwner)
			{
				player.CrimeData.AddCrime(new ViolatingCurfew());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				if (base.npc.CurrentVehicle != null)
				{
					(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.NetworkObject, includeColleagues: false);
					(base.npc as PoliceOfficer).BeginVehiclePursuit_Networked(player.NetworkObject, base.npc.CurrentVehicle.NetworkObject, beginAsSighted: true);
				}
				else
				{
					(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.NetworkObject);
				}
			}
		}

		protected override void RespondToFirstNonLethalAttack(Player perpetrator, Impact impact)
		{
			base.RespondToFirstNonLethalAttack(perpetrator, impact);
			perpetrator.CrimeData.AddCrime(new Assault());
			if (perpetrator.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
			{
				perpetrator.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				officer.BeginFootPursuit_Networked(perpetrator.NetworkObject);
			}
			else
			{
				perpetrator.CrimeData.Escalate();
				officer.BeginFootPursuit_Networked(perpetrator.NetworkObject);
			}
		}

		protected override void RespondToLethalAttack(Player perpetrator, Impact impact)
		{
			base.RespondToLethalAttack(perpetrator, impact);
			perpetrator.CrimeData.AddCrime(new DeadlyAssault());
			if (perpetrator.CrimeData.CurrentPursuitLevel < PlayerCrimeData.EPursuitLevel.Lethal)
			{
				perpetrator.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Lethal);
				officer.BeginFootPursuit_Networked(perpetrator.NetworkObject);
			}
		}

		protected override void RespondToRepeatedNonLethalAttack(Player perpetrator, Impact impact)
		{
			base.RespondToRepeatedNonLethalAttack(perpetrator, impact);
			if (!perpetrator.CrimeData.IsCrimeOnRecord(typeof(Assault)))
			{
				perpetrator.CrimeData.AddCrime(new Assault());
			}
			if (perpetrator.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
			{
				perpetrator.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				officer.BeginFootPursuit_Networked(perpetrator.NetworkObject);
			}
			else
			{
				perpetrator.CrimeData.Escalate();
				officer.BeginFootPursuit_Networked(perpetrator.NetworkObject);
			}
		}

		protected override void RespondToAnnoyingImpact(Player perpetrator, Impact impact)
		{
			base.RespondToAnnoyingImpact(perpetrator, impact);
			base.npc.VoiceOverEmitter.Play(EVOLineType.Annoyed);
			base.npc.dialogueHandler.PlayReaction("annoyed", 2.5f, network: false);
			base.npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "annoyed", 20f, 3);
			if (InstanceFinder.IsServer)
			{
				base.npc.behaviour.FacePlayerBehaviour.SetTarget(perpetrator.NetworkObject);
				base.npc.behaviour.FacePlayerBehaviour.Enable_Networked(null);
			}
		}

		public override void RespondToAimedAt(Player player)
		{
			base.RespondToAimedAt(player);
			if (player.CrimeData.CurrentPursuitLevel < PlayerCrimeData.EPursuitLevel.Lethal)
			{
				player.CrimeData.AddCrime(new Assault());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Lethal);
			}
		}

		public override void ImpactReceived(Impact impact)
		{
			base.ImpactReceived(impact);
			if (officer.PursuitBehaviour.Active)
			{
				officer.PursuitBehaviour.ResetArrestProgress();
			}
		}

		public override void GunshotHeard(NoiseEvent gunshotSound)
		{
			base.GunshotHeard(gunshotSound);
			if (gunshotSound.source != null && gunshotSound.source.GetComponent<Player>() != null)
			{
				officer.behaviour.FacePlayerBehaviour.SetTarget(gunshotSound.source.GetComponent<Player>().NetworkObject);
				officer.behaviour.FacePlayerBehaviour.SendEnable();
			}
		}
	}
}
